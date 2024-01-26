using Dapper;
using Pet_Shop_API.Models;
using Pet_Shop_API.Validation;
using System.Data.Common;
using System.Data.SqlClient;

namespace Pet_Shop_API.Services
{
    public interface IProductService
    {
        Task<Product> AddProduct(AddNewProduct newProduct);
        Task<IEnumerable<Product>> GetAllProducts();
        Task<IEnumerable<Product>> GetAvailableProducts();
        Task<Product> GetProductById(int productId);
        Task <IEnumerable<Product>> GetProductByCategory(int categoryId);
        Task<IEnumerable<Categories>> GetAllCategories();
        Task<Categories> GetCategoryById(int categoryId);
        Task<bool> UpdateProduct(int productId, Product updateProduct);
        Task<bool> DeleteProductById(int productId);
        Task<IEnumerable<Product>> GetProductByPrice(PriceRange priceRange);


    }
    public class ProductService : IProductService
    {
        private readonly IConfiguration _configuration;
        public ProductService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<Product> AddProduct(AddNewProduct newProduct)
        {
            try
            {
                var productValidator = new ProductValidator(_configuration);
                var validatorResults = await productValidator.ValidateAsync(newProduct);

                if (!validatorResults.IsValid)
                {
                    return null;
                }

                using (var connection = Connection)
                {
                    connection.Open();
                    string insertQuery = @"
                    INSERT INTO Products(Name, Description, Price, Quantity, IsAvailable, CategoryId) 
                    VALUES(@Name, @Description, @Price, @Quantity, @IsAvailable, @CategoryId);
                    SELECT SCOPE_IDENTITY();";
                    int productId = await connection.QuerySingleAsync<int>(insertQuery, newProduct);
                    string selectQuery = "SELECT * FROM Products WHERE Id = @Id";
                    var insertedProduct = await connection.QueryFirstOrDefaultAsync<Product>(selectQuery, new { Id = productId });
                    return insertedProduct;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding product: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteProductById(int productId)
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Products WHERE Id = @Id";
                    var existingProduct = await connection.QueryFirstOrDefaultAsync(query, new { Id = productId });

                    if (existingProduct == null)
                    {
                        return false;
                    }
                    else
                    {
                        var deleteQuery = await connection.ExecuteAsync("DELETE FROM Products WHERE Id = @Id", new { Id = existingProduct.Id });
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Categories>> GetAllCategories()
        {
            try
            {
                using(var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Categories";
                    var existingCategories = await connection.QueryAsync<Categories>(query);
                    if(existingCategories == null || !existingCategories.Any())
                    {
                        return null;
                    }
                    else
                    {
                        return existingCategories.ToList();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Products";
                    var getAllProducts = await connection.QueryAsync<Product>(query);
                    if (getAllProducts == null || !getAllProducts.Any())
                    {
                        return null;
                    }
                    else
                    {
                        return getAllProducts.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Product>> GetAvailableProducts()
        {
            try
            {
              using(var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Products WHERE IsAvailable = 1;";
                    var getAvailableProducts = await connection.QueryAsync<Product>(query);
                    if(getAvailableProducts == null || !getAvailableProducts.Any())
                    {
                        return null;
                    }
                    else
                    {
                        return getAvailableProducts.ToList();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<Categories> GetCategoryById(int categoryId)
        {
            try
            {
                using(var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Categories WHERE Id = @Id";
                    var existingCategoryById = await connection.QueryFirstOrDefaultAsync<Categories>(query, new { Id = categoryId });
                    if(existingCategoryById == null)
                    {
                        return null;
                    }
                    else
                    {
                        return existingCategoryById;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<Product>> GetProductByCategory(int categoryId)
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Products WHERE CategoryId = @CategoryId";
                    var products = await connection.QueryAsync<Product>(query, new { CategoryId = categoryId });

                    return products.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }


        public async Task<Product> GetProductById(int productId)
        {
            try
            {
                using(var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Products WHERE Id = @Id";
                    var existingProduct = await connection.QueryAsync<Product>(query, new {Id = productId});
                    if(existingProduct == null || !existingProduct.Any())
                    {
                        return null;
                    }
                    else
                    {
                        return existingProduct.First();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<Product>> GetProductByPrice(PriceRange priceRange)
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Products WHERE Price >= @MinPrice AND Price <= @MaxPrice;";
                    var parameters = new { MinPrice = priceRange.MinPrice, MaxPrice = priceRange.MaxPrice };
                    var result = await connection.QueryAsync<Product>(query, parameters);
                    return result;
                }
            }
            catch 
            {
                return null;
            }
        }


        public async Task<bool> UpdateProduct(int productId, Product updateProduct)
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var existingProduct = await connection.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = productId });
                    if (existingProduct == null)
                    {
                        return false;
                    }
                    var updateQuery = "UPDATE Products SET Name = @Name, Description = @Description, Price = @Price, Quantity = @Quantity, IsAvailable = @IsAvailable, CategoryId = @CategoryId WHERE Id = @Id";
                    var affectedRows = await connection.ExecuteAsync(updateQuery, new
                    {
                        Id = productId,
                        Name = updateProduct.Name,
                        Description = updateProduct.Description,
                        Price = updateProduct.Price,
                        Quantity = updateProduct.Quantity,
                        IsAvailable = updateProduct.IsAvailable,
                        CategoryId = updateProduct.CategoryId
                    });

                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                return false;
            }
        }
    }
}
