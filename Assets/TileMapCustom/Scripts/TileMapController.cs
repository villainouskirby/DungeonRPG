using UnityEngine;

public class TileMapController : MonoBehaviour
{
    private Material _tileMaterial;

    public void InitTileMap(GraphicsBuffer buffer)
    {
        if (_tileMaterial == null)
        {
            SpriteRenderer rend = GetComponent<SpriteRenderer>();
            if (rend != null) _tileMaterial = rend.sharedMaterial;
        }

        _tileMaterial.SetBuffer("_MapDataBuffer", buffer);
    }
}
