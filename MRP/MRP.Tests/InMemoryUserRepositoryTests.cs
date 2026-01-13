using MRP.Model;
using MRP.Services;
using NUnit.Framework;

namespace MRP.Tests
{
    public class InMemoryUserRepositoryTests
    {
        [Test]
        public void Add_AssignsIncrementingIds()
        {
            var repo = new InMemoryUserRepository();
            var u1 = new User("alice", "pw1");
            var u2 = new User("bob", "pw2");

            repo.Add(u1);
            repo.Add(u2);

            Assert.That(u1.Id, Is.EqualTo(1));
            Assert.That(u2.Id, Is.EqualTo(2));
        }

        [Test]
        public void Find_ReturnsUserWithMatchingCredentials()
        {
            var repo = new InMemoryUserRepository();
            repo.Add(new User("alice", "secret"));

            var user = repo.Find("alice", "secret");

            Assert.That(user, Is.Not.Null);
            Assert.That(user!.Username, Is.EqualTo("alice"));
        }

        [Test]
        public void UpdateProfile_UpdatesDisplayNameAndBio()
        {
            var repo = new InMemoryUserRepository();
            var user = new User("alice", "pw");
            repo.Add(user);

            var updated = repo.UpdateProfile(user.Id, "Alice A.", "bio");

            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.DisplayName, Is.EqualTo("Alice A."));
            Assert.That(updated.Bio, Is.EqualTo("bio"));
        }
    }
}
