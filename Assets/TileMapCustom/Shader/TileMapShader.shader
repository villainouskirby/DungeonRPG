Shader "Custom/TileMap"
{
    Properties
    {
        _DefaultColor ("DefaultColor", color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Global
            int _TextureSize;
            float4 _TileMapTargetCamera;
            float4 _PlayerPos;
            int _CenterChunkX;
            int _CenterChunkY;
            int _ViewBoxSize;
            int _ViewChunkSize;
            int _ChunkSize;
            float _TileSize;
            fixed4 _DefaultColor;

            // Matching TileType Texture - _TileTexture[TileType]
            UNITY_DECLARE_TEX2DARRAY(_TileTexture);

            // Only MapData
            StructuredBuffer<int> _MapDataBuffer;
            // Chunk Mapping
            StructuredBuffer<int> _MappingBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 tilePos : TEXCOORD0;
                nointerpolation int2 playerTilePos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                // calc Object scale
                float scaleX = length(float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20));
                float scaleY = length(float3(unity_ObjectToWorld._m01, unity_ObjectToWorld._m11, unity_ObjectToWorld._m21));

                // calc pixel world pos by camera under (object center = camera pos)
                float2 cameraCorrectWorldPos = float2(scaleX * v.vertex.x, scaleY * v.vertex.y) + _TileMapTargetCamera.xy;
                // calc pixel grid pos
                o.tilePos = cameraCorrectWorldPos / _TileSize;
                // calc Palyer pixel grid pos;
                o.playerTilePos = floor(_PlayerPos / _TileSize);

                return o;
            }

            int PosToIndex(int2 pos, int sizeX)
            {
                // indexing [x, y] to Row Array
                return mad(pos.y, sizeX, pos.x);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                int2 chunkPos = floor(i.tilePos / _ChunkSize);
                int sideViewChunkSize = (_ViewChunkSize - 1) * 0.5;
                int2 localChunkPos = chunkPos - int2(_CenterChunkX, _CenterChunkY) + sideViewChunkSize;

                float2 localPos = fmod(i.tilePos, _ChunkSize);
                int2 localTileIndex = floor(localPos);
                float2 tileUV = frac(i.tilePos);


                int localChunkIndex = PosToIndex(localChunkPos, _ViewChunkSize);
                int localChunkOffset = _MappingBuffer[localChunkIndex];
                int2 distance = abs(i.playerTilePos - floor(i.tilePos));
                int maxDistance = max(distance.x, distance.y);

                // Check Index
                float validCoord = step(maxDistance, _ViewBoxSize);


                // Get index (from tileIndex to Array index)
                int index = localChunkOffset * _ChunkSize * _ChunkSize + PosToIndex(localTileIndex, _ChunkSize);
                index = validCoord * index;
                // 0 -> WrongIndex
                // Make Index in buffer

                // Get Tile Type
                int tileType = _MapDataBuffer[index];
                // Check Texture Index
                float validTile = step(-0.5, tileType);
                int safeTileType = clamp(tileType, 0, _TextureSize - 1);
                // Make Index in textureArray

                // Check Valid 0 -> Out! 1 -> Ok
                float valid = validCoord * validTile;

                // Get Target Texture
                float4 targetColor = UNITY_SAMPLE_TEX2DARRAY(_TileTexture, float3(tileUV, safeTileType));
                // 0 -> Out -> _DefaultColor / 1 -> Ok -> targetColor

                targetColor.a = step(0.01, targetColor.a);

                return lerp(_DefaultColor, targetColor, valid);
            }
            ENDCG
        }
    }
}
