using System.Linq;
using MRP.Model;
using MRP.Services;
using Xunit;
using Assert = Xunit.Assert;
namespace MRP.Tests
{
    public class InMemoryMediaRepositoryTests
    {
        [Fact]
        public void Add_AssignsIncreasingIds()
        {
            var repo = new InMemoryMediaRepository();

            var a = repo.Add(new Media("A", MediaType.Movie, 2000));
            var b = repo.Add(new Media("B", MediaType.Movie, 2001));

            Assert.True(a.Id > 0);
            Assert.True(b.Id > a.Id);
        }

        [Fact]
        public void Get_ReturnsNullIfNotFound()
        {
            var repo = new InMemoryMediaRepository();
            Assert.Null(repo.Get(12345));
        }

        [Fact]
        public void Search_ByTitle_IsCaseInsensitive()
        {
            var repo = new InMemoryMediaRepository();
            repo.Add(new Media("Matrix", MediaType.Movie, 1999));
            repo.Add(new Media("Avatar", MediaType.Movie, 2009));

            var res = repo.Search("mat").ToList();
            Assert.Single(res);
            Assert.Equal("Matrix", res[0].Title);
        }

        [Fact]
        public void Delete_RemovesMedia()
        {
            var repo = new InMemoryMediaRepository();
            var m = repo.Add(new Media("X", MediaType.Movie, 2000));

            Assert.True(repo.Delete(m.Id));
            Assert.Null(repo.Get(m.Id));
        }

        [Fact]
        public void Filter_ByGenre_Works()
        {
            var repo = new InMemoryMediaRepository();

            var a = new Media("A", MediaType.Movie, 2000) { Genres = new() { "SciFi" } };
            var b = new Media("B", MediaType.Movie, 2000) { Genres = new() { "Drama" } };

            repo.Add(a);
            repo.Add(b);

            var res = repo.Filter(genre: "scifi").ToList();
            Assert.Single(res);
            Assert.Equal("A", res[0].Title);
        }
    }
}
