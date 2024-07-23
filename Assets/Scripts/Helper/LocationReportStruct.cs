using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Niantic.Lightship.Maps.Coordinates;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Helper
{
    [Serializable]
    public struct LocationReport
    {
        public string name;
        public SerializableLatLng coords;
        public string description;
        public string author;
        public string date;
        public bool demo;

        public static LocationReport[] Locations;
        public static LocationReport[] DemoReports;
        
        public static void SaveDemoReport(LocationReport demoReport)
        {
            var demoList = DemoReports == null ? new List<LocationReport>() : DemoReports.ToList();
            demoList.Add(demoReport);
            var reports = new LocationReportArray()
            {
                LocationReports = demoList.ToArray()
            };
            var json = JsonUtility.ToJson(reports);
            var path = Path.Combine(Application.persistentDataPath, "demo_locations.json");
            Debug.Log(path);
            File.WriteAllText(path, json);
        }

        public static LocationReport[] LoadDemoReports()
        {
            var path = Path.Combine(Application.persistentDataPath, "demo_locations.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var demoReportArray = JsonUtility.FromJson<LocationReportArray>(json);
                DemoReports = demoReportArray.LocationReports;
            }
            else
            {
                Debug.Log("Demo Datei nicht da :(");
                DemoReports = null;
            }

            return DemoReports;
        }
        
        public static IEnumerator LoadLocations(Action<LocationReport[]> callback)
        {
            var filePath = Path.Combine(Application.streamingAssetsPath, "locations.json");

            // Wenn Plattform Android, UnityWebRequest verwenden
            if (Application.platform == RuntimePlatform.Android)
            {
                using (UnityWebRequest www = UnityWebRequest.Get(filePath))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError(www.error);
                        Locations = null;
                    }
                    else
                    {
                        var json = www.downloadHandler.text;
                        var locationReportArray = JsonUtility.FromJson<LocationReportArray>(json);
                        Locations = locationReportArray.LocationReports;
                    }
                }
            }
            else
            {
                // Für andere Plattformen direktes Lesen verwenden
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var locationReportArray = JsonUtility.FromJson<LocationReportArray>(json);
                    Locations = locationReportArray.LocationReports;
                }
                else
                {
                    Debug.Log("Datei nicht da :(");
                    Locations = null;
                }
            }
            callback?.Invoke(Locations);
        }

        public static LocationReport FindLocation(string name)
        {
            return Locations.First(location => location.name == name);
        }
    }

    
    public struct LocationReportArray
    {
        public LocationReport[] LocationReports;
    }
    
    
}
