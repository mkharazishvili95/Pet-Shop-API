using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Pet_Shop_API.Identity;
using Pet_Shop_API.Models;
using Pet_Shop_API.Validation;
using System.Data.Common;
using System.Data.SqlClient;

namespace Pet_Shop_API.Services
{
    public interface IAdminService
    {
        Task<User> Login(UserLoginModel loginAdmin);
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetUserById(int userId);
        Task<User> BalanceTopUp(BalanceTopUpModel balanceTopUpModel);
        Task<Categories> AddNewCategory(Categories newCategory);
        Task<bool> DeleteCategoryById(int categoryId);
    }
    public class AdminService : IAdminService
    {
        private readonly IConfiguration _configuration;
        public AdminService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<Categories> AddNewCategory(Categories newCategory)
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var addNewCategory = await connection.ExecuteAsync("INSERT INTO Categories (Name) VALUES (@Name);", newCategory);
                    return addNewCategory > 0 ? newCategory : null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding category: {ex.Message}");
                return null;
            }
        }


        public async Task<User> BalanceTopUp(BalanceTopUpModel balanceTopUpModel)
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var existingUser = await connection.QueryAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = balanceTopUpModel.UserId });

                    if (!existingUser.Any())
                    {
                        return null;
                    }
                    else
                    {
                        var query1 = "UPDATE Users SET Balance = @Balance WHERE Id = @Id";
                        var topUpBalance = await connection.ExecuteAsync(query1, new { Id = balanceTopUpModel.UserId, Balance = balanceTopUpModel.Amount });

                        return existingUser.First();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteCategoryById(int categoryId)
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Categories WHERE Id = @Id";
                    var existingCategory = await connection.QueryFirstOrDefaultAsync(query, new { Id = categoryId });

                    if (existingCategory == null)
                    {
                        return false;
                    }
                    else
                    {
                        var deleteQuery = await connection.ExecuteAsync("DELETE FROM Categories WHERE Id = @Id", new { Id = existingCategory.Id });
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            try
            {
                using(var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Users";
                    var userList =  connection.Query<User>(query);
                    if(userList == null || !userList.Any())
                    {
                        return null;
                    }
                    else
                    {
                        return userList.ToList();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<User> GetUserById(int userId)
        {
            try
            {
                using(var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Users WHERE Id = @Id";
                    var existingUser = await connection.QueryAsync<User>(query, new {Id = userId});
                    if(existingUser == null || !existingUser.Any())
                    {
                        return null;
                    }
                    else
                    {
                        return existingUser.First();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<User> Login(UserLoginModel loginAdmin)
        {
            try
            {
                if(string.IsNullOrEmpty(loginAdmin.Email) || string.IsNullOrEmpty(loginAdmin.Password))
                {
                    return null;
                }
                using(var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Users WHERE Email = @Email";
                    var existingAdmin = await connection.QueryFirstOrDefaultAsync<User>(query, new {Email = loginAdmin.Email});
                    if (loginAdmin.Email.ToUpper() != existingAdmin?.Email.ToUpper())
                    {
                        return null;
                    }
                    if(loginAdmin.Password != existingAdmin.Password)
                    {
                        return null;
                    }
                    else
                    {
                        return existingAdmin;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
