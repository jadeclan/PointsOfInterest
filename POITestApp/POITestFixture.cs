using System;
using NUnit.Framework;
using POI;
using System.IO;

namespace UnitTests
{
    [TestFixture]
    public class POITestFixture
    {
        IPOIDataService _poiService;

        [SetUp]
        public void Setup()
        {
            string storagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _poiService = new POIJsonService(storagePath);

            // Clear any existing json files
            foreach(string filename in Directory.EnumerateFiles(storagePath, "*.json"))
            {
                File.Delete(filename);
            }
        }

        [TearDown]
        public void Tear() { }

        [Test]
        public void CreatePOI()
        {
            PointOfInterest newPOI = new PointOfInterest();
            newPOI.Name = "New Test Point of Interest";
            newPOI.Description = "Test to create new POI";
            newPOI.Address = "120 Lake Tahoe Place SE, Calgary, Alberta";
            _poiService.SavePOI(newPOI);
            int testId = newPOI.Id.Value;

            // Refresh cache to assure data saved
            _poiService.RefreshCache();

            // Verify new point of interest exists
            PointOfInterest poi = _poiService.GetPOI(testId);

            Assert.NotNull(poi);
            Assert.AreEqual(poi.Name, "New Test Point of Interest");
        }
        [Test]
        public void UpdatePOI()
        {
            PointOfInterest testPOI = new PointOfInterest();
            testPOI.Name = "Update Test Point of Interest";
            testPOI.Description = "Poi being saved to test update POI";
            testPOI.Address = "120 Lake Tahoe Place SE, Calgary, Alberta";
            _poiService.SavePOI(testPOI);
            int testId = testPOI.Id.Value;

            // Refresh cache to assure data saved
            _poiService.RefreshCache();

            PointOfInterest poi = _poiService.GetPOI(testId);
            poi.Description = "Updated description for update poi";
            _poiService.SavePOI(poi);

            // Refresh cache to assure data saved
            _poiService.RefreshCache();

            poi = _poiService.GetPOI(testId);
            Assert.NotNull(poi);
            Assert.AreEqual(poi.Description, "Updated description for update poi");
        }

        [Test]
        public void DeletePOI()
        {
            PointOfInterest testPOI = new PointOfInterest();
            testPOI.Name = "Delete POI";
            testPOI.Description = "Poi being saved to test delete";
            testPOI.Address = "120 Lake Tahoe Place SE, Calgary, Alberta";
            _poiService.SavePOI(testPOI);
            int testId = testPOI.Id.Value;

            // Refresh cache to assure data saved
            _poiService.RefreshCache();

            PointOfInterest deletePoi = _poiService.GetPOI(testId);
            Assert.IsNotNull(deletePoi);
            _poiService.DeletePOI(deletePoi);
            
            // Refresh cache to assure data saved
            _poiService.RefreshCache();

            PointOfInterest poi = _poiService.GetPOI(testId);
            Assert.Null(poi);
        }
    }
}