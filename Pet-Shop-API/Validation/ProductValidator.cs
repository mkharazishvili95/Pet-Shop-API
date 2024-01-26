using Dapper;
using FluentValidation;
using Pet_Shop_API.Models;
using System.Data.Common;
using System.Data.SqlClient;

namespace Pet_Shop_API.Validation
{
    public class ProductValidator : AbstractValidator<AddNewProduct>
    {
        private readonly IConfiguration _configuration;
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public ProductValidator(IConfiguration configuration) 
        {
            _configuration = configuration;

            RuleFor(product => product.Name).NotEmpty().WithMessage("Enter Product Name!");
            RuleFor(product => product.Description).NotEmpty().WithMessage("Enter Product Description!");
            RuleFor(product => product.Quantity).NotEmpty().WithMessage("Enter Product Quantity!");
            RuleFor(product => product.Price).NotEmpty().WithMessage("Enter Product Price!");
            RuleFor(product => product.IsAvailable).NotEmpty().WithMessage("Is product Available now? Enter true or false!");
            RuleFor(product => product.CategoryId).NotEmpty().WithMessage("Enter Product Category by ID!")
                .Must(ExistingProductCategory).WithMessage("Enter Category from existing categories!");
        }
        private bool ExistingProductCategory(int categoryId)
        {
            using (var connection = Connection)
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Categories WHERE Id = @Id";
                int count = connection.ExecuteScalar<int>(query, new { Id = categoryId });
                return count > 0;
            }
        }
    }
}
