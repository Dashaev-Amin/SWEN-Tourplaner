using System.Linq;
using MRP.Services;
using NUnit.Framework;

namespace MRP.Tests
{
    public class InMemoryFavoritesRepositoryTests
    {
        [Test]
        public void Add_FirstTime_ReturnsTrue_SecondTimeFalse()
        {
            var repo = new InMemoryFavoritesRepository();

            var first = repo.Add(1, 10);
            var second = repo.Add(1, 10);

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
        }

        [Test]
        public void Remove_Existing_RemovesAndIsNotFavorite()
        {
            var repo = new InMemoryFavoritesRepository();
            repo.Add(1, 10);

            var removed = repo.Remove(1, 10);
            var isFavorite = repo.IsFavorite(1, 10);
            var ids = repo.GetMediaIds(1).ToList();

            Assert.That(removed, Is.True);
            Assert.That(isFavorite, Is.False);
            Assert.That(ids, Is.Empty);
        }
    }
}
