using UnityEngine;
using Mahjong.Model;
using GamePlay.Client.Controller;
using DG.Tweening;

// Use the Oculus Interaction namespace
using Oculus.Interaction;
using System;

[RequireComponent(typeof(Oculus.Interaction.Grabbable))]
[RequireComponent(typeof(Rigidbody))]
public class VRHandTile : MonoBehaviour
{
    private DiscardArea discardIndicator;

    public bool IsLastDraw;
    public Tile Tile { get; private set; }
    public bool IsHeld { get; private set; }

    private Grabbable oculusGrabbable;
    private Rigidbody rb;
    private Vector3 originalLocalPosition;
    private const float hoverLiftAmount = 0.02f;
    private const float AnimationDuration = 0.2f;

    // --- FIX: Correctly assigning the indicator ---
    internal void SetDiscardIndicator(GamePlay.Client.Controller.DiscardArea area)
    {
        this.discardIndicator = area;
    }

    void Awake()
    {
        oculusGrabbable = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();
        originalLocalPosition = transform.localPosition;

        oculusGrabbable.WhenPointerEventRaised += HandlePointerEvent;

    }

    private void OnDestroy()
    {
        if (oculusGrabbable != null)
        {
            oculusGrabbable.WhenPointerEventRaised -= HandlePointerEvent;
            
        }
    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        // We switch based on the type of event coming in
        switch (evt.Type)
        {
            case PointerEventType.Select:
                OnGrab();
                break;
            case PointerEventType.Unselect:
            case PointerEventType.Cancel:
                OnDrop();
                break;

        }
    }


    public void SetTile(Tile tile)
    {
        this.Tile = tile;
    }

    public void SetLock(bool isLocked)
    {
        if (oculusGrabbable != null)
        {
            oculusGrabbable.MaxGrabPoints = isLocked ? 0 : -1;
        }
    }

    // --- UPDATED STATE HANDLERS ---

    private void OnGrab()
    {
        if (oculusGrabbable.MaxGrabPoints == 0) return;

        IsHeld = true;

        // Tell the border: "I am holding a tile, add +1 to your count"
        if (discardIndicator != null)
        {
            Debug.Log($"[VRHandTile] Register hold {Tile}");

            discardIndicator.RegisterHold();
        }
    }

    private void OnDrop()
    {
        // Only run this logic if we were actually holding it
        if (IsHeld)
        {
            IsHeld = false;

            // Tell the border: "I dropped a tile, subtract -1 from your count"
            if (discardIndicator != null)
            {
                Debug.Log($"[VRHandTile] Unregister hold {Tile}");
                discardIndicator.UnregisterHold();
            }
        }
    }

    // --- PHYSICS TRIGGER ---

    private void OnTriggerEnter(Collider other)
    {
        // We check if we hit the object associated with our DiscardArea script
        if (IsHeld && discardIndicator != null && other.gameObject == discardIndicator.gameObject)
        {
            Debug.Log($"[VRHandTile] Discarding {Tile}");

            Tile tileToDiscard = Tile;
            bool wasLastDraw = IsLastDraw;

            ForceDrop(); // This handles the unregistering logic too

            ClientBehaviour.Instance.OnDiscardTile(tileToDiscard, wasLastDraw);
        }
    }

    private void ForceDrop()
    {
        // If we force drop, we must ensure we tell the border 
        // that we aren't holding it anymore!
        if (IsHeld)
        {
            IsHeld = false;
            if (discardIndicator != null)
            {
                discardIndicator.UnregisterHold();
            }
        }

        if (oculusGrabbable != null)
        {
            oculusGrabbable.enabled = false;
        }

        gameObject.SetActive(false);
    }

}