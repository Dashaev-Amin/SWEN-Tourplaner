using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TourPlanner.BL;
using TourPlanner.DAL;
using TourPlanner.Models;

namespace TourPlanner.Tests
{
    [TestFixture]
    public class SearchTests
    {
        private Mock<ITourRepository> _repoMock = null!;
        private Mock<IOrsService> _orsMock = null!;
        private Mock<IMapImageService> _mapMock = null!;
        private TourService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<ITourRepository>();
            _orsMock = new Mock<IOrsService>();
            _mapMock = new Mock<IMapImageService>();
            _service = new TourService(
                _repoMock.Object,
                _orsMock.Object,
                _mapMock.Object,
                Path.GetTempPath(),
                NullLogger<TourService>.Instance);
        }

        [Test]
        public async Task SearchAsync_WithEmptyQuery_ReturnsAllTours()
        {
            var tours = new List<Tour>
            {
                new() { Name = "Tour A", From = "Wien", To = "Graz" },
                new() { Name = "Tour B", From = "Linz", To = "Salzburg" }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tours);

            var result = (await _service.SearchAsync("")).ToList();

            Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task SearchAsync_FindsByTourName()
        {
            var tours = new List<Tour>
            {
                new() { Name = "Alpenrunde", From = "A", To = "B" },
                new() { Name = "Stadtspaziergang", From = "C", To = "D" }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tours);

            var result = (await _service.SearchAsync("Alpen")).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Alpenrunde"));
        }

        [Test]
        public async Task SearchAsync_FindsByLogComment()
        {
            var tour = new Tour
            {
                Name = "Tour X", From = "A", To = "B",
                TourLogs = new List<TourLog>
                {
                    new() { Comment = "Schoenes Wetter heute", Difficulty = 1, TotalTime = 1, TotalDistance = 5, Rating = 4 }
                }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Tour> { tour });

            var result = (await _service.SearchAsync("Wetter")).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task SearchAsync_FindsByComputedLabel_Kinderfreundlich()
        {
            var tour = new Tour
            {
                Name = "Leichte Tour", From = "A", To = "B",
                TourLogs = new List<TourLog>
                {
                    new() { Difficulty = 1, TotalTime = 0.5, TotalDistance = 2, Rating = 5 }
                }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Tour> { tour });

            var result = (await _service.SearchAsync("Kinder")).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task SearchAsync_FindsByComputedLabel_Beliebt()
        {
            var tour = new Tour { Name = "Populaere Tour", From = "A", To = "B" };
            for (int i = 0; i < 5; i++)
                tour.TourLogs.Add(new TourLog { Difficulty = 2, TotalTime = 1, TotalDistance = 5, Rating = 4 });

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Tour> { tour });

            var result = (await _service.SearchAsync("Beliebt")).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task SearchAsync_IsCaseInsensitive()
        {
            var tours = new List<Tour>
            {
                new() { Name = "GROSSBUCHSTABEN", From = "A", To = "B" }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tours);

            var result = (await _service.SearchAsync("grossbuchstaben")).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task SearchAsync_NoMatch_ReturnsEmptyList()
        {
            var tours = new List<Tour>
            {
                new() { Name = "Tour A", From = "Wien", To = "Graz" }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tours);

            var result = (await _service.SearchAsync("xyz123nichtvorhanden")).ToList();

            Assert.That(result, Is.Empty);
        }
    }
}
