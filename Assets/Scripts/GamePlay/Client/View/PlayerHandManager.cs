using System.Collections;
using System.Collections.Generic;
using Mahjong.Logic;
using Mahjong.Model;
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
            }

            lastDrawTransform = drawnHolder.GetChild(0);
            lastDrawInstance = lastDrawTransform.GetComponent<TileInstance>();
            lastDrawVR = lastDrawTransform.GetComponent<VRHandTile>(); // New line
            if (lastDrawVR != null) lastDrawVR.IsLastDraw = true; // Set this prefab's role
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
            discarding = true;
            if (discardingLastDraw) lastDrawTransform.gameObject.SetActive(false);
            else
            {
                int tileIndex = Random.Range(0, Count);
                handTileTransforms[tileIndex].gameObject.SetActive(false);
            }
            StartCoroutine(StopDiscarding());
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
