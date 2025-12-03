using UnityEngine;
using System.Collections.Generic;

namespace GamePlay.Client.Controller
{
    public class DiscardArea : MonoBehaviour
    {
        [Tooltip("Drag the visual mesh (the glowing box) here.")]
        public GameObject VisualBorder;

        // We use a HashSet so a tile cannot be counted twice by accident
        private HashSet<VRHandTile> activeHolders = new HashSet<VRHandTile>();

        private void Awake()
        {
            if (VisualBorder != null) VisualBorder.SetActive(false);
        }

        public void RegisterHold(VRHandTile tile)
        {
            if (tile == null) return;

            // Add returns true if it wasn't already in the set
            if (activeHolders.Add(tile))
            {
                UpdateVisuals();
            }
        }

        public void UnregisterHold(VRHandTile tile)
        {
            if (tile == null) return;

            // Remove returns true if it was actually in the set
            if (activeHolders.Remove(tile))
            {
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (VisualBorder == null) return;

            // Only show border if at least 1 valid tile is registered
            bool shouldShow = activeHoldCount > 0;

            if (VisualBorder.activeSelf != shouldShow)
            {
                VisualBorder.SetActive(shouldShow);
            }
        }

        // Helper property to check count
        private int activeHoldCount => activeHolders.Count;
    }
}