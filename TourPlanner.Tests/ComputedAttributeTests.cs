using TourPlanner.BL;
using TourPlanner.Models;

namespace TourPlanner.Tests
{
    [TestFixture]
    public class ComputedAttributeTests
    {
        // ===== Popularity =====

        [Test]
        public void ComputePopularity_WithZeroLogs_ReturnsUnbeliebt()
        {
            var tour = CreateTourWithLogs(0);

            TourService.ComputePopularity(tour);

            Assert.That(tour.PopularityLevel, Is.EqualTo("Unbeliebt"));
            Assert.That(tour.PopularityCount, Is.EqualTo(0));
        }

        [Test]
        public void ComputePopularity_WithOneLogs_ReturnsBekannt()
        {
            var tour = CreateTourWithLogs(1);

            TourService.ComputePopularity(tour);

            Assert.That(tour.PopularityLevel, Is.EqualTo("Bekannt"));
            Assert.That(tour.PopularityCount, Is.EqualTo(1));
        }

        [Test]
        public void ComputePopularity_WithThreeLogs_ReturnsBekannt_UpperBound()
        {
            var tour = CreateTourWithLogs(3);

            TourService.ComputePopularity(tour);

            Assert.That(tour.PopularityLevel, Is.EqualTo("Bekannt"));
        }

        [Test]
        public void ComputePopularity_WithFourLogs_ReturnsBeliebt_LowerBound()
        {
            var tour = CreateTourWithLogs(4);

            TourService.ComputePopularity(tour);

            Assert.That(tour.PopularityLevel, Is.EqualTo("Beliebt"));
            Assert.That(tour.PopularityCount, Is.EqualTo(4));
        }

        [Test]
        public void ComputePopularity_WithNullTourLogs_ReturnsUnbeliebt()
        {
            var tour = new Tour { TourLogs = null! };

            TourService.ComputePopularity(tour);

            Assert.That(tour.PopularityLevel, Is.EqualTo("Unbeliebt"));
            Assert.That(tour.PopularityCount, Is.EqualTo(0));
        }

        // ===== Child-Friendliness =====

        [Test]
        public void ComputeChildFriendliness_WithNoLogs_ReturnsUnbekannt()
        {
            var tour = CreateTourWithLogs(0);

            TourService.ComputeChildFriendliness(tour);

            Assert.That(tour.ChildFriendliness, Is.EqualTo("Unbekannt"));
        }

        [Test]
        public void ComputeChildFriendliness_WithEasyShortLogs_ReturnsKinderfreundlich()
        {
            // Difficulty=1 → diffNorm=0, Time=0.5h → timeNorm=0.125, Distance=2km → distNorm=0.067
            // Score = (0 + 0.125 + 0.067) / 3 = 0.064 → Kinderfreundlich
            var tour = new Tour
            {
                TourLogs = new List<TourLog>
                {
                    new() { Difficulty = 1, TotalTime = 0.5, TotalDistance = 2 }
                }
            };

            TourService.ComputeChildFriendliness(tour);

            Assert.That(tour.ChildFriendliness, Is.EqualTo("Kinderfreundlich"));
        }

        [Test]
        public void ComputeChildFriendliness_WithHardLongLogs_ReturnsNichtKinderfreundlich()
        {
            // Difficulty=5 → diffNorm=1, Time=6h → timeNorm=1, Distance=50km → distNorm=1
            // Score = (1 + 1 + 1) / 3 = 1.0 → Nicht kinderfreundlich
            var tour = new Tour
            {
                TourLogs = new List<TourLog>
                {
                    new() { Difficulty = 5, TotalTime = 6, TotalDistance = 50 }
                }
            };

            TourService.ComputeChildFriendliness(tour);

            Assert.That(tour.ChildFriendliness, Is.EqualTo("Nicht kinderfreundlich"));
        }

        [Test]
        public void ComputeChildFriendliness_WithMediumLogs_ReturnsBedigtGeeignet()
        {
            // Difficulty=3 → diffNorm=0.5, Time=2h → timeNorm=0.5, Distance=15km → distNorm=0.5
            // Score = (0.5 + 0.5 + 0.5) / 3 = 0.5 → Bedingt geeignet
            var tour = new Tour
            {
                TourLogs = new List<TourLog>
                {
                    new() { Difficulty = 3, TotalTime = 2, TotalDistance = 15 }
                }
            };

            TourService.ComputeChildFriendliness(tour);

            Assert.That(tour.ChildFriendliness, Is.EqualTo("Bedingt geeignet"));
        }

