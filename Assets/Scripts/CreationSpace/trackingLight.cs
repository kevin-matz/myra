

using System;
using System.Linq;
using Helper;
using Niantic.Lightship.AR.Loader;
using Niantic.Lightship.AR.LocationAR;
using Niantic.Lightship.AR.PersistentAnchors;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace CreationSpace
{
    public class TrackingObserverLight : MonoBehaviour
    {
        //UI
        [SerializeField]
        private TextMeshProUGUI anchorTrackingStateText;
        

        [SerializeField] 
        private GameObject modal;
        
        [Serializable]
        public struct TrackingUi
        {
            public GameObject thing;
            public bool automaticEnabling;
        }
        [SerializeField] 
        private TrackingUi[] uiEnabledWhileTracking;
        
        [SerializeField] 
        private GameObject anchorImage;
        
        //Occlusion
        [SerializeField] 
        private AROcclusionManager occlusionManager;

        [SerializeField] 
        private GameObject occlusionCheckBox;
        
        [SerializeField]
        private bool occlusion = false;
        
        [SerializeField] 
        private OcclusionPreferenceMode occlusionPreferenceMode;
        
        //ARLocations
        [SerializeField]
        private ARLocationManager arLocationManager;
    
        [SerializeField]
        private ARLocation[] spaces;

        private ARLocation _actSpace;

        [SerializeField] 
        private GameObject markerPlane;

        
        private void OnEnable()
        {
            arLocationManager.locationTrackingStateChanged += OnLocationTrackingStateChanged;
        }
        
        private void Start()
        {
            //Occlusion Setup
            occlusionCheckBox.SetActive(occlusion);
            occlusionManager.requestedOcclusionPreferenceMode = occlusion ? occlusionPreferenceMode : OcclusionPreferenceMode.NoOcclusion;
            
            //UI Setup
            anchorImage.SetActive(true);
            foreach (var ui in uiEnabledWhileTracking)
            {
                ui.thing.SetActive(false);
            }
            
            //ARLocationManager Setup
            SetLocation();
            
        }
        

        private void OnDisable()
        {
            arLocationManager.locationTrackingStateChanged -= OnLocationTrackingStateChanged;
        }
        
        public void ToggleOcclusion()
        {
            occlusion = !occlusion;
            occlusionCheckBox.SetActive(occlusion);
            occlusionManager.requestedOcclusionPreferenceMode = occlusion ? occlusionPreferenceMode : OcclusionPreferenceMode.NoOcclusion;
        }

        private bool  SetupLocationManager()
        {
            anchorTrackingStateText.text = "Vorbereiten...";
            if (string.IsNullOrWhiteSpace(LightshipSettings.Instance.ApiKey))
            {
                if (anchorTrackingStateText != null)
                {
                    anchorTrackingStateText.text = "Kein API Key";
                }

                return false;
            }

            if (arLocationManager == null)
            {
                if (anchorTrackingStateText != null)
                {
                    anchorTrackingStateText.text = "Kein Location Manager verfügbar";
                }

                return false;
            }

            if (arLocationManager.ARLocations.Length >= 1) return true;
            anchorTrackingStateText.text = "Keine AR Locations verfügbar";
            return false;

        }
        
        private void SetLocation()
        {
            var locationName = PlayerPrefs.GetString("locationName");

            if (LocationReport.Locations == null)
            {
                StartCoroutine(LocationReport.LoadLocations(_ =>
                {
                    modal.GetComponent<ModalHandler>().location = LocationReport.FindLocation(locationName);
                }));
            }
            else
            {
                modal.GetComponent<ModalHandler>().location = LocationReport.FindLocation(locationName);
            }

            _actSpace = spaces.First(space => space.name == locationName);
            if (_actSpace == null) return;
            
            arLocationManager.StopTracking();
            arLocationManager.SetARLocations(_actSpace);
            //Instantiate(markerPlane, _actSpace.transform, true);
            arLocationManager.StartTracking();
            anchorTrackingStateText.text = "Lokalisiere...";
        }

        private void OnLocationTrackingStateChanged(ARLocationTrackedEventArgs args)
        {
            if (args.Tracking)
            {
                anchorImage.SetActive(false);
                foreach (var ui in uiEnabledWhileTracking)
                {
                    if(ui.automaticEnabling) ui.thing.SetActive(true);
                }
                
                if (anchorTrackingStateText != null)
                {
                    anchorTrackingStateText.text = $"Ankerpunkt erkannt";
                }
            }
            else
            {
                anchorImage.SetActive(true);
                foreach (var ui in uiEnabledWhileTracking)
                {
                    ui.thing.SetActive(false);
                }
                
                if (anchorTrackingStateText != null)
                {
                    anchorTrackingStateText.text = $"Ankerpunkt verloren";
                }
            }
        }
        
        
    }
}