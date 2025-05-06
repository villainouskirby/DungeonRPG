using UnityEngine;

public class TileMapController : MonoBehaviour
{
    private Material _tileMaterial;

    public void InitTileMap(GraphicsBuffer buffer)
    {
        if (_tileMaterial == null)
        {
            SpriteRenderer rend = GetComponent<SpriteRenderer>();
            Material layerTileMapMaterial = rend.material;
            if (rend != null) _tileMaterial = layerTileMapMaterial;
        }

        _tileMaterial.SetBuffer("_MapDataBuffer", buffer);
    }
}
