using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System;
using System.Data.SqlClient;
using Microsoft.WindowsAzure.MobileServices;
using CarShare.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Android.Content;
using Android.Gms.Maps;
using Android.Util;

namespace CarShare
{
    [Activity(Label = "CarShare", MainLauncher = true)]
    public class MainActivity : Activity
    {
        static ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
        ISharedPreferencesEditor edit = pref.Edit(); //out to session helper? future problem.
        public const string TAG = "MainActivity";

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            MapsInitializer.Initialize(ApplicationContext);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            EditText username = (EditText)FindViewById(Resource.Id.username);
            EditText password = (EditText)FindViewById(Resource.Id.passwordInput);
            username.Text = pref.GetString("UserName", "");
            password.Text = pref.GetString("Password", "");
            var login = FindViewById(Resource.Id.loginButton);
            login.Click += TryLoginAsync;
            //if(pref.GetString("LoggedIn","false") == "true")
            //{
            //    StartActivity(typeof(SetUpProfileActivity));
            //}
            var register = FindViewById(Resource.Id.registerButton);
            register.Click += GoToRegister;
            var tester = FindViewById(Resource.Id.testButton);
            tester.Click += GoToTest;



        }

        private async void TryLoginAsync(object sender, EventArgs e)
        {
            ProgressDialog progress;
            progress = new Android.App.ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(Android.App.ProgressDialogStyle.Spinner);
            progress.SetMessage("Logging In... Please wait...");
            progress.SetCancelable(false);
            progress.Show();
            EditText username = (EditText)FindViewById(Resource.Id.username);
            EditText password = (EditText)FindViewById(Resource.Id.passwordInput);
            string user = username.Text.Trim();
            string passwordInput = password.Text;
            
            if (!await DBHelper.DoesUserExist(user))
            {
                progress.Hide();
                Toast.MakeText(ApplicationContext, "Invalid Login", ToastLength.Short).Show();
            }
            else
            {
                Users u = await DBHelper.GetUser(user);
                if(PasswordStorage.VerifyPassword(passwordInput, u.Password))
                {                  
                    edit.PutString("UserID", u.ID);
                    edit.PutString("UserName", u.Username);
                    edit.PutString("LoggedIn", "true");
                    edit.PutString("Password", passwordInput); 
                    edit.Commit();
                                  
                    Toast.MakeText(ApplicationContext, "Success", ToastLength.Short).Show();
                    if (await DBHelper.IsUserProfileCreated(u.ID))
                    {
                        //go to main menu
                        UserProfiles up = await DBHelper.GetUsersProfile(u.ID);
                        edit.PutString("FirstName", up.Firstname);
                        edit.PutString("LastName", up.Lastname);
                        edit.Commit();
                        progress.Hide();
                        StartActivity(typeof(MainProfileActivity));
                    }
                    else
                    {
                        progress.Hide();
                        StartActivity(typeof(SetUpProfileActivity));
                    }
                    
                }
                else
                {
                    progress.Hide();
                    Toast.MakeText(ApplicationContext, "Invalid Login", ToastLength.Short).Show();
                }
            }
        }

        private void GoToRegister(object sender, EventArgs e)
        {
            StartActivity(typeof(RegisterActivity));
        }

        private async void GoToTest(object sender, EventArgs e)
        {
            //await DBHelper.GetUsersPassengerJourneys("");
            //ProgressDialog progress;
            //progress = new Android.App.ProgressDialog(this);
            //progress.Indeterminate = true;
            //progress.SetProgressStyle(Android.App.ProgressDialogStyle.Spinner);
            //progress.SetMessage("Creating Demo Data...");
            //progress.SetCancelable(false);
            //progress.Show();
            //bool success = await DBHelper.SetupDemoData();
            //progress.Hide();
            //if (success)
            //{
            //    Toast.MakeText(ApplicationContext, "Data Created", ToastLength.Short).Show();
            //}         
        }







    }
}

