using Helper;
using UnityEngine;

namespace Map
{
    public class FlagHandler : MonoBehaviour
    {
        public LocationReport location;
        
        [HideInInspector]
        public GameObject modal;

        public Color demoFlagColor;

        public void IsADemoFlag()
        {
            var renderer = GetComponent<Renderer>();
            renderer.materials[0].color = demoFlagColor;
        }
        
        public void OnTouched()
        {
            PlayerPrefs.SetString("locationName", location.name);
            modal.GetComponent<ModalHandler>().location = location;
            modal.SetActive(true);
        }
    }
}
