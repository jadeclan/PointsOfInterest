using System;
using System.IO;
using Android.App;
using Android.Graphics;

namespace POI
{
    public class POIData
    {
        public static readonly IPOIDataService Service = new 
            POIJsonService(
            System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path,
                "POI"));

        public static Bitmap GetImageFile(int poiId)
        {
            string filename = Service.GetImageFilename(poiId);
            if (File.Exists(filename))
            {
                Java.IO.File imageFile = new Java.IO.File(filename);
                return BitmapFactory.DecodeFile(imageFile.Path);
            }
            else
                return null;
        }
    }
}