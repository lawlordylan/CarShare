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
    public class Search
    {
        public string ID { get; set; }
        public string CreatedBy { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public double FromLat { get; set; }
        public double FromLon { get; set; }
        public double ToLat { get; set; }
        public double ToLon { get; set; }
        public double Range { get; set; }
        public string MaxDepartureDateTime { get; set; }
        public string MinDepartureDateTime { get; set; }
        public string DepartureDate { get; set; }
        public string Matched { get; set; }
    }
}