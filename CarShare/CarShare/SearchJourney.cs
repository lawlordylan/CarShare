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
using Android.Gms.Maps;
using Android.Locations;
using Android.Gms.Maps.Model;
using Android.Util;
using Android.Text.Format;
using CarShare.Models;
using Microsoft.WindowsAzure.MobileServices;
using Android.Graphics;
using Newtonsoft.Json;

namespace CarShare
{
    [Activity(Label = "SearchJourney")]
    public class SearchJourney : Activity, IOnMapReadyCallback
    {
        static ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
        ISharedPreferencesEditor edit = pref.Edit();

        string UserID = pref.GetString("UserID", "");
        private GoogleMap _map;
        private MapFragment _mapFragment;
        LocationManager locMgr;
        MarkerOptions currentLocation;
        MarkerOptions fromLocation;
        MarkerOptions toLocation;
        CircleOptions fromCircleOp;
        CircleOptions toCircleOp;
        Marker toMarker;
        Marker fromMarker;
        Circle fromCircle;
        Circle toCircle;

        Button _dateSelectButton;
        Button _timeSelectButton;
        RadioButton before;
        RadioButton after;
        RadioButton around;
        TextView depDate;
        TextView depTime;
        TextView range;
        SeekBar rangeFinder;
        DateTime initialDepDate;
        DateTime initialDepTime;
        DateTime minDepTime;
        DateTime maxDepTime;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SearchJourney);

