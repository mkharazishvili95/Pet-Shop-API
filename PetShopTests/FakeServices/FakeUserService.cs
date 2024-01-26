using System.Threading.Tasks;
using Pet_Shop_API.Helpers;
using Pet_Shop_API.Identity;
using Pet_Shop_API.Models;
using Pet_Shop_API.Services;

namespace FakeServices
{
    public class FakeUserService : IUserService
    {
        public Task<User> LoginUser(UserLoginModel loginModel)
        {
            var fakeUser = new User
            {
                FirstName = "Mikheil",
                LastName = "Kharazishvili",
                Age = 28,
                ContactNumber = 599999999,
                Email = "misho123@gmail.com",
                Password = HashSettings.HashPassword("misho123"),
                Address = "Georgia, Tbilisi"
            };

            return Task.FromResult(fakeUser);
        }

        public Task<User> RegisterNewUser(UserRegisterModel registerModel)
        {
            var fakeUser = new User
            {
                FirstName = registerModel.FirstName,
                LastName = registerModel.LastName,
                Age = registerModel.Age,
                ContactNumber = registerModel.ContactNumber,
                Email = registerModel.Email,
                Password = HashSettings.HashPassword("misho123"),
                Address = registerModel.Address
            };

            return Task.FromResult(fakeUser);
        }
    }
}
