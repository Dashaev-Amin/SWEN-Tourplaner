using NUnit.Framework;
using MRP.Services;

namespace MRP.Tests
{
    public class TokenServiceTests
    {
        [Test]
        public void Issue_ReturnsNonEmptyToken()
        {
            var svc = new TokenService();
            var token = svc.Issue("alice");

            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.Not.Empty);
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
        public void TryGetUser_MissingBearerPrefix_Fails()
        {
            var svc = new TokenService();
            var token = svc.Issue("alice");

            var ok = svc.TryGetUser(token, out var username);

            Assert.That(ok, Is.False);
            Assert.That(username, Is.Null.Or.Empty);
        }

        [Test]
        public void TryGetUser_NullHeader_Fails()
        {
            var svc = new TokenService();

            var ok = svc.TryGetUser(null, out var username);

            Assert.That(ok, Is.False);
            Assert.That(username, Is.Null.Or.Empty);
        }

        [Test]
        public void TryGetUser_InvalidToken_Fails()
        {
            var svc = new TokenService();

            var ok = svc.TryGetUser("Bearer this-is-not-a-valid-token", out var username);

            Assert.That(ok, Is.False);
            Assert.That(username, Is.Null.Or.Empty);
        }
    }
}
