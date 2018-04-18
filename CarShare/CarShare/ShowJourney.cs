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
using Microsoft.WindowsAzure.MobileServices;
using Android.Gms.Maps;
using Android.Locations;
using Android.Gms.Maps.Model;
using CarShare.Models;
using Android.Database;
using static Android.Views.View;

namespace CarShare
{
    [Activity(Label = "ShowJourney")]
    public class ShowJourney : Activity, IOnMapReadyCallback
    {
        public static MobileServiceClient MobileService =
       new MobileServiceClient("https://c00197013.azurewebsites.net");

        static ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
        ISharedPreferencesEditor edit = pref.Edit();

        string UserID = pref.GetString("UserID", "");
        Journeys thisJourney;
        private GoogleMap _map;
        private MapFragment _mapFragment;
        LocationManager locMgr;
        MarkerOptions fromLocation;
        MarkerOptions toLocation;

        TextView tvFrom;
        TextView tvTo;
        TextView tvDate;
        TextView tvTime;
        TextView tvPassengerNumbers;
        ListView lvPassengers;
        ListView lstApplicants;
        Button btnApplicants;
        string[] passengers;
        List<UserProfiles> applicantsUserProfiles = new List<UserProfiles>();
        List<UserProfiles> passengersUserProfiles = new List<UserProfiles>();
        ArrayAdapter passengerAdapter;
        ArrayAdapter applicantsAdapter;
        List<string> passengerNames = new List<string>();
        List<string> applicantsNames = new List<string>();
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ShowJourney);
            string jID = pref.GetString("SelectedJourneyID", "");
            thisJourney = await DBHelper.GetJourneys(pref.GetString("SelectedJourneyID", ""));
            SetTextViewMessages();
            
