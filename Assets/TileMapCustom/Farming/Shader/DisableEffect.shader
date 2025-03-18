Shader "Farm/DisableEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _IsDisable ("IsDisable", float) = 0
        _BlurColor ("Blur Color", color) = (1, 1, 1, 1)
        _BlurStrength ("Blur Strength", float) = 0.8
        _BlurCorrect ("Blur Correct", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _IsDisable;
            float4 _BlurColor;
            float _BlurStrength;
            float _BlurCorrect;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float validBlur = _BlurStrength * _IsDisable + _BlurStrength * (1 - _BlurCorrect);
                validBlur = clamp(validBlur, 0, _BlurStrength);

                fixed4 blurColor = lerp(col, _BlurColor, validBlur);

                return blurColor;
            }
            ENDCG
        }
    }
}