            #region setup map markers
            currentLocation = new MarkerOptions();
            currentLocation.SetTitle("Current Position");
            currentLocation.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueBlue));
            currentLocation.SetPosition(getCurrentPosition());
            currentLocation.Visible(false);

            fromLocation = new MarkerOptions();
            fromLocation.SetTitle("From Here");
            fromLocation.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));
            fromLocation.SetPosition(getCurrentPosition());
            fromLocation.Visible(false);

            toLocation = new MarkerOptions();
            toLocation.SetTitle("To Here");
            toLocation.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));
            toLocation.SetPosition(getCurrentPosition());
            toLocation.Visible(false);

            fromCircleOp = new CircleOptions();
            fromCircleOp.InvokeCenter(getCurrentPosition());
            fromCircleOp.InvokeRadius(1000);
            fromCircleOp.InvokeFillColor(0X66FF0000);
            fromCircleOp.InvokeStrokeColor(0X66FF0000);
            fromCircleOp.InvokeStrokeWidth(0);
            fromCircleOp.Visible(false);
            

            toCircleOp = new CircleOptions();
            toCircleOp.InvokeCenter(getCurrentPosition());
            toCircleOp.InvokeRadius(1000);
            toCircleOp.InvokeFillColor(Color.Green);
            toCircleOp.InvokeStrokeColor(Color.Green);
            toCircleOp.InvokeStrokeWidth(0);
            toCircleOp.Visible(false);
            #endregion

            // Create your application here
            InitMapFragment();
            SetupCurrentLocationButton();
            SetupSearchButton();
            SetupSetFromButton();
            SetupSetToButton();
            SetupForm();

        }

        protected override void OnResume()
        {
            base.OnResume();
            SetupMapIfNeeded();
        }
        private void SetupForm()
        {
            depDate = FindViewById<TextView>(Resource.Id.textViewSearchJourneyDepDate);
            _dateSelectButton = FindViewById<Button>(Resource.Id.buttonSearchJourneySelectDate);
            _dateSelectButton.Click += DateSelect_OnClick;
            depTime = FindViewById<TextView>(Resource.Id.buttonSearchJourneySelectTime);
            _timeSelectButton = FindViewById<Button>(Resource.Id.buttonSearchJourneySelectTime);
            _timeSelectButton.Click += TimeSelect_OnClick;
            before = FindViewById<RadioButton>(Resource.Id.radioSearchJourneyBefore);
            before.Click += beforeClicked;
            after = FindViewById<RadioButton>(Resource.Id.radioSearchJourneyAfter);
            after.Click += afterClicked;
            around = FindViewById<RadioButton>(Resource.Id.radioSearchJourneyAround);
            around.Click += aroundClicked;
            range = FindViewById<TextView>(Resource.Id.textViewSearchRange);
            rangeFinder = FindViewById<SeekBar>(Resource.Id.seekBarSearchRange);
            range.Text = string.Format("{0}km", rangeFinder.Progress);
            rangeFinder.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) => {
                if (e.FromUser)
                {
                    range.Text = string.Format("{0}km", (double)e.Progress/2);
                    toCircle.Radius = e.Progress * 500;
                    fromCircle.Radius = e.Progress * 500;
                }
            };
        }
        private void beforeClicked(object sender, EventArgs e)
        {
            after.Checked = false;
            around.Checked = false;
            maxDepTime = initialDepTime;
            minDepTime = initialDepTime.AddHours(-1);
        }
        private void afterClicked(object sender, EventArgs e)
        {
            before.Checked = false;
            around.Checked = false;
            maxDepTime = initialDepTime.AddHours(1); ;
            minDepTime = initialDepTime;
        }
        private void aroundClicked(object sender, EventArgs e)
        {
            after.Checked = false;
            before.Checked = false;
            maxDepTime = initialDepTime.AddMinutes(30);
            minDepTime = initialDepTime.AddMinutes(-30);
        }
        private void HandleDateChange(object sender, EventArgs e)
        {
            DatePicker dp = (DatePicker)sender;
            depDate = FindViewById<TextView>(Resource.Id.textViewSearchJourneyDepDate);
            string date = dp.DateTime.DayOfWeek.ToString();
            depDate.SetText("Departure Date: " + date, TextView.BufferType.Normal);
        }
        private void SetupMapMarkers()
        {
            _map.AddMarker(currentLocation);
            _map.AddMarker(fromLocation);
            _map.AddMarker(toLocation);
            fromCircle = _map.AddCircle(fromCircleOp);
            toCircle =_map.AddCircle(toCircleOp);
        }
        private void InitMapFragment()
        {
            _mapFragment = FragmentManager.FindFragmentByTag("mapFrameSearch") as MapFragment;
            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeNormal)
                    .InvokeZoomControlsEnabled(false)
                    .InvokeCompassEnabled(true)
                    .InvokeCamera(new CameraPosition(getCurrentPosition(), 18, 65, 0))
                    ;

                FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                _mapFragment = MapFragment.NewInstance(mapOptions);
                fragTx.Add(Resource.Id.mapFrameSearch, _mapFragment, "mapFrameSearch");
                fragTx.Commit();
            }
            _mapFragment.GetMapAsync(this);
        }
        private void SetupCurrentLocationButton()
        {
            Button currentLocationButton = FindViewById<Button>(Resource.Id.buttonSearchJourneyMyLocation);
            currentLocationButton.Click += (sender, e) => {
                CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
                builder.Target(getCurrentPosition());
                builder.Zoom(18);
                builder.Bearing(155);
                CameraPosition cameraPosition = builder.Build();

                // AnimateCamera provides a smooth, animation effect while moving
                // the camera to the the position.
                currentLocation.SetPosition(getCurrentPosition());
                currentLocation.Visible(true);
                _map.AddMarker(currentLocation);
                _map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));

            };
        }
        private void SetupSearchButton()
        {
            Button saveButton = FindViewById<Button>(Resource.Id.buttonSearchJourneySearch);
            saveButton.Click += async (sender, e) => {
                if (fromMarker.Position != null &&
                toMarker.Position != null &&
                initialDepDate != null &&
                initialDepTime != null)
                {
                    Search userSearch = new Search()
                    {
                        CreatedBy = pref.GetString("UserID", ""),
                        DepartureDate = initialDepDate.ToShortDateString(),
                        MaxDepartureDateTime = maxDepTime.ToShortTimeString(),
                        MinDepartureDateTime = minDepTime.ToShortTimeString(),
                        From = LocationHelper.ReverseGeoLoc(fromMarker.Position.Latitude, fromMarker.Position.Longitude),
                        To = LocationHelper.ReverseGeoLoc(toMarker.Position.Latitude, toMarker.Position.Longitude),
                        FromLat = fromMarker.Position.Latitude,
                        FromLon = fromMarker.Position.Longitude,
                        ToLat = toMarker.Position.Latitude,
                        ToLon = toMarker.Position.Longitude,
                        Range = 20
                    };
                    //Get a list of Journeys (not expired) in the right area (distance = 20km for now)
                    List<Journeys> candidates = await DBHelper.SearchJourneys(userSearch);
                    //handle search
                    if (candidates.Count == 0)
                    {
                        //ask if user wants to list this search
                    }
                    else
                    {
                        //show results
                        string saveSearchResults = JsonConvert.SerializeObject(candidates);
                        edit.PutString("SearchResults",saveSearchResults);
                        edit.Commit();
                        StartActivity(typeof(SearchJourneyResultsActivity));
                    }
                    string message = "Number of hits: " + candidates.Count;
                    Toast.MakeText(ApplicationContext,message, ToastLength.Short).Show();
                    //StartActivity(typeof(MainProfileActivity));
                }
                else
                {
                    string message = "Please ensure you have selected all required Information \n " +
                    "From Location \nTo Location \nNumber of available passenger slots \nDeparture Date and Time \n" +
                    "\n Plus any additional dates in case of a recurring journey.";
                    Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();
                }


            };
        }

        private async void CreateJourney()
        {
            Journeys j = new Journeys();
            j.CreatedBy = UserID;
            j.DepartureDate = initialDepDate.ToShortDateString();
            j.DepartureDateTime = initialDepTime.ToShortTimeString();
            j.DriverID = UserID;
            j.From = LocationHelper.ReverseGeoLoc(fromMarker.Position.Latitude, fromMarker.Position.Longitude);
            j.FromLat = fromMarker.Position.Latitude;
            j.FromLon = fromMarker.Position.Longitude;
            j.To = LocationHelper.ReverseGeoLoc(toMarker.Position.Latitude, toMarker.Position.Longitude);
            j.ToLat = toMarker.Position.Latitude;
            j.ToLon = toMarker.Position.Longitude;
            j.Passengers = "";
            ProgressDialog progress;
            progress = new ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetMessage("Creating... Please wait...");
            progress.SetCancelable(false);
            progress.Show();
            CurrentPlatform.Init();
            DBHelper.InsertJourney(j);
            progress.Hide();
            Toast.MakeText(ApplicationContext, "Journey created!", ToastLength.Short).Show();
        }
        private void SetupSetFromButton()
        {
            Button animateButton = FindViewById<Button>(Resource.Id.buttonSetSearchJourneySetFrom);

            animateButton.Click += (sender, e) => {
                fromLocation.SetPosition(_map.CameraPosition.Target);
                fromLocation.Visible(true);
                if (fromMarker != null)
                {
                    fromMarker.Remove();
                }
                fromMarker = _map.AddMarker(fromLocation);
                fromCircleOp.InvokeCenter(_map.CameraPosition.Target);
                fromCircleOp.InvokeRadius(rangeFinder.Progress * 500);
                fromCircleOp.Visible(true);
                if (fromCircle != null)
                {
                    fromCircle.Remove();
                }
                fromCircle = _map.AddCircle(fromCircleOp);
            };
        }
        private void SetupSetToButton()
        {
            Button animateButton = FindViewById<Button>(Resource.Id.buttonSearchJourneySetTo);
            animateButton.Click += (sender, e) => {
                toLocation.SetPosition(_map.CameraPosition.Target);
                toLocation.Visible(true);
                if (toMarker != null)
                {
                    toMarker.Remove();
                }
                toMarker = _map.AddMarker(toLocation);
                toCircleOp.InvokeCenter(_map.CameraPosition.Target);
                toCircleOp.InvokeRadius(rangeFinder.Progress * 500);
                toCircleOp.Visible(true);
                if (toCircle != null)
                {
                    toCircle.Remove();
                }
                toCircle = _map.AddCircle(toCircleOp);

            };
        }
        private LatLng getCurrentPosition()
        {
            locMgr = GetSystemService(Context.LocationService) as LocationManager;
            string p = LocationManager.GpsProvider;
            Location l = locMgr.GetLastKnownLocation(p);
            if (l == null)
            {
                return new LatLng(52.8365, -6.9341);
            }
            return new LatLng(l.Latitude, l.Longitude);
        }

        public void OnMapReady(GoogleMap map)
        {
            _map = map;
            SetupMapMarkers();
        }

        private void SetupMapIfNeeded()
        {
            if (_map == null)
            {
                if (_map != null)
                {
                    MarkerOptions markerOpt1 = new MarkerOptions();
                    markerOpt1.SetPosition(getCurrentPosition());
                    markerOpt1.SetTitle("Current Position");
                    markerOpt1.InvokeIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueCyan));
                    _map.AddMarker(markerOpt1);



                    // We create an instance of CameraUpdate, and move the map to it.
                    CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngZoom(getCurrentPosition(), 15);
                    _map.MoveCamera(cameraUpdate);
                }
            }
        }
        void DateSelect_OnClick(object sender, EventArgs eventArgs)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
            {
                depDate.Text = "Departure Date: " + time.DayOfWeek.ToString() + " " + time.ToLongDateString();
                initialDepDate = time;
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }
        void TimeSelect_OnClick(object sender, EventArgs eventArgs)
        {
            TimePickerFragment frag = TimePickerFragment.NewInstance(
                delegate (DateTime time)
                {
                   
                    depTime.Text = "Departure Time: "+ time.ToShortTimeString();
                    initialDepTime = time;
                });

            frag.Show(FragmentManager, TimePickerFragment.TAG);
        }
    }

    
}