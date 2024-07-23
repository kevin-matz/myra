using UnityEngine;

public class PostItNote : MonoBehaviour
{
    public string Text;
    
    private void Update()
    {
        var mainCam = Camera.main;
        if (mainCam == null)
            return;
        
        transform.rotation = Quaternion.LookRotation(mainCam.transform.forward);
    }
}
