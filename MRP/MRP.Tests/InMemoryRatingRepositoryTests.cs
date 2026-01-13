using MRP.Services;
using NUnit.Framework;

namespace MRP.Tests
{
    public class InMemoryRatingRepositoryTests
    {
        [Test]
        public void Upsert_WithComment_IsUnconfirmed()
        {
            var repo = new InMemoryRatingRepository();

            var r = repo.Upsert(1, 1, 5, "nice");

            Assert.That(r.Id, Is.EqualTo(1));
            Assert.That(r.MediaId, Is.EqualTo(1));
            Assert.That(r.UserId, Is.EqualTo(1));
            Assert.That(r.Stars, Is.EqualTo(5));
            Assert.That(r.Comment, Is.EqualTo("nice"));
            Assert.That(r.Confirmed, Is.False);
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
        public void Like_SucceedsOnce_SecondTimeFails()
        {
            var repo = new InMemoryRatingRepository();
            var r = repo.Upsert(1, 1, 5, null);

            var first = repo.Like(r.Id, 2);
            var second = repo.Like(r.Id, 2);

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
        }

        [Test]
        public void Confirm_SucceedsOnlyForOwner()
        {
            var repo = new InMemoryRatingRepository();
            var r = repo.Upsert(1, 1, 5, "c");

            var wrongUser = repo.Confirm(r.Id, 2);
            var owner = repo.Confirm(r.Id, 1);

            Assert.That(wrongUser, Is.False);
            Assert.That(owner, Is.True);
            Assert.That(repo.Get(r.Id)!.Confirmed, Is.True);
        }
    }
}
