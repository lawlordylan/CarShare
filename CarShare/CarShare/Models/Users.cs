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

namespace CarShare.Models
{
    public class Users
    {
        public string ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }        
    }
}