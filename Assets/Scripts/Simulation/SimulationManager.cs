using UnityEngine;
using UnityEngine.Events;

namespace Simulation
{
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance;
        public UnityEvent simulationToggle;
        public bool simulation = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Destroy(Instance);
                Instance = this;
            }
        }

        public void Stop()
        {
            simulation = false;
            simulationToggle.Invoke();
        }
        
        public void SimulationToggle()
        {
            simulation = !simulation;
            simulationToggle.Invoke();
        }
        
    }
}
