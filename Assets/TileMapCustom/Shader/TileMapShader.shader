Shader "Custom/TileMap"
{
    Properties
    {
        _TileSize ("Tile Size", float) = 1.0
        _DefaultColor ("DefaultColor", color) = (1, 1, 1, 1)
        _BlurColor ("Blur Color", color) = (1, 1, 1, 1)
        _BlurStrength ("Blur Strength", float) = 0.5
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            int _TextureSize;

            float _TileSize;
            fixed4 _DefaultColor;
            fixed4 _BlurColor;
            float _BlurStrength;

            // Global
            float4 _TileMapTargetCamera;
            float4 _PlayerPos;
            int _FOVRadius;

            // Matching TileType Texture - _TileTexture[TileType]
            UNITY_DECLARE_TEX2DARRAY(_TileTexture);

            // buffer Header Info
            // 0 -> grid X Size, 1 -> grid Y Size
            // after 3 -> tileType Data...
            StructuredBuffer<int> _MapDataBuffer;

            // buffer Header Info
            // 0 -> Default Value (0)
            // after 1 -> blur Data...
            StructuredBuffer<int> _BlurMapDataBuffer;


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 tilePos : TEXCOORD0;
                nointerpolation int3 mapDataBufferInfo : TEXCOORD1;
                nointerpolation int2 playerTileIndex : TEXCOORD2;
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

                o.mapDataBufferInfo = int3(_MapDataBuffer[0], _MapDataBuffer[1], _MapDataBuffer[2]);
                o.playerTileIndex = floor(_PlayerPos / _TileSize);

                return o;
            }

            int PosToIndex(int2 pos, int sizeX)
            {
                // indexing [x, y] to Row Array
                return mad(pos.y, sizeX, pos.x);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Get Tile Info from tilePos
                int2 tileIndex = floor(i.tilePos);
                float2 tileUV = frac(i.tilePos); 

                // Check Index
                float validX = step(0, tileIndex.x) * step(tileIndex.x, i.mapDataBufferInfo.x - 1);
                float validY = step(0, tileIndex.y) * step(tileIndex.y, i.mapDataBufferInfo.y - 1);
                float validCoord = validX * validY;

                // Get index (from tileIndex to Array index)
                int index = PosToIndex(tileIndex, i.mapDataBufferInfo.x);
                index = validCoord * index;
                // Make Index in buffer

                // Get Tile Type
                int tileType = _MapDataBuffer[3 + index];
                int safeTileType = clamp(tileType, 0, _TextureSize - 1);
                // Make Index in textureArray

                // Check Texture Index
                float validTile = step(-0.5, tileType);

                // Check Valid 0 -> Out! 1 -> Ok
                float valid = validCoord * validTile;

                // Get Target Texture
                float4 targetColor = UNITY_SAMPLE_TEX2DARRAY(_TileTexture, float3(tileUV, safeTileType));
                // 0 -> Out -> _DefaultColor / 1 -> Ok -> targetColor
                float4 returnColor = lerp(_DefaultColor, targetColor, valid);

                // Get relativePos ( by Player )
                int2 relativeIndex = (tileIndex - i.playerTileIndex) + int2(_FOVRadius, _FOVRadius);

                float validBlurX = step(0, relativeIndex.x) * step(relativeIndex.x, _FOVRadius * 2);
                float validBlurY = step(0, relativeIndex.y) * step(relativeIndex.y, _FOVRadius * 2);
                float validBlurCoord = validBlurX * validBlurY;

                int blurIndex = PosToIndex(relativeIndex, _FOVRadius * 2 + 1) + 1;
                int safeBlurIndex = blurIndex * validBlurCoord;

                int blurValid = _BlurMapDataBuffer[safeBlurIndex];
                blurValid = blurValid * validCoord;

                // Blur Process
                return lerp(returnColor, _BlurColor, _BlurStrength * (1 - blurValid));
            }
            ENDCG
        }
    }
}
