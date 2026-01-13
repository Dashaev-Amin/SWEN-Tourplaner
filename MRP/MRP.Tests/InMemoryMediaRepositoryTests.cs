using System.Linq;
using MRP.Model;
using MRP.Services;
using NUnit.Framework;

namespace MRP.Tests
{
    public class InMemoryMediaRepositoryTests
    {
        [Test]
        public void Add_AssignsIncreasingIds()
        {
            var repo = new InMemoryMediaRepository();

            var a = repo.Add(new Media("A", MediaType.Movie, 2000));
            var b = repo.Add(new Media("B", MediaType.Movie, 2001));

            Assert.That(a.Id, Is.GreaterThan(0));
            Assert.That(b.Id, Is.GreaterThan(a.Id));
        }

        [Test]
        public void Update_UpdatesFields()
        {
            var repo = new InMemoryMediaRepository();
            var m = repo.Add(new Media("Old", MediaType.Movie, 1999)
            {
                Description = "d1",
                Genres = new() { "Drama" },
                AgeRestriction = 18
            });

            var updated = new Media("New", MediaType.Series, 2005)
            {
                Description = "d2",
                Genres = new() { "SciFi" },
                AgeRestriction = 12
            };

            var result = repo.Update(m.Id, updated);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Title, Is.EqualTo("New"));
            Assert.That(result.Description, Is.EqualTo("d2"));
            Assert.That(result.MediaType, Is.EqualTo(MediaType.Series));
            Assert.That(result.ReleaseYear, Is.EqualTo(2005));
            Assert.That(result.Genres, Is.EquivalentTo(new[] { "SciFi" }));
            Assert.That(result.AgeRestriction, Is.EqualTo(12));
        }

        [Test]
        public void Delete_RemovesMedia()
        {
            var repo = new InMemoryMediaRepository();
            var m = repo.Add(new Media("X", MediaType.Movie, 2000));

            var deleted = repo.Delete(m.Id);

            Assert.That(deleted, Is.True);
            Assert.That(repo.Get(m.Id), Is.Null);
        }

        [Test]
        public void Search_ByTitle_PartialCaseInsensitive()
        {
            var repo = new InMemoryMediaRepository();
            repo.Add(new Media("Matrix", MediaType.Movie, 1999));
            repo.Add(new Media("Avatar", MediaType.Movie, 2009));

            var res = repo.Search("mat").ToList();

            Assert.That(res.Count, Is.EqualTo(1));
            Assert.That(res[0].Title, Is.EqualTo("Matrix"));
        }

        [Test]
        public void Filter_ByGenre_CaseInsensitive()
        {
            var repo = new InMemoryMediaRepository();

            var a = new Media("A", MediaType.Movie, 2000) { Genres = new() { "SciFi" } };
            var b = new Media("B", MediaType.Movie, 2000) { Genres = new() { "Drama" } };

            repo.Add(a);
            repo.Add(b);

            var res = repo.Filter(genre: "scifi").ToList();

            Assert.That(res.Count, Is.EqualTo(1));
            Assert.That(res[0].Title, Is.EqualTo("A"));
        }

        [Test]
        public void Filter_ByMediaType_AndReleaseYear()
        {
            var repo = new InMemoryMediaRepository();
            repo.Add(new Media("Movie 2010", MediaType.Movie, 2010));
            repo.Add(new Media("Series 2010", MediaType.Series, 2010));
            repo.Add(new Media("Series 2011", MediaType.Series, 2011));

            var res = repo.Filter(mediaType: MediaType.Series, releaseYear: 2010).ToList();

            Assert.That(res.Count, Is.EqualTo(1));
            Assert.That(res[0].Title, Is.EqualTo("Series 2010"));
        }

        [Test]
        public void Filter_ByAgeRestriction_AndMinRating()
        {
            var repo = new InMemoryMediaRepository();
            var a = new Media("A", MediaType.Movie, 2000) { AgeRestriction = 12, AvgScore = 4.2 };
            var b = new Media("B", MediaType.Movie, 2000) { AgeRestriction = 16, AvgScore = 4.8 };
            var c = new Media("C", MediaType.Movie, 2000) { AgeRestriction = 12, AvgScore = 3.0 };

            repo.Add(a);
            repo.Add(b);
            repo.Add(c);

            var res = repo.Filter(ageRestriction: 12, minRating: 4.0).ToList();

            Assert.That(res.Count, Is.EqualTo(1));
            Assert.That(res[0].Title, Is.EqualTo("A"));
        }

        [Test]
        public void Sort_ByScoreDesc()
        {
            var repo = new InMemoryMediaRepository();
            repo.Add(new Media("Low", MediaType.Movie, 2000) { AvgScore = 2.0 });
            repo.Add(new Media("High", MediaType.Movie, 2000) { AvgScore = 4.9 });
            repo.Add(new Media("Mid", MediaType.Movie, 2000) { AvgScore = 3.5 });

            var res = repo.Filter(sortBy: "score").ToList();

            Assert.That(res[0].Title, Is.EqualTo("High"));
            Assert.That(res[1].Title, Is.EqualTo("Mid"));
            Assert.That(res[2].Title, Is.EqualTo("Low"));
        }
    }
}
