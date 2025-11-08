using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class HighlightWallOnGrab : MonoBehaviour
{
    [Header("References")]
    public GameObject wallObject;                     // The wall to affect
    public Texture highlightTexture;                  // Texture shown while grabbing

    private Material wallMaterial;
    private Texture originalTexture;

    private HandGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<HandGrabInteractable>();
    }

    void Start()
    {
        if (wallObject == null)
        {
            Debug.LogError("Wall Object not assigned on " + gameObject.name);
            return;
        }

        Renderer renderer = wallObject.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("No Renderer found on wallObject or its children!");
            return;
        }

        wallMaterial = renderer.material;
        originalTexture = wallMaterial.mainTexture;

        if (grabInteractable != null)
        {
            grabInteractable.WhenPointerEventRaised += HandlePointerEvent;
        }
        else
        {
            Debug.LogError("No HandGrabInteractable found on " + gameObject.name);
        }
    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (wallMaterial == null) return;

        if (evt.Type == PointerEventType.Select)
        {
            // When grabbed
            if (highlightTexture != null)
                wallMaterial.mainTexture = highlightTexture;
        }
        else if (evt.Type == PointerEventType.Unselect)
        {
            // When released
            wallMaterial.mainTexture = originalTexture;
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
            grabInteractable.WhenPointerEventRaised -= HandlePointerEvent;
    }
}
