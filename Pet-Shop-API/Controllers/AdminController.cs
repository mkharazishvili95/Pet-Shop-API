using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Pet_Shop_API.Helpers;
using Pet_Shop_API.Identity;
using Pet_Shop_API.Models;
using Pet_Shop_API.Services;
using Pet_Shop_API.Validation;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pet_Shop_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;
        public AdminController(IAdminService adminService, IConfiguration configuration, AppSettings appSettings)
        {
            _adminService = adminService;
            _configuration = configuration;
            _appSettings = appSettings;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        [HttpPost("AdminLogin")]
        public async Task<IActionResult> AdminLogin(UserLoginModel adminLogin)
        {
            if(string.IsNullOrEmpty(adminLogin.Email) || string.IsNullOrEmpty(adminLogin.Password))
            {
                return BadRequest(new { LoginError = "Email or Password is empty!" });
            }
            using(var connection = Connection)
            {
                connection.Open();
                var query = "SELECT * FROM Users WHERE Email = @Email";
                var existingAdmin = await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = adminLogin.Email });
                if (adminLogin.Email.ToUpper() != existingAdmin?.Email.ToUpper())
                {
                    return BadRequest(new { LoginError = "Email or Password is incorrect!" });
                }
                if(adminLogin.Password != existingAdmin.Password)
                {
                    return BadRequest(new { LoginError = "Email or Password is incorrect!" });
                }
                if(adminLogin.Email != existingAdmin.Email || adminLogin.Password != existingAdmin.Password)
                {
                    return BadRequest(new { LoginError = "Email or Password is incorrect!" });
                }
                else
                {
                    var newLogg = await connection.ExecuteAsync(
                        @"INSERT INTO Loggs (LoggedUser, UserId, LoggedDate) VALUES (@LoggedUser, @UserId, @LoggedDate);",
                        new
                        {
                            LoggedUser = existingAdmin.Email,
                            UserId = existingAdmin.Id,
                            LoggedDate = DateTime.Now
                        });

                    var tokenString = GenerateToken(existingAdmin);

                    return Ok(new
                    {
                        Message = "You have successfully Logged!",
                        Email = existingAdmin.Email,
                        Role = existingAdmin.Role,
                        Token = tokenString
                    });
                }
            }
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("AddNewCategory")]
        public async Task<IActionResult> AddNewCategory(Categories newCategory)
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var addNewCategory = await connection.ExecuteAsync("INSERT INTO Categories (Name) VALUES (@Name);", newCategory);
                    if (addNewCategory > 0)
                    {
                        return Ok($"Category: {addNewCategory} has successfully added by Admin!");
                    }
                    else
                    {
                        return BadRequest("Error!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding category: {ex.Message}");
                return null;
            }
        }

        


        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            using(var connection = Connection)
            {
                connection.Open();
                var userList = _adminService.GetAllUsers();
                if(userList == null)
                {
                    return NotFound(new { Message = "There is no any Users in the database yet!" });
                }
                else
                {
                    return Ok(userList);
                }
            }
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var existingUser = await _adminService.GetUserById(userId);
            if(existingUser == null)
            {
                return NotFound(new { Message = $"There is no any User by ID: {userId}" });
            }
            else
            {
                return Ok(existingUser);
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPost("Balance/TopUp")]
        public async Task<IActionResult> BalanceTopUp([FromBody] BalanceTopUpModel balanceTopUpModel)
        {
            try
            {
                var updatedUser = await _adminService.BalanceTopUp(balanceTopUpModel);

                if (updatedUser == null)
                {
                    return NotFound(new {Message = $"User by ID: {balanceTopUpModel.UserId} not found!"});
                }

                return Ok(new {SuccessMessage = "Balance top up has successfully progressed!"});
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("CancelPurchase")]
        public async Task<IActionResult> CancelPurchase(int purchaseId)
        {
            using (var connection = Connection)
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var purchase = connection.QueryFirstOrDefault<PurchaseModel>("SELECT * FROM Purchases WHERE Id = @Id", new { Id = purchaseId }, transaction);
                        if (purchase == null)
                        {
                            transaction.Rollback();
                            return BadRequest(new { ErrorMessage = $"There is no any Purchase by ID: {purchaseId} to cancel!" });
                        }
                        var product = connection.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = purchase.ProductId }, transaction);
                        var user = connection.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = purchase.UserId }, transaction);
                        var receiverBank = connection.QueryFirstOrDefault<Bank>("SELECT * FROM Bank WHERE Id = @Id", new { Id = 1 }, transaction) ?? new Bank();
                        if (product == null || user == null || receiverBank == null)
                        {
                            transaction.Rollback();
                            return BadRequest("Error: User, Product or Bank does not exist!");
                        }
                        var paymentCalculator = (product.Price * purchase.Quantity);
                        user.Balance += paymentCalculator;
                        receiverBank.Balance -= paymentCalculator;
                        product.Quantity += purchase.Quantity;
                        if (product.Quantity > 0 && !product.IsAvailable)
                        {
                            product.IsAvailable = true;
                        }
                        connection.Execute("UPDATE Users SET Balance = @Balance WHERE Id = @Id", new { Balance = user.Balance, Id = user.Id }, transaction);
                        connection.Execute("UPDATE Bank SET Balance = @Balance WHERE Id = @Id", new { Balance = receiverBank.Balance, Id = 1 }, transaction);
                        connection.Execute("UPDATE Products SET Quantity = @Quantity, IsAvailable = @IsAvailable WHERE Id = @Id", new { Quantity = product.Quantity, IsAvailable = product.IsAvailable, Id = product.Id }, transaction);
                        connection.Execute("DELETE FROM Purchases WHERE Id = @Id", new { Id = purchase.Id }, transaction);
                        transaction.Commit();
                        return Ok(new { SuccessMessage = "Purchase has been successfully canceled by Admin!" });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest($"Error: {ex.Message}");
                    }
                }
            }
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("PurchaseCancelationWithoutRefund")]
        public async Task<IActionResult> PurchaseCancelationWithoutRefund(int purchaseId)
        {
            using (var connection = Connection)
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var purchase = connection.QueryFirstOrDefault<PurchaseModel>("SELECT * FROM Purchases WHERE Id = @Id", new { Id = purchaseId }, transaction);
                        if (purchase == null)
                        {
                            transaction.Rollback();
                            return BadRequest(new { ErrorMessage = $"There is no any Purchase by ID: {purchaseId} to cancel!" });
                        }
                        var product = connection.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = purchase.ProductId }, transaction);
                        var user = connection.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = purchase.UserId }, transaction);
                        var receiverBank = connection.QueryFirstOrDefault<Bank>("SELECT * FROM Bank WHERE Id = @Id", new { Id = 1 }, transaction) ?? new Bank();
                        if (product == null || user == null || receiverBank == null)
                        {
                            transaction.Rollback();
                            return BadRequest("Error: User, Product or Bank does not exist!");
                        }
                        product.Quantity += purchase.Quantity;
                        if (product.Quantity > 0 && !product.IsAvailable)
                        {
                            product.IsAvailable = true;
                        }
                        connection.Execute("UPDATE Products SET Quantity = @Quantity, IsAvailable = @IsAvailable WHERE Id = @Id", new { Quantity = product.Quantity, IsAvailable = product.IsAvailable, Id = product.Id }, transaction);
                        connection.Execute("DELETE FROM Purchases WHERE Id = @Id", new { Id = purchase.Id }, transaction);
                        transaction.Commit();
                        return Ok(new { SuccessMessage = "Purchase has been successfully canceled by Admin!" });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest($"Error: {ex.Message}");
                    }
                }
            }
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("DeleteCategoryById")]
        public async Task<IActionResult> DeleteCategoryById(int categoryId)
        {
            try
            {
                var isDeleted = await _adminService.DeleteCategoryById(categoryId);

                if (!isDeleted)
                {
                    return BadRequest(new { ErrorMessage = $"There is no category with ID: {categoryId} to delete!" });
                }

                return Ok(new { SuccessMessage = "Category has been successfully deleted!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        private object GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Email.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(365),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }
    }
}
