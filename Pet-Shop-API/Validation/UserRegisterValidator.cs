using FluentValidation;
using Pet_Shop_API.Identity;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System.Data.Common;

namespace Pet_Shop_API.Validation
{
    public class UserRegisterValidator : AbstractValidator<UserRegisterModel>
    {
        private readonly IConfiguration _configuration;
        public UserRegisterValidator(IConfiguration configuration) 
        {
            _configuration = configuration;

            RuleFor(user => user.FirstName).NotEmpty().WithMessage("Enter your FirstName!");
            RuleFor(user => user.LastName).NotEmpty().WithMessage("Enter your LastName!");
            RuleFor(user => user.Age).NotEmpty().WithMessage("Enter your Age!")
                .GreaterThanOrEqualTo(18).WithMessage("Your age must be 18 or more to register!")
                .LessThanOrEqualTo(85).WithMessage("Enter your correct Age!");
            RuleFor(user => user.ContactNumber).NotEmpty().WithMessage("Enter your Contact number!")
                .GreaterThanOrEqualTo(500000000).WithMessage("Enter valid Contact Number!")
                .LessThanOrEqualTo(599999999).WithMessage("Enter valid Contact Number!")
                .Must(DifferentContactNumber).WithMessage("Contact Number already exists. Try another!");
            RuleFor(user => user.Address).NotEmpty().WithMessage("Enter your Address!")
                .Length(5, 100).WithMessage("Enter your Address!");
            RuleFor(user => user.Email).NotEmpty().WithMessage("Enter your E-Mail address!")
                .EmailAddress().WithMessage("Enter your valid E-mail Address!")
                .Must(DifferentEmail).WithMessage("E-Mail address already exists. Try another!");
            RuleFor(user => user.Password).NotEmpty().WithMessage("Enter your Password!")
                .Length(6, 15).WithMessage("Your Password must be from 6 to 15 chars or numbers!");
        }
        private DbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        private bool DifferentEmail(string email)
        {
            using (var connection = Connection)
            {
                connection.Open();
                var result = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Users WHERE Email = @Email", new { Email = email });
                return result == 0;
            }
        }
        private bool DifferentContactNumber(int contactNumber)
        {
            using (var connection = Connection)
            {
                connection.Open();
                var result = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Users WHERE ContactNumber = @ContactNumber", new { ContactNumber = contactNumber });
                return result == 0;
            }
        }
    }
}
