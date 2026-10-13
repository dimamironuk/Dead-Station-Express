Shader "Custom/GrassWind"
{
    Properties
    {
        _MainTex ("Grass Texture", 2D) = "white" {}
        _Color ("Grass Color", Color) = (1, 1, 1, 1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _WindSpeed ("Wind Speed", Float) = 2.0
        _WindStrength ("Wind Strength", Float) = 0.15
        [Toggle] _DoubleSided ("Double Sided Global Illumination", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200

        Cull Off // Двостороннє відображення трави

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float _Cutoff;
            float _WindSpeed;
            float _WindStrength;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS   : NORMAL;
            float2 uv         : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv          : TEXCOORD0;
            float3 normalWS    : TEXCOORD1;
            float3 positionWS  : TEXCOORD2;
        };

        // Анімація вітру: зсуваємо вершину по X залежно від часу та світової позиції
        float3 ApplyWind(float3 positionOS, float3 positionWS, float uvY)
        {
            float wave = sin(_Time.y * _WindSpeed + positionWS.x + positionWS.z);
            positionOS.x += wave * _WindStrength * uvY;
            return positionOS;
        }

        Varyings vert(Attributes IN)
        {
            Varyings OUT;

            // Світова позиція для розрахунку хвилі вітру (до зсуву)
            float3 worldPosForWind = TransformObjectToWorld(IN.positionOS.xyz);

            // Зсуваємо тільки верхню частину травинки (IN.uv.y)
            float3 windPositionOS = ApplyWind(IN.positionOS.xyz, worldPosForWind, IN.uv.y);

            VertexPositionInputs vertexInput = GetVertexPositionInputs(windPositionOS);

            OUT.positionHCS = vertexInput.positionCS;
            OUT.positionWS = vertexInput.positionWS;
            OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

            // Направляємо нормалі вгору, щоб трава освітлювалася рівномірно
            OUT.normalWS = float3(0, 1, 0);

            return OUT;
        }

        half4 frag(Varyings IN) : SV_Target
        {
            half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
            half4 col = texColor * _Color;

            // Alpha cutout
            clip(col.a - _Cutoff);

            // Просте освітлення від основного джерела світла (URP)
            Light mainLight = GetMainLight();
            half3 normalWS = normalize(IN.normalWS);
            half NdotL = saturate(dot(normalWS, mainLight.direction));
            half3 lighting = mainLight.color * NdotL + unity_AmbientSky.rgb;

            col.rgb *= lighting;

            return col;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 3.0
            ENDHLSL
        }

        // Прохід для тіней, щоб трава коректно кидала тінь з урахуванням вітру та cutout
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 3.0
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
