using System.Collections.Generic;
using System.Linq;
using Mahjong.Logic;
using Mahjong.Model;
using UnityEngine;

namespace GamePlay.Client.View
{
    public class PlayerRiverManager : MonoBehaviour
    {
        private const float Width = MahjongConstants.TileWidth + MahjongConstants.TileRiverGapCol;
        private const float Height = -(MahjongConstants.TileHeight + MahjongConstants.TileRiverGapRow);
        private const float Thickness = -MahjongConstants.TileThickness / 2;
        [HideInInspector] public RiverTile[] RiverTiles;
        private List<Transform> tiles = new List<Transform>();
        private List<TileInstance> tileInstances = new List<TileInstance>();

        private void Init()
        {
            // If already initialized and has items, stop.
            if (tiles != null && tiles.Count > 0) return;

            tiles = new List<Transform>();
            tileInstances = new List<TileInstance>();

            int rawCount = transform.childCount;
            Debug.Log($"[River Debug] Scanning {rawCount} objects inside River Manager...");

            for (int i = 0; i < rawCount; i++)
            {
                Transform child = transform.GetChild(i);

                // Try to find the script on the object OR its children
                var instance = child.GetComponent<TileInstance>();
                if (instance == null) instance = child.GetComponentInChildren<TileInstance>();

                if (instance != null)
                {
                    // FOUND A VALID TILE
                    tiles.Add(child);
                    tileInstances.Add(instance);

                    // Hide it initially
                    child.gameObject.SetActive(false);
                }
                else
                {
                    // FOUND JUNK (Collider? Light? Empty object?) -> IGNORE IT
                    Debug.LogWarning($"[River Debug] Ignoring object '{child.name}' - It is not a Tile.");
                }
            }

            Debug.Log($"[River Debug] Init finished. Found {tiles.Count} valid tiles.");
        }

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            // --- FIX 1: Ensure Init is called before doing anything ---
            if (tiles == null || tileInstances == null) Init();

            if (RiverTiles == null)
            {
                DisableInvalidRiver();
                return;
            }

            int validTileCount = 0;
            int lastValidRichi = -1;

            // show river tiles
            for (int i = 0; i < RiverTiles.Length; i++)
            {
                var riverTile = RiverTiles[i];
                if (riverTile.IsGone) continue;

                // Safety check: Stop if we run out of visual slots
                if (validTileCount >= tiles.Count) break;

                var t = tiles[validTileCount];
                var instance = tileInstances[validTileCount];

                if (t == null || instance == null) break; // Safety check

                t.gameObject.SetActive(true);
                if (riverTile.IsRichi)
                {
                    lastValidRichi = validTileCount;
                    t.localRotation = MahjongConstants.RichiTile;
                }
                else
                {
                    t.localRotation = MahjongConstants.RiverTile;
                }
                // calculate and set position of this tile
                t.localPosition = GetLocalPosition(validTileCount, lastValidRichi);
                instance.SetTile(riverTile.Tile);
                validTileCount++;
            }

            // disable extra tiles
            for (int i = validTileCount; i < tiles.Count; i++)
            {
                if (tiles[i] != null)
                {
                    // 1. CLEAN IT: Reset color to white before hiding
                    if (tileInstances[i] != null)
                    {
                        tileInstances[i].ShineOff();
                    }

                    // 2. HIDE IT: Now it's safe to turn off
                    tiles[i].gameObject.SetActive(false);
                }
            }
        }

        public TileInstance GetLastTile()
        {
            // --- FIX 2: Ensure Init is called here too ---
            if (tileInstances == null) Init();

            if (RiverTiles == null) return null;
            int count = RiverTiles.Count(t => !t.IsGone);
            if (count == 0 || count > tileInstances.Count) return null;

            return tileInstances[count - 1];
        }

        public void ShineOff()
        {
            // --- FIX 3: Ensure Init is called here too ---
            if (tileInstances == null) Init();

            foreach (var tile in tileInstances)
            {
                if (tile != null)
                    tile.ShineOff();
            }
        }

        private Vector3 GetLocalPosition(int validTileIndex, int lastValidRichi)
{
            int row = validTileIndex / MahjongConstants.TilesPerRowInRiver;
            int col = validTileIndex % MahjongConstants.TilesPerRowInRiver;

            if (row >= MahjongConstants.MaxRowInRiver)
            {
                col += (row - MahjongConstants.MaxRowInRiver + 1) * MahjongConstants.TilesPerRowInRiver;
                row = MahjongConstants.MaxRowInRiver - 1;
            }

            // --- COORDINATE FIX ---
            float xPos = col * Width;

            // CHANGED: Removed the negative sign. 
            // row * Height = Move Forward (Away from the start point)
            float rowPos = row * Height;

            // --- RICHI OFFSET LOGIC ---
            if (lastValidRichi >= 0)
            {
                int richiRow = lastValidRichi / MahjongConstants.TilesPerRowInRiver;

                if (richiRow < row)
                {
                    // Rows after the richi row don't need X offset, just Z
                }
                else if (validTileIndex == lastValidRichi)
                {
                    // The Richi tile itself
                    xPos += (MahjongConstants.TileHeight - MahjongConstants.TileWidth) / 2;
                }
                else
                {
                    // Tiles after the Richi tile in the same row
                    xPos += MahjongConstants.TileHeight - MahjongConstants.TileWidth;
                }
            }

            // Return: X (Left/Right), Thickness (Up/Down), Z (Forward/Back)
            return new Vector3(xPos, rowPos, Thickness);
        }

        private void DisableInvalidRiver()
        {
            // --- FIX 4: Ensure Init is called here too ---
            if (tiles == null) Init();

            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] != null)
                    tiles[i].gameObject.SetActive(false);
            }
        }
    }
}