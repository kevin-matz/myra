using Map;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Helper
{
    public class ModalHandler : MonoBehaviour
    {
        public TextMeshProUGUI locationName;
        public TextMeshProUGUI locationDescription;
        public TextMeshProUGUI locationAuthor;
        public TextMeshProUGUI locationDate;
        public GameObject demoModal;
        public CameraTopDownController camera;
        public LocationReport location;
        private void OnEnable()
        {
            if (demoModal != null && demoModal.activeInHierarchy)
            {
                gameObject.SetActive(false);
                return;
            }

            locationName.text = location.name;
            locationDescription.text = location.description;
            locationAuthor.text = location.author;
            locationDate.text = location.date;
            if (camera != null) camera.touchEnabled = false;
        }

        public void HideUI()
        {
            gameObject.SetActive(false);
            if (camera != null) camera.touchEnabled = true;
        }
        
        public void OpenCreationSpace()
        {
            if(!location.demo) SceneManager.LoadScene(3);
            else
            {
                OpenDemoModal();
            }
        }

        public void OpenPlayground()
        {
            if(!location.demo) SceneManager.LoadScene(2);
            else
            {
                OpenDemoModal();
            }
        }
        
        public void OpenShowcase()
        {
            if(!location.demo) SceneManager.LoadScene(1);
            else
            { 
                OpenDemoModal();
            }
        }

        private void OpenDemoModal()
        {
            gameObject.SetActive(false);
            demoModal.SetActive(true);
            demoModal.GetComponent<DemoHandler>().SetupDemoModal("Nicht verfügbar","Diese app ist nur ein Prototyp. Um eine Meldung einzusehen öffne einen roten Demo Ort.");
        }
    }
}
