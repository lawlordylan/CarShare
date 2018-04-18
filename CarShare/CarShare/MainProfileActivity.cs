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
using System.IO;
using Android.Graphics.Drawables;

namespace CarShare
{
    [Activity(Label = "MainProfileActivity")]
    public class MainProfileActivity : Activity
    {
        static ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
        ISharedPreferencesEditor edit = pref.Edit(); //out to session helper? future problem.
        string firstName = pref.GetString("FirstName", "User");
        string lastName = pref.GetString("LastName", "");

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MainProfile);

            SetupProfileImage();
            SetupButtons();
            
            // Create your application here

        }

        protected override void OnResume()
        {
            base.OnResume();       
        }
        private void SetupButtons()
        {
            Button driverUpcoming = FindViewById<Button>(Resource.Id.buttonDriverUpcomingJourney);
            driverUpcoming.Click += DriverUpcoming_Click;
            Button driverNewJourney = FindViewById<Button>(Resource.Id.buttonDriverCreateJourney);
            driverNewJourney.Click += DriverNewJourney_Click;
            Button driverOldJourneys = FindViewById<Button>(Resource.Id.buttonDriverCompletedJourney);
            driverOldJourneys.Click += DriverOldJourneys_Click;
            Button passengerUpcoming = FindViewById<Button>(Resource.Id.buttonPassengerUpcomingJourneys);
            passengerUpcoming.Click += PassengerUpcoming_Click;
            Button passengerSearch = FindViewById<Button>(Resource.Id.buttonPassengerSearchJourney);
            passengerSearch.Click += PassengerSearch_Click;
            Button passengerOldJourneys = FindViewById<Button>(Resource.Id.buttonPassengerCompletedJourney);
            passengerOldJourneys.Click += PassengerOldJourneys_Click;
            Button logOut = FindViewById<Button>(Resource.Id.buttonLogOut);
            logOut.Click += LogOut_Click;
        }

        private void LogOut_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainActivity));
        }

        private void PassengerOldJourneys_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(ViewOldPassengerJourneysActivity));
        }

        private void PassengerSearch_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(SearchJourney));
        }

        private void PassengerUpcoming_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(ViewPassengerJourneysActivity));
        }

        private void DriverOldJourneys_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(DriverOldJourneysActivity));
        }

        private void DriverNewJourney_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(CreateJourneyActivity));
        }

        private void DriverUpcoming_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(DriverJourneysActivity));
        }

        private void SetupProfileImage()
        {
            string profileIcon = GetProfileIcon(firstName);
            ImageView profileImage = FindViewById<ImageView>(Resource.Id.imageViewProfilePic);
            Stream ims = Assets.Open(profileIcon);
            Drawable d = Drawable.CreateFromStream(ims, null);
            profileImage.SetImageDrawable(d);
        }

        private string GetProfileIcon(string username)
        {
            char c = username.ToUpper()[0];
            return "profileIcons/" + c + ".png";

        }
    }
}