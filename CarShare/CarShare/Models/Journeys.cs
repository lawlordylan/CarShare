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
using Android.Gms.Maps.Model;

namespace CarShare.Models
{
    public class Journeys
    {
        public string ID { get; set; }
        public string CreatedBy { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public double FromLat { get; set; }
        public double FromLon { get; set; }
        public double ToLat { get; set; }
        public double ToLon { get; set; }
        public string DriverID { get; set; }
        public int NoOfPassengers { get; set; }
        public string Passengers { get; set; }
        public string DepartureDateTime { get; set; }
        public string DepartureDate { get; set; }
        public string Applicants { get; set; }
        public bool Filled { get; set; }
        public bool Completed { get; set; }
    }
}