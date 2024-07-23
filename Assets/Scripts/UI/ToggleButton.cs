using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] private UnityEvent onToggleOn = new();
    [SerializeField] private UnityEvent onToggleOff = new();

    [SerializeField] private bool onlyManualToggling;
    
    private Button button;
    private bool state;
    private ColorBlock toggledOffColors;
    private ColorBlock toggledOnColors;
    
    private void Start()
    {
        button = GetComponent<Button>();
        toggledOffColors = button.colors;
        toggledOnColors = toggledOffColors;
        toggledOnColors.normalColor = toggledOnColors.highlightedColor;
        toggledOnColors.selectedColor = toggledOnColors.highlightedColor;

        if (!onlyManualToggling)
        {
            button.onClick.AddListener(Toggle);
        }
    }

    private void Toggle()
    {
        SetToggleState(!state);
    }

    public bool GetToggleState()
    {
        return state;
    }

    public void SetToggleState(bool newState)
    {
        state = newState;
        button.colors = state ? toggledOnColors : toggledOffColors;
        
        if (state)
        {
            onToggleOn.Invoke();
        }
        else
        {
            onToggleOff.Invoke();
        }
    }
}
