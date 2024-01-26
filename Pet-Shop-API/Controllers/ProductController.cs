using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pet_Shop_API.Identity;
using Pet_Shop_API.Models;
using Pet_Shop_API.Services;
using Pet_Shop_API.Validation;
using System.Data.Common;
using System.Data.SqlClient;

namespace Pet_Shop_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IConfiguration _configuration;
        public ProductController(IProductService productService, IConfiguration configuration)
        {
            _productService = productService;
            _configuration = configuration;
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct(AddNewProduct newProduct)
        {
            var productValidator = new ProductValidator(_configuration);
            var validatorResults = await productValidator.ValidateAsync(newProduct);
            if (!validatorResults.IsValid)
            {
                return BadRequest(validatorResults.Errors);
            }
            else
            {
                if(newProduct.Quantity > 0 && newProduct.IsAvailable == false)
                {
                    return BadRequest(new { IncorrectProductError = "Enter correct data, please!" });
                }
                await _productService.AddProduct(newProduct);
                return Ok(new { SuccessMessage = "Product has successfully added!" });
            }
        }

        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var allProducts = await _productService.GetAllProducts();

                if (allProducts == null || !allProducts.Any())
                {
                    return NotFound(new { Message = "There is no any Product yet!" });
                }
                else
                {
                    return Ok(allProducts);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Error = "Internal Server Error!" });
            }
        }
        [HttpGet("GetAvailableProducts")]
        public async Task<IActionResult> GetAvailableProducts()
        {
            try
            {
                var availableProducts = await _productService.GetAvailableProducts();
                if(availableProducts == null || !availableProducts.Any())
                {
                    return NotFound(new { Message = "There is no any available products yet!" });
                }
                else
                {
                    return Ok(availableProducts);
                }
            }catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Error = "Internal Server Error!" });
            }
        }
        [HttpGet("GetProductById")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try
            {
                var getProductById = await _productService.GetProductById(productId);
                if(getProductById == null)
                {
                    return NotFound(new { Message = $"There is no any product by ID: {productId}" });
                }
                else
                {
                    return Ok(getProductById);
                }
            }catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Error = "Internal Server Error!" });
            }
        }
        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var existingCategories = await _productService.GetAllCategories();
                if(existingCategories == null || !existingCategories.Any())
                {
                    return NotFound(new { Message = "There is no any categories yet!" });
                }
                else
                {
                    return Ok(existingCategories);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Error = "Internal Server Error!" });
            }
        }

        [HttpGet("GetCategoryById")]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            try
            {
                var existingCategoryById = await _productService.GetCategoryById(categoryId);
                if(existingCategoryById == null)
                {
                    return NotFound(new { Message = $"There is no any category by ID: {categoryId}" });
                }
                else
                {
                    return Ok(existingCategoryById);
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Error = "Internal Server Error!" });
            }
        }
        [HttpGet("GetProductsByCategory")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            try
            {
                var productsByCategory = await _productService.GetProductByCategory(categoryId);

                if (productsByCategory == null || !productsByCategory.Any())
                {
                    return NotFound(new { Message = $"No products found for Category ID: {categoryId}" });
                }
                else
                {
                    return Ok(productsByCategory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Error = "Internal Server Error!" });
            }
        }

        [HttpGet("GetProductByPrice")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductByPrice(PriceRange priceRange)
        {
            try
            {
                var getProductByPriceRange = await _productService.GetProductByPrice(priceRange);

                if (getProductByPriceRange == null || !getProductByPriceRange.Any())
                {
                    return NotFound($"There is no any product in the range of: {priceRange.MinPrice} - {priceRange.MaxPrice} GEL.");
                }
                else
                {
                    return Ok(getProductByPriceRange);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal Server Error!");
            }
        }



        [Authorize]
        [HttpPost("BuyProduct")]
        public IActionResult BuyProduct(ProductBuyingModel buyProduct)
        {
            var userId = int.Parse(User.Identity.Name);
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var user = connection.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = userId }, transaction);
                        if (user == null)
                        {
                            transaction.Rollback();
                            return BadRequest(new { Message = "User not found!" });
                        }
                        var product = connection.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = buyProduct.ProductId }, transaction);
                        if (product == null)
                        {
                            transaction.Rollback();
                            return BadRequest(new { Message = "There is no any product by this ID!" });
                        }
                        var receiverBank = connection.QueryFirstOrDefault<Bank>("SELECT * FROM Bank WHERE Id = @Id", new { Id = 1 }, transaction) ?? new Bank();
                        if (product.Quantity == 0)
                        {
                            transaction.Rollback();
                            return BadRequest(new { Message = $"This product ({product.Name}) is out of stock!" });
                        }
                        var paymentCalculator = product.Price * buyProduct.Quantity;
                        if (user.Balance < paymentCalculator)
                        {
                            transaction.Rollback();
                            return BadRequest(new { Message = $"You do not have enough money to buy this product. It costs: {paymentCalculator} Gel, and your balance is: {user.Balance} Gel" });
                        }
                        if (buyProduct.Quantity > product.Quantity)
                        {
                            transaction.Rollback();
                            return BadRequest(new { Message = $"There is not enough {product.Name} in stock. There are only {product.Quantity} pcs available!" });
                        }
                        user.Balance -= paymentCalculator;
                        receiverBank.Balance += paymentCalculator;
                        product.Quantity -= buyProduct.Quantity;
                        connection.Execute("UPDATE Users SET Balance = @Balance WHERE Id = @Id", new { Balance = user.Balance, Id = userId }, transaction);
                        connection.Execute("UPDATE Bank SET Balance = @Balance WHERE Id = @Id", new { Balance = receiverBank.Balance, Id = 1 }, transaction);
                        connection.Execute("UPDATE Products SET Quantity = @Quantity WHERE Id = @Id", new { Quantity = product.Quantity, Id = product.Id }, transaction);
                        var newPurchase = new PurchaseModel
                        {
                            UserId = userId,
                            BuyingDate = DateTime.UtcNow,
                            ProductId = product.Id,
                            TotalPayment = paymentCalculator,
                            Quantity = buyProduct.Quantity
                        };
                        connection.Execute("INSERT INTO Purchases (UserId, BuyingDate, ProductId, TotalPayment, Quantity) VALUES (@UserId, @BuyingDate, @ProductId, @TotalPayment, @Quantity)", newPurchase, transaction);
                        transaction.Commit();
                        if (product.Quantity == 0)
                        {
                            connection.Execute("UPDATE Products SET IsAvailable = 0 WHERE Id = @Id", new { Id = product.Id });
                            return Ok(new { Message = "You have successfully bought this product!" });
                        }
                        return Ok(new { Message = "You have successfully bought this product!" });
                    }
                }
                catch (Exception ex)
                {
                    return Ok(new { Message = "Error!" });
                }
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(int productId, Product updateProduct)
        {
            try
            {
                using (var connection = Connection)
                {
                    connection.Open();
                    var query = "SELECT * FROM Products WHERE Id = @Id";
                    var existingProduct = await connection.QueryFirstOrDefaultAsync<Product>(query, new { Id = productId });
                    if (existingProduct == null)
                    {
                        return BadRequest(new { ErrorMessage = $"There is no product with ID: {productId} to update!" });
                    }
                    var updateModel = new Product
                    {
                        Id = productId,
                        Name = updateProduct.Name,
                        Description = updateProduct.Description,
                        Price = updateProduct.Price,
                        Quantity = updateProduct.Quantity,
                        IsAvailable = updateProduct.IsAvailable,
                        CategoryId = updateProduct.CategoryId
                    };
                    var success = await _productService.UpdateProduct(productId, updateModel);

                    if (success)
                    {
                        return Ok(new { SuccessMessage = "Product has been successfully updated!" });
                    }
                    else
                    {
                        return StatusCode(500, new { ErrorMessage = "Failed to update the product." });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                return StatusCode(500, new { ErrorMessage = "Internal Server Error!" });
            }
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("DeleteProductById")]
        public async Task<IActionResult> DeleteCategoryById(int productId)
        {
            try
            {
                var isDeleted = await _productService.DeleteProductById(productId);

                if (!isDeleted)
                {
                    return BadRequest(new { ErrorMessage = $"There is no product with ID: {productId} to delete!" });
                }

                return Ok(new { SuccessMessage = "Product has been successfully deleted!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
