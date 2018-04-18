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
using System.Xml;
using Android.Gms.Maps.Model;
using Android.Locations;

namespace CarShare.Models
{
    public class LocationHelper : Activity
    {
        const double pi = Math.PI;
        const double R = 6371.0;
        const double Lat2KmRatio = 0.008993215;
        const double Lon2KmRatio = 0.014607411;

        public static string ReverseGeoLoc(double latitude, double longitude)
        {          
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load("http://maps.googleapis.com/maps/api/geocode/xml?latlng=" + latitude + "," + longitude + "&sensor=false");
                XmlNode element = doc.SelectSingleNode("//GeocodeResponse/status");
                if (element.InnerText == "ZERO_RESULTS")
                {
                    return ("No data available for the specified location");
                }
                else if (element.InnerText != "OK")
                {
                    return ReverseGeoLoc(latitude, longitude);
                }
                else
                {
                    element = doc.SelectSingleNode("//GeocodeResponse/result/formatted_address");
                }
                
                return (element.InnerText);                
            }
            catch (Exception ex)
            {
                return ("(Address lookup failed: ) " + ex.Message);
            }
        }
        public static string ReverseGeoLoc(LatLng location)
        {
            double latitude = location.Latitude;
            double longitude = location.Longitude;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load("http://maps.googleapis.com/maps/api/geocode/xml?latlng=" + latitude + "," + longitude + "&sensor=false");
                XmlNode element = doc.SelectSingleNode("//GeocodeResponse/status");
                if (element.InnerText == "ZERO_RESULTS")
                {
                    return ("No data available for the specified location");
                }
                else if (element.InnerText != "OK")
                {
                    return ReverseGeoLoc(latitude, longitude);
                }
                else
                {
                    element = doc.SelectSingleNode("//GeocodeResponse/result/formatted_address");
                }

                return (element.InnerText);
            }
            catch (Exception ex)
            {
                return ("(Address lookup failed: ) " + ex.Message);
            }
        }
        public static LatLng GetLatLngFromString(string l)
        {
            string latlng = l.Substring(l.IndexOf('('));
            latlng = latlng.TrimEnd(')');
            latlng = latlng.TrimStart('(');
            string[] laln = latlng.Split(',');
            return new LatLng(Convert.ToDouble(laln[0]), Convert.ToDouble(laln[1]));
        }
        public static double GetDistance(LatLng from, LatLng to)
        {
            double dLat = degree2Rad(to.Latitude - from.Latitude);  // deg2rad below
            double dLon = degree2Rad(to.Longitude - from.Longitude);
            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(degree2Rad(from.Latitude)) * Math.Cos(degree2Rad(to.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                ;
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = R * c; // Distance in km


            return d;
        }
        public static double GetDistance(string from, string to)
        {
            LatLng from2 = GetLatLngFromString(from);
            LatLng to2 = GetLatLngFromString(to);          
            
            double dLat = degree2Rad(to2.Latitude - from2.Latitude);  // deg2rad below
            double dLon = degree2Rad(to2.Longitude - from2.Longitude);
            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(degree2Rad(from2.Latitude)) * Math.Cos(degree2Rad(to2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                ;
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = R * c; // Distance in km 
            
            
            return d;
        }
        public static double degree2Rad(double d)
        {
            return d * (pi / 180);
        }
        public static LatLng getMaxLatLng(LatLng source, double distance)
        {
            double newLatitude = source.Latitude + (distance * Lat2KmRatio);
            double newLongitude = source.Longitude + (distance * Lon2KmRatio);
            return new LatLng(newLatitude, newLongitude);
        }
        public static LatLng getMinLatLng(LatLng source, double distance)
        {
            double newLatitude = source.Latitude - (distance * Lat2KmRatio);
            double newLongitude = source.Longitude - (distance * Lon2KmRatio);
            return new LatLng(newLatitude, newLongitude);
        }

        

    }
}