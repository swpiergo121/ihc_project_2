using UnityEngine;
using Mahjong.Model;
using GamePlay.Client.Controller;
using DG.Tweening;
using Oculus.Interaction;

[RequireComponent(typeof(Grabbable))]
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
    private Quaternion originalLocalRotation; // Store rotation too
    private const float hoverLiftAmount = 0.02f;
    private const float AnimationDuration = 0.2f;

    private bool isLocked = false;

    // --- NEW BOUNDARY SETTINGS ---
    [Header("Safety Boundaries (Meters)")]
    [Tooltip("How far can the tile go in positive X/Y/Z before resetting?")]
    public Vector3 PositiveLimits = new Vector3(0.5f, 0.5f, 0.5f);

    [Tooltip("How far can the tile go in negative X/Y/Z before resetting?")]
    public Vector3 NegativeLimits = new Vector3(0.5f, 1.0f, 0.5f); // 1.0f down allows for some falling before reset

    internal void SetDiscardIndicator(GamePlay.Client.Controller.DiscardArea area)
    {
        this.discardIndicator = area;
    }

    void Awake()
    {
        oculusGrabbable = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();

        // Capture where the tile SUPPOSED to be
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        oculusGrabbable.WhenPointerEventRaised += HandlePointerEvent;
    }

    private void OnDestroy()
    {
        if (oculusGrabbable != null) oculusGrabbable.WhenPointerEventRaised -= HandlePointerEvent;
    }

    private void OnDisable()
    {
        if (IsHeld && discardIndicator != null) discardIndicator.UnregisterHold(this);
        IsHeld = false;

        // Safety: If disabled, ensure we reset position so next time it appears correct
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
    }

    // --- NEW UPDATE LOOP ---
    private void Update()
    {
        // 1. If we are holding it, do NOT reset (let the player reach far)
        // 2. If it is locked (kinematic), it won't move anyway
        if (IsHeld || isLocked) return;

        // 3. Calculate how far we are from home
        // We use the parent's inverse transform point to get the position relative to the HandHolder
        // This handles cases where the table itself is rotated.
        Vector3 currentRelPos = transform.parent.InverseTransformPoint(transform.position);
        Vector3 diff = currentRelPos - originalLocalPosition;

        // 4. Check Boundaries
        bool needsReset = false;

        // Check X (Right / Left)
        if (diff.x > PositiveLimits.x || diff.x < -NegativeLimits.x) needsReset = true;

        // Check Y (Up / Down)
        else if (diff.y > PositiveLimits.y || diff.y < -NegativeLimits.y) needsReset = true;

        // Check Z (Forward / Back)
        else if (diff.z > PositiveLimits.z || diff.z < -NegativeLimits.z) needsReset = true;

        if (needsReset)
        {
            ResetPosition();
        }
    }

    public void ResetPosition()
    {
        Debug.Log($"[VRHandTile] Tile {Tile} went out of bounds. Resetting.");

        // 1. Kill Physics Momentum
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 2. Teleport Home
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
    }

    // ... (Rest of your existing code: HandlePointerEvent, SetTile, SetLock, Handlers) ...

    private void HandlePointerEvent(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select: OnGrab(); break;
            case PointerEventType.Unselect: OnDrop(); break;
            case PointerEventType.Cancel: OnDrop(); break;
            case PointerEventType.Hover: OnHoverEnter(); break;
            case PointerEventType.Unhover: OnHoverExit(); break;
        }
    }

    public void SetTile(Tile tile) { this.Tile = tile; }

    public void SetLock(bool locked)
    {
        this.isLocked = locked;
        if (oculusGrabbable != null) oculusGrabbable.MaxGrabPoints = locked ? 0 : -1;
        if (locked && IsHeld) ForceDrop();

        // If we lock the tile, ensure it snaps to the perfect position
        if (locked) ResetPosition();
    }

    private void OnGrab()
    {
        if (isLocked) return;
        if (oculusGrabbable.MaxGrabPoints == 0) return;
        IsHeld = true;
        if (discardIndicator != null) discardIndicator.RegisterHold(this);
    }

    private void OnDrop()
    {
        if (IsHeld)
        {
            IsHeld = false;
            if (discardIndicator != null) discardIndicator.UnregisterHold(this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isLocked && discardIndicator != null && other.gameObject == discardIndicator.gameObject)
        {
            if (!gameObject.activeSelf) return;

            Debug.Log($"[VRHandTile] Discarding {Tile}");
            Tile tileToDiscard = Tile;
            bool wasLastDraw = IsLastDraw;
            ForceDrop();
            ClientBehaviour.Instance.OnDiscardTile(tileToDiscard, wasLastDraw);
        }
    }

    private void ForceDrop()
    {
        if (oculusGrabbable != null) oculusGrabbable.enabled = false;

        if (IsHeld)
        {
            IsHeld = false;
            // Unregister visual border logic if you are using it
            if (discardIndicator != null) discardIndicator.UnregisterHold(this);
        }

        gameObject.SetActive(false);
    }

    private void OnHoverEnter()
    {
        if (isLocked || IsHeld) return;
        //transform.DOLocalMoveY(originalLocalPosition.y + hoverLiftAmount, AnimationDuration);
    }

    private void OnHoverExit()
    {
        if (isLocked || IsHeld) return;
      //  transform.DOLocalMoveY(originalLocalPosition.y, AnimationDuration);
    }
}