Shader "Tilemap/LitTilemap"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _DefaultColor("DefaultColor", color) = (0, 0, 0, 1)

        _FogColor       ("Fog Color", Color) = (0.7, 0.8, 1.0, 1)
        _DistanceStart  ("Dist Fog Start", Float) = 10
        _DistanceEnd    ("Dist Fog End",   Float) = 30
        _DistanceCorrect    ("Dist Fog Correct",   Float) = 5
        _ExtHeightFactor("Height Num", Float) = 0
        _HeightStrength ("Height Mix", Range(0,1)) = 0

        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment

            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
            #pragma multi_compile _ DEBUG_DISPLAY

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2  uv          : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0;
                half2   lightingUV  : TEXCOORD1;
                float2  tilePos     : TEXCOORD2;
                float2  worldPos    : TexCOORD3;
                nointerpolation int2 playerTilePos : TEXCOORD4;
                #if defined(DEBUG_DISPLAY)
                float3  positionWS  : TEXCOORD5;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            half4 _MainTex_ST;
            float4 _Color;
            half4 _RendererColor;
            float _DistanceStart;
            float _DistanceEnd;
            float _DistanceCorrect;
            float4 _FogColor;

            /// TileMap
            // Global
            int _TextureSize;
            float4 _TileMapTargetCamera;
            float4 _PlayerPos;
            int _CenterChunkX;
            int _CenterChunkY;
            int _ViewBoxSize;
            int _ViewChunkSize;
            int _ChunkSize;
            int _CurrentHeight;
            float _TileSize;
            float4 _DefaultColor;

            // Matching TileType Texture - _TileTexture[TileType]
            Texture2DArray _TileTexture;
            SamplerState sampler_TileTexture;

            // Only MapData
            StructuredBuffer<int> _MapDataBuffer;
            // Chunk Mapping
            StructuredBuffer<int> _MappingBuffer;
            /// TileMap

            #if USE_SHAPE_LIGHT_TYPE_0
            SHAPE_LIGHT(0)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_1
            SHAPE_LIGHT(1)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif


            int PosToIndex(int2 pos, int sizeX)
            {
                // indexing [x, y] to Row Array
                return mad(pos.y, sizeX, pos.x);
            }


            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(v.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);

                o.color = v.color * _Color * _RendererColor;

                // TileMap Logic
                float scaleX = length(float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20));
                float scaleY = length(float3(unity_ObjectToWorld._m01, unity_ObjectToWorld._m11, unity_ObjectToWorld._m21));
                float2 cameraCorrectWorldPos = float2(scaleX * v.positionOS.x, scaleY * v.positionOS.y) + _TileMapTargetCamera.xy;
                o.worldPos = cameraCorrectWorldPos;
                o.tilePos = cameraCorrectWorldPos / _TileSize;
                o.playerTilePos = floor(_PlayerPos / _TileSize);

                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                //TileMap Logic
                int2 chunkPos = floor(i.tilePos / _ChunkSize);

                int sideViewChunkSize = (int)((_ViewChunkSize - 1) * 0.5);

                int2 localChunkPos = chunkPos - int2(_CenterChunkX, _CenterChunkY) + sideViewChunkSize;
                float2 localPos = fmod(i.tilePos, _ChunkSize);
                int2 localTileIndex = floor(localPos);
                float2 tileUV = frac(i.tilePos);

                int localChunkIndex = PosToIndex(localChunkPos, _ViewChunkSize);
                int localChunkOffset = _MappingBuffer[localChunkIndex];

                int2 distance = abs(_PlayerPos - i.worldPos);
                int maxDistance = max(distance.x, distance.y);
                
                float validCoord = step(maxDistance, _ViewBoxSize);

                int index = localChunkOffset * _ChunkSize * _ChunkSize + PosToIndex(localTileIndex, _ChunkSize);
                index = validCoord * index;

                int tileType = _MapDataBuffer[index];

                float validTile = step(-0.5, tileType);
                int safeTileType = clamp(tileType, 0, _TextureSize - 1);

                float valid = validCoord * validTile;

                float4 targetColor = _TileTexture.Sample(sampler_TileTexture, float3(tileUV, safeTileType));

                const half4 main = i.color * targetColor;
                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);

                 // Fog Logic
                float  distXY  = length(_PlayerPos.xy - i.worldPos.xy);
                distXY += _DistanceCorrect;
                float  distFog = clamp(distXY, _DistanceStart, _DistanceEnd) / _DistanceEnd;

                // temp
                //float  heightFog = abs(_ExtHeightFactor - _CurrentHeight);
                //float  fogFactor = lerp(distFog, heightFog, saturate(_HeightStrength));
                float fogFactor = distFog;

                float3 fogedColor = lerp(main.rgb, _FogColor.rgb, fogFactor);
                // End Fog Logic

                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(fogedColor.rgb, main.a, mask, surfaceData);
                InitializeInputData(i.uv, i.lightingUV, inputData);
                float4 lightedColor = CombinedShapeLightShared(surfaceData, inputData);

                return lerp(_DefaultColor, lightedColor, valid);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
