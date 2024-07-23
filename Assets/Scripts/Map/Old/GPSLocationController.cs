// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections;
using Niantic.Lightship.Maps;
using Niantic.Lightship.Maps.Core.Coordinates;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif



public class GpsLocationController : MonoBehaviour
{
    [SerializeField]
    private LightshipMapView lightshipMapView;
    
    

    private double _lastGpsUpdateTime;
    private Vector3 _targetMapPosition;
    private Vector3 _currentMapPosition;
    private float _lastMapViewUpdateTime;

    
    public Action<string> OnGpsError;

    private const float WalkThreshold = 0.5f;
    private const float TeleportThreshold = 200f;

    private static bool IsLocationServiceInitializing
        => Input.location.status == LocationServiceStatus.Initializing;

    private void Start()
    {
        //lightshipMapView.MapOriginChanged += OnMapViewOriginChanged;
        _currentMapPosition = _targetMapPosition = transform.position;

        StartCoroutine(UpdateGpsLocation());
    }

    private void OnMapViewOriginChanged(LatLng center)
    {
        var offset = _targetMapPosition - _currentMapPosition;
        _currentMapPosition = lightshipMapView.LatLngToScene(center);
        _targetMapPosition = _currentMapPosition + offset;
    }

    private IEnumerator UpdateGpsLocation()
    {
        yield return null;

        
#if UNITY_ANDROID
            // Request location permission for Android
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
                while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                {
                    // Wait until permission is enabled
                    yield return new WaitForSeconds(1.0f);
                }
            }
#endif
            // Check if the user has location service enabled.
            if (!Input.location.isEnabledByUser)
            {
                OnGpsError?.Invoke("Location permission not enabled");
                yield break;
            }

            // Starts the location service.
            Input.location.Start();

            // Waits until the location service initializes
            int maxWait = 20;
            while (IsLocationServiceInitializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            // If the service didn't initialize in 20
            // seconds, this cancels location service use.
            if (maxWait < 1)
            {
                OnGpsError?.Invoke("GPS initialization timed out");
                yield break;
            }

            // If the connection failed this cancels location service use.
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                OnGpsError?.Invoke("Unable to determine device location");
                yield break;
            }

            while (isActiveAndEnabled)
            {
                var gpsInfo = Input.location.lastData;
                if (gpsInfo.timestamp > _lastGpsUpdateTime)
                {
                    _lastGpsUpdateTime = gpsInfo.timestamp;
                    var location = new LatLng(gpsInfo.latitude, gpsInfo.longitude);
                    UpdatePlayerLocation(location);
                }

                yield return null;
            }

            // Stops the location service if there is no
            // need to query location updates continuously.
            Input.location.Stop();
        
    }

    private void UpdatePlayerLocation(in LatLng location)
    {
        // New GPS location data available, will lerp the player's
        // position to this new coordinate, or jump if it is far.
        _targetMapPosition = lightshipMapView.LatLngToScene(location);
        transform.position = _targetMapPosition;
    }

    public void Update()
    {
        // Update the map view position based on where our player is.
        // This will actually be last frame's position, but the map
        // update needs to happen first as the player is positioned
        // on the map relative to the offset to the tile parent node.
        //UpdateMapViewPosition();

        // Maintain the player's position on the map, and interpolate
        // to new coordinates as they come in.  Interpolate player's
        // map position without the camera offset, so that camera
        // movements don't result in lerps.  Jump rather than
        // interpolate if the coordinates are really far.

        //####################################################################################################################################
        
        /*var movementVector = _targetMapPosition - _currentMapPosition;
        var movementDistance = movementVector.magnitude;

        switch (movementDistance)
        {
            case > TeleportThreshold:
                _currentMapPosition = _targetMapPosition;
                break;

            case > WalkThreshold:
            {
                // If the player is not stationary,
                // rotate to face their movement vector
                var forward = movementVector.normalized;
                var rotation = Quaternion.LookRotation(forward, Vector3.up);
                transform.rotation = rotation;
                break;
            }
        }

        _currentMapPosition = Vector3.Lerp(
            _currentMapPosition,
            _targetMapPosition,
            Time.deltaTime);

        transform.position = _currentMapPosition;*/
        
    }

   

    public void UpdateMapViewPosition()
    {
        // Only update the map tile view periodically so as not to spam tile fetches
        if (Time.time < _lastMapViewUpdateTime + 1.0f)
        {
            return;
        }

        _lastMapViewUpdateTime = Time.time;

        // Update the map's view based on where our player is
        lightshipMapView.SetMapCenter(_targetMapPosition);
    }
}
