using Pet_Shop_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeServices
{
    [TestFixture]
    public class ProductServiceTests
    {
        private FakeProductService fakeProductService;

        [SetUp]
        public void Setup()
        {
            fakeProductService = new FakeProductService();
        }

        [Test]
        public async Task AddProduct_ValidModel_ReturnsNewProduct()
        {
            var newProduct = new AddNewProduct
            {
                Name = "NewFakeProduct",
                Description = "Description for NewFakeProduct",
                Price = 19.99,
                Quantity = 30,
                IsAvailable = true,
                CategoryId = 3
            };
            var result = await fakeProductService.AddProduct(newProduct);
            Assert.NotNull(result);
            Assert.AreEqual("NewFakeProduct", result.Name);
        }
        [Test]
        public async Task GetAllCategories_ReturnsFakeCategories()
        {
            var result = await fakeProductService.GetAllCategories();
            Assert.NotNull(result);
            Assert.IsInstanceOf<IEnumerable<Categories>>(result);
            var categoriesList = result.ToList();
            Assert.AreEqual(2, categoriesList.Count);
            Assert.AreEqual(1, categoriesList[0].Id);
            Assert.AreEqual("FakeCategory1", categoriesList[0].Name);
            Assert.AreEqual(2, categoriesList[1].Id);
            Assert.AreEqual("FakeCategory2", categoriesList[1].Name);
        }

        [Test]
        public async Task GetCategoryById_ExistingCategoryId_ReturnsCategory()
        {
            var existingCategoryId = 1;
            var result = await fakeProductService.GetCategoryById(existingCategoryId);
            Assert.NotNull(result);
            Assert.AreEqual(existingCategoryId, result.Id);
        }

        [Test]
        public async Task GetCategoryById_NonExistingCategoryId_ReturnsNull()
        {
            var nonExistingCategoryId = 100;
            var result = await fakeProductService.GetCategoryById(nonExistingCategoryId);
            Assert.Null(result);
        }

        [Test]
        public async Task DeleteProductById_ExistingProductId_ReturnsTrue()
        {
            var existingProductId = 1;
            var result = await fakeProductService.DeleteProductById(existingProductId);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeleteProductById_NonExistingProductId_ReturnsFalse()
        {
            var nonExistingProductId = 3;
            var result = await fakeProductService.DeleteProductById(nonExistingProductId);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task AddProduct_InvalidModel_ReturnsNull()
        {
            var invalidProduct = new AddNewProduct
            {
                Description = "InvalidProduct",
                Price = 10.00,
                Quantity = 10,
                IsAvailable = true,
                CategoryId = 1
            };
            var result = await fakeProductService.AddProduct(invalidProduct);
            Assert.Null(result);
        }

        [Test]
        public async Task GetAllProducts_ReturnsAllFakeProducts()
        {
            var result = await fakeProductService.GetAllProducts();
            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetAvailableProducts_ReturnsAvailableProducts()
        { 
            var result = await fakeProductService.GetAvailableProducts();
            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetProductByCategory_ExistingCategoryId_ReturnsProducts()
        {
            var existingCategoryId = 1;
            var result = await fakeProductService.GetProductByCategory(existingCategoryId);
            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public async Task GetProductByCategory_NonExistingCategoryId_ReturnsEmptyList()
        {
            var nonExistingCategoryId = 100;
            var result = await fakeProductService.GetProductByCategory(nonExistingCategoryId);
            Assert.NotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetProductById_ExistingProductId_ReturnsProduct()
        {
            var existingProductId = 1;
            var result = await fakeProductService.GetProductById(existingProductId);
            Assert.NotNull(result);
            Assert.AreEqual(existingProductId, result.Id);
        }

        [Test]
        public async Task GetProductById_NonExistingProductId_ReturnsNull()
        {
            var nonExistingProductId = 100;
            var result = await fakeProductService.GetProductById(nonExistingProductId);
            Assert.Null(result);
        }

        [Test]
        public async Task UpdateProduct_ExistingProductId_ReturnsTrue()
        {
            var existingProductId = 1;
            var updateProduct = new Product
            {
                Name = "UpdatedFakeProduct",
                Description = "Updated Description",
                Price = 29.99,
                Quantity = 15,
                IsAvailable = true,
                CategoryId = 2
            };
            var result = await fakeProductService.UpdateProduct(existingProductId, updateProduct);
            Assert.True(result);
        }

        [Test]
        public async Task UpdateProduct_NonExistingProductId_ReturnsFalse()
        {
            var nonExistingProductId = 100;
            var updateProduct = new Product
            {
                Name = "UpdatedFakeProduct",
                Description = "Updated Description",
                Price = 29.99,
                Quantity = 15,
                IsAvailable = true,
                CategoryId = 2
            };
            var result = await fakeProductService.UpdateProduct(nonExistingProductId, updateProduct);
            Assert.False(result);
        }
        [Test]
        public async Task GetProductByPrice_ShouldReturnProductsInPriceRange()
        {
            var fakeProductService = new FakeProductService();
            var priceRange = new PriceRange { MinPrice = 10.00, MaxPrice = 15.00 };
            var result = await fakeProductService.GetProductByPrice(priceRange);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
        }
        [Test]
        public async Task GetProductByPrice_ShouldReturnProductsInPriceRange2()
        {
            var fakeProductService = new FakeProductService();
            var priceRange = new PriceRange { MinPrice = 10.00, MaxPrice = 100.00 };
            var result = await fakeProductService.GetProductByPrice(priceRange);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetProductByPrice_ShouldReturnEmptyListIfNoProductsInPriceRange()
        {
            var fakeProductService = new FakeProductService();
            var priceRange = new PriceRange { MinPrice = 30.0, MaxPrice = 40.0 };
            var result = await fakeProductService.GetProductByPrice(priceRange);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
    }
}
