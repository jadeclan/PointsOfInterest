using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics.Drawables;
using Android.Locations;

namespace POI
{
    [Activity( Label = "Points of Interest", 
        MainLauncher = true, 
                Icon = "@drawable/james")]
    public class POIListActivity : Activity, ILocationListener
    {
        ListView _poiListView;
        POIListViewAdapter _adapter;
        LocationManager _locMgr;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.POIList);

            // Obtain a reference to LocationManager
            _locMgr = GetSystemService(Context.LocationService) as LocationManager;

            _poiListView = FindViewById<ListView>(Resource.Id.POIListView);
            _adapter = new POIListViewAdapter(this);
            _poiListView.Adapter = _adapter;

            _poiListView.ItemClick += POIClicked;
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locMgr.RemoveUpdates(this);
        }

        protected override void OnResume()
        {
            base.OnResume();

            _adapter.NotifyDataSetChanged();

            // Set criterion for getting locations
            Criteria criteria = new Criteria();
            criteria.Accuracy = Accuracy.NoRequirement;
            criteria.PowerRequirement = Power.NoRequirement;

            string provider = _locMgr.GetBestProvider(criteria, true);

            // Parameters are provider, time in milliseconds, distance
            // in meters and the pending intent.
            _locMgr.RequestLocationUpdates(provider, 20000, 100, this);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.POIListViewMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.actionNew:
                    StartActivity(typeof(POIDetailActivity));
                    return true;

                case Resource.Id.actionRefresh:
                    POIData.Service.RefreshCache();
                    _adapter.NotifyDataSetChanged();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
        protected void POIClicked(Object sender, ListView.ItemClickEventArgs e)
        {
            // setup the intent to pass the POI id to the detail view
            Intent poiDetailIntent = new Intent(this, typeof(POIDetailActivity));
            poiDetailIntent.PutExtra("poiId", (int)e.Id);
            StartActivity(poiDetailIntent);
        }

        public void OnLocationChanged(Location location)
        {
            _adapter.CurrentLocation = location;
            _adapter.NotifyDataSetChanged();
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
        }
    }
}

