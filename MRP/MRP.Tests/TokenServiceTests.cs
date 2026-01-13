using MRP.Services;
using NUnit.Framework;

namespace MRP.Tests
{
    public class TokenServiceTests
    {
        [Test]
        public void Issue_ReturnsNonEmptyToken()
        {
            var svc = new TokenService();
            var token = svc.Issue("alice");

            Assert.That(token, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void TryGetUser_WithValidBearerToken_ReturnsUsername()
        {
            var svc = new TokenService();
            var token = svc.Issue("alice");

            var ok = svc.TryGetUser($"Bearer {token}", out var username);

            Assert.That(ok, Is.True);
            Assert.That(username, Is.EqualTo("alice"));
        }

        [Test]
        public void TryGetUser_InvalidHeaderOrToken_Fails()
        {
            var svc = new TokenService();
            var token = svc.Issue("alice");

            var missingBearer = svc.TryGetUser(token, out var username1);
            var invalidToken = svc.TryGetUser("Bearer invalid-token", out var username2);

            Assert.That(missingBearer, Is.False);
            Assert.That(username1, Is.Null.Or.Empty);
            Assert.That(invalidToken, Is.False);
            Assert.That(username2, Is.Null.Or.Empty);
        }
    }
}
