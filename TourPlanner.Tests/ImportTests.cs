using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TourPlanner.BL;
using TourPlanner.DAL;
using TourPlanner.Models;

namespace TourPlanner.Tests
{
    [TestFixture]
    public class ImportTests
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
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Tour>()))
                .ReturnsAsync((Tour t) => t); // gibt die Tour zurueck wie sie reinkommt
            _service = new TourService(
                _repoMock.Object,
                _orsMock.Object,
                _mapMock.Object,
                Path.GetTempPath(),
                NullLogger<TourService>.Instance);
        }

        [Test]
        public async Task ImportAsync_AssignsNewTourIds()
        {
            var originalId = Guid.NewGuid();
            var tours = new List<Tour>
            {
                new() { Id = originalId, Name = "Importierte Tour", From = "A", To = "B" }
            };

            var result = (await _service.ImportAsync(tours)).ToList();

            Assert.That(result[0].Id, Is.Not.EqualTo(originalId));
        }

        [Test]
        public async Task ImportAsync_AssignsNewLogIds_AndMatchesTourId()
        {
            var originalLogId = Guid.NewGuid();
            var tours = new List<Tour>
            {
                new()
                {
                    Name = "Tour mit Log", From = "A", To = "B",
                    TourLogs = new List<TourLog>
                    {
                        new() { Id = originalLogId, Difficulty = 2, TotalTime = 1, TotalDistance = 5, Rating = 3 }
                    }
                }
            };

            var result = (await _service.ImportAsync(tours)).ToList();
            var importedLog = result[0].TourLogs.First();

            Assert.That(importedLog.Id, Is.Not.EqualTo(originalLogId));
            Assert.That(importedLog.TourId, Is.EqualTo(result[0].Id));
        }
    }
}
