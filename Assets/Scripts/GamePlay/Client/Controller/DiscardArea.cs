using UnityEngine;

namespace GamePlay.Client.Controller
{
    public class DiscardArea : MonoBehaviour
    {
        // Drag the PARENT (The Bright Wall) into this slot in the Inspector
        public GameObject VisualBorder;

        private int activeHoldCount = 0;

        private void Awake()
        {
            // This turns off the PARENT when the game starts
            if (VisualBorder != null) VisualBorder.SetActive(false);
        }

        public void RegisterHold()
        {
            activeHoldCount++;
            UpdateVisuals();
        }

        public void UnregisterHold()
        {
            activeHoldCount--;
            if (activeHoldCount < 0) activeHoldCount = 0;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (VisualBorder == null) return;

            bool shouldShow = activeHoldCount > 0;
            Debug.Log($"[DiscardArea] Register hold number {shouldShow}");
            // This toggles the PARENT
            if (VisualBorder.activeSelf != shouldShow)
            {
                VisualBorder.SetActive(true);
            }
        }
    }
}