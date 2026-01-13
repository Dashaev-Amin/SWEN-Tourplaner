using System.Linq;
using NUnit.Framework;
using MRP.Services;

namespace MRP.Tests
{
    public class InMemoryRatingRepositoryTests
    {
        [Test]
        public void Upsert_CreatesNewRating()
        {
            var repo = new InMemoryRatingRepository();
            var r = repo.Upsert(1, 1, 5, "nice");
            Assert.That(r.Id, Is.EqualTo(1));
            Assert.That(r.MediaId, Is.EqualTo(1));
            Assert.That(r.UserId, Is.EqualTo(1));
            Assert.That(r.Stars, Is.EqualTo(5));
            Assert.That(r.Comment, Is.EqualTo("nice"));
        }

        [Test]
        public void Upsert_SameUserSameMedia_UpdatesSameRatingId()
        {
            var repo = new InMemoryRatingRepository();
            var r1 = repo.Upsert(1, 1, 5, "a");
            var r2 = repo.Upsert(1, 1, 3, "b");
            Assert.That(r2.Id, Is.EqualTo(r1.Id));
            Assert.That(r2.Stars, Is.EqualTo(3));
            Assert.That(r2.Comment, Is.EqualTo("b"));
        }

        [Test]
        public void Get_ReturnsNullIfMissing()
        {
            var repo = new InMemoryRatingRepository();
            Assert.That(repo.Get(999), Is.Null);
        }

        [Test]
        public void GetByMedia_ReturnsOnlyThatMedia()
        {
            var repo = new InMemoryRatingRepository();
            repo.Upsert(1, 1, 5, null);
            repo.Upsert(2, 1, 4, null);
            Assert.That(repo.GetByMedia(1).Count(), Is.EqualTo(1));
        }

        [Test]
        public void GetByUser_ReturnsOnlyThatUser()
        {
            var repo = new InMemoryRatingRepository();
            repo.Upsert(1, 1, 5, null);
            repo.Upsert(1, 2, 4, null);
            Assert.That(repo.GetByUser(1).Count(), Is.EqualTo(1));
        }

        [Test]
        public void Delete_FailsIfNotOwner()
        {
            var repo = new InMemoryRatingRepository();
            var r = repo.Upsert(1, 1, 5, null);
            Assert.That(repo.Delete(r.Id, 2), Is.False);
            Assert.That(repo.Get(r.Id), Is.Not.Null);
        }

        [Test]
        public void Delete_SucceedsIfOwner()
        {
            var repo = new InMemoryRatingRepository();
            var r = repo.Upsert(1, 1, 5, null);
            Assert.That(repo.Delete(r.Id, 1), Is.True);
            Assert.That(repo.Get(r.Id), Is.Null);
        }

        [Test]
        public void Like_FailsForOwnRating()
        {
            var repo = new InMemoryRatingRepository();
            var r = repo.Upsert(1, 1, 5, null);
            Assert.That(repo.Like(r.Id, 1), Is.False);
        }

        [Test]
        public void Like_SucceedsOnce_SecondTimeFails()
        {
            var repo = new InMemoryRatingRepository();
            var r = repo.Upsert(1, 1, 5, null);
            Assert.That(repo.Like(r.Id, 2), Is.True);
            Assert.That(repo.Like(r.Id, 2), Is.False);
        }

        [Test]
        public void Confirm_SucceedsOnlyForOwner()
        {
            var repo = new InMemoryRatingRepository();
            var r = repo.Upsert(1, 1, 5, "c");
            Assert.That(repo.Confirm(r.Id, 2), Is.False);
            Assert.That(repo.Confirm(r.Id, 1), Is.True);
            Assert.That(repo.Get(r.Id)!.Confirmed, Is.True);
        }
    }
}
