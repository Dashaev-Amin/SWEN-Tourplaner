using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TourPlanner.BL;
using TourPlanner.BL.Exceptions;
using TourPlanner.DAL;
using TourPlanner.Models;

namespace TourPlanner.Tests
{
    [TestFixture]
    public class ExceptionTests
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
            // ORS gibt immer null zurueck (kein Netzwerk-Call)
            _orsMock.Setup(o => o.GeocodeAsync(It.IsAny<string>())).ReturnsAsync((ValueTuple<double, double>?)null);
            _service = new TourService(
                _repoMock.Object,
                _orsMock.Object,
                _mapMock.Object,
                Path.GetTempPath(),
                NullLogger<TourService>.Instance);
        }

        [Test]
        public void DeleteAsync_NonExistentTour_ThrowsTourNotFoundException()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Tour?)null);

            Assert.ThrowsAsync<TourNotFoundException>(async () =>
                await _service.DeleteAsync(id));
        }

        [Test]
        public void DeleteAsync_NonExistentTour_ExceptionContainsId()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Tour?)null);

            var ex = Assert.ThrowsAsync<TourNotFoundException>(async () =>
                await _service.DeleteAsync(id));

            Assert.That(ex!.Message, Does.Contain(id.ToString()));
        }

        [Test]
        public void CreateAsync_RepoThrows_WrapsAsTourValidationException()
        {
            var tour = new Tour { Name = "Test", From = "A", To = "B" };
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Tour>()))
                .ThrowsAsync(new InvalidOperationException("DB constraint violation"));

            var ex = Assert.ThrowsAsync<TourValidationException>(async () =>
                await _service.CreateAsync(tour));

            Assert.That(ex!.InnerException, Is.TypeOf<InvalidOperationException>());
        }
    }
}
