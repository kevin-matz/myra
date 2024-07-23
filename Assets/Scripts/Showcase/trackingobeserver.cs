

using System;
using System.Linq;
using Niantic.Lightship.AR.LocationAR;
using Niantic.Lightship.AR.PersistentAnchors;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Helper;
using UnityEngine.Serialization;


namespace Showcase
{
    public class TrackingObserver : MonoBehaviour
    {
        //UI
        [SerializeField]
        private TextMeshProUGUI anchorTrackingStateText;
    
        [SerializeField]
        private TextMeshProUGUI variantText;

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
        private Helper.Showcase[] showcases;

        private Helper.Showcase _actShowcase;

        private int _actVariant = 0;
        
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
            
            //Varianten Setup
            if (_actShowcase.variants == null) return;
            foreach (var variant in _actShowcase.variants)
            {
                variant.SetActive(false);
            }

            _actShowcase.variants[_actVariant].SetActive(true);
        }

        private void VariantSwitcher()
        {
            foreach (var variant in _actShowcase.variants)
            {
                variant.SetActive(false);
            }
            _actShowcase.variants[_actVariant].SetActive(true);
            var variantCount = _actVariant + 1;
            variantText.text = "Entwurf " + variantCount;
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
            
            _actShowcase = showcases.First(showcase => showcase.arLocation.name == locationName);
            if (_actShowcase.arLocation == null) return;
            
            arLocationManager.StopTracking();
            arLocationManager.SetARLocations(_actShowcase.arLocation);
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

        public void NextVariant()
        {
            if (_actVariant == _actShowcase.variants.Length - 1) _actVariant = 0;
            else _actVariant += 1;
            VariantSwitcher();
        }
        
        public void PrevVariant()
        {
            if (_actVariant == 0) _actVariant = _actShowcase.variants.Length - 1;
            else _actVariant -= 1;
            VariantSwitcher();
        }
        
        
    }
}