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
using Newtonsoft.Json;

namespace CarShare
{
    [Activity(Label = "SearchJourneyResultsActivity", NoHistory = true)]
    public class SearchJourneyResultsActivity : Activity
    {
        static ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
        ISharedPreferencesEditor edit = pref.Edit(); //out to session helper? future problem.

        ListView lv;
        List<Journeys> userJourneys;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SearchJourneysResults);
            string jsonJourney = pref.GetString("SearchResults", "");
            userJourneys = JsonConvert.DeserializeObject<List<Journeys>>(jsonJourney);
            string[] items = new string[1];
            if (userJourneys.Count > 0)
            {
                items = GetJourneyDetails(userJourneys);
            }
            else
            {
                items[0] = "No Upcomin Journeys Found";
            }

            IListAdapter adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, items);
            lv = FindViewById<ListView>(Resource.Id.listViewSearchJourneyResults);
            lv.Adapter = adapter;
            lv.ItemClick += ListView_ItemClick;
            Button backBtn = FindViewById<Button>(Resource.Id.buttonSearchJourneyResultsBack);
            backBtn.Click += (sender, e) => { Finish(); };
            // Create your application here
        }

        private string[] GetJourneyDetails(List<Journeys> j)
        {
            List<string> items = new List<string>();
            foreach (Journeys i in j)
            {
                if (DateTime.Parse(i.DepartureDate) >= DateTime.Now && i.Filled == false)
                {
                    string d = Convert.ToDateTime(i.DepartureDate).DayOfWeek.ToString();
                    string dd = Convert.ToDateTime(i.DepartureDate).Day.ToString();
                    string m = Convert.ToDateTime(i.DepartureDate).Month.ToString();
                    string y = Convert.ToDateTime(i.DepartureDate).Year.ToString();
                    string from = i.From;
                    string to = i.To;

                    string details = String.Format("{0} {6}/{1}/{2} {3} \nFrom: {4} \nTo: {5}", d, m, y, i.DepartureDateTime, from, to, dd);
                    items.Add(details);
                }
                
            }
            if (items.Count > 0)
            {
                return items.ToArray();
            }
            else
            {
                return new string[1] { "No Upcomin Journeys Found" };
            }
            
        }

        private async void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var menu = new PopupMenu(this, lv.GetChildAt(e.Position - lv.FirstVisiblePosition));
            menu.Inflate(Resource.Layout.SearchJourneyMenu);
            Journeys j = userJourneys[e.Position];
            menu.MenuItemClick += async (s, a) =>
            {
                switch (a.Item.ItemId)
                {
                    case Resource.Id.pop_buttonSearchApply:
                        // Add user to applicants for journey
                        
                        j.Applicants += pref.GetString("UserID", "")+",";
                        DBHelper.UpdateJourney(j);
                        UserProfiles up = await DBHelper.GetUsersProfile(j.DriverID);
                        DBHelper.SendApplicationNoticeEmail(up.Email, j);
                        StartActivity(typeof(MainProfileActivity));
                        
                        break;
                    case Resource.Id.pop_buttonSearchView:
                        edit.PutString("SelectedJourneyID", j.ID);
                        edit.Commit();
                        StartActivity(typeof(ShowJourney));
                        break;
                }
            };
            menu.Show();
        }
    }
}