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

namespace POI
{
    public interface IPOIDataService
    {
        IReadOnlyList<PointOfInterest> POIs { get; }
        void RefreshCache();
        PointOfInterest GetPOI(int id);
        void SavePOI(PointOfInterest poi);
        void DeletePOI(PointOfInterest poi);
        string GetImageFilename(int id);
    }
}