using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using Newtonsoft.Json;

namespace POI
{
    public class POIJsonService : IPOIDataService
    {
        private string _storagePath;
        private List<PointOfInterest> _pois = new List<PointOfInterest>();
        public POIJsonService(string storagePath)
        {
            _storagePath = storagePath;
            // create the storage path if it does not exist
            if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
            RefreshCache();
            Console.WriteLine("POI storage path is {0}", _storagePath);
        }

        #region IPOIDataService Implementation
        public void RefreshCache()
        {
            _pois.Clear();

            string[] filenames = Directory.GetFiles(_storagePath, "*.json");

            foreach(string filename in filenames)
            {
                string poiString = File.ReadAllText(filename);
                PointOfInterest poi = JsonConvert.DeserializeObject<PointOfInterest>(poiString);
                _pois.Add(poi);
            }
        }
        public PointOfInterest GetPOI(int id)
        {
            PointOfInterest poi = _pois.Find(p => p.Id == id);
            return poi;
        }
        public void SavePOI(PointOfInterest poi)
        {
            Boolean newPOI = false;
            if (!poi.Id.HasValue)
            {
                poi.Id = getNextId();
                newPOI = true;
            }

            // Serialize data
            string poiString = JsonConvert.SerializeObject(poi);
            File.WriteAllText(getFileName(poi.Id.Value), poiString);

            // TODO: Include file save error handling
            // Update cache if filesave successful
            if (newPOI) _pois.Add(poi);
        }
        public void DeletePOI(PointOfInterest poi)
        {
            if (poi.Id != null)
            {
                File.Delete(getFileName((int)poi.Id));
                if (File.Exists(GetImageFilename((int)poi.Id)))
                {
                    File.Delete(GetImageFilename((int)poi.Id));
                }
                _pois.Remove(poi);
            } else
            {
                // Something went wrong, nothing to delete.
                // TODO: Error handling in the event of a null id
            }
        }
        public IReadOnlyList<PointOfInterest> POIs
        {
            get { return _pois; }
        }
        #endregion
        private int getNextId()
        {
            if(_pois.Count == 0) return 1;
            return _pois.Max(p => p.Id.Value) + 1;
        }
        private string getFileName(int id)
        {
            return Path.Combine(_storagePath, "poi" + id.ToString() + ".json");
        }
        public string GetImageFilename(int id)
        {
            return Path.Combine(_storagePath, "poiimage" + id.ToString() + ".jpg");
        }
    }    
}