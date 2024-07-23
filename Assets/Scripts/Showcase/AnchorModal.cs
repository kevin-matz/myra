using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnchorModal : MonoBehaviour
{
    public GameObject modal;
    public Sprite staufi;
    public Sprite bernd;
    public Sprite brunnen;
    public Sprite fhe;
    
    

    private Transform image;
    private Image imageComponent;

    private void Start()
    {
        image = modal.transform.Find("AnchorImage");
        imageComponent = image.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        var locationName = PlayerPrefs.GetString("locationName");
        switch (locationName)
        {
            case "Stauffenbergallee":
                imageComponent.sprite = staufi;
                break;
            case "Bernd das Brot":
                imageComponent.sprite = bernd;
                break;
            case "Trinkwasserspender":
                imageComponent.sprite = brunnen;
                break;
            case "FHE Karte":
                imageComponent.sprite = fhe;
                break;
        }
    }
}
