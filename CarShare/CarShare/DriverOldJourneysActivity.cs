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
using CarShare.Models;
using Android.Gms.Maps.Model;
using Android.Gms.Maps;

namespace CarShare
{
    [Activity(Label = "DriverOldJourneysActivity")]
    public class DriverOldJourneysActivity : Activity
    {
        static ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
        ISharedPreferencesEditor edit = pref.Edit(); //out to session helper? future problem.

        ListView lv;
        List<Journeys> userJourneys;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DriversOldJourneys);
            userJourneys = await DBHelper.GetUsersOldJourneys(pref.GetString("UserID", ""));
            string[] items;
            if (userJourneys.Count > 0)
            {
                items = GetJourneyDetails(userJourneys);
            }
            else
            {
                items = new string[1] { "No Old Journeys" };
            }

            IListAdapter adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, items);
            lv = FindViewById<ListView>(Resource.Id.listViewOldDriverJourneys);
            lv.Adapter = adapter;
            lv.ItemClick += ListView_ItemClick;
            Button backBtn = FindViewById<Button>(Resource.Id.buttonDriverOldJourneyBack);
            backBtn.Click += (sender, e) => { Finish(); };
            // Create your application here
        }

        private string[] GetJourneyDetails(List<Journeys> j)
        {
            List<string> items = new List<string>();
            foreach (Journeys i in j)
            {
                
                string d = Convert.ToDateTime(i.DepartureDate).DayOfWeek.ToString();
                string dd = Convert.ToDateTime(i.DepartureDate).Day.ToString();
                string m = Convert.ToDateTime(i.DepartureDate).Month.ToString();
                string y = Convert.ToDateTime(i.DepartureDate).Year.ToString();
                string from = i.From;
                string to = i.To;
                string details = String.Format("{0} {6}/{1}/{2} {3} \n\nFrom: {4} \nTo: {5}\n\n", d, m, y, i.DepartureDateTime, from, to, dd);
                items.Add(details);
            }
            return items.ToArray();
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var menu = new PopupMenu(this, lv.GetChildAt(e.Position));
            menu.Inflate(Resource.Layout.JourneyMenu);
            menu.MenuItemClick += (s, a) =>
            {
                switch (a.Item.ItemId)
                {
                    case Resource.Id.pop_button1:
                        Journeys j = userJourneys[e.Position];
                        edit.PutString("SelectedJourneyID", j.ID);
                        edit.Commit();
                        StartActivity(typeof(ShowJourney));
                        // goto details for selected journey
                        break;
                    case Resource.Id.pop_button2:
                        // cancel/delete this journey, if it has no passengers.
                        break;
                }
            };
            menu.Show();
        }
    }
}