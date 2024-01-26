using Pet_Shop_API.Models;
using Pet_Shop_API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FakeServices
{
    public class FakeProductService : IProductService
    {
        private List<Product> fakeProducts;
        private int nextProductId = 1;

        public FakeProductService()
        {
            fakeProducts = new List<Product>
            {
                new Product
                {
                    Id = nextProductId++,
                    Name = "FakeProduct1",
                    Description = "Description for FakeProduct1",
                    Price = 10.99,
                    Quantity = 50,
                    IsAvailable = true,
                    CategoryId = 1
                },
                new Product
                {
                    Id = nextProductId++,
                    Name = "FakeProduct2",
                    Description = "Description for FakeProduct2",
                    Price = 99.99,
                    Quantity = 20,
                    IsAvailable = true,
                    CategoryId = 9
                },
            };
        }

        public Task<Product> AddProduct(AddNewProduct newProduct)
        {
            try
            {
                if (string.IsNullOrEmpty(newProduct.Name))
                {
                    return Task.FromResult<Product>(null);
                }

                var product = new Product
                {
                    Id = nextProductId++,
                    Name = newProduct.Name,
                    Description = newProduct.Description,
                    Price = newProduct.Price,
                    Quantity = newProduct.Quantity,
                    IsAvailable = newProduct.IsAvailable,
                    CategoryId = newProduct.CategoryId
                };

                fakeProducts.Add(product);

                return Task.FromResult(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding product: {ex.Message}");
                return Task.FromResult<Product>(null);
            }
        }

        public Task<IEnumerable<Product>> GetAllProducts()
        {
            return Task.FromResult<IEnumerable<Product>>(fakeProducts);
        }

        public Task<IEnumerable<Product>> GetAvailableProducts()
        {
            var availableProducts = fakeProducts.Where(p => p.IsAvailable).ToList();
            return Task.FromResult<IEnumerable<Product>>(availableProducts);
        }

        public Task<IEnumerable<Product>> GetProductByCategory(int categoryId)
        {
            var productsByCategory = fakeProducts.Where(p => p.CategoryId == categoryId).ToList();
            return Task.FromResult<IEnumerable<Product>>(productsByCategory);
        }

        public Task<Product> GetProductById(int productId)
        {
            var existingProduct = fakeProducts.FirstOrDefault(p => p.Id == productId);
            return Task.FromResult(existingProduct);
        }

        public Task<IEnumerable<Categories>> GetAllCategories()
        {
            var fakeCategories = new List<Categories>
            {
                new Categories { Id = 1, Name = "FakeCategory1" },
                new Categories { Id = 2, Name = "FakeCategory2" }
            };

            return Task.FromResult<IEnumerable<Categories>>(fakeCategories);
        }
        public Task<Categories> GetCategoryById(int categoryId)
        {
            var fakeCategories = new List<Categories>
            {
                new Categories { Id = 1, Name = "FakeCategory1" },
                new Categories { Id = 2, Name = "FakeCategory2" }
            };
            var existingCategory = fakeCategories.FirstOrDefault(c => c.Id == categoryId);
            return Task.FromResult(existingCategory);
        }

        public Task<bool> UpdateProduct(int productId, Product updateProduct)
        {
            try
            {
                var existingProduct = fakeProducts.FirstOrDefault(p => p.Id == productId);

                if (existingProduct == null)
                {
                    return Task.FromResult(false);
                }

                existingProduct.Name = updateProduct.Name;
                existingProduct.Description = updateProduct.Description;
                existingProduct.Price = updateProduct.Price;
                existingProduct.Quantity = updateProduct.Quantity;
                existingProduct.IsAvailable = updateProduct.IsAvailable;
                existingProduct.CategoryId = updateProduct.CategoryId;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task<bool> DeleteProductById(int productId)
        {
            try
            {
                var existingProduct = fakeProducts.FirstOrDefault(p => p.Id == productId);
                if (existingProduct == null)
                {
                    return Task.FromResult(false);
                }
                fakeProducts.Remove(existingProduct);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product: {ex.Message}");
                return Task.FromResult(false);
            }
        }
        public async Task<IEnumerable<Product>> GetProductByPrice(PriceRange priceRange)
        {
            try
            {
                var result = fakeProducts.Where(p => p.Price >= priceRange.MinPrice && p.Price <= priceRange.MaxPrice).ToList();
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
