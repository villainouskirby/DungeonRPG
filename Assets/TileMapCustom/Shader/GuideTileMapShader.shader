Shader "Custom/GuideTileMap"
{
    Properties
    {
        _GuideTileSize ("Guide Tile Size", float) = 1.0
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

            float _GuideTileSize;
            float _TileSize;
            fixed4 _DefaultColor;
            fixed4 _BlurColor;
            float _BlurStrength;

            float _ScaleX;
            float _ScaleY;

            // Global
            float4 _TileMapTargetCamera;


            // Matching TileType Texture - _TileTexture[TileType]
            UNITY_DECLARE_TEX2DARRAY(_TileTexture);

            // buffer Header Info
            // 0 -> grid X Size, 1 -> grid Y Size
            // after 3 -> tileType Data...
            StructuredBuffer<int> _MapDataBuffer;
            // buffer Header Info
            // 0 -> Default Value (1)
            // after 1 -> blur Data...

            // BlurMapDataArray sort by row
            StructuredBuffer<int> _BlurMapDataBufferRow;
            // BlurMapDataArray sort by column
            StructuredBuffer<int> _BlurMapDataBufferColumn;


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
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                float2 correctUV = v.uv;
                correctUV = correctUV - 0.5;

                // calc pixel world pos by camera under (object center = camera pos)
                float2 cameraCorrectWorldPos = float2(_ScaleX * correctUV.x, _ScaleY * correctUV.y) + _TileMapTargetCamera.xy / _TileSize * _GuideTileSize;
                // calc pixel grid pos
                o.tilePos = cameraCorrectWorldPos / _GuideTileSize;

                o.mapDataBufferInfo = int3(_MapDataBuffer[0], _MapDataBuffer[1], _MapDataBuffer[2]);
                return o;
            }

            int PosToRowIndex(int2 pos, int sizeX)
            {
                // indexing [x, y] to Row Array
                return mad(pos.y, sizeX, pos.x);
            }

            int PosToColumnIndex(int2 pos, int sizeY)
            {
                // indexing [x, y] to Column Array
                return mad(pos.x, sizeY, pos.y);
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
                int rowIndex = PosToRowIndex(tileIndex, i.mapDataBufferInfo.x);
                int columnIndex = PosToColumnIndex(tileIndex, i.mapDataBufferInfo.y);
                int safeRowIndex = validCoord * rowIndex;
                int safeColumnIndex = validCoord * columnIndex;
                // Make Index in buffer

                // Get Tile Type
                int tileType = _MapDataBuffer[3 + safeRowIndex];
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

                int blurValidRow = _BlurMapDataBufferRow[safeRowIndex];
                int blurValidColumn = _BlurMapDataBufferColumn[safeColumnIndex];

                int blurValid = blurValidRow + blurValidColumn;
                blurValid = clamp(blurValid, 0, 1);

                // Blur Process
                return lerp(_BlurColor, returnColor, (1-_BlurStrength) * (1-blurValid));
            }
            ENDCG
        }
    }
}
