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
    [Activity(Label = "RegisterActivity")]
    public class RegisterActivity : Activity
    {
        public static MobileServiceClient MobileService =
        new MobileServiceClient("https://c00197013.azurewebsites.net");

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Register);
            // Create your application here
            var register = FindViewById(Resource.Id.tryRegisterButton);
            register.Click += TryRegister;
            EditText username = (EditText)FindViewById(Resource.Id.usernameReg);
            username.TextChanged += ValidateUserInput;
            EditText password = (EditText)FindViewById(Resource.Id.passwordInput1);
            password.TextChanged += ValidateUserInput;
            EditText confirmPass = (EditText)FindViewById(Resource.Id.passwordInput2);
            confirmPass.TextChanged += ValidateUserInput;            
        }

        private async void TryRegister(object sender, EventArgs e)
        {
            ProgressDialog progress;
            progress = new Android.App.ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(Android.App.ProgressDialogStyle.Spinner);
            progress.SetMessage("Registering... Please wait...");
            progress.SetCancelable(false);
            progress.Show();
            EditText username = (EditText)FindViewById(Resource.Id.usernameReg);
            EditText password = (EditText)FindViewById(Resource.Id.passwordInput1);
            EditText confirmPassword = (EditText)FindViewById(Resource.Id.passwordInput2);

            if (password.Text == confirmPassword.Text)
            {
                string hashedPassword = PasswordStorage.CreateHash(password.Text);
                string userName = username.Text.Trim();
                //CurrentPlatform.Init();
                Users newUser = new Users { Username = userName, Password = hashedPassword };
                List<Users> allUsers = await MobileService.GetTable<Users>().ToListAsync();
                Users u = allUsers.FirstOrDefault(x => x.Username == newUser.Username);
                if (u == null)
                {
                    DBHelper.InsertNewUser(newUser);
                    //MobileService.GetTable<Users>().InsertAsync(newUser);
                    progress.Hide();
                    Toast.MakeText(ApplicationContext, "User " + newUser.Username + " created! You can now log in!", ToastLength.Short).Show();
                    StartActivity(typeof(MainActivity));
                }
                else
                {
                    progress.Hide();
                    Toast.MakeText(ApplicationContext, "User " + u.Username + " already exists!", ToastLength.Short).Show();
                }                
            }
            else
            {
                progress.Hide();
                string message = "Passwords don't match.";
                Toast.MakeText(ApplicationContext, message, ToastLength.Short).Show();
            }           
        }

        private void ValidateUserInput(object sender, TextChangedEventArgs e)
        {
            EditText input = (EditText)sender;
            string pattern = "[^a-zA-Z0-9#?]";
            if (Regex.IsMatch(input.Text, pattern))
            {
                string message = "Input must only contain alphabetic characters, numbers, ? and #";
                Toast.MakeText(ApplicationContext, message, ToastLength.Short).Show();
                input.Text = "";
            }
        }
    }
}