using System.Collections.Generic;
using Helper;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Playground.Tools
{
    public class PlaceMoveRotateTool : PlaygroundTool
    {
        public PlaceMoveRotateTool() : base("place_move_rotate")
        {
        }

        [SerializeField] private PlaygroundHandler playgroundHandler;

        private void Start()
        {
            playgroundHandler = GetComponent<PlaygroundHandler>();
        }

        [SerializeField] private AssetsGallery assetGallery;
        private GameObject selectedObject;
        private CollisionDetectionComponent selectedObjectCollisionDetector;
        private Outline selectedObjectOutline;
        private Vector3 selectedObjectInitialPosition = Vector3.zero;
        private bool isObjectPendingDeselection;
    
        private bool IsObjectSelected()
        {
            return selectedObject != null;
        }
        
        private void SpawnObject(Vector3 spawnPoint, Transform parent)
        {
            var obj = Instantiate(assetGallery.SelectedPrefab, spawnPoint, parent.rotation, parent);
            obj.tag = "SpawnedObject";
            var isPostItNote = obj.TryGetComponent(out PostItNote _);
            if (!isPostItNote)
            {
                obj.AddComponent<CollisionDetectionComponent>();
            }
            Select(obj);
        }
    
        private void Select(GameObject obj)
        {
            selectedObject = obj;
            if (selectedObject.gameObject.TryGetComponent(out Outline outline))
            {
                outline.enabled = true;
                selectedObjectOutline = outline;
            }
            else
            {
                var newOutline = selectedObject.gameObject.AddComponent<Outline>();
                newOutline.OutlineMode = Outline.Mode.OutlineAll;
                newOutline.OutlineWidth = 3f;
                selectedObjectOutline = newOutline;
            }

            selectedObjectCollisionDetector = selectedObject.GetComponent<CollisionDetectionComponent>();
            selectedObjectInitialPosition = selectedObject.transform.localPosition;

            var wasPostItSelected = selectedObject.TryGetComponent(out PostItNote _);
            if (wasPostItSelected)
            {
                playgroundHandler.ShowEditNoteButton();
            }
            
            playgroundHandler.ShowDeleteSelectedButton();
        }
    
        private void Deselect(bool destroy = false)
        {
            if (!IsObjectSelected() || !selectedObject.gameObject.TryGetComponent(out Outline outline))
                return;
            
            outline.enabled = false;
            
            var isPostItSelected = selectedObject.TryGetComponent(out PostItNote _);
            if (isPostItSelected)
            {
                playgroundHandler.HideEditNoteButton();
            }
            
            playgroundHandler.HideDeleteSelectedButton();

            if (destroy)
            {
                Destroy(selectedObject);
            }
            
            selectedObject = null;
            selectedObjectCollisionDetector = null;
            selectedObjectOutline = null;
            if (dragTargetPoint != null)
            {
                dragTargetPoint.SetActive(false);
            }
            isObjectPendingDeselection = false;
        }
    
        private void DelayedDeselectImpl()
        {
            if (!isObjectPendingDeselection)
                return;
            
            Deselect();
        }
    
        private void DelayedDeselect()
        {
            isObjectPendingDeselection = true;
            Invoke(nameof(DelayedDeselectImpl), 0.25f);
        }
        
        public override void OnActivate()
        {
            base.OnActivate();
            playgroundHandler.SetAssetGalleryActive(true);
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            playgroundHandler.SetAssetGalleryActive(false);
        }

        protected override void UpdateInternal()
        {
        }

        private static Vector3 GetPostItNoteTargetPointFromRaycastHit(RaycastHit hitInfo, Ray ray)
        {
            var rayDirectionToHit = hitInfo.point - ray.origin;
            var offsetVector = -rayDirectionToHit.normalized * PlaygroundHandler.CurrentScale;
            return hitInfo.point + offsetVector;
        }

        private static bool IsOverUnrelatedUIObject(Vector2 position)
        {
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current)
            {
                position = position
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            if (results.Count <= 0)
                return false;

            var isOverObjectWithKeepSelectionTag = false;
            foreach (var result in results)
            { 
                if (result.gameObject.CompareTag("KeepSelection"))
                    isOverObjectWithKeepSelectionTag = true;
            }

            return !isOverObjectWithKeepSelectionTag;
        }

        private bool canDrag;

        protected override void UpdateInternalWhenActive()
        {
            if (assetGallery.SelectedPrefab == null)
                return;
            
            HandleDragging();

            if (Input.touchCount != 2)
            {
                /* Stop dragging if user was rotating last frame */
                if (_isRotating)
                {
                    canDrag = false;
                }
                
                _isRotating = false;
            }
            
            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);

                var pos = touch.position;
                
                if (touch.phase == TouchPhase.Began && PlatformAgnosticInput.IsOverUIObject(pos))
                {
                    /* Don't want to deselect if the UI element has the KeepSelection tag */
                    if (IsOverUnrelatedUIObject(pos))
                    {
                        Deselect();
                    }
                    return;
                }
                
                var cam = Camera.main;
                if (cam == null)
                    return;
                
                var ray = cam.ScreenPointToRay(new Vector3(pos.x, pos.y, 0));
                
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                    {
                        Physics.Raycast(ray, out var hitInfo);
                        if (hitInfo.transform != null)
                        {
                            canDrag = true;
                            if (dragTargetPoint != null)
                            {
                                dragTargetPoint.SetActive(false);
                            }

                            var isPostItNoteAssetSelected = assetGallery.SelectedPrefab.TryGetComponent(out PostItNote _);
                            if (isPostItNoteAssetSelected)
                            {
                                var wasPostItNoteClicked = hitInfo.transform.TryGetComponent(out PostItNote _);
                                if (wasPostItNoteClicked)
                                {
                                    Deselect();
                                    Select(hitInfo.transform.gameObject);
                                }
                                else
                                {
                                    if (IsObjectSelected())
                                    {
                                        DelayedDeselect();
                                    }
                                    else
                                    {
                                        Deselect();
                                        SpawnObject(GetPostItNoteTargetPointFromRaycastHit(hitInfo, ray), PlaygroundHandler.GetGroundPlane().transform);
                                    }
                                }
                            }
                            else
                            {
                                if (hitInfo.transform.CompareTag("SpawnedObject"))
                                {
                                    Deselect();
                                    Select(hitInfo.transform.gameObject);
                                }
                                else if (hitInfo.transform.CompareTag("MarkerTrackedPlane"))
                                {
                                    if (IsObjectSelected())
                                    {
                                        DelayedDeselect();
                                    }
                                    else
                                    {
                                        Deselect();
                                        SpawnObject(hitInfo.point, hitInfo.transform);  
                                    }
                                }
                            }
                        }
                        else if (!PlatformAgnosticInput.IsOverUIObject(pos))
                        {
                            DelayedDeselect();
                        }

                        break;
                    }
                    case TouchPhase.Moved:
                    {
                        if (selectedObject == null)
                            break;
                        
                        if (isObjectPendingDeselection)
                            break;
                        
                        RaycastHit hitInfo;
                        var isMovingPostItNote = selectedObject.TryGetComponent(out PostItNote _); 
                        if (isMovingPostItNote)
                        {
                            const int everythingButPostItsMask = ~(1 << 7);
                            Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, everythingButPostItsMask);
                        }
                        else
                        {
                            const int groundMask = 1 << 6;
                            Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, groundMask);
                        }
                        if (hitInfo.transform != null && IsObjectSelected())
                        {
                            if (dragTargetPoint == null)
                            {
                                dragTargetPoint = new GameObject("Drag Target Point");
                                dragTargetPoint.transform.parent = isMovingPostItNote ? PlaygroundHandler.GetGroundPlane().transform : hitInfo.transform;
                            }
                            else
                            {
                                dragTargetPoint.SetActive(true);
                            }

                            if (isMovingPostItNote)
                            {
                                dragTargetPoint.transform.position = GetPostItNoteTargetPointFromRaycastHit(hitInfo, ray);
                            }
                            else
                            {
                                dragTargetPoint.transform.position = hitInfo.point;
                            }
                        }
                            
                        break;
                    }
                    case TouchPhase.Ended:
                    {
                        if (selectedObjectCollisionDetector != null && selectedObjectCollisionDetector.IsColliding)
                        {
                            selectedObjectOutline.OutlineColor = Color.white;
                            selectedObjectOutline.OutlineWidth = 3f;
                            selectedObject.transform.localPosition = selectedObjectInitialPosition;
                            selectedObjectOutline.OutlineMode = Outline.Mode.OutlineAll;
                            dragTargetPoint.SetActive(false);
                        }
                        
                        break;
                    }
                }
            }
            else if (Input.touchCount == 2)
            {
                isObjectPendingDeselection = false;
                HandleScaling();
                HandleRotation();
            }
        }

        private GameObject dragTargetPoint;
        
        private void HandleDragging()
        {
            if (!canDrag)
                return;
            
            if (dragTargetPoint == null || !dragTargetPoint.activeSelf)
                return;
            
            var hasArrivedAtTargetPosition = (selectedObject.transform.position - dragTargetPoint.transform.position).magnitude < 0.01f;
            if (hasArrivedAtTargetPosition)
            {
                dragTargetPoint.SetActive(false);
                return;
            }

            HandleCollisionCheck();
            
            selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, dragTargetPoint.transform.position, 10f * Time.deltaTime);
        }

        private void HandleCollisionCheck()
        {
            if (selectedObject == null || selectedObjectOutline == null || selectedObjectCollisionDetector == null)
                return;

            if (selectedObjectCollisionDetector.IsColliding)
            {
                selectedObjectOutline.OutlineColor = Color.red;
                selectedObjectOutline.OutlineWidth = 5f;
                selectedObjectOutline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
            }
            else
            {
                selectedObjectOutline.OutlineColor = Color.white;
                selectedObjectOutline.OutlineWidth = 3f;
                selectedObjectInitialPosition = selectedObject.transform.localPosition;
                selectedObjectOutline.OutlineMode = Outline.Mode.OutlineAll;
            }
        }

        private Vector2 _previousTouch1PosForRotation;
        private Vector2 _previousTouch2PosForRotation;
        private bool _isRotating;
        
        private void HandleRotation()
        {
            var touch1 = Input.GetTouch(0);
            var touch2 = Input.GetTouch(1);

            var currentTouch1Pos = touch1.position;
            var currentTouch2Pos = touch2.position;

            if (!_isRotating)
            {
                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    _previousTouch1PosForRotation = currentTouch1Pos;
                    _previousTouch2PosForRotation = currentTouch2Pos;
                }
                else
                {
                    _isRotating = true;
                }
            }
            else
            {
                var prevDir = (_previousTouch2PosForRotation - _previousTouch1PosForRotation).normalized;
                var currentDir = (currentTouch2Pos - currentTouch1Pos).normalized;

                var angle = -Vector2.SignedAngle(prevDir, currentDir);
                var yAxis = selectedObject.transform.up;
                selectedObject.transform.Rotate(yAxis, angle, Space.World);

                _previousTouch1PosForRotation = currentTouch1Pos;
                _previousTouch2PosForRotation = currentTouch2Pos;
            }
        }

        private Vector2 _previousTouch1PosForScale;
        private Vector2 _previousTouch2PosForScale;

        private void HandleScaling()
        {
            if (selectedObject == null)
                return;

            var isPrimitiveObject = selectedObject.TryGetComponent(out PrimitiveObject _);
            if (!isPrimitiveObject)
                return;
            
            var touch1 = Input.GetTouch(0);
            var touch2 = Input.GetTouch(1);

            var currentTouch1Pos = touch1.position;
            var currentTouch2Pos = touch2.position;

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                _previousTouch1PosForScale = currentTouch1Pos;
                _previousTouch2PosForScale = currentTouch2Pos;
            }
            else
            {
                var previousDistance = Vector2.Distance(_previousTouch1PosForScale, _previousTouch2PosForScale);
                var currentDistance = Vector2.Distance(currentTouch1Pos, currentTouch2Pos);
                var scaleChange = currentDistance / previousDistance;

                var newScale = selectedObject.transform.localScale * scaleChange;
                selectedObject.transform.localScale = newScale;

                _previousTouch1PosForScale = currentTouch1Pos;
                _previousTouch2PosForScale = currentTouch2Pos;
            }
        }

        public void DeleteSelectedObject()
        {
            Deselect(true);
        }

        [SerializeField] private NoteWindow editNoteModal;

        public void EditNote()
        {
            editNoteModal.ShowWindow(selectedObject.GetComponent<PostItNote>());
        }
    }
}