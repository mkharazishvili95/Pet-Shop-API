using Pet_Shop_API.Identity;
using Pet_Shop_API.Models;

namespace FakeServices
{
    [TestFixture]
    public class TokenGeneratorTests
    {
        private IAccessTokenGenerator _fakeTokenGenerator;

        [SetUp]
        public void Setup()
        {
            _fakeTokenGenerator = new FakeTokenGenerator();
        }

        [Test]
        public void GenerateToken_ReturnsFakeToken()
        {
            var user = new User
            {
                Id = 1,
                Email = "misho123@gmail.com",
                Role = "User"
            };
            var generatedToken = _fakeTokenGenerator.GenerateToken(user);
            Assert.IsNotNull(generatedToken);
        }
    }
}