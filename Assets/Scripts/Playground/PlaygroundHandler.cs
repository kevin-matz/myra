using System;
using System.Collections.Generic;
using DG.Tweening;
using Helper;
using Playground.Tools;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaygroundHandler : MonoBehaviour
{
    [Serializable]
    public struct PlaygroundMapTile
    {
        public string LocationName;
        public GameObject PlaneToSpawn;
    }

    [SerializeField] private List<PlaygroundMapTile> mapTiles;

    private readonly List<PlaygroundTool> tools = new();

    private PlaygroundTool FindToolById(string id)
    {
        foreach (var tool in tools)
        {
            if (tool.Id == id)
            {
                return tool;
            }
        }

        return null;
    }
    
    public void UseTool(string id)
    {
        foreach (var tool in tools)
        {
            var wasActive = tool.IsActive;
            tool.IsActive = tool.Id == id;
            if (tool.IsActive)
            {
                tool.OnActivate();
            }
            else if (wasActive)
            {
                tool.OnDeactivate();
            }
        }
    }
    
    public void ToggleTool(string id)
    {
        UseTool(FindToolById(id).IsActive ? "none" : id);
        foreach (var tool in tools)
        {
            tool.GuiToolButton.GetComponent<ToggleButton>().SetToggleState(tool.IsActive);
        }
    }

    [SerializeField] private ModalHandler detailInfoModalHandler;
    [SerializeField] private GameObject arMarkerModal;
    
    private void Start()
    {
        var toolComponents = GetComponents<PlaygroundTool>();
        tools.AddRange(toolComponents);

        if (LocationReport.Locations == null)
        {
            StartCoroutine(LocationReport.LoadLocations(_ =>
            {
                detailInfoModalHandler.location = LocationReport.FindLocation(PlayerPrefs.GetString("locationName"));
            }));
        }
        else
        {
            detailInfoModalHandler.location = LocationReport.FindLocation(PlayerPrefs.GetString("locationName"));
        }
        
        if (!TryGetComponent(out ARTrackedImageManager trackedImageManager))
            return;

        foreach (var mapTile in mapTiles)
        {
            if (mapTile.LocationName != PlayerPrefs.GetString("locationName"))
                continue;

            trackedImageManager.trackedImagePrefab = mapTile.PlaneToSpawn;
            break;
        }
        
        trackedImageManager.trackedImagesChanged += args =>
        {
            if (!arMarkerModal.activeSelf)
                return;
            
            var markerFound = args.added.Count > 0 || (args.updated.Count > 0 && args.updated[0].trackingState == TrackingState.Tracking);
            if (markerFound)
            {
                arMarkerModal.SetActive(false);
            }
        };
    }

    [SerializeField] private ToggleButton buildingsButton;
    private readonly float[] scaleSteps = { 4.5f, 0.06f, 0.03f, 0.015f };

    /*
    0 -> 1:1
    1 -> 1:75
    2 -> 1:150
    3 -> 1:300
    */
    public void SetScale(int step)
    {
        var is1To1 = step == 0;
        if (is1To1)
        {
            ShowChangePovToolButton();
        }
        else
        {
            HideChangePovToolButton();
            var changePovTool = (ChangePovTool)FindToolById("change_pov");
            changePovTool.ResetPov();
            if (changePovTool.IsActive)
            {
                UseTool("none");
            }
        }
        
        const float animationDelay = 0.2f;
        
        // 0.015 is the original scale in order for the assets to be sized correctly
        var plane = GameObject.FindWithTag("MarkerTrackedPlane");
        plane.transform.DOScale(new Vector3(scaleSteps[step], scaleSteps[step], scaleSteps[step]), animationDelay);
        plane.transform.DOLocalMoveY(is1To1 ? -7f : 0.1f, animationDelay);
        CurrentScale = scaleSteps[step];
        
        HighlightScaleButton(step);
        
        /* "Level of detail" */
        var is1To300 = step == 3;
        if (is1To300)
        {
            if (buildingsButton.GetToggleState())
            {
                buildingsButton.SetToggleState(false);
            }
        }
        else
        {
            if (!buildingsButton.GetToggleState())
            {
                buildingsButton.SetToggleState(true);
            }
        }
    }

    public static float CurrentScale = 0.015f;

    public static GameObject GetGroundPlane()
    {
        return GameObject.FindWithTag("MarkerTrackedPlane");
    }
    
    /*---------------------------------------------------
        UI Helpers
    ---------------------------------------------------*/
    
    [SerializeField] private List<Button> scaleButtons;

    private void HighlightScaleButton(int index)
    {
        for (var i = 0; i < scaleButtons.Count; i++)
        {
            scaleButtons[i].interactable = i != index;
        }
    }

    [SerializeField] private GameObject editNoteButton;
    [SerializeField] private GameObject editNoteModal;

    public void ShowEditNoteButton()
    {
        editNoteButton.GetComponent<Button>().interactable = true;
        ((RectTransform)editNoteButton.transform).DOAnchorPosX(50, 0.25f);
    }
    
    public void HideEditNoteButton()
    {
        editNoteButton.GetComponent<Button>().interactable = false;
        ((RectTransform)editNoteButton.transform).DOAnchorPosX(-400, 0.25f);
    }
    
    [SerializeField] private GameObject deleteSelectedButton;

    public void ShowDeleteSelectedButton()
    {
        deleteSelectedButton.GetComponent<Button>().interactable = true;
        ((RectTransform)deleteSelectedButton.transform).DOAnchorPosX(50, 0.25f);
    }
    
    public void HideDeleteSelectedButton()
    {
        deleteSelectedButton.GetComponent<Button>().interactable = false;
        ((RectTransform)deleteSelectedButton.transform).DOAnchorPosX(-400, 0.25f);
    }

    public void ShowEditNoteModal()
    {
        editNoteModal.SetActive(true);
    }
    
    [SerializeField] private GameObject changePovToolButton;

    private void ShowChangePovToolButton()
    {
        ((RectTransform)changePovToolButton.transform).DOAnchorPosX(-50, 0.25f);
    }

    private void HideChangePovToolButton()
    {
        ((RectTransform)changePovToolButton.transform).DOAnchorPosX(300, 0.25f);
    }

    [SerializeField] private GameObject assetGallery;

    public void SetAssetGalleryActive(bool state)
    {
        assetGallery.SetActive(state);
    }

    [SerializeField] private GameObject pathToolButtonGroup;
    
    public void SetPathToolButtonGroupActive(bool state)
    {
        pathToolButtonGroup.SetActive(state);
    }

    [SerializeField] private ToggleIconButton satelliteMapToggleButton;
    
    public void ToggleSatelliteMapView()
    {
        GetGroundPlane().GetComponent<PlaneMaterialSwitcher>().SetMaterial(satelliteMapToggleButton.GetToggleState() ? PlaneMaterialSwitcher.PlaneMaterialType.Satellite : PlaneMaterialSwitcher.PlaneMaterialType.Map);
    }
    
    public void ShowBuildings(bool visible)
    {
        GetGroundPlane().transform.GetChild(0).gameObject.SetActive(visible);
    }
}