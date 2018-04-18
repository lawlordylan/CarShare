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
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Android.Text;
using Microsoft.WindowsAzure.MobileServices;
using CarShare.Models;

namespace CarShare
{
    [Activity(Label = "SetUpProfileActivity")]
    public class SetUpProfileActivity : Activity
    {

        public static MobileServiceClient MobileService =
        new MobileServiceClient("https://c00197013.azurewebsites.net");

        static ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
        ISharedPreferencesEditor edit = pref.Edit();

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ProfileSetUp);
            #region setUpDropdowns
            Spinner genderSelect = FindViewById<Spinner>(Resource.Id.gender);

            //genderSelect.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.gender, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            genderSelect.Adapter = adapter;
            Spinner countySelect = FindViewById<Spinner>(Resource.Id.county);

            //genderSelect.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var countyAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.county, Android.Resource.Layout.SimpleSpinnerItem);

            countyAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            countySelect.Adapter = countyAdapter;
            #endregion

            var saveButton = FindViewById(Resource.Id.trySaveButton);
            saveButton.Click += ValidateForm;
            //Create your application here
        }
        private async void TrySave()
        {
            ProgressDialog progress;
            progress = new Android.App.ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(Android.App.ProgressDialogStyle.Spinner);
            progress.SetMessage("Logging In... Please wait...");
            progress.SetCancelable(false);
            progress.Show();
            CurrentPlatform.Init();
            EditText firstName = (EditText)FindViewById(Resource.Id.firstName);
            EditText lastName = (EditText)FindViewById(Resource.Id.lastName);
            EditText email = (EditText)FindViewById(Resource.Id.email);
            EditText phoneNo = (EditText)FindViewById(Resource.Id.phoneNo);
            Spinner gender = (Spinner)FindViewById(Resource.Id.gender);
            Spinner county = (Spinner)FindViewById(Resource.Id.county);
            UserProfiles newUserInfo = new UserProfiles { UsersID = pref.GetString("UserID","NULL"), Firstname = firstName.Text,
            Lastname = lastName.Text, Email = email.Text, PhoneNo = phoneNo.Text, Gender = gender.SelectedItem.ToString(),
             County = county.SelectedItem.ToString()};
            MobileService.GetTable<UserProfiles>().InsertAsync(newUserInfo);
            progress.Hide();
            Toast.MakeText(ApplicationContext, "User " + pref.GetString("UserName", "NULL") + " info created!", ToastLength.Short).Show();
        }
        private void ValidateForm(object sender, EventArgs e)
        {
            EditText firstName = (EditText)FindViewById(Resource.Id.firstName);
            EditText lastName = (EditText)FindViewById(Resource.Id.lastName);
            EditText email = (EditText)FindViewById(Resource.Id.email);
            EditText phoneNo = (EditText)FindViewById(Resource.Id.phoneNo);
            Spinner gender = (Spinner)FindViewById(Resource.Id.gender);
            Spinner county = (Spinner)FindViewById(Resource.Id.county);
            string namePattern = "[^a-zA-Z]";
            string phonePattern = "[^0-9+]";
            bool validInput = false;
            #region fieldValidation
            if (firstName.Text == "")
            {
                firstName.Error = "Cannot be Empty";
                firstName.RequestFocus();
            }
            else if (lastName.Text == "")
            {
                lastName.Error = "Cannot be Empty";
                lastName.RequestFocus();
            }
            else if (phoneNo.Text == "")
            {
                phoneNo.Error = "Cannot be Empty";
                phoneNo.RequestFocus();
            }
            else if (email.Text == "")
            {
                email.Error = "Cannot be Empty";
                email.RequestFocus();
            }
            else if (Regex.IsMatch(firstName.Text, namePattern))
            {
                firstName.Error = "Invalid characters in name";
                firstName.RequestFocus();
            }
            else if (Regex.IsMatch(lastName.Text, namePattern))
            {
                lastName.Error = "Invalid characters in name";
                lastName.RequestFocus();
            }
            else if (Regex.IsMatch(phoneNo.Text, phonePattern))
            {
                phoneNo.Error = "Invalid phone number. Use numbers (0-9) only";
                phoneNo.RequestFocus();
            }
            else if (!isValidEmail(email.Text))
            {
                email.Error = "Invalid email address";
                email.RequestFocus();
            }
            else
            {
                validInput = true;
            }
            #endregion
            if(validInput)
            {
                edit.PutString("FirstName", firstName.Text);
                edit.PutString("LastName", lastName.Text);
                edit.Commit();
                TrySave();
                StartActivity(typeof(MainProfileActivity));
            }
        }
        public static bool isValidEmail(string inputEmail)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(inputEmail))
                return (true);
            else
                return (false);
        }
    }
}