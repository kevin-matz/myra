using System;
using Helper;
using Niantic.Lightship.Maps;
using Niantic.Platform.Debugging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityInput = UnityEngine.Input;



namespace Map
{
    public class CameraTopDownController : MonoBehaviour
    {
        [SerializeField]
        private float _mouseScrollSpeed = 0.1f;

        [SerializeField]
        private float _pinchScrollSpeed = 0.002f;

        [SerializeField]
        private float _minimumMapRadius = 10.0f;
    
        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private LightshipMapView _mapView;

        public bool touchEnabled = true;
        
        private bool _isPinchPhase;
        private bool _isPanPhase;
        private float _lastPinchDistance;
        private Vector3 _lastWorldPosition;
        private float _mapRadius;

        private void FlagOpener()
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
    
        private void Start()
        {
            Assert.That(_camera.orthographic);
            Assert.That(_mapView.IsMapCenteredAtOrigin);
            _mapRadius = (float)_mapView.MapRadius;
            _camera.orthographicSize = _mapRadius;
        }

        private void Update()
        {
            FlagOpener();
            if (!touchEnabled) return;
            // Mouse scroll wheel moved
            if (UnityInput.mouseScrollDelta.y != 0)
            {
                var mousePosition = new Vector2(UnityInput.mousePosition.x, UnityInput.mousePosition.y);

                // Don't zoom if the mouse pointer is over a UI object
                if (!PlatformAgnosticInput.IsOverUIObject(mousePosition))
                {
                    var sizeDelta = UnityInput.mouseScrollDelta.y * _mouseScrollSpeed * _mapRadius;
                    var newMapRadius = Math.Max(_mapRadius - sizeDelta, _minimumMapRadius);

                    _mapView.SetMapRadius(newMapRadius);
                    _camera.orthographicSize = newMapRadius;
                    _mapRadius = newMapRadius;
                }
            }

            // UI element was pressed, so ignore all touch input this frame
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                return;
            }

            // Pinch logic
            if (UnityInput.touchCount == 2)
            {
                Vector2 touch0;
                Vector2 touch1;

                if (_isPinchPhase == false)
                {
                    // Pinch started so reset pan position
                    ResetPanTouch();

                    touch0 = UnityInput.GetTouch(0).position;
                    touch1 = UnityInput.GetTouch(1).position;
                    _lastPinchDistance = Vector2.Distance(touch0, touch1);

                    _isPinchPhase = true;
                }
                else
                {
                    touch0 = UnityInput.GetTouch(0).position;
                    touch1 = UnityInput.GetTouch(1).position;
                    float distance = Vector2.Distance(touch0, touch1);

                    var sizeDelta = (distance - _lastPinchDistance) * _pinchScrollSpeed * _mapRadius;
                    var newMapRadius = Math.Max(_mapRadius - sizeDelta, _minimumMapRadius);

                    _mapView.SetMapRadius(newMapRadius);
                    _camera.orthographicSize = newMapRadius;
                    _mapRadius = newMapRadius;

                    _lastPinchDistance = distance;
                }
            }
            // No pinch
            else
            {
                // Pinch so reset pan position
                if (_isPinchPhase && _isPanPhase && PlatformAgnosticInput.TouchCount == 1)
                {
                    ResetPanTouch();
                }

                _isPinchPhase = false;
            }

            // Pan camera by swiping
            if (PlatformAgnosticInput.TouchCount >= 1)
            {
                if (_isPanPhase == false)
                {
                    _isPanPhase = true;
                    ResetPanTouch();
                }
                else
                {
                    Vector3 currentInputPos = PlatformAgnosticInput.GetTouch(0).position;
                    currentInputPos.z = _camera.nearClipPlane;
                    var currentWorldPosition = _camera.ScreenToWorldPoint(currentInputPos);
                    currentWorldPosition.y = 0.0f;

                    var offset = currentWorldPosition - _lastWorldPosition;
                    _mapView.OffsetMapCenter(offset);
                    _lastWorldPosition = currentWorldPosition;
                }
            }
            else
            {
                _isPanPhase = false;
            }
        }

        private void ResetPanTouch()
        {
            Vector3 currentInputPos = PlatformAgnosticInput.GetTouch(0).position;
            var currentWorldPosition = _camera.ScreenToWorldPoint(currentInputPos);
            currentWorldPosition.y = 0.0f;

            _lastWorldPosition = currentWorldPosition;
        }
    }
}
