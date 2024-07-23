using System.Collections;
using System.Collections.Generic;
using Map;
using TMPro;
using UnityEngine;

public class NewReport : MonoBehaviour
{
    
    public TMP_InputField title;
    public TMP_InputField description;
    public TMP_InputField author;
    public GameObject mapController;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveReport()
    {
        mapController.GetComponent<MapController>().AddFlag(title.text, description.text, author.text);
    }
}
