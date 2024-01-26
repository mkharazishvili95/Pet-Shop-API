
using Pet_Shop_API.Identity;
using Pet_Shop_API.Models;

namespace FakeServices
{
    public class FakeTokenGenerator : IAccessTokenGenerator
    {
        public string GenerateToken(User user)
        {
            string fakeToken = "Fake Access Token =)";
            return fakeToken;
        }
    }
}