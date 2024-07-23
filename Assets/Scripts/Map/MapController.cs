// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections;
using System.IO;
using Helper;
using Niantic.Lightship.Maps;
using Niantic.Lightship.Maps.Coordinates;
using Niantic.Lightship.Maps.Core.Coordinates;
using Niantic.Lightship.Maps.MapLayers.Components;
using Niantic.Lightship.Maps.ObjectPools;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif



namespace Map
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] 
        private LocationReport[] locations;
        
        [SerializeField] 
        private LocationReport[] demoReports;
    
        [SerializeField]
        private LightshipMapView lightshipMapView;
    
        [SerializeField] 
        private GameObject modal;
        
        [SerializeField] 
        private GameObject demoModal;
        
        [SerializeField]
        private LayerGameObjectPlacement astroSpawner;
        
        [SerializeField]
        private LayerGameObjectPlacement flagSpawner;
    
        [SerializeField]
        private SerializableLatLng testLocation;
    
        private double _lastGpsUpdateTime;

        private LatLng _location;
        private PooledObject<GameObject> _actualAstroBoy = new PooledObject<GameObject>();
        private double _defaultMapRadius;
        
        public Action<string> OnGpsError;
        
        

        private static bool IsLocationServiceInitializing
            => Input.location.status == LocationServiceStatus.Initializing;

        

        private void Start()
        {
            _defaultMapRadius = lightshipMapView.MapRadius;
            StartCoroutine(LocationReport.LoadLocations(LocationSetup));
        }

        private void LocationSetup(LocationReport[] loadedLocations)
        {
            if (loadedLocations == null)
            {
                demoModal.SetActive(true);
                demoModal.GetComponent<DemoHandler>().SetupDemoModal("Fehler","Die Demo Meldungen konnten nicht geladen werden.");
            }
            locations = loadedLocations;
            
            demoReports = LocationReport.LoadDemoReports();
            
            
            if (Application.isEditor)
            {
                _location = testLocation;
                MadeMapReady();
            }
            else
            {
                StartCoroutine(UpdateGpsLocation());
            }
        }

        private void MadeMapReady()
        {
            //Map an Pos Laden
            lightshipMapView.SetMapCenter(_location);
            
            //Flags Spawnen
            if(locations != null) foreach (var location in locations)
            {
                var bialetti = flagSpawner.PlaceInstance(location.coords, Quaternion.identity).Value.GetComponent<FlagHandler>();
                bialetti.name = location.name;
                bialetti.location = location;
                bialetti.modal = modal;
                if(location.demo) bialetti.IsADemoFlag();
            }
            
            //DemoFlags Spawnen
            if(demoReports != null) foreach (var report in demoReports)
            {
                var trex = flagSpawner.PlaceInstance(report.coords, Quaternion.identity).Value.GetComponent<FlagHandler>();
                trex.name = report.name;
                trex.location = report;
                trex.modal = modal;
                trex.IsADemoFlag();
            }
            
            //Astro Boi Spawnen
            _actualAstroBoy = astroSpawner.PlaceInstance(Vector3.zero);
        }

        public void AstroLocalization()
        {
            lightshipMapView.SetMapCenter(_location);
            lightshipMapView.SetMapRadius(_defaultMapRadius);
            if (Camera.main != null) Camera.main.orthographicSize = (float)_defaultMapRadius;
        }
        

        
        private IEnumerator UpdateGpsLocation()
        {
        
            yield return null;

        
#if UNITY_ANDROID
            // Request location permission for Android
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
                while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                {
                    // Wait until permission is enabled
                    yield return new WaitForSeconds(1.0f);
                }
            }
#endif
            // Check if the user has location service enabled.
            if (!Input.location.isEnabledByUser)
            {
                OnGpsError?.Invoke("Location permission not enabled");
                yield break;
            }

            // Starts the location service.
            Input.location.Start();

            // Waits until the location service initializes
            int maxWait = 20;
            while (IsLocationServiceInitializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            // If the service didn't initialize in 20
            // seconds, this cancels location service use.
            if (maxWait < 1)
            {
                OnGpsError?.Invoke("GPS initialization timed out");
                yield break;
            }

            // If the connection failed this cancels location service use.
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                OnGpsError?.Invoke("Unable to determine device location");
                yield break;
            }
            
            var gpsInfo = Input.location.lastData;
            _location = new LatLng(gpsInfo.latitude, gpsInfo.longitude);
            _lastGpsUpdateTime = gpsInfo.timestamp;
            
            MadeMapReady();

            while (isActiveAndEnabled)
            {
                gpsInfo = Input.location.lastData;
                if (gpsInfo.timestamp > _lastGpsUpdateTime)
                {
                    //Neue GPS Location ziehen
                    _lastGpsUpdateTime = gpsInfo.timestamp;
                    _location = new LatLng(gpsInfo.latitude, gpsInfo.longitude);
                    //Neu spawnen des AstroBois
                    _actualAstroBoy.Dispose();
                    _actualAstroBoy  = astroSpawner.PlaceInstance(_location, Quaternion.identity);
                }
                
                yield return null;
            }
            
            Input.location.Stop();
        
        }

        public void AddFlag(string flagName = "Flagge", string description = "Eine Meldung!", string author = "Planende Person")
        {
            var report = new LocationReport()
            {
                name = flagName,
                coords = _location,
                description = description,
                author = author,
                date = DateTime.Now.ToString("dd.MM.yyyy"),
                demo = true
            };
            var trex = flagSpawner.PlaceInstance(_location, Quaternion.identity).Value.GetComponent<FlagHandler>();
            trex.name = report.name;
            trex.location = report;
            trex.modal = modal;
            trex.IsADemoFlag();
            LocationReport.SaveDemoReport(report);
        }
    }
}
