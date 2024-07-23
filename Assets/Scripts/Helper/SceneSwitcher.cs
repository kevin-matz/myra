using UnityEngine;
using UnityEngine.SceneManagement;

namespace Helper
{
    public class SceneSwitcher : MonoBehaviour
    {
        public int scene = 0;
        public void SwitchScene()
        {
            SceneManager.LoadScene(scene);
        }
    }
}
