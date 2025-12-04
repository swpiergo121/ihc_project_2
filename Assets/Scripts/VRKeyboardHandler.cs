using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class VRKeyboardHandler : MonoBehaviour, IPointerClickHandler, ISelectHandler
{
    public TMP_InputField inputField;
    private TouchScreenKeyboard overlayKeyboard;

    void Start()
    {
        if (inputField == null) inputField = GetComponent<TMP_InputField>();
    }

    // Try detecting via Selection (often more reliable for InputFields)
    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("Input Field Selected! Opening Keyboard...");
        OpenKeyboard();
    }

    // Also keep Click detection just in case
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Input Field Clicked! Opening Keyboard...");
        OpenKeyboard();
    }

    private void OpenKeyboard()
    {
        // Prevent opening if already open
        if (overlayKeyboard != null && overlayKeyboard.status == TouchScreenKeyboard.Status.Visible)
            return;

        overlayKeyboard = TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.Default);

        if (overlayKeyboard != null) Debug.Log("Keyboard Open Command Sent.");
        else Debug.LogError("Failed to initialize keyboard (Are you in the Editor?)");
    }

    private void Update()
    {
        if (overlayKeyboard != null && overlayKeyboard.status == TouchScreenKeyboard.Status.Visible)
        {
            inputField.text = overlayKeyboard.text;
        }
    }
}