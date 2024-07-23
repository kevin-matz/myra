using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleMap : MonoBehaviour
{
    public GameObject cube;  // Ziehe den Cube hier hinein im Unity Editor

    // Drei verschiedene Maßstäbe
    private Vector3 scale1 = new Vector3(0.1f, 0.1f, 0.1f);
    private Vector3 scale2 = new Vector3(0.3f, 0.3f, 0.3f);
    private Vector3 scale3 = new Vector3(0.5f, 0.5f, 0.5f);

    // Methoden für die Skalierung
    public void SetScale1()
    {
        cube.transform.localScale = scale1;
    }

    public void SetScale2()
    {
        cube.transform.localScale = scale2;
    }

    public void SetScale3()
    {
        cube.transform.localScale = scale3;
    }
}
