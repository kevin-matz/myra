using System;
using UnityEngine;
using UnityEngine.UI;

public class AssetsGallery : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform categoryPanel;
    [SerializeField] private RectTransform assetsPanel;
    
    [SerializeField] private GameObject assetUiCategoryPrefab;
    [SerializeField] private GameObject assetUiItemPrefab;

    [HideInInspector] public GameObject SelectedPrefab;
    
    [Serializable]
    public struct AssetGalleryCategory
    {
        public string Id;
        public Sprite Image;
        public AssetGalleryItem[] Children;
    }
    
    [Serializable]
    public struct AssetGalleryItem
    {
        public Sprite Image;
        public GameObject Prefab;
    }
    
    [SerializeField] private AssetGalleryCategory[] categories;

    private void Start()
    {
        foreach (var category in categories)
        {
            var uiCategory = Instantiate(assetUiCategoryPrefab, categoryPanel.transform);
            uiCategory.transform.GetChild(0).GetComponent<Image>().sprite = category.Image;
            uiCategory.GetComponent<Button>().onClick.AddListener(() =>
            {
                OpenCategory(category.Id);
            });
        }
    }

    private void ClearAssetsPanel()
    {
        /* Start at index 1 so the back button will not be destroyed */
        for (var i = 1; i < assetsPanel.transform.childCount; i++)
        {
            Destroy(assetsPanel.transform.GetChild(i).gameObject);
        }
    }

    private void OpenCategory(string id)
    {
        foreach (var category in categories)
        {
            if (category.Id != id)
                continue;

            foreach (var galleryItem in category.Children)
            {
                var uiItem = Instantiate(assetUiItemPrefab, assetsPanel.transform);
                uiItem.transform.GetChild(0).GetComponent<Image>().sprite = galleryItem.Image;
                uiItem.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectedPrefab = galleryItem.Prefab;
                });
            }
            TogglePanel();
            return;
        }

        throw new ArgumentException("Unable to find requested category id");
    }
    
    public void TogglePanel()
    {
        if (scrollRect.content == categoryPanel)
        {
            scrollRect.content = assetsPanel;
            categoryPanel.gameObject.SetActive(false);
            assetsPanel.gameObject.SetActive(true);
        }
        else
        {
            scrollRect.content = categoryPanel;
            categoryPanel.gameObject.SetActive(true);
            assetsPanel.gameObject.SetActive(false);
            ClearAssetsPanel();
        }

        scrollRect.horizontalNormalizedPosition = 0;
    }
}
