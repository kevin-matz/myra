using System.Collections.Generic;
using UnityEngine;

public class PlaneMaterialSwitcher : MonoBehaviour
{
    public enum PlaneMaterialType
    {
        Satellite,
        Map
    }

    private MeshRenderer meshRenderer;
    
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    
    [SerializeField] private Material satelliteMaterial;
    [SerializeField] private Material mapMaterial;

    public void SetMaterial(PlaneMaterialType type)
    {
        meshRenderer.SetMaterials(type == PlaneMaterialType.Satellite
            ? new List<Material> { satelliteMaterial }
            : new List<Material> { mapMaterial });
    }
}