            fromLocation = new MarkerOptions();
            fromLocation.SetTitle("From Here");
            fromLocation.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));
            fromLocation.SetPosition(new LatLng(thisJourney.FromLat,thisJourney.FromLon));

            toLocation = new MarkerOptions();
            toLocation.SetTitle("To Here");
            toLocation.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));
            toLocation.SetPosition(new LatLng(thisJourney.ToLat, thisJourney.ToLon));

            InitMapFragment();
            // Create your application here
        }
        private async void SetTextViewMessages()
        {
            tvFrom = FindViewById<TextView>(Resource.Id.textViewShowJourneyFrom);
            tvTo = FindViewById<TextView>(Resource.Id.textViewShowJourneyTo);
            tvDate = FindViewById<TextView>(Resource.Id.textViewShowJourneyDate);
            tvTime = FindViewById<TextView>(Resource.Id.textViewShowJourneyTime);
            tvPassengerNumbers = FindViewById<TextView>(Resource.Id.textViewShowPassengerNumbers);
            tvFrom.Text = "From: " + thisJourney.From;
            tvTo.Text = "To: " + thisJourney.To;
            tvDate.Text = thisJourney.DepartureDate;
            tvTime.Text = thisJourney.DepartureDateTime;
            passengers = thisJourney.Passengers.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in passengers)
            {
                if (s.Length > 10)
                {
                    passengersUserProfiles.Add(await DBHelper.GetUsersProfile(s));
                }
            }
            foreach (UserProfiles up in passengersUserProfiles)
            {
                passengerNames.Add(up.Firstname + " " + up.Lastname);

            }
            lvPassengers = FindViewById<ListView>(Resource.Id.listViewShowJourneyPassengers);
            passengerAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, passengerNames);
            lvPassengers.Adapter = passengerAdapter;
            tvPassengerNumbers.Text = "No of Passengers: " + passengerNames.Count() + "/" + thisJourney.NoOfPassengers;
            if (UserID == thisJourney.DriverID)
            {
                //show passengers
                //button checks applicants
                btnApplicants = FindViewById<Button>(Resource.Id.buttonShowJourneyApplicants);
                btnApplicants.Text = "Check Applications";
                btnApplicants.Click += ShowApplicants;

            }
        }
        private async void ShowApplicants(Object sender, EventArgs e)
        {
            if (thisJourney.Passengers.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries).Count() >= thisJourney.NoOfPassengers)
            {
                Toast.MakeText(ApplicationContext, "Car is full, cannot accept any more Applicants", ToastLength.Long).Show();
            }
            else
            {
                applicantsUserProfiles.Clear();
                applicantsNames.Clear();
                string[] appl = thisJourney.Applicants.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in appl)
                {
                    if (s.Length > 10)
                    {
                        applicantsUserProfiles.Add(await DBHelper.GetUsersProfile(s));
                    }
                }
                foreach (UserProfiles up in applicantsUserProfiles)
                {
                    applicantsNames.Add(up.Firstname + " " + up.Lastname);
                }
                lstApplicants = FindViewById<ListView>(Resource.Id.listViewShowJourneyApplicants);
                applicantsAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, applicantsNames);

                lstApplicants.Adapter = applicantsAdapter;
                lstApplicants.Visibility = ViewStates.Visible;
                lstApplicants.ItemClick += ListView_ItemClick;

            }

        }
        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var menu = new PopupMenu(this, lstApplicants.GetChildAt(e.Position - lstApplicants.FirstVisiblePosition));
            UserProfiles up = applicantsUserProfiles[e.Position];
            menu.Inflate(Resource.Layout.ApplicantsMenu);
            menu.MenuItemClick += (s, a) =>
            {
                switch (a.Item.ItemId)
                {
                    case Resource.Id.ApplicantAccept:
                        //Add Applicant to passengers for journey, increment num passengers by 1
                        if (passengerNames.Count() < thisJourney.NoOfPassengers)
                        {                          
                            thisJourney.Passengers += up.UsersID + ","; // userId
                            string[] appl = thisJourney.Applicants.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            thisJourney.Applicants = "";
                            if (thisJourney.Passengers.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries).Count() < thisJourney.NoOfPassengers)
                            {
                                foreach (string st in appl)
                                {
                                    if (st != up.UsersID)
                                    {
                                        thisJourney.Applicants += st + ",";
                                    }
                                }
                            }
                            
                            applicantsAdapter.Clear();
                            applicantsAdapter.NotifyDataSetChanged();
                            passengerNames.Add(up.Firstname + " " + up.Lastname);
                            passengerAdapter.Add(up.Firstname + " " + up.Lastname);
                            passengerAdapter.NotifyDataSetChanged();
                            lstApplicants.Visibility = ViewStates.Invisible;
                            tvPassengerNumbers.Text = "No of Passengers: " + passengerNames.Count() + "/" + thisJourney.NoOfPassengers;
                            //get applicants email and send them message
                            DBHelper.UpdateJourney(thisJourney);
                            DBHelper.SendApplicationEmail(up.Email, thisJourney);
                        }
                        break;
                    case Resource.Id.ApplicantRefuse:
                        string[] appl2 = thisJourney.Applicants.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        thisJourney.Applicants = "";
                        foreach (string st in appl2)
                        {
                            if (st != up.UsersID)
                            {
                                thisJourney.Applicants += st + ",";
                            }
                        }
                        applicantsAdapter.Clear();
                        applicantsAdapter.NotifyDataSetChanged();
                        DBHelper.UpdateJourney(thisJourney);
                        // cancel/delete this journey, if it has no passengers.
                        break;
                }
            };
            menu.Show();
        }
        private void SetupMapMarkers()
        {
            _map.AddMarker(fromLocation);
            _map.AddMarker(toLocation);
        }
        private void InitMapFragment()
        {
            _mapFragment = FragmentManager.FindFragmentByTag("mapFrameShowJourney") as MapFragment;
            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeNormal)
                    .InvokeZoomControlsEnabled(false)
                    .InvokeCompassEnabled(true)
                    .InvokeCamera(new CameraPosition(new LatLng(thisJourney.FromLat, thisJourney.FromLon), 11, 65, 0))
                    ;

                FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                _mapFragment = MapFragment.NewInstance(mapOptions);
                fragTx.Add(Resource.Id.mapFrameShowJourney, _mapFragment, "mapFrameShowJourney");
                fragTx.Commit();
            }
            _mapFragment.GetMapAsync(this);
        }
        public void OnMapReady(GoogleMap map)
        {
            _map = map;
            SetupMapMarkers();
        }
        protected override void OnResume()
        {
            base.OnResume();
            SetupMapIfNeeded();
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
        public LatLng getCurrentPosition()
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
        
    }
}