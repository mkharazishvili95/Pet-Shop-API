using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pet_Shop_API.Helpers;
using Pet_Shop_API.Identity;
using Pet_Shop_API.Models;
using Pet_Shop_API.Services;

namespace FakeServices
{
    public class FakeAdminService : IAdminService
    {
        private List<User> fakeUsers;
        private List<Categories> fakeCategories;

        public FakeAdminService()
        {
            fakeCategories = new List<Categories>
            {
                new Categories { Id = 1, Name = "FakeCategory1" },
                new Categories { Id = 2, Name = "FakeCategory2" }
            };

            fakeUsers = new List<User>
            {
                new User
                {
                    Id = 1,
                    FirstName = "Gela",
                    LastName = "Tsanava",
                    Email = "G.Tsanava@gmail.com",
                    Password = HashSettings.HashPassword("Gtsanava123"),
                    Balance = 100
                },
            };
        }

        public Task<bool> DeleteCategoryById(int categoryId)
        {
            try
            {
                var existingCategory = fakeCategories.FirstOrDefault(c => c.Id == categoryId);

                if (existingCategory == null)
                {
                    return Task.FromResult(false);
                }
                else
                {
                    fakeCategories.Remove(existingCategory);
                    return Task.FromResult(true);
                }
            }
            catch
            {
                return Task.FromResult(false);
            }
        }


        public Task<Categories> AddNewCategory(Categories newCategory)
        {
            try
            {
                newCategory.Id = fakeCategories.Count + 1;
                fakeCategories.Add(newCategory);
                return Task.FromResult(newCategory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding category: {ex.Message}");
                return Task.FromResult<Categories>(null);
            }
        }
        public Task<User> BalanceTopUp(BalanceTopUpModel balanceTopUpModel)
        {
            try
            {
                var existingUser = fakeUsers.FirstOrDefault(u => u.Id == balanceTopUpModel.UserId);

                if (existingUser == null)
                {
                    return Task.FromResult<User>(null);
                }
                else
                {
                    existingUser.Balance += balanceTopUpModel.Amount;
                    return Task.FromResult(existingUser);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Task.FromResult<User>(null);
            }
        }

        public Task<IEnumerable<User>> GetAllUsers()
        {
            return Task.FromResult<IEnumerable<User>>(fakeUsers);
        }

        public Task<User> GetUserById(int userId)
        {
            var existingUser = fakeUsers.FirstOrDefault(u => u.Id == userId);
            return Task.FromResult(existingUser);
        }

        public Task<User> Login(UserLoginModel loginAdmin)
        {
            var existingUser = fakeUsers.FirstOrDefault(u => u.Email.ToUpper() == loginAdmin.Email?.ToUpper());

            if (existingUser == null || existingUser.Password != loginAdmin.Password)
            {
                return Task.FromResult<User>(null);
            }
            else
            {
                return Task.FromResult(existingUser);
            }
        }
    }
}
