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
using Android.Locations;
using Android.Content.PM;
using Android.Provider;
using Android.Graphics;

namespace POI
{
    [Activity(Label = "POIDetailActivity")]
    public class POIDetailActivity : Activity, ILocationListener
    {
        const int CAPTURE_PHOTO = 0;

        PointOfInterest _poi;
        LocationManager _locMgr;

        EditText _nameEditText;
        EditText _descriptionEditText;
        EditText _addressEditText;
        EditText _latitudeEditText;
        EditText _longitudeEditText;
        ImageView _poiImageView;
        ImageButton _locationImageButton;
        ImageButton _mapImageButton;
        ImageButton _photoImageButton;

        ProgressDialog _progressDialog;
        bool _obtainingLocation = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.POIDetail);
            _nameEditText = FindViewById<EditText>(Resource.Id.nameEditText);
            _descriptionEditText = FindViewById<EditText>(Resource.Id.descEditText);
            _addressEditText = FindViewById<EditText>(Resource.Id.addrEditText);
            _latitudeEditText = FindViewById<EditText>(Resource.Id.latEditText);
            _longitudeEditText = FindViewById<EditText>(Resource.Id.longEditText);
            _poiImageView = FindViewById<ImageView>(Resource.Id.poiImageView);
            _locationImageButton = FindViewById<ImageButton>(Resource.Id.locationImageButton);
            _mapImageButton = FindViewById<ImageButton>(Resource.Id.mapImageButton);
            _photoImageButton = FindViewById<ImageButton>(Resource.Id.photoImageButton);

            // Obtain a reference to LocationManager
            _locMgr = GetSystemService(Context.LocationService) as LocationManager;

            _locationImageButton.Click += GetLocationClicked;
            _mapImageButton.Click += MapClicked;
            _photoImageButton.Click += NewPhotoClicked;

            if (Intent.HasExtra("poiId"))
            {
                int poiId = Intent.GetIntExtra("poiId", -1);
                _poi = POIData.Service.GetPOI(poiId);

                Bitmap poiImage = POIData.GetImageFile(_poi.Id.Value);
                _poiImageView.SetImageBitmap(poiImage);
                if (poiImage != null)
                    poiImage.Dispose();
            }
            else
                _poi = new PointOfInterest();

