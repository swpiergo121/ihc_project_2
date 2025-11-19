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
        private Transform[] tiles;
        private TileInstance[] tileInstances;

        private void Init()
        {
            // If already initialized, do nothing
            if (tiles != null && tileInstances != null) return;

            int count = transform.childCount;
            tiles = new Transform[count];
            tileInstances = new TileInstance[count];
            for (int i = 0; i < count; i++)
            {
                tiles[i] = transform.GetChild(i);
                tileInstances[i] = tiles[i].GetComponent<TileInstance>();
            }
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
                if (validTileCount >= tiles.Length) break;

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
            for (int i = validTileCount; i < tiles.Length; i++)
            {
                if (tiles[i] != null)
                    tiles[i].gameObject.SetActive(false);
            }
        }

        public TileInstance GetLastTile()
        {
            // --- FIX 2: Ensure Init is called here too ---
            if (tileInstances == null) Init();

            if (RiverTiles == null) return null;
            int count = RiverTiles.Count(t => !t.IsGone);
            if (count == 0 || count > tileInstances.Length) return null;

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
            if (lastValidRichi < 0)
                return new Vector3(col * Width, row * Height, Thickness);
            else
            {
                int richiRow = lastValidRichi / MahjongConstants.TilesPerRowInRiver;
                if (richiRow < row) return new Vector3(col * Width, row * Height, Thickness);
                else if (validTileIndex == lastValidRichi)
                    return new Vector3(col * Width + (MahjongConstants.TileHeight - MahjongConstants.TileWidth) / 2, row * Height, Thickness);
                else
                    return new Vector3(col * Width + MahjongConstants.TileHeight - MahjongConstants.TileWidth, row * Height, Thickness);
            }
        }

        private void DisableInvalidRiver()
        {
            // --- FIX 4: Ensure Init is called here too ---
            if (tiles == null) Init();

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null)
                    tiles[i].gameObject.SetActive(false);
            }
        }
    }
}