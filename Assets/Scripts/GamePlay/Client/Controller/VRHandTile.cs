using UnityEngine;
using Mahjong.Model;
using GamePlay.Client.Controller;
using DG.Tweening;

// Use the Oculus Interaction namespace
using Oculus.Interaction;

// This script now requires the Oculus Grabbable
[RequireComponent(typeof(Oculus.Interaction.Grabbable))]
[RequireComponent(typeof(Rigidbody))]
public class VRHandTile : MonoBehaviour
{
    [Tooltip("Drag your scene's 'DiscardIndicator' GameObject here.")]
    [SerializeField] private DiscardArea discardIndicator; // Your DiscardArea script from before

    public bool IsLastDraw;
    public Tile Tile { get; private set; }
    public bool IsHeld { get; private set; }

    // This is our reference to the Oculus grab script
    private Grabbable oculusGrabbable;
    private Rigidbody rb;

    // Store original position for hover animation
    private Vector3 originalLocalPosition;
    private const float hoverLiftAmount = 0.02f;
    private const float AnimationDuration = 0.2f;

    void Awake()
    {
        // Get the required components
        oculusGrabbable = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();
        originalLocalPosition = transform.localPosition;

        // --- NEW EVENT SUBSCRIPTIONS ---
        // Grabbable inherits from PointableElement, which gives us these events

        // 1. Listen for Grab/Release events
        // WhenPointerEventRaised is the main event for Select, Unselect, etc.
        oculusGrabbable.WhenPointerEventRaised += HandlePointerEvent;

      
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events when this object is destroyed
        if (oculusGrabbable != null)
        {
            oculusGrabbable.WhenPointerEventRaised -= HandlePointerEvent;
           
        }
    }

    // --- Main Event Handler ---

    // This method is called by the Grabbable script for Select, Unselect, etc.
    private void HandlePointerEvent(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:
                OnGrab();
                break;

            case PointerEventType.Unselect:
                OnDrop();
                break;

            case PointerEventType.Cancel:
                // A Cancel is a type of drop
                OnDrop();
                break;
        }
    }

    // --- Public Methods (Called by PlayerHandManager) ---
    // (These are unchanged)

    public void SetTile(Tile tile)
    {
        this.Tile = tile;
    }

    public void SetLock(bool isLocked)
    {
        // The Oculus Grabbable doesn't have a simple 'enabled' property.
        // Instead, we can control its 'MaxGrabPoints'
        if (oculusGrabbable != null)
        {
            oculusGrabbable.MaxGrabPoints = isLocked ? 0 : -1; // 0 = not grabbable, -1 = unlimited
        }
    }

    // --- Internal State Handlers ---

    private void OnGrab()
    {
        if (oculusGrabbable.MaxGrabPoints == 0) return; // Locked
        IsHeld = true;

        // NOTE: We do NOT change rb.isKinematic here.
        // The 'Grabbable' script's "Kinematic While Selected"
        // property will handle all physics changes for us.
    }

    private void OnDrop()
    {
        IsHeld = false;
        // The 'Grabbable' script will also handle reparenting
        // and physics when it's released.
    }

    // --- Physics Trigger (THE DISCARD LOGIC) ---
    // This method is IDENTICAL to the one from before.
    // It doesn't care *how* the tile is held, only *that* it is held.

    private void OnTriggerEnter(Collider other)
    {
        if (IsHeld && discardIndicator != null && other.gameObject == discardIndicator.gameObject)
        {
            Debug.Log($"[VRHandTile] Discarding {Tile} via DiscardArea");

            Tile tileToDiscard = Tile;
            bool wasLastDraw = IsLastDraw;

            // Force the hand to drop this object
            ForceDrop();

            // Tell the central controller to discard the tile.
            ClientBehaviour.Instance.OnDiscardTile(tileToDiscard, wasLastDraw);
        }
    }

    // --- Helper Methods ---

    private void ForceDrop()
    {
        // To force a drop, we tell all interactors (hands) 
        // that are currently selecting this object to Unselect.
        if (oculusGrabbable != null)
        {
            oculusGrabbable.enabled = false;
        }

        // Hide the tile.
        gameObject.SetActive(false);
    }

    // --- Hover Events (Unchanged from XRI version) ---

    private void OnHoverEnter(PointerEvent args)
    {
        if (oculusGrabbable.MaxGrabPoints == 0 || IsHeld) return;
        transform.DOLocalMoveY(originalLocalPosition.y + hoverLiftAmount, AnimationDuration);
        // ... (Hint logic)
    }

    private void OnHoverExit(PointerEvent args)
    {
        if (oculusGrabbable.MaxGrabPoints == 0 || IsHeld) return;
        transform.DOLocalMoveY(originalLocalPosition.y, AnimationDuration);
        // ... (Hint logic)
    }
}