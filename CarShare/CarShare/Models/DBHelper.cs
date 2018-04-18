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
using System.Threading.Tasks;
using Android.Gms.Maps.Model;
using SendGrid;
using SendGrid.Helpers.Mail;
using Android.Content.Res;

namespace CarShare.Models
{
    public static class DBHelper
    {
        public static MobileServiceClient MobileService =
        new MobileServiceClient("https://c00197013.azurewebsites.net");

        public const string SendGridAPIKey = "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";
        public static async void InsertNewUser(Users u)
        {
            CurrentPlatform.Init();
            MobileService.GetTable<Users>().InsertAsync(u);
            
        }
        public static async void InsertNewUser(string userName, string password)
        {
            CurrentPlatform.Init();
            string hashedPassword = PasswordStorage.CreateHash(password);
            //CurrentPlatform.Init();
            Users u = new Users { Username = userName, Password = hashedPassword };
            MobileService.GetTable<Users>().InsertAsync(u);
        }
        public static async Task<Users> GetUser(string userName)
        {
            CurrentPlatform.Init();
            List<Users> ls = await MobileService.GetTable<Users>().ToListAsync();
            Users u = ls.FirstOrDefault(x => x.Username == userName);
            return u;
        }
        public static async Task<List<Users>> GetAllUsers()
        {
            CurrentPlatform.Init();
            List<Users> ls = await MobileService.GetTable<Users>().ToListAsync();
            return ls;
        }
        public static async Task<bool> DoesUserExist(string userName)
        {
            CurrentPlatform.Init();
            List<Users> ls = await MobileService.GetTable<Users>().ToListAsync();
            Users u = ls.FirstOrDefault(x => x.Username == userName);
            if (u != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static async void InsertUserProfile(UserProfiles up)
        {
            CurrentPlatform.Init();
            MobileService.GetTable<UserProfiles>().InsertAsync(up);
        }
        public static async void InsertUserProfile(string userID, string firstName, string lastName, string email, string phoneNo, string gender, string county)
        {
            CurrentPlatform.Init();
            UserProfiles up = new UserProfiles
            {
                UsersID = userID,
                Firstname = firstName,
                Lastname = lastName,
                Email = email,
                PhoneNo = phoneNo,
                Gender = gender,
                County = county
            };
            MobileService.GetTable<UserProfiles>().InsertAsync(up);
        }
        public static async Task<UserProfiles> GetUsersProfile(string userID)
        {
            CurrentPlatform.Init();
            List<UserProfiles> ls = await MobileService.GetTable<UserProfiles>().ToListAsync();
            UserProfiles u = ls.FirstOrDefault(x => x.UsersID == userID);
            return u;
        }
        public static async Task<bool> IsUserProfileCreated(string userID)
        {
            CurrentPlatform.Init();
            List<UserProfiles> ls = await MobileService.GetTable<UserProfiles>().ToListAsync();
            UserProfiles u = ls.FirstOrDefault(x => x.UsersID == userID);
            if (u != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static async void InsertJourney(Journeys j)
        {
            CurrentPlatform.Init();
            MobileService.GetTable<Journeys>().InsertAsync(j);
        }
        public static async void UpdateJourney(Journeys j)
        {
            CurrentPlatform.Init();
            MobileService.GetTable<Journeys>().UpdateAsync(j);
        }
        public static async Task<List<Journeys>> GetUsersJourneys(string userID)
        {
            CurrentPlatform.Init();
            IMobileServiceTable<Journeys> mTable = MobileService.GetTable<Journeys>();
            List<Journeys> items = await mTable
                .Where(x => x.CreatedBy == userID)
                .ToListAsync();

            if (items.Count > 0)
            {
                List<Journeys> result = new List<Journeys>();
                DateTime now = DateTime.Now;
                foreach (Journeys j in items)
                {
                    DateTime dep = DateTime.Parse(j.DepartureDate);
                    if (dep >= now)
                    {
                        result.Add(j);
                    }
                }
                return result;
            }
            else
            {
                return new List<Journeys>();
            }           
        }
        public static async Task<List<Journeys>> GetUsersOldJourneys(string userID)
        {
            CurrentPlatform.Init();
            IMobileServiceTable<Journeys> mTable = MobileService.GetTable<Journeys>();
            List<Journeys> items = await mTable
                .Where(x => x.CreatedBy == userID)
                .ToListAsync();
            if (items.Count > 0)
            {
                List<Journeys> result = new List<Journeys>();
                DateTime now = DateTime.Now;
                foreach (Journeys j in items)
                {
                    DateTime dep = DateTime.Parse(j.DepartureDate);
                    if (dep < now)
                    {
                        result.Add(j);
                    }
                }
                return result;
            }
            else
            {
                return new List<Journeys>();
            }
        }
        public static async Task<List<Journeys>> GetUsersPassengerJourneys(string userID)
        {
            CurrentPlatform.Init();
            IMobileServiceTable<Journeys> mTable = MobileService.GetTable<Journeys>();
            List<Journeys> items = await mTable
                .Where(x => x.Passengers.Contains(userID))
                .ToListAsync();

            if (items.Count > 0)
            {
                List<Journeys> result = new List<Journeys>();
                DateTime now = DateTime.Now;
                foreach (Journeys j in items)
                {
                    DateTime dep = DateTime.Parse(j.DepartureDate);
                    if (dep >= now)
                    {
                        result.Add(j);
                    }
                }
                return result;
            }
            else
            {
                return new List<Journeys>();
            }
        }
        public static async Task<List<Journeys>> GetUsersOldPassengerJourneys(string userID)
        {
            CurrentPlatform.Init();
            IMobileServiceTable<Journeys> mTable = MobileService.GetTable<Journeys>();
            List<Journeys> items = await mTable
                .Where(x => x.Passengers.Contains(userID))
                .ToListAsync();
            if (items.Count > 0)
            {
                List<Journeys> result = new List<Journeys>();
                DateTime now = DateTime.Now;
                foreach (Journeys j in items)
                {
                    DateTime dep = DateTime.Parse(j.DepartureDate);
                    if (dep < now)
                    {
                        result.Add(j);
                    }
                }
                return result;
            }
            else
            {
                return new List<Journeys>();
            }
        }

        public static async Task<Journeys> GetJourneys(string ID)
        {
            CurrentPlatform.Init();
            IMobileServiceTable<Journeys> mTable = MobileService.GetTable<Journeys>();
            List<Journeys> items = await mTable
                .Where(x => x.ID == ID)
                .ToListAsync();
            if (items.Count > 0)
            {
                return items[0];
            }
            else
            {
                return new Journeys();
            }
        }
        public static async void InsertSearch(Search s)
        {
            CurrentPlatform.Init();
            MobileService.GetTable<Search>().InsertAsync(s);
        }
        public static async Task<List<Journeys>>SearchJourneys(Search search)
        {
            LatLng maxFrom = LocationHelper.getMaxLatLng(new LatLng(search.FromLat, search.FromLon), search.Range);
            LatLng minFrom = LocationHelper.getMinLatLng(new LatLng(search.FromLat, search.FromLon), search.Range);
            LatLng maxTo = LocationHelper.getMaxLatLng(new LatLng(search.ToLat, search.ToLon), search.Range);
            LatLng minTo = LocationHelper.getMinLatLng(new LatLng(search.ToLat, search.ToLon), search.Range);
            CurrentPlatform.Init();
            IMobileServiceTable<Journeys> mTable = MobileService.GetTable<Journeys>();
            List<Journeys> items = await mTable
                .Where(x => x.FromLat <= maxFrom.Latitude && x.FromLon <= maxFrom.Longitude
                && x.FromLat >= minFrom.Latitude && x.FromLon >= minFrom.Longitude
                && x.ToLat <= maxTo.Latitude && x.ToLon <= maxTo.Longitude
                && x.ToLat >= minTo.Latitude && x.ToLon >= minTo.Longitude
                && x.Completed == false && x.Filled == false)
                .ToListAsync();
            if (items.Count > 0)
            {
                List<Journeys> result = new List<Journeys>();
                DateTime depDate = DateTime.Parse(search.DepartureDate);
                DateTime minTime = DateTime.Parse(search.MinDepartureDateTime);
                DateTime maxTime = DateTime.Parse(search.MaxDepartureDateTime);
                foreach (Journeys j in items)
                {
                    DateTime dep = DateTime.Parse(j.DepartureDate);
                    if (dep == depDate && minTime <= DateTime.Parse(j.DepartureDateTime) && maxTime >= DateTime.Parse(j.DepartureDateTime))
                    {
                        result.Add(j);
                    }
                }
                return result;
            }
            else
            {
                return new List<Journeys>();
            }
        }
        public static async void SendApplicationEmail(string recipient, Journeys j)
        {
            string message = string.Format("Hey your application to CarShare was accepted! \nDetails: \nFrom: {0}\nTo: {1}\nOn: {2}\nAt: {3}\n\nLog In to the app to view more details!", j.From, j.To, j.DepartureDate, j.DepartureDateTime);
            var client = new SendGridClient(SendGridAPIKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("do-not-reply@carsharing.com", "CAIRDE - Application"),
                Subject = "CarSharing Journey Application",
                PlainTextContent = message,
                HtmlContent = "<strong>"+ message.Replace("\n","<br />") + "</strong>"
            };
            msg.AddTo(new EmailAddress(recipient, "User"));
            var response = await client.SendEmailAsync(msg);
        }
        public static async void SendApplicationNoticeEmail(string recipient, Journeys j)
        {
            string message = string.Format("Hey your journey on the {2} has new applicants. \n\nDetails: \nFrom: {0}\nTo: {1}\nOn: {2}\nAt: {3}\n\nLog In to the app to review the applicants!", j.From, j.To, j.DepartureDate, j.DepartureDateTime);
            var client = new SendGridClient(SendGridAPIKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("do-not-reply@carsharing.com", "CAIRDE - New Applicants"),
                Subject = "CarSharing Journey Application",
                PlainTextContent = message,
                HtmlContent = "<strong>" + message.Replace("\n", "<br />") + "</strong>"
            };
            msg.AddTo(new EmailAddress(recipient, "User"));
            var response = await client.SendEmailAsync(msg);
        }
        public static async Task<bool> SetupDemoData() //This method will populate the database with test users, and test journeys, old and new etc
        {
            Random rnd = new Random();
            List<Users> demoUsers = new List<Users>();
            List<UserProfiles> demoProfiles = new List<UserProfiles>();
            #region setupUsers
            string password = PasswordStorage.CreateHash("Carshar1ng");
            string[] userNames = new string[30] {"Dylan","Paul","Evelin","Zoe","Tommy",
            "Carl","Derek","Carol","Dean","Erica",
            "Roisin","Laura","Ken","Ron","Ross",
            "Barbie","Sean","Dan","Rory","Rachael",
            "Phoebe","Dolores","Leo","Lucifer","Carlos",
            "Renee","Zed","Damien","Nicole","David"};
            foreach (string s in userNames)
            {
                Users user = new Users() { Username = s, Password = password };
                InsertNewUser(user);
            }
            foreach (string s in userNames)
            {
                Users user = await GetUser(s);
                demoUsers.Add(user);
            }
            #endregion
            #region setup Profiles
            string lastName = "TestUser";
            string email = "carsharing.user@gmail.com";
            string phone = "0857584241";
            string[] gender = new string[3] { "Male", "Female", "Prefer not to Disclose" };
            string[] county = new string[6] { "Carlow", "Dublin", "Cork", "Galway","Waterford","Limerick" };
            foreach (Users u in demoUsers)
            {
                UserProfiles userProfile = new UserProfiles()
                {
                    UsersID = u.ID,
                    Firstname = u.Username,
                    Lastname = lastName,
                    Email = email,
                    PhoneNo = phone,
                    Gender = gender[rnd.Next(0, gender.Length)],
                    County = county[rnd.Next(0, county.Length)]
                };
                InsertUserProfile(userProfile);
            }
            foreach (Users u in demoUsers)
            {
                UserProfiles up = await GetUsersProfile(u.ID);
                demoProfiles.Add(up);
            }
            #endregion
            #region Create Journeys
            int i;
            LatLng[] locations = new LatLng[6]
            {
                new LatLng(52.836438, -6.932914),
                new LatLng(53.347548, -6.259539),
                new LatLng(51.891959, -8.483876),
                new LatLng(53.269885, -9.056660),
                new LatLng(52.261530, -7.111773),
                new LatLng(52.667267, -8.631729)
            };
            DateTime[] depDates = new DateTime[11]
            {
                DateTime.Now.AddDays(-1),
                DateTime.Now,
                DateTime.Now.AddDays(1),
                DateTime.Now.AddDays(2),
                DateTime.Now.AddDays(3),
                DateTime.Now.AddDays(4),
                DateTime.Now.AddDays(5),
                DateTime.Now.AddDays(6),
                DateTime.Now.AddDays(7),
                DateTime.Now.AddDays(8),
                DateTime.Now.AddDays(9)
            };
            DateTime baseTime = new DateTime(2018, 4, 4, 7, 0, 0);
            DateTime[] depTimes = new DateTime[10]
            {
                baseTime,
                baseTime.AddMinutes(30),
                baseTime.AddMinutes(60),
                baseTime.AddMinutes(90),
                baseTime.AddMinutes(120),
                baseTime.AddMinutes(150),
                baseTime.AddMinutes(180),
                baseTime.AddMinutes(600),
                baseTime.AddMinutes(660),
                baseTime.AddMinutes(720)
            };
            for(i = 0; i < 1000 ; i++)
            {
                LatLng loc = locations[rnd.Next(0, 6)];
                LatLng des = locations[rnd.Next(0, 6)];
                string user = demoUsers[rnd.Next(0, demoUsers.Count)].ID;
                while (des.Latitude == loc.Latitude)
                {
                    des = locations[rnd.Next(0, 6)];
                }
                Journeys randJourney = new Journeys()
                {
                    CreatedBy = user,
                    From = LocationHelper.ReverseGeoLoc(loc),
                    To = LocationHelper.ReverseGeoLoc(des),
                    FromLat = loc.Latitude,
                    ToLat = des.Latitude,
                    FromLon = loc.Longitude,
                    ToLon = des.Longitude,
                    DriverID = user,
                    NoOfPassengers = rnd.Next(1, 5),
                    DepartureDate = depDates[rnd.Next(0, depDates.Count())].ToShortDateString(),
                    DepartureDateTime = depTimes[rnd.Next(0, depTimes.Count())].ToShortTimeString(),
                    Filled = false,
                    Completed = false,
                    Applicants = "",
                    Passengers = ""
                };
                InsertJourney(randJourney);
            }
            #endregion

            return true;

        }
    }
}