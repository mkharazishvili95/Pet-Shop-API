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
    public class AdminServiceTests
    {
        private FakeAdminService fakeAdminService;

        [SetUp]
        public void Setup()
        {
            fakeAdminService = new FakeAdminService();
        }

        [Test]
        public async Task BalanceTopUp_ValidModel_ReturnsUserWithUpdatedBalance()
        {
            var balanceTopUpModel = new BalanceTopUpModel
            {
                UserId = 1,
                Amount = 50
            };
            var result = await fakeAdminService.BalanceTopUp(balanceTopUpModel);
            Assert.NotNull(result);
            Assert.AreEqual(150, result.Balance);
        }

        [Test]
        public async Task GetAllUsers_ReturnsAllFakeUsers()
        {
            var result = await fakeAdminService.GetAllUsers();
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public async Task GetUserById_ExistingUserId_ReturnsUser()
        {
            var existingUserId = 1;
            var result = await fakeAdminService.GetUserById(existingUserId);
            Assert.NotNull(result);
            Assert.AreEqual(existingUserId, result.Id);
        }

        [Test]
        public async Task GetUserById_NonExistingUserId_ReturnsNull()
        {
            var nonExistingUserId = 2;
            var result = await fakeAdminService.GetUserById(nonExistingUserId);
            Assert.Null(result);
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsUser()
        {
            var validLoginModel = new UserLoginModel
            {
                Email = "G.Tsanava@gmail.com",
                Password = HashSettings.HashPassword("Gtsanava123")
            };
            var result = await fakeAdminService.Login(validLoginModel);
            Assert.NotNull(result);
            Assert.AreEqual("Gela", result.FirstName);
        }

        [Test]
        public async Task AddNewCategory_ValidCategory_ReturnsAddedCategory()
        {
            var newCategory = new Categories
            {
                Name = "NewFakeCategory"
            };
            var result = await fakeAdminService.AddNewCategory(newCategory);
            Assert.NotNull(result);
            Assert.AreEqual(newCategory.Name, result.Name);
            Assert.AreNotEqual(0, result.Id);
        }

        [Test]
        public async Task AddNewCategory_InvalidCategory_ReturnsNotNull()
        {
            var invalidCategory = new Categories();
            var result = await fakeAdminService.AddNewCategory(invalidCategory);
            Assert.NotNull(result);
        }
        [Test]
        public async Task DeleteCategoryById_ExistingCategoryId_ReturnsTrue()
        {
            var existingCategoryId = 1;
            var result = await fakeAdminService.DeleteCategoryById(existingCategoryId);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeleteCategoryById_NonExistingCategoryId_ReturnsFalse()
        {
            var nonExistingCategoryId = 3;
            var result = await fakeAdminService.DeleteCategoryById(nonExistingCategoryId);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task Login_InvalidCredentials_ReturnsNull()
        {
            var invalidLoginModel = new UserLoginModel
            {
                Email = "Fake@gmail.com",
                Password = "IncorrectPassword=)"
            };
            var result = await fakeAdminService.Login(invalidLoginModel);
            Assert.Null(result);
        }
    }
}