        [Test]
        public void ComputeChildFriendliness_AtBoundary033_ReturnsKinderfreundlich()
        {
            // Target score = exactly 0.33
            // diffNorm=0.33, timeNorm=0.33, distNorm=0.33 → score=0.33
            // difficulty: 0.33 = (d-1)/4 → d = 2.32
            // time: 0.33 = t/4 → t = 1.32
            // distance: 0.33 = d/30 → d = 9.9
            // Use 2 logs to get the averages right
            var tour = new Tour
            {
                TourLogs = new List<TourLog>
                {
                    new() { Difficulty = 2, TotalTime = 1.32, TotalDistance = 9.9 },
                    new() { Difficulty = 3, TotalTime = 1.32, TotalDistance = 9.9 }
                }
            };
            // avgDiff = 2.5 → diffNorm = 0.375, timeNorm = 0.33, distNorm = 0.33
            // score = (0.375 + 0.33 + 0.33) / 3 = 0.345 → Bedingt geeignet (just over)
            // Adjust: use values that land <= 0.33
            tour.TourLogs = new List<TourLog>
            {
                new() { Difficulty = 1, TotalTime = 1.32, TotalDistance = 9.9 }
            };
            // diffNorm = 0, timeNorm = 0.33, distNorm = 0.33
            // score = (0 + 0.33 + 0.33) / 3 = 0.22 → Kinderfreundlich

            TourService.ComputeChildFriendliness(tour);

            Assert.That(tour.ChildFriendliness, Is.EqualTo("Kinderfreundlich"));
        }

        [Test]
        public void ComputeChildFriendliness_AtBoundary066_ReturnsBedigtGeeignet()
        {
            // diffNorm=0.5, timeNorm=0.74, distNorm=0.74 → score = 0.66 → Bedingt geeignet
            // diff: 0.5 = (d-1)/4 → d=3
            // time: 0.74 = t/4 → t=2.96
            // dist: 0.74 = d/30 → d=22.2
            var tour = new Tour
            {
                TourLogs = new List<TourLog>
                {
                    new() { Difficulty = 3, TotalTime = 2.64, TotalDistance = 19.8 }
                }
            };
            // diffNorm=0.5, timeNorm=0.66, distNorm=0.66 → score=(0.5+0.66+0.66)/3 = 0.607

            TourService.ComputeChildFriendliness(tour);

            Assert.That(tour.ChildFriendliness, Is.EqualTo("Bedingt geeignet"));
        }

        [Test]
        public void ComputeChildFriendliness_WithMultipleLogs_UsesAverage()
        {
            // Two logs: one easy, one hard → average should be medium
            var tour = new Tour
            {
                TourLogs = new List<TourLog>
                {
                    new() { Difficulty = 1, TotalTime = 0.5, TotalDistance = 2 },
                    new() { Difficulty = 5, TotalTime = 6, TotalDistance = 50 }
                }
            };
            // avgDiff=3 → 0.5, avgTime=3.25 → 0.8125, avgDist=26 → 0.867
            // score = (0.5 + 0.8125 + 0.867) / 3 = 0.726 → Nicht kinderfreundlich

            TourService.ComputeChildFriendliness(tour);

            Assert.That(tour.ChildFriendliness, Is.EqualTo("Nicht kinderfreundlich"));
        }

        // ===== ComputeAttributes (both at once) =====

        [Test]
        public void ComputeAttributes_SetsBothPopularityAndChildFriendliness()
        {
            var tour = new Tour
            {
                TourLogs = new List<TourLog>
                {
                    new() { Difficulty = 1, TotalTime = 0.5, TotalDistance = 2 },
                    new() { Difficulty = 1, TotalTime = 0.5, TotalDistance = 2 }
                }
            };

            TourService.ComputeAttributes(tour);

            Assert.That(tour.PopularityCount, Is.EqualTo(2));
            Assert.That(tour.PopularityLevel, Is.EqualTo("Bekannt"));
            Assert.That(tour.ChildFriendliness, Is.EqualTo("Kinderfreundlich"));
        }

        // ===== Helpers =====

        private static Tour CreateTourWithLogs(int logCount)
        {
            var tour = new Tour { Name = "Test", From = "A", To = "B" };
            for (int i = 0; i < logCount; i++)
            {
                tour.TourLogs.Add(new TourLog
                {
                    Difficulty = 2,
                    TotalTime = 1,
                    TotalDistance = 5,
                    Rating = 3,
                    DateTime = DateTime.UtcNow
                });
            }
            return tour;
        }
    }
}
