using Helper;
using Niantic.Lightship.Maps.MapLayers.Components;
using UnityEngine;

namespace Map.Old
{
    public class BialettiSpawner : MonoBehaviour
    {
        [SerializeField] 
        private LocationReport[] locations;
    
        [SerializeField]
        private LayerGameObjectPlacement objectSpawner;

        [SerializeField] 
        private GameObject modal;
    
        public void Spawn()
        {
            foreach (var location in locations)
            {
                var bialetti = objectSpawner.PlaceInstance(location.coords, Quaternion.identity).Value.GetComponent<FlagHandler>();
                bialetti.name = location.name;
                bialetti.location = location;
                bialetti.modal = modal;
            }
        }
    }
}
