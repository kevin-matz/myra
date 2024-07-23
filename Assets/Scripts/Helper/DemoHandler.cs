using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Helper
{
    public class DemoHandler : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;

        public void SetupDemoModal(string demoTitle, string demoDescription)
        {
            title.text = demoTitle;
            description.text = demoDescription;
        }
    }
}