            UpdateUI();
        }
        protected void UpdateUI()
        {
            _nameEditText.Text = _poi.Name;
            _descriptionEditText.Text = _poi.Description;
            _addressEditText.Text = _poi.Address;
            _latitudeEditText.Text = _poi.Latitude.ToString();
            _longitudeEditText.Text = _poi.Longitude.ToString();
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.POIDetailMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            base.OnPrepareOptionsMenu(menu);

            // disable delete for a new POI
            if (!_poi.Id.HasValue)
            {
                IMenuItem item = menu.FindItem(Resource.Id.actionDelete);
                item.SetEnabled(false);
            }

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.actionSave:
                    SavePOI();
                    return true;

                case Resource.Id.actionDelete:
                    DeletePOI();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
        protected void SavePOI()
        {
            bool errors = false;

            if (String.IsNullOrEmpty(_nameEditText.Text))
            {
                _nameEditText.Error = "Name cannot be empty";
                errors = true;
            }
            else
                _nameEditText.Error = null;

            double? tempLatitude = null;
            if (!String.IsNullOrEmpty(_latitudeEditText.Text))
            {
                try
                {
                    tempLatitude = Double.Parse(_latitudeEditText.Text);
                    if ((tempLatitude > 90) | (tempLatitude < -90))
                    {
                        _latitudeEditText.Error = "Latitude must be a decimal value between -90 and 90";
                        errors = true;
                    }
                    else
                        _latitudeEditText.Error = null;
                }
                catch
                {
                    _latitudeEditText.Error = "Latitude must be valid decimal number";
                    errors = true;
                }
            }

            double? tempLongitude = null;
            if (!String.IsNullOrEmpty(_longitudeEditText.Text))
            {
                try
                {
                    tempLongitude = Double.Parse(_longitudeEditText.Text);
                    if ((tempLongitude > 180) | (tempLongitude < -180))
                    {
                        _longitudeEditText.Error = "Longitude must be a decimal value between -180 and 180";
                        errors = true;
                    }
                    else
                        _longitudeEditText.Error = null;
                }
                catch
                {
                    _longitudeEditText.Error = "Longitude must be valid decimal number";
                    errors = true;
                }
            }

            if (!errors)
            {
                _poi.Name = _nameEditText.Text;
                _poi.Description = _descriptionEditText.Text;
                _poi.Address = _addressEditText.Text;
                _poi.Latitude = tempLatitude;
                _poi.Longitude = tempLongitude;

                POIData.Service.SavePOI(_poi);
                Toast toast = Toast.MakeText(this, String.Format("{0} saved.", _poi.Name), ToastLength.Short);
                toast.Show();
                Finish();
            }
        }

        protected void DeletePOI()
        {
            AlertDialog.Builder alertConfirm = new AlertDialog.Builder(this);
            alertConfirm.SetCancelable(false);
            alertConfirm.SetPositiveButton("OK", ConfirmDelete);
            alertConfirm.SetNegativeButton("Cancel", delegate { });
            alertConfirm.SetMessage(String.Format("Are you sure you want to delete {0}?", _poi.Name));
            alertConfirm.Show();
        }

        protected void ConfirmDelete(object sender, EventArgs e)
        {
            POIData.Service.DeletePOI(_poi);
            Toast toast = Toast.MakeText(this, String.Format("{0} deleted.", _poi.Name), ToastLength.Short);
            toast.Show();
            Finish();
        }
        protected void GetLocationClicked(object sender, EventArgs e)
        {
            _obtainingLocation = true;
            _progressDialog = ProgressDialog.Show(this, "", "Obtaining location...");

            Criteria criteria = new Criteria();
            criteria.Accuracy = Accuracy.NoRequirement;
            criteria.PowerRequirement = Power.NoRequirement;

            _locMgr.RequestSingleUpdate(criteria, this, null);
        }

        public void OnLocationChanged(Location location)
        {
            _latitudeEditText.Text = location.Latitude.ToString();
            _longitudeEditText.Text = location.Longitude.ToString();

            Geocoder geocdr = new Geocoder(this);
            IList<Address> addresses = geocdr.GetFromLocation(location.Latitude, location.Longitude, 5);

            if (addresses.Any())
            {
                UpdateAddressFields(addresses.First());
            }

            _progressDialog.Cancel();
            _obtainingLocation = false;
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }

        protected void UpdateAddressFields(Address addr)
        {
            if (String.IsNullOrEmpty(_nameEditText.Text))
                _nameEditText.Text = addr.FeatureName;

            if (String.IsNullOrEmpty(_addressEditText.Text))
            {
                for (int i = 0; i < addr.MaxAddressLineIndex; i++)
                {
                    if (!String.IsNullOrEmpty(_addressEditText.Text))
                        _addressEditText.Text += System.Environment.NewLine;
                    _addressEditText.Text += addr.GetAddressLine(i);
                }
            }
        }

        public void MapClicked(object sender, EventArgs e)
        {
            Android.Net.Uri geoUri;
            if (String.IsNullOrEmpty(_addressEditText.Text))
            {
                geoUri = Android.Net.Uri.Parse(String.Format("geo:{0},{1}", _poi.Latitude, _poi.Longitude));
            }
            else
            {
                geoUri = Android.Net.Uri.Parse(String.Format("geo:0,0?q={0}", _addressEditText.Text));
            }

            Intent mapIntent = new Intent(Intent.ActionView, geoUri);

            PackageManager packageManager = PackageManager;
            IList<ResolveInfo> activities = packageManager.QueryIntentActivities(mapIntent, 0);
            if (activities.Count == 0)
            {
                AlertDialog.Builder alertConfirm = new AlertDialog.Builder(this);
                alertConfirm.SetCancelable(false);
                alertConfirm.SetPositiveButton("OK", delegate { });
                alertConfirm.SetMessage("No map app available.");
                alertConfirm.Show();
            }
            else
                StartActivity(mapIntent);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutBoolean("obtaininglocation", _obtainingLocation);

            // if we were waiting on location updates; cancel
            if (_obtainingLocation)
            {
                _locMgr.RemoveUpdates(this);
            }
        }
        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            _obtainingLocation = savedInstanceState.GetBoolean("obtaininglocation");

            // if we were waiting on location updates; restart
            if (_obtainingLocation)
                GetLocationClicked(this, new EventArgs());
        }
        public void NewPhotoClicked(Object sender, EventArgs e)
        {
            if (!_poi.Id.HasValue)
            {
                AlertDialog.Builder alertConfirm = new AlertDialog.Builder(this);
                alertConfirm.SetCancelable(false);
                alertConfirm.SetPositiveButton("OK", delegate { });
                alertConfirm.SetMessage("You must save the POI prior to attaching a photo.");
                alertConfirm.Show();
            }
            else
            {

                Intent cameraIntent = new Intent(MediaStore.ActionImageCapture);

                PackageManager packageManager = PackageManager;
                IList<ResolveInfo> activities = packageManager.QueryIntentActivities(cameraIntent, 0);
                if (activities.Count == 0)
                {
                    AlertDialog.Builder alertConfirm = new AlertDialog.Builder(this);
                    alertConfirm.SetCancelable(false);
                    alertConfirm.SetPositiveButton("OK", delegate { });
                    alertConfirm.SetMessage("No camera app available to capture photos.");
                    alertConfirm.Show();
                }
                else
                {

                    Java.IO.File imageFile = new Java.IO.File(POIData.Service.GetImageFilename(_poi.Id.Value));
                    Android.Net.Uri imageUri = Android.Net.Uri.FromFile(imageFile);
                    cameraIntent.PutExtra(MediaStore.ExtraOutput, imageUri);
                    cameraIntent.PutExtra(MediaStore.ExtraSizeLimit, 2 * 1024 * 1024);

                    StartActivityForResult(cameraIntent, CAPTURE_PHOTO);
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == CAPTURE_PHOTO)
            {

                if (resultCode == Result.Ok)
                {
                    // display saved image
                    Bitmap poiImage = POIData.GetImageFile(_poi.Id.Value);
                    _poiImageView.SetImageBitmap(poiImage);
                    if (poiImage != null)
                        poiImage.Dispose();
                }
                else
                {
                    // let the user know the photo was cancelled
                    Toast toast = Toast.MakeText(this, "No picture captured.", ToastLength.Short);
                    toast.Show();
                }
            }
            else
                base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}