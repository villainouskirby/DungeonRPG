Shader "Custom/TileMap"
{
    Properties
    {
        _TileSize ("Tile Size", float) = 1.0
        _DefaultColor ("DefaultColor", color) = (1, 1, 1, 1)
        _ViewTargetMode ("On/Off ViewTarget Mode (0 Off / 1 On)", float) = 0
        _ViewTargetTileSize ("ViewTarget Tile Size", float) = 1.0
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
            float _ViewTargetMode;
            float _ViewTargetTileSize;


            // Matching TileType Texture - _TileTexture[TileType]
            UNITY_DECLARE_TEX2DARRAY(_TileTexture);

            // buffer Header Info
            // 0 -> grid X Size, 1 -> grid Y Size
            // after 3 -> tileType Data...
            StructuredBuffer<int> _TileMapBuffer;


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 tilePos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                // calc Object scale
                float scaleX = length(float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20));
                float scaleY = length(float3(unity_ObjectToWorld._m01, unity_ObjectToWorld._m11, unity_ObjectToWorld._m21));

                float2 correctViewGridPos;
                if(_ViewTargetMode == 1.0)
                    correctViewGridPos = _WorldSpaceCameraPos.xy / _ViewTargetTileSize * _TileSize;
                else
                    correctViewGridPos = _WorldSpaceCameraPos.xy;


                // calc pixel world pos by camera under (object center = camera pos)
                float2 cameraCorrectWorldPos = float2(scaleX * v.vertex.x, scaleY * v.vertex.y) + correctViewGridPos;
                
                // calc pixel grid pos
                o.tilePos = cameraCorrectWorldPos / _TileSize;

                return o;
            }

            int PosToIndex(int2 pos)
            {
                // _TIleMapBuffer[0] == Grid X Size
                return mad(pos.y, _TileMapBuffer[0], pos.x);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // if _TileMapBuffer Empty -> return _DefaultColor



                int2 tileIndex = floor(i.tilePos);
                float2 tileUV = frac(i.tilePos); 
                // _TileMapBuffer[2] == BufferSize
                int bufferSize = _TileMapBuffer[2];

                if (tileIndex.x < 0 || tileIndex.y < 0 || tileIndex.x >= _TileMapBuffer[0] || tileIndex.y >= _TileMapBuffer[1])
                    return _DefaultColor;

                int index = PosToIndex(tileIndex);
                int tileType = _TileMapBuffer[3 + index];

                if(tileType == -1)
                    return _DefaultColor;

                // finally return texture
                return UNITY_SAMPLE_TEX2DARRAY(_TileTexture, float3(tileUV, tileType));
            }
            ENDCG
        }
    }
}
