using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleIconButton : MonoBehaviour
{
    [SerializeField] private UnityEvent onToggleOn = new();
    [SerializeField] private UnityEvent onToggleOff = new();
    
    private Button button;
    private bool state;
    
    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Toggle);
    }

    private void Toggle()
    {
        transform.GetChild(0).gameObject.SetActive(state);
        transform.GetChild(1).gameObject.SetActive(!state);
        state = !state;

        if (state)
        {
            onToggleOn.Invoke();
        }
        else
        {
            onToggleOff.Invoke();
        }
    }

    public bool GetToggleState()
    {
        return state;
    }
}
