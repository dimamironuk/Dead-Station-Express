Shader "Custom/NeonFresnelGlow"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0, 0.6, 1, 1)
        _RimColor ("Rim (Glow) Color", Color) = (0.3, 0.8, 1, 1)
        _RimPower ("Rim Power (гострота світіння)", Range(0.1, 8)) = 2.5
        _RimIntensity ("Rim Intensity (яскравість країв)", Range(0, 10)) = 3.0
        _BaseIntensity ("Base Intensity (яскравість центру)", Range(0, 10)) = 1.0
        _PulseSpeed ("Pulse Speed (швидкість пульсації)", Float) = 2.0
        _PulseAmount ("Pulse Amount (сила пульсації, 0 = вимк.)", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200

        Cull Off
        ZWrite Off
        Blend SrcAlpha One // Адитивне змішування - лазер буде "світитись" крізь інші прозорі об'єкти

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _RimColor;
            float _RimPower;
            float _RimIntensity;
            float _BaseIntensity;
            float _PulseSpeed;
            float _PulseAmount;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS   : NORMAL;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float3 normalWS    : TEXCOORD0;
            float3 viewDirWS   : TEXCOORD1;
        };

        Varyings vert(Attributes IN)
        {
            Varyings OUT;

            VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
            VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS);

            OUT.positionHCS = vertexInput.positionCS;
            OUT.normalWS = normalInput.normalWS;
            OUT.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

            return OUT;
        }

        half4 frag(Varyings IN) : SV_Target
        {
            float3 normalWS = normalize(IN.normalWS);
            float3 viewDirWS = normalize(IN.viewDirWS);

            // Fresnel: чим більше нормаль "відвернута" від камери, тим сильніше світіння на краях
            float fresnel = 1.0 - saturate(dot(normalWS, viewDirWS));
            fresnel = pow(fresnel, _RimPower);

            // Легка пульсація яскравості для "живого" неонового ефекту
            float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

            half3 baseGlow = _BaseColor.rgb * _BaseIntensity;
            half3 rimGlow = _RimColor.rgb * fresnel * _RimIntensity;

            half3 finalColor = (baseGlow + rimGlow) * pulse;

            // Альфа росте разом з fresnel, щоб краї були помітнішими, а центр — трохи прозорішим
            half alpha = saturate(_BaseColor.a * 0.6 + fresnel * _RimColor.a);

            return half4(finalColor, alpha);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardGlow"
            Tags { "LightMode" = "UniversalForward" }

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
