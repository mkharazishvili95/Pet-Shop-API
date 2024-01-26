using System.Threading.Tasks;
using FakeServices;
using NUnit.Framework;
using Pet_Shop_API.Helpers;
using Pet_Shop_API.Identity;
using Pet_Shop_API.Models;
using Pet_Shop_API.Services;

namespace FakeServices
{
    [TestFixture]
    public class UserServiceTests
    {
        private FakeUserService fakeUserService;

        [SetUp]
        public void Setup()
        {
            fakeUserService = new FakeUserService();
        }

        [Test]
        public async Task RegisterNewUser_ValidModel_ReturnsUser()
        {
            var userRegisterModel = new UserRegisterModel
            {
                FirstName = "Mikheil",
                LastName = "Kharazishvili",
                Age = 28,
                ContactNumber = 599999999,
                Email = "misho123@gmail.com",
                Password = HashSettings.HashPassword("misho123"),
                Address = "Georgia, Tbilisi"
            };
            var result = await fakeUserService.RegisterNewUser(userRegisterModel);
            Assert.NotNull(result);
            Assert.AreEqual(userRegisterModel.FirstName, result.FirstName);
            Assert.AreEqual(userRegisterModel.LastName, result.LastName);
            Assert.AreEqual(userRegisterModel.Age, result.Age);
            Assert.AreEqual(userRegisterModel.ContactNumber, result.ContactNumber);
            Assert.AreEqual(userRegisterModel.Email, result.Email);
            Assert.AreEqual(userRegisterModel.Address, result.Address);
        }

        [Test]
        public async Task LoginUser_ValidCredentials_ReturnsUser()
        {
            var userLoginModel = new UserLoginModel
            {
                Email = "misho123@gmail.com",
                Password = HashSettings.HashPassword("misho123"),
            };
            var result = await fakeUserService.LoginUser(userLoginModel);
            Assert.NotNull(result);
            Assert.AreEqual("Mikheil", result.FirstName);
            Assert.AreEqual("Kharazishvili", result.LastName);
            Assert.AreEqual(28, result.Age);
            Assert.AreEqual(599999999, result.ContactNumber);
            Assert.AreEqual("misho123@gmail.com", result.Email);
            Assert.AreEqual("Georgia, Tbilisi", result.Address);
        }
    }
}
