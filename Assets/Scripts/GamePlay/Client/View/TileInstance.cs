using Mahjong.Model;
using Managers;
using UnityEngine;

namespace GamePlay.Client.View
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TileInstance : MonoBehaviour
    {
        public Tile Tile;
        public Canvas Canvas;

        private MeshRenderer meshRenderer;
        private Color originalColor = Color.white; // Default to white
        private bool initialized = false;

        private void Awake() // CHANGED FROM OnEnable TO Awake
        {
            if (!initialized)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer.material.HasProperty("_Color"))
                {
                    originalColor = meshRenderer.material.color;
                }
                initialized = true;
            }
        }

        public void SetTile(Tile tile)
        {
            if (!initialized) Awake(); // Safety check

            if (tile.Rank == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            Tile = tile;

            if (ResourceManager.Instance != null)
            {
                var texture = ResourceManager.Instance.GetTileTexture(tile);
                if (texture != null)
                {
                    meshRenderer.material.mainTexture = texture;
                }
            }

        }

        public void Shine()
        {
           
            meshRenderer.material.color = Color.yellow;
           
        }

        public void ShineOff()
        {
           
            meshRenderer.material.color = originalColor;
            
        }
    }
}