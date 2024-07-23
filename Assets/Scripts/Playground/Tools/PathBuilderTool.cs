using System;
using System.Collections.Generic;
using Helper;
using UnityEngine;
using UnityEngine.Rendering;

namespace Playground.Tools
{
    public class PathBuilderTool : PlaygroundTool
    {
        public PathBuilderTool() : base("path_builder")
        {
        }
        
        [SerializeField] private PlaygroundHandler playgroundHandler;

        private void Start()
        {
            playgroundHandler = GetComponent<PlaygroundHandler>();
        }

        [SerializeField] private GameObject pointMarkerPrefab;
        [SerializeField] private Material previewLineMaterial;
        [SerializeField] private Material redPathMaterial;
        [SerializeField] private Material greenPathMaterial;
        [SerializeField] private Material bluePathMaterial;
        [SerializeField] private GameObject defaultButtonGroup;
        [SerializeField] private GameObject colorPickerButtonGroup;
        private GameObject currentEditedPath;
        
        private Vector3[] GetPointsOnPath(Transform path)
        {
            List<Vector3> points = new();
            for (var i = 0; i < path.childCount; i++)
            {
                var pathPoint = path.GetChild(i);
                points.Add(pathPoint.position);
            }

            return points.ToArray();
        }

        public void FinishPath(int colorIndex)
        {
            HideColorPicker();
            
            var lineRenderer = currentEditedPath.GetComponent<LineRenderer>();
            lineRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
            lineRenderer.generateLightingData = true;
            lineRenderer.numCapVertices = 10;
            lineRenderer.material = colorIndex switch
            {
                0 => redPathMaterial,
                1 => greenPathMaterial,
                2 => bluePathMaterial,
                _ => redPathMaterial
            };
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.textureScale = new Vector2(1, 1);

            for (var i = 0; i < currentEditedPath.transform.childCount; i++)
            {
                var pathPoint = currentEditedPath.transform.GetChild(i);
                Destroy(pathPoint.GetChild(0).gameObject);
            }

            currentEditedPath = null;
        }

        public void OnSubmitClicked()
        {
            if (currentEditedPath == null)
                return;

            ShowColorPicker();
        }

        private void ShowColorPicker()
        {
            defaultButtonGroup.SetActive(false);
            colorPickerButtonGroup.SetActive(true);
        }
        
        private void HideColorPicker()
        {
            defaultButtonGroup.SetActive(true);
            colorPickerButtonGroup.SetActive(false);
        }
        
        public override void OnActivate()
        {
            base.OnActivate();
            playgroundHandler.SetPathToolButtonGroupActive(true);
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            playgroundHandler.SetPathToolButtonGroupActive(false);
        }

        protected override void UpdateInternal()
        {
            var paths = GameObject.FindGameObjectsWithTag("Path");
            foreach (var path in paths)
            {
                if (!path.TryGetComponent(out LineRenderer line))
                    continue;

                var points = GetPointsOnPath(path.transform);
                if (points.Length < 2)
                    continue;
                
                line.positionCount = points.Length;
                line.SetPositions(points);
            }
        }

        protected override void UpdateInternalWhenActive()
        {
            if (Input.touchCount != 1)
                return;
            
            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began)
                return;
            
            var pos = touch.position;
            
            if (PlatformAgnosticInput.IsOverUIObject(pos))
                return;
            
            var cam = Camera.main;
            if (cam == null)
                return;
            
            var ray = cam.ScreenPointToRay(new Vector3(pos.x, pos.y, 0));
            const int groundMask = 1 << 6;
            if (!Physics.Raycast(ray, out var hitInfo, float.PositiveInfinity, groundMask))
                return;
            
            var target = hitInfo.point;
            target.y += 0.01f;

            if (currentEditedPath == null)
            {
                currentEditedPath = new GameObject("Path");
                currentEditedPath.tag = "Path";
                currentEditedPath.transform.parent = GameObject.FindWithTag("MarkerTrackedPlane").transform;
                currentEditedPath.transform.localScale = new Vector3(33f, 33f, 33f);
                var lineRenderer = currentEditedPath.AddComponent<LineRenderer>();
                lineRenderer.alignment = LineAlignment.TransformZ;
                lineRenderer.transform.rotation = Quaternion.Euler(90, 0, 0);
                lineRenderer.startWidth = 0.015f;
                lineRenderer.endWidth = 0.015f;
                lineRenderer.numCornerVertices = 7;
                lineRenderer.material = previewLineMaterial;
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.textureScale = new Vector2(10, 0.33f);
                lineRenderer.positionCount = 0;
                lineRenderer.SetPositions(Array.Empty<Vector3>());
            }
                        
            var pathPoint = new GameObject("Point");
            pathPoint.transform.position = target;
            pathPoint.transform.parent = currentEditedPath.transform;
            var particle = Instantiate(pointMarkerPrefab, target, Quaternion.identity, pathPoint.transform);
            particle.name = "Marker";
                        
            if (currentEditedPath.transform.childCount >= 2)
            {
                var lineRenderer = currentEditedPath.GetComponent<LineRenderer>();
                var points = GetPointsOnPath(currentEditedPath.transform);
                lineRenderer.positionCount = points.Length;
                lineRenderer.SetPositions(points);
            }
        }

        public void Cancel()
        {
            Destroy(currentEditedPath);
            currentEditedPath = null;
        }

        public void DeleteAllPaths()
        {
            Cancel();
            var paths = GameObject.FindGameObjectsWithTag("Path");
            foreach (var path in paths)
            {
                Destroy(path);
            }
        }
    }
}