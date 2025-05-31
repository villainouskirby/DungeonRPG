Shader "Custom/PixelOutline_NoBranch"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

        _Active("Active", float) = 0
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _PixelSize("Pixel Size", Float) = 1
        _Threshold("Alpha Threshold", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Lighting Off

        Pass
        {
            Tags { "LightMode"="Universal2D" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4   _MainTex_ST;
            float4   _MainTex_TexelSize;
            fixed4   _OutlineColor;
            float    _PixelSize;
            float    _Threshold;
            float    _Active;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex   = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                float2 baseTexel = float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);
                float2 texelSize = baseTexel * _PixelSize;
                float centerAlpha = tex2D(_MainTex, uv).a;

                float neighborMax = 0.0;
                neighborMax = max(neighborMax, tex2D(_MainTex, uv + float2(-1,  0) * texelSize).a);
                neighborMax = max(neighborMax, tex2D(_MainTex, uv + float2(-1,  1) * texelSize).a);
                neighborMax = max(neighborMax, tex2D(_MainTex, uv + float2( 0, -1) * texelSize).a);
                neighborMax = max(neighborMax, tex2D(_MainTex, uv + float2(-1, -1) * texelSize).a);
                neighborMax = max(neighborMax, tex2D(_MainTex, uv + float2( 0,  1) * texelSize).a);
                neighborMax = max(neighborMax, tex2D(_MainTex, uv + float2( 1, -1) * texelSize).a);
                neighborMax = max(neighborMax, tex2D(_MainTex, uv + float2( 1,  0) * texelSize).a);
                neighborMax = max(neighborMax, tex2D(_MainTex, uv + float2( 1,  1) * texelSize).a);

                float isCenterOpaque = step(_Threshold, centerAlpha);
                float isNeighborOpaque = step(_Threshold, neighborMax);
                float outlineMask = isNeighborOpaque * (1.0 - isCenterOpaque);

                return fixed4(_OutlineColor.rgb * outlineMask, _OutlineColor.a * outlineMask) * _Active;
            }
            ENDCG
        }
    }
}
