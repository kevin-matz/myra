using UnityEngine;

public class BialettiOpener : MonoBehaviour
{
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        RaycastHit hit;
        
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = _camera.ScreenPointToRay(touch.position);
           
            if (!Physics.Raycast(ray, out hit))
            {
                return;
            }
        } 
        else if (Input.GetMouseButtonDown(0))
        {
            Ray rayMouse = _camera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(rayMouse, out hit))
            {
                return;
            }
        }
        else return;
        
        
        if (hit.collider == null) return;
            
        hit.collider.gameObject.SendMessage("OnTouched", SendMessageOptions.DontRequireReceiver);
    }
}
