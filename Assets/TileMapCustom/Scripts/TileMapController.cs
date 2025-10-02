using UnityEngine;

public class TileMapController : MonoBehaviour
{
    private Material _tileMaterial;

    public void InitTileMap(GraphicsBuffer buffer, int layer, float layerOrder)
    {
        if (_tileMaterial == null)
        {
            SpriteRenderer rend = GetComponent<SpriteRenderer>();
            Material layerTileMapMaterial = rend.material;
            if (rend != null) _tileMaterial = layerTileMapMaterial;
        }

        _tileMaterial.SetBuffer("_MapDataBuffer", buffer);
        _tileMaterial.SetFloat("_LayerIndex", layer);
        _tileMaterial.SetFloat("_LayerOrder", layerOrder);
    }
}
