using Dapper;
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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pet_Shop_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;
        public UserController(IUserService userService, IConfiguration configuration, AppSettings appSettings)
        {
            _userService = userService;
            _configuration = configuration;
            _appSettings = appSettings;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterModel newUser)
        {
            try
            {
                var userValidator = new UserRegisterValidator(_configuration);
                var validatorResults = userValidator.Validate(newUser);

                if (!validatorResults.IsValid)
                {
                    return BadRequest(validatorResults.Errors);
                }
                else
                {
                    using (var connection = Connection)
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                var newUserEntity = new User()
                                {
                                    FirstName = newUser.FirstName,
                                    LastName = newUser.LastName,
                                    Age = newUser.Age,
                                    ContactNumber = newUser.ContactNumber,
                                    Email = newUser.Email,
                                    Password = HashSettings.HashPassword(newUser.Password),
                                    Address = newUser.Address
                                };

                                string insertQuery = @"
                            INSERT INTO Users (FirstName, LastName, Age, ContactNumber, Email, Password, Address)
                            VALUES (@FirstName, @LastName, @Age, @ContactNumber, @Email, @Password, @Address);
                            SELECT SCOPE_IDENTITY();";

                                int userId = await connection.QuerySingleAsync<int>(insertQuery, newUserEntity, transaction);
                                transaction.Commit();
                                var insertedUser = await connection.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @UserId", new { UserId = userId });

                                return Ok(new { SuccessMessage = $"User: {insertedUser.Email} has successfully registered!" });
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                Console.WriteLine($"Registration failed: {ex.Message}");
                                return BadRequest(new { Error = "Registration Error!" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Validation failed: {ex.Message}");
                return BadRequest(new { Error = "Validation Error!" });
            }
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginModel loginModel)
        {
            using (var connection = Connection)
            {
                connection.Open();
                var existingUser = await _userService.LoginUser(loginModel);

                if (string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
                {
                    return BadRequest(new { Error = "Enter your Email and Password!" });
                }

                if (loginModel.Email.ToUpper() != existingUser?.Email.ToUpper())
                {
                    return BadRequest(new { Error = "Email or Password is incorrect!" });
                }

                if (HashSettings.HashPassword(loginModel.Password) != existingUser?.Password)
                {
                    return BadRequest(new { Error = "Email or Password is incorrect!" });
                }

                if (loginModel.Email.ToUpper() == existingUser?.Email.ToUpper() && HashSettings.HashPassword(loginModel.Password) == existingUser?.Password)
                {
                    var newLogg = await connection.ExecuteAsync(
                        @"INSERT INTO Loggs (LoggedUser, UserId, LoggedDate) VALUES (@LoggedUser, @UserId, @LoggedDate);",
                        new { 
                            LoggedUser = existingUser.Email, UserId = existingUser.Id, LoggedDate = DateTime.Now 
                        });

                    var tokenString = GenerateToken(existingUser);

                    return Ok(new
                    {
                        Message = "You have successfully Logged!",
                        Email = existingUser.Email,
                        Role = existingUser.Role,
                        Token = tokenString
                    });
                }
                return BadRequest(new { Error = "Login Error!" });
            }
        }
        [Authorize]
        [HttpGet("GetMyBalance")]
        public async Task<IActionResult> GetMyBalance()
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var userId = int.Parse(User.Identity.Name);
                    var existingUserQuery = "SELECT * FROM Users WHERE Id = @Id";
                    var existingUser = await connection.QueryFirstOrDefaultAsync<User>(existingUserQuery, new { Id = userId });

                    if (existingUser != null)
                    {
                        return Ok($"Your balance is: {existingUser.Balance} GEL.");
                    }
                    else
                    {
                        return NotFound(new {Message = "User not found!"});
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error!");
            }
        }

        [Authorize]
        [HttpGet("GetMyProfile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var userId = int.Parse(User.Identity.Name);
                    var existingUserQuery = "SELECT * FROM Users WHERE Id = @Id";
                    var existingUser = await connection.QueryFirstOrDefaultAsync<User>(existingUserQuery, new { Id = userId });

                    if (existingUser != null)
                    {
                        return Ok(new {existingUser});
                    }
                    else
                    {
                        return NotFound(new { Message = "User not found!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error!");
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
