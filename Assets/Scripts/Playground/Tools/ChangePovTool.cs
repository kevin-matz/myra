using DG.Tweening;
using Helper;
using UnityEngine;

namespace Playground.Tools
{
    public class ChangePovTool : PlaygroundTool
    {
        public ChangePovTool() : base("change_pov")
        {
        }

        protected override void UpdateInternal()
        {
        }

        protected override void UpdateInternalWhenActive()
        {
            if (Input.touchCount != 1)
                return;

            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended)
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
            
            ChangePov(hitInfo.point, hitInfo.transform);
        }
        
        private void ChangePov(Vector3 requestedPointOfView, Transform groundPlane, bool instant = false)
        {
            /* Ignore Y */
            requestedPointOfView.y = groundPlane.position.y;

            GameObject currentPovObj = null;
            
            /* Find persisted pov, otherwise use local point (0, 0, 0) on the plane */
            for (var i = 0; i < groundPlane.childCount; i++)
            {
                var child = groundPlane.GetChild(i);
                if (child.name != "Plane POV")
                    continue;

                currentPovObj = child.gameObject;
                break;
            }

            if (currentPovObj == null)
            {
                currentPovObj = new GameObject("Plane POV");
                currentPovObj.transform.parent = groundPlane;
                currentPovObj.transform.localPosition = Vector3.zero;
            }
            
            /* Target world space position calculation */
            var originWorldSpacePosition = currentPovObj.transform.position;
            var groundMoveDir = (originWorldSpacePosition - requestedPointOfView).normalized;
            groundMoveDir.y = 0;
            var moveDistance = (new Vector3(requestedPointOfView.x, 0, requestedPointOfView.z) - new Vector3(originWorldSpacePosition.x, 0, originWorldSpacePosition.z)).magnitude;
            var targetPlanePosition = groundPlane.position + groundMoveDir * moveDistance;
            
            /* Translate target world space position to local space */
            var origPlanePosition = groundPlane.position;
            groundPlane.position = targetPlanePosition;
            var targetLocalPlanePosition = groundPlane.localPosition;
            groundPlane.position = origPlanePosition;
            
            /* Move the plane locally */
            if (instant)
            {
                groundPlane.localPosition = targetLocalPlanePosition;
            }
            else
            {
                groundPlane.DOLocalMove(targetLocalPlanePosition, 0.25f);
            }
            
            /* Persist pov */
            currentPovObj.transform.position = requestedPointOfView;
        }

        public void ResetPov()
        {
            var groundPlane = PlaygroundHandler.GetGroundPlane().transform;
            GameObject currentPovObj = null;
            for (var i = 0; i < groundPlane.childCount; i++)
            {
                var child = groundPlane.GetChild(i);
                if (child.name != "Plane POV")
                    continue;

                currentPovObj = child.gameObject;
                break;
            }
            
            if (currentPovObj == null)
                return;
            
            var tempOriginObj = new GameObject();
            tempOriginObj.transform.parent = groundPlane;
            tempOriginObj.transform.localPosition = Vector3.zero;
            
            ChangePov(tempOriginObj.transform.position, groundPlane, true);
            Destroy(tempOriginObj);
            Destroy(currentPovObj);
        }
    }
}