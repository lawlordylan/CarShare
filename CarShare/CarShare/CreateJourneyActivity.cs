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

namespace CarShare
{
    [Activity(Label = "CreateJourneyActivity")]
    public class CreateJourneyActivity : Activity, IOnMapReadyCallback
    {
        public static MobileServiceClient MobileService =
        new MobileServiceClient("https://c00197013.azurewebsites.net");

        static ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
        ISharedPreferencesEditor edit = pref.Edit();

        string UserID = pref.GetString("UserID", "");
        private GoogleMap _map;
        private MapFragment _mapFragment;
        LocationManager locMgr;
        MarkerOptions currentLocation;
        MarkerOptions fromLocation;
        MarkerOptions toLocation;

        Marker toMarker;
        Marker fromMarker;

        Spinner numPassengers;
        Button _dateSelectButton;
        Button _timeSelectButton;
        Button addDate;
        RadioButton notReccuring;
        RadioButton isRecurring;
        TextView depDate;
        TextView depTime;
        TextView additionalDates;
        List<DateTime> additionalDatesList;
        DateTime initialDepDate;
        DateTime initialDepTime;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.CreateJourney);

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

            #endregion

            // Create your application here
            InitMapFragment();
            SetupCurrentLocationButton();
            SetupSetLocationButton();
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
            numPassengers = FindViewById<Spinner>(Resource.Id.spinnerNumOfPassengers);
            ArrayAdapter adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.passengersNumber, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            numPassengers.Adapter = adapter;
            depDate = FindViewById<TextView>(Resource.Id.textViewDepDate);
            _dateSelectButton = FindViewById<Button>(Resource.Id.selectDateButton);
            _dateSelectButton.Click += DateSelect_OnClick;
            depTime = FindViewById<TextView>(Resource.Id.textViewDepTime);
            _timeSelectButton = FindViewById<Button>(Resource.Id.timeSelectButton);
            _timeSelectButton.Click += TimeSelect_OnClick;
            notReccuring = FindViewById<RadioButton>(Resource.Id.radioNo);
            notReccuring.Click += notRecClicked;
            isRecurring = FindViewById<RadioButton>(Resource.Id.radioYes);
            isRecurring.Click += isRecClicked;
            addDate = FindViewById<Button>(Resource.Id.buttonAddDates);
            addDate.Click += addDateClicked;
            additionalDates = FindViewById<TextView>(Resource.Id.textViewExtraDates);
            additionalDatesList = new List<DateTime>();

        }

        private void addDateClicked(object sender, EventArgs e)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
            {
                additionalDates.Text += time.DayOfWeek.ToString() + " " + time.ToLongDateString() + "\n";
                additionalDatesList.Add(time);
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }
        private void notRecClicked(object sender, EventArgs e)
        {
            notReccuring.Checked = true;
            isRecurring.Checked = false;
            addDate.Visibility = ViewStates.Invisible;
            additionalDates.Text = "";
            additionalDatesList.Clear();
        }
        private void isRecClicked(object sender, EventArgs e)
        {          
            notReccuring.Checked = false;
            isRecurring.Checked = true;
            addDate.Visibility = ViewStates.Visible;          
        }
        private void HandleDateChange(object sender, EventArgs e)
        {
            DatePicker dp = (DatePicker)sender;
            depDate = FindViewById<TextView>(Resource.Id.textViewDepDate);
            string date = dp.DateTime.DayOfWeek.ToString();
            depDate.SetText("Departure Date: " + date, TextView.BufferType.Normal);
        }
        private void SetupMapMarkers()
        {
            _map.AddMarker(currentLocation);
            _map.AddMarker(fromLocation);
            _map.AddMarker(toLocation);
        }
        private void InitMapFragment()
        {
            _mapFragment = FragmentManager.FindFragmentByTag("mapFrame") as MapFragment;
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
                fragTx.Add(Resource.Id.mapFrame, _mapFragment, "mapFrame");
                fragTx.Commit();
            }
            _mapFragment.GetMapAsync(this);
        }

        private void SetupCurrentLocationButton()
        {
            Button currentLocationButton = FindViewById<Button>(Resource.Id.buttonFindMe);
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
        private void SetupSetLocationButton()
        {
            Button saveButton = FindViewById<Button>(Resource.Id.buttonSave);
            saveButton.Click += (sender, e) => {
                if (fromMarker.Position != null &&
                toMarker.Position != null &&
                initialDepDate != null &&
                initialDepTime != null)
                {
                    CreateJourney();
                    StartActivity(typeof(MainProfileActivity));
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
            j.NoOfPassengers = Convert.ToInt32(numPassengers.SelectedItem.ToString());
            if(DateTime.Now > initialDepDate)
            {
                j.Completed = true;
            }
            else
            {
                j.Completed = false;
            }
            j.Filled = false;
            j.Passengers = "";
            j.Applicants = "";
            ProgressDialog progress;
            progress = new ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetMessage("Creating... Please wait...");
            progress.SetCancelable(false);
            progress.Show();
            CurrentPlatform.Init();
            DBHelper.InsertJourney(j);
            if (additionalDatesList != null)
            {
                foreach (DateTime d in additionalDatesList)
                {
                    j.DepartureDate = d.ToShortDateString();
                    DBHelper.InsertJourney(j);
                }
            }
            progress.Hide();
            Toast.MakeText(ApplicationContext, "Journey created!", ToastLength.Short).Show();
        }
        private void SetupSetFromButton()
        {
            Button animateButton = FindViewById<Button>(Resource.Id.buttonSetFrom);
            
            animateButton.Click += (sender, e) => {
                fromLocation.SetPosition(_map.CameraPosition.Target);
                fromLocation.Visible(true);
                if (fromMarker != null)
                {
                    fromMarker.Remove();
                }
                fromMarker = _map.AddMarker(fromLocation);
            };
        }
        private void SetupSetToButton()
        {
            Button animateButton = FindViewById<Button>(Resource.Id.buttonSetTo);
            animateButton.Click += (sender, e) => {
                toLocation.SetPosition(_map.CameraPosition.Target);
                toLocation.Visible(true);
                if (toMarker != null)
                {
                    toMarker.Remove();
                }
                toMarker = _map.AddMarker(toLocation);

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
                    depTime.Text = "Departure Time: " + time.ToShortTimeString();
                    initialDepTime = time;
                });

            frag.Show(FragmentManager, TimePickerFragment.TAG);
        }
    }

    public class DatePickerFragment : DialogFragment,
                                  DatePickerDialog.IOnDateSetListener
    {
        // TAG can be any string of your choice.
        public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();

        // Initialize this value to prevent NullReferenceExceptions.
        Action<DateTime> _dateSelectedHandler = delegate { };

        public static DatePickerFragment NewInstance(Action<DateTime> onDateSelected)
        {
            DatePickerFragment frag = new DatePickerFragment();
            frag._dateSelectedHandler = onDateSelected;
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime currently = DateTime.Now;
            DatePickerDialog dialog = new DatePickerDialog(Activity,
                                                           this,
                                                           currently.Year,
                                                           currently.Month - 1,
                                                           currently.Day);
            return dialog;
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
            DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
            Log.Debug(TAG, selectedDate.ToLongDateString());
            _dateSelectedHandler(selectedDate);
        }
    }

    public class TimePickerFragment : DialogFragment, TimePickerDialog.IOnTimeSetListener
    {
        public static readonly string TAG = "MyTimePickerFragment";
        Action<DateTime> timeSelectedHandler = delegate { };

        public static TimePickerFragment NewInstance(Action<DateTime> onTimeSelected)
        {
            TimePickerFragment frag = new TimePickerFragment();
            frag.timeSelectedHandler = onTimeSelected;
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime currentTime = DateTime.Now;
            bool is24HourFormat = DateFormat.Is24HourFormat(Activity);
            TimePickerDialog dialog = new TimePickerDialog
                (Activity, this, currentTime.Hour, currentTime.Minute, is24HourFormat);
            return dialog;
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            DateTime currentTime = DateTime.Now;
            DateTime selectedTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hourOfDay, minute, 0);
            Log.Debug(TAG, selectedTime.ToLongTimeString());
            timeSelectedHandler(selectedTime);
        }
    }
}