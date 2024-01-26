using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Pet_Shop_API.Helpers;
using Pet_Shop_API.Identity;
using Pet_Shop_API.Models;
using Pet_Shop_API.Validation;
using System.Data.Common;
using System.Data.SqlClient;

namespace Pet_Shop_API.Services
{

    public interface IUserService
    {
        Task<User> RegisterNewUser(UserRegisterModel registerModel);
        Task<User> LoginUser(UserLoginModel loginModel);
    }
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<User> LoginUser(UserLoginModel loginModel)
        {
            try
            {
                if(string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
                {
                    return null;
                }
                using(var connection = Connection)
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM Users WHERE Email = @Email;";
                    var existingUser = await connection.QueryFirstOrDefaultAsync<User>(selectQuery, new { Email = loginModel.Email });
                    if(existingUser == null)
                    {
                        return null;
                    }
                    if(HashSettings.HashPassword(loginModel.Password) != existingUser.Password)
                    {
                        return null;
                    }
                    if(loginModel.Email.ToUpper() != existingUser.Email.ToUpper())
                    {
                        return null;
                    }
                    else
                    {
                        return existingUser;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<User> RegisterNewUser(UserRegisterModel registerModel)
        {
            try
            {
                var userValidator = new UserRegisterValidator(_configuration);
                var validatorResults = userValidator.Validate(registerModel);

                if (!validatorResults.IsValid)
                {
                    return null;
                }
                else
                {
                    using (var connection = Connection)
                    {
                        connection.Open();

                        var newUser = new User()
                        {
                            FirstName = registerModel.FirstName,
                            LastName = registerModel.LastName,
                            Age = registerModel.Age,
                            ContactNumber = registerModel.ContactNumber,
                            Email = registerModel.Email,
                            Password = HashSettings.HashPassword(registerModel.Password),
                            Address = registerModel.Address
                        };
                        string insertQuery = @"
                        INSERT INTO Users (FirstName, LastName, Age, ContactNumber, Email, Password, Address)
                        VALUES (@FirstName, @LastName, @Age, @ContactNumber, @Email, @Password, @Address);
                        SELECT LAST_INSERT_ID();";

                        int userId = await connection.QuerySingleAsync<int>(insertQuery, newUser);
                        var insertedUser = await connection.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE UserId = @UserId", new { UserId = userId });
                        return insertedUser;
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
