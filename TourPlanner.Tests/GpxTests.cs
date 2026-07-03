using System.Xml.Linq;
using TourPlanner.API.Controllers;
using TourPlanner.Models;

namespace TourPlanner.Tests
{
    [TestFixture]
    public class GpxTests
    {
        [Test]
        public void ConvertGeoJsonToGpx_ValidLineString_GeneratesCorrectTrackPointCount()
        {
            var tour = new Tour
            {
                Name = "Test Tour",
                RouteGeometry = """{"type":"LineString","coordinates":[[16.37,48.21],[15.44,47.07],[13.04,47.80]]}"""
            };

            var gpx = TourController.ConvertGeoJsonToGpx(tour);

            var doc = XDocument.Parse(gpx);
            XNamespace ns = "http://www.topografix.com/GPX/1/1";
            var trkpts = doc.Descendants(ns + "trkpt").ToList();

            Assert.That(trkpts, Has.Count.EqualTo(3));
        }

        [Test]
        public void ConvertGeoJsonToGpx_CorrectLatLonOrder()
        {
            // GeoJSON: [lon, lat] → GPX: lat="lat" lon="lon"
            var tour = new Tour
            {
                Name = "Test Tour",
                RouteGeometry = """{"type":"LineString","coordinates":[[16.37,48.21]]}"""
            };

            var gpx = TourController.ConvertGeoJsonToGpx(tour);

            var doc = XDocument.Parse(gpx);
            XNamespace ns = "http://www.topografix.com/GPX/1/1";
            var trkpt = doc.Descendants(ns + "trkpt").First();

            Assert.That(trkpt.Attribute("lat")!.Value, Is.EqualTo("48.21"));
            Assert.That(trkpt.Attribute("lon")!.Value, Is.EqualTo("16.37"));
        }

        [Test]
        public void ConvertGeoJsonToGpx_ContainsTourNameInTrack()
        {
            var tour = new Tour
            {
                Name = "Wien-Graz Runde",
                RouteGeometry = """{"type":"LineString","coordinates":[[16.37,48.21],[15.44,47.07]]}"""
            };

            var gpx = TourController.ConvertGeoJsonToGpx(tour);

            var doc = XDocument.Parse(gpx);
            XNamespace ns = "http://www.topografix.com/GPX/1/1";
            var name = doc.Descendants(ns + "name").First().Value;

            Assert.That(name, Is.EqualTo("Wien-Graz Runde"));
        }

        [Test]
        public void ConvertGeoJsonToGpx_IsValidGpx11()
        {
            var tour = new Tour
            {
                Name = "Test",
                RouteGeometry = """{"type":"LineString","coordinates":[[16.37,48.21],[15.44,47.07]]}"""
            };

            var gpx = TourController.ConvertGeoJsonToGpx(tour);

            var doc = XDocument.Parse(gpx);
            XNamespace ns = "http://www.topografix.com/GPX/1/1";
            var root = doc.Root!;

            Assert.That(root.Name.LocalName, Is.EqualTo("gpx"));
            Assert.That(root.Attribute("version")!.Value, Is.EqualTo("1.1"));
            Assert.That(root.Attribute("creator")!.Value, Is.EqualTo("TourPlanner"));
        }

        [Test]
        public void SanitizeFileName_RemovesInvalidChars()
        {
            var result = TourController.SanitizeFileName("Tour: Wien/Graz <2024>");

            Assert.That(result, Does.Not.Contain(":"));
            Assert.That(result, Does.Not.Contain("/"));
            Assert.That(result, Does.Not.Contain("<"));
            Assert.That(result, Does.Not.Contain(">"));
        }

        [Test]
        public void SanitizeFileName_EmptyInput_ReturnsTour()
        {
            var result = TourController.SanitizeFileName("");

            Assert.That(result, Is.EqualTo("tour"));
        }
    }
}
