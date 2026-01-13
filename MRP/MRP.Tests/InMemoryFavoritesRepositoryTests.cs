using System.Linq;
using NUnit.Framework;
using MRP.Services;

namespace MRP.Tests
{
    public class InMemoryFavoritesRepositoryTests
    {
        [Test]
        public void Add_FirstTime_ReturnsTrue()
        {
            var repo = new InMemoryFavoritesRepository();
            Assert.That(repo.Add(1, 10), Is.True);
        }

        [Test]
        public void Add_SecondTime_ReturnsFalse()
        {
            var repo = new InMemoryFavoritesRepository();
            repo.Add(1, 10);
            Assert.That(repo.Add(1, 10), Is.False);
        }

        [Test]
        public void Remove_NotExisting_ReturnsFalse()
        {
            var repo = new InMemoryFavoritesRepository();
            Assert.That(repo.Remove(1, 10), Is.False);
        }

        [Test]
        public void Remove_Existing_ReturnsTrue()
        {
            var repo = new InMemoryFavoritesRepository();
            repo.Add(1, 10);
            Assert.That(repo.Remove(1, 10), Is.True);
        }

        [Test]
        public void GetMediaIds_ReturnsAllFavoritesForUser()
        {
            var repo = new InMemoryFavoritesRepository();
            repo.Add(1, 10);
            repo.Add(1, 11);

            var ids = repo.GetMediaIds(1).ToList();
            Assert.That(ids.Count, Is.EqualTo(2));
            Assert.That(ids, Does.Contain(10));
            Assert.That(ids, Does.Contain(11));
        }
    }
}
