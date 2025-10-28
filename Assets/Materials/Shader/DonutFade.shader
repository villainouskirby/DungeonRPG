Shader "UI/DonutFade"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Inner ("Inner Radius", Range(0,1)) = 0.3
        _Outer ("Outer Radius", Range(0,1)) = 0.5
        _Curve ("Curve Strength", Range(0.1, 100)) = 5
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ UNITY_UI_CLIP_RECT

            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };
            fixed4 _Color;
            float _Inner, _Outer, _Curve;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - 0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = length(i.uv);
                float t = saturate((dist - _Inner) / (_Outer - _Inner));
                float alpha = 1.0 - pow(t, 1 / _Curve);
                alpha *= step(_Inner, dist);

                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}