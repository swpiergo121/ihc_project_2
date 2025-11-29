using System.Collections;
using System.Collections.Generic;
using Mahjong.Logic;
using Mahjong.Model;
using GamePlay.Client.Controller;

using UnityEngine;


namespace GamePlay.Client.View
{
    public class PlayerHandManager : MonoBehaviour
    {
        public Transform handHolder;
        public Transform drawnHolder;
        [HideInInspector] public int Count;
        [HideInInspector] public IList<Tile> HandTiles = null;
        [HideInInspector] public Tile? LastDraw = null;
        private Transform[] handTileTransforms;
        private TileInstance[] handTileInstances;
        private Transform lastDrawTransform;
        private TileInstance lastDrawInstance;
        private bool discarding = false;
        private WaitForSeconds discardingWait = new WaitForSeconds(MahjongConstants.PlayerHandTilesSortDelay);
        private VRHandTile[] handTileVR;
        private VRHandTile lastDrawVR;
        [Header("VR Setup")]
        public DiscardArea HandDiscardBorder;

        // Add this variable to the class to remember the previous frame's data
        private Tile? _cachedLastDraw = null;

        public void AssignDiscardBorder(DiscardArea border)
        {
            HandDiscardBorder = border;

            // Pass it down to the VR tiles
            if (handTileVR != null)
            {
                foreach (var tile in handTileVR)
                {
                    if (tile != null) tile.SetDiscardIndicator(HandDiscardBorder);
                }
            }

            if (lastDrawVR != null) lastDrawVR.SetDiscardIndicator(HandDiscardBorder);
        }

        private void OnEnable()
        {
            handTileTransforms = new Transform[handHolder.childCount];
            handTileInstances = new TileInstance[handHolder.childCount];
            handTileVR = new VRHandTile[handHolder.childCount]; // New line

            for (int i = 0; i < handHolder.childCount; i++)
            {
                handTileTransforms[i] = handHolder.GetChild(i);
                handTileInstances[i] = handTileTransforms[i].GetComponent<TileInstance>();
                handTileVR[i] = handTileTransforms[i].GetComponent<VRHandTile>(); // New line
                                                                                  // 2. ASSIGN THE BORDER HERE
                if (handTileVR[i] != null)
                {
                    handTileVR[i].SetDiscardIndicator(HandDiscardBorder);
                }
            }

            lastDrawTransform = drawnHolder.GetChild(0);
            lastDrawInstance = lastDrawTransform.GetComponent<TileInstance>();
            lastDrawVR = lastDrawTransform.GetComponent<VRHandTile>(); // New line
            if (lastDrawVR != null)
            {
                lastDrawVR.IsLastDraw = true;
                lastDrawVR.SetDiscardIndicator(HandDiscardBorder); // Assign here too
            }
        }

        private void Update()
        {
            if (!discarding)
            {
                HoldTiles();
                LastDrawTile();
            }
        }

        private void HoldTiles()
        {
            if (Count > handHolder.childCount)
            {
                Debug.LogWarning($"Not enough tiles to show, cap to {handHolder.childCount}");
            }
            for (int i = 0; i < handHolder.childCount; i++)
            {
                handTileTransforms[i].gameObject.SetActive(i < Count);
            }
            if (HandTiles == null) return;
            for (int i = 0; i < HandTiles.Count; i++)
            {
                handTileInstances[i].SetTile(HandTiles[i]);
            }

            if (HandTiles == null) return;

            for (int i = 0; i < HandTiles.Count; i++)
            {
                handTileInstances[i].SetTile(HandTiles[i]); // Update the visual
                handTileVR[i].SetTile(HandTiles[i]);        // Update the interaction logic
            }
        }

        private void LastDrawTile()
        {
            if (LastDraw == null)
            {
                // lastDrawTransform.gameObject.SetActive(false);
                return;
            }
            lastDrawInstance.SetTile((Tile)LastDraw); // Update the visual
            lastDrawVR.SetTile((Tile)LastDraw);       // Update the interaction logic
            var p = drawnHolder.transform.localPosition;
            drawnHolder.transform.localPosition = new Vector3(
                Count * MahjongConstants.HandTileWidth + MahjongConstants.LastDrawGap, p.y, p.z);
        }

        public void DiscardTile(bool discardingLastDraw)
        {
            // 1. If there is no data, hide the object and reset cache
            if (LastDraw == null)
            {
                if (lastDrawTransform.gameObject.activeSelf)
                    lastDrawTransform.gameObject.SetActive(false);

                _cachedLastDraw = null;
                return;
            }

            // 2. Check: Is this a BRAND NEW tile we just drew?
            // We compare the current data (LastDraw) with the history (_cachedLastDraw).
            // (Tile? comparison works automatically in C#).
            bool isNewDraw = !object.Equals(LastDraw, _cachedLastDraw);

            // Update the cache for next frame
            _cachedLastDraw = LastDraw;

            // 3. If it IS a new draw, we MUST force it to appear.
            if (isNewDraw)
            {
                lastDrawTransform.gameObject.SetActive(true);
                // Also update the visuals immediately
                lastDrawInstance.SetTile((Tile)LastDraw);
                if (lastDrawVR != null) lastDrawVR.SetTile((Tile)LastDraw);
            }

            // 4. THE FIX: 
            // If it is NOT a new draw, check if the object is hidden.
            // If it is hidden, it means the player physically threw it.
            // DO NOT turn it back on.
            if (!lastDrawTransform.gameObject.activeSelf)
            {
                return;
            }

            // 5. Standard Visual Update (Only runs if the tile is visibly active)
            lastDrawInstance.SetTile((Tile)LastDraw);
            if (lastDrawVR != null) lastDrawVR.SetTile((Tile)LastDraw);

            var p = drawnHolder.transform.localPosition;
            drawnHolder.transform.localPosition = new Vector3(
                Count * MahjongConstants.HandTileWidth + MahjongConstants.LastDrawGap, p.y, p.z);
        }

        private IEnumerator StopDiscarding()
        {
            yield return discardingWait;
            discarding = false;
        }

        public void OpenUp()
        {
            // Reveal hand tiles
            handHolder.localRotation = Quaternion.Euler(90, 0, 0);
            var p = handHolder.localPosition;
            handHolder.localPosition = new Vector3(p.x, MahjongConstants.TileThickness / 2, p.z);
            // Reveal last draw
            drawnHolder.localRotation = Quaternion.Euler(90, 0, 0);
            p = drawnHolder.localPosition;
            drawnHolder.localPosition = new Vector3(p.x, MahjongConstants.TileThickness / 2, p.z);
        }

        public void StandUp()
        {
            // Un-reveal hand tiles
            handHolder.localRotation = Quaternion.Euler(0, 0, 0);
            var p = handHolder.localPosition;
            handHolder.localPosition = new Vector3(p.x, 0, p.z);
            // Un-reveal last draw
            drawnHolder.localRotation = Quaternion.Euler(0, 0, 0);
            p = drawnHolder.localPosition;
            drawnHolder.localPosition = new Vector3(p.x, 0, p.z);
        }

        public void CloseDown()
        {
            // Close hand tiles
            handHolder.localRotation = Quaternion.Euler(-90, 0, 0);
            var p = handHolder.localPosition;
            handHolder.localPosition = new Vector3(p.x, MahjongConstants.TileThickness / 2, p.z);
            // Close last draw
            drawnHolder.localRotation = Quaternion.Euler(-90, 0, 0);
            p = drawnHolder.localPosition;
            drawnHolder.localPosition = new Vector3(p.x, MahjongConstants.TileThickness / 2, p.z);
        }
    }
}
