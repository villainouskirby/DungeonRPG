Shader "Custom/GuideTileMapPlus1"
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
        Tags { "Queue" = "Overlay" "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Matching TileType Texture - _TileTexture[TileType]
            UNITY_DECLARE_TEX2DARRAY(_TileTexture);

            // buffer Header Info
            // 0 -> grid X Size, 1 -> grid Y Size
            // after 3 -> tileType Data...
            StructuredBuffer<int> _MapDataBuffer;
            // buffer Header Info
            // 0 -> Default Value (1)
            // after 1 -> blur Data..
            .
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
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);  // 월드 변환을 포함한 변환
                //o.pos.xyz += _WorldPosition.xyz;  // 월드 위치로 이동

                // 기본 UV 매핑
                o.uv = v.vertex.xy * 0.5 + 0.5;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return float4(1,1,1,0.2);
                // 텍스처 샘플링
                //return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
