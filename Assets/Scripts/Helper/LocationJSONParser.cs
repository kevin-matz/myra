using System.Collections;
using System.Collections.Generic;
using System.IO;
using Helper;
using Niantic.Lightship.Maps.Coordinates;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class LocationJsonParser : MonoBehaviour
{

    public string filePath;
    public void Save()
    {
        var report = new LocationReport();
        report.name = "Ralle";
        report.coords = new SerializableLatLng(latitude: 50.1, longitude: 11.4);
        report.description = "Jetzt n Schnitzel dit wär was!";
        var json = JsonUtility.ToJson(report);
        Debug.Log(json);
        filePath = Path.Combine(Application.dataPath, "report.json");
        Debug.Log(filePath);
        File.WriteAllText(filePath, json);
        Debug.Log(filePath);
    }

    public void Load()
    {
        filePath = Path.Combine(Application.dataPath, "report.json");
        Debug.Log(filePath);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            var report = JsonUtility.FromJson<LocationReport>(json);
            Debug.Log(report.name);
        } else Debug.Log("Datei nicht da :(");
    }
}
