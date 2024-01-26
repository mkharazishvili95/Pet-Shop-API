# Pet-Shop-API
This is my WEB API project (on .Net 8) where users can register, login and buy items for their pets.
## Details
After registering and entering the system, the user is assigned a validation token,
which allows it to perform the following operations: Get all products list (which is added by admin), buy the product,The amount paid by the user goes to the bank account, whose model I have created as Bank.
He can also get information about his balance and his profile.

And the administrator has the right to receive information about all users, as well as about a specific user according to his ID.
Admin can also add a new product, add a new category or update an existing one. He also has the right to delete the product 
(after deleting the product, orders related to it are automatically canceled and the amount paid for this product is automatically returned to the customer, 
which is transferred from the bank account).
He also has the right to cancel the order and in this case the amount paid to the customer will be returned.

### User Register Form:
/api/User/Register
{
  "firstName": "string",
  "lastName": "string",
  "age": (int),
  "contactNumber": (int - 9 numbers. from 500000000 to 599999999),
  "email": "string",
  "password": "string",
  "address": "string"
}

### User Login Form:
/api/User/Login
{
   "email" : "string",
  "password": "string"
}
/api/User/GetMyBalance     <= (User can know his own balance)
/api/User/GetMyProfile     <= (User can know his own profile information)

### Product:
/api/Product/GetAllProducts  <=(Get all product information, that is added by Admin)
/api/Product/BuyProduct                           <=(Buying product by User)
{
  "productId": int,                              <=(enter productId)
  "quantity": int                                <=(enter product quantity, how many pcs do you want to buy?)
}
/api/Product/GetProductsByCategory?categoryId=(id?)                <=(Get product by categoryId)
/api/Product/GetAllCategories                                      <=(GetAllCategories, that are added by Admin)
/api/Product/GetCategoryById                                       <=(Get category by categoryId)
/api/Product/GetProductById?productId=?(id?)                       <=(Get product by productId)
/api/Product/GetAvailableProducts                                  <=(Get Available products)
/api/Product/GetAllProducts                                        <=(Get all products, that are added by Admin)
/api/Product/GetProductByPrice                                     <=(Users can get products by price range):
{
  "minPrice": (double),
  "maxPrice": (double)
}

### Admin:
/api/Admin/AddNewCategory                        <=(Admin can add new categories):
{
"name": "string"
}
/api/Admin/GetAllUsers                           <=(Getting information about all users who are registered)
/api/Admin/GetUserById=userId={id?}              <=(Getting information about User by this ID)
/api/Product/AddProduct                            <=(Add new product):
{
  "name": "string",
  "description": "string",
  "price": (double),
  "quantity": (int),
  "isAvailable": (boolean),
  "categoryId": (int)
}

​/api​/Product/UpdateProduct?productId={id?}                     <=(update existing product):
{
  "name": "string",
  "description": "string",
  "price": (double),
  "quantity": (int),
  "isAvailable": (boolean),
  "categoryId": (int)
}
/api/Admin/DeleteProductById?productId={id?}                    <=(Delete product by Admin)
/api/Admin/CancelPurchase?purchaseId={id?}                  <=(Cancel purchase(When it's cancel, User automaticly gets refund))
/api/Admin/PurchaseCancelationWithoutRefund?purchaseId=(id?)  <=(Cancel purchase, without any refund.)
/api/Admin/Balance/TopUp                <=(Admin can change User's balance):
{
  "userId": (int),
  "amount": (int)
}



## What I have made:
I created models: User, Product, Bank, PurchaseModel.
I created 2 Role-based authorization. (I used the following Nuget packages: 
Microsoft.AspNetCore.Identity.EntityFrameworkCore, Microsoft.AspNetCore.Authentication.JwtBearer) I have Admin and User in the project. 
To receive services, a person must first register. I created a user registration model and a login model. I made validations for the registration model 
(eg the user's age must be 18 or older and E-mail must not be identical to the and E-Mail of the user in the database). For validations I used: FluentValidation. 
After user registration and logging in, log information is stored in the database (Select * from dbo.Loggs - outputs UserName, role and log-in date of the logged-in user), 
for this I created a Logs model.
After logging in, the user is assigned a token that is generated (I specified a conditional time of 365 days - 1 year)
And after writing the secret key of this token to JWT.IO, this token becomes valid, which allows the user to perform various operations and receive services.
I created the already mentioned Product model, and I also made validations for it. Only admin can add products. I also created services: AdminService, UserService, ProductService. 
I wrote business logic in it. Users who are already logged in can see their balance, as well as information on their profile. View the list of added products and buy any. After purchasing the products, the amount on his balance will be transferred to the bank account that I have created.
The admin has the right to get information about all users, get information about specific users by Id.
Admin can also add a new product or modify an existing one. Also delete the product. He can also cancel any customer order. 
(After cancellation, the user will automatically get back the amount paid for this product, which will be transferred from the bank to his account)
I used DAPPER ORM for the connection of Database(petShopSQL) (Microsoft SQL Server Management Studio).
The passwords entered in the database are in a hashed state, for this I used: TweetinviAPI.
I tested the project with Postman to output the status codes I expected and everything works fine.
I also created a NUnit project where I created FakeServices where I included the existing services in the project. I wrote the tests and all 31 tests ran successfully.
Everything works perfectly.
