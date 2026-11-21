Shader "Custom/CyberToon"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.098, 0.827, 0.941, 1)
        _ShadowTint ("Shadow Tint", Color) = (0.039, 0.051, 0.102, 1)
        _Bands ("Toon Bands", Range(2, 6)) = 3
        _BandSoftness ("Band Softness", Range(0.005, 0.15)) = 0.03
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _OutlineColor ("Outline Color", Color) = (0.039, 0.051, 0.102, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.02)) = 0.0025
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowTint;
                half _Bands;
                half _BandSoftness;
                half4 _EmissionColor;
                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(posInputs.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 albedo = tex.rgb * _BaseColor.rgb;

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                half3 N = normalize(input.normalWS);
                half3 L = normalize(mainLight.direction);
                half ndotl = saturate(dot(N, L)) * mainLight.shadowAttenuation;

                half bands = max(2.0h, floor(_Bands + 0.5h));
                half scaled = ndotl * bands;
                half bandIndex = floor(scaled);
                half fracPart = frac(scaled);
                half edge = smoothstep(0.5h - _BandSoftness, 0.5h + _BandSoftness, fracPart);
                half quantized = saturate((bandIndex + edge) / bands);

                half3 ambient = SampleSH(N) * 0.35h;
                half3 litColor = lerp(_ShadowTint.rgb, albedo * mainLight.color, quantized);
                half3 color = litColor + albedo * ambient + _EmissionColor.rgb;
                color = MixFog(color, input.fogCoord);

                return half4(color, 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowTint;
                half _Bands;
                half _BandSoftness;
                half4 _EmissionColor;
                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END

            Varyings vertOutline(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                float4 clipPos = posInputs.positionCS;

                if (_OutlineWidth > 0.0001h)
                {
                    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                    float3 normalVS = TransformWorldToViewDir(normalWS, true);
                    float2 offset = normalize(normalVS.xy + float2(1e-5, 1e-5)) * (_OutlineWidth * clipPos.w);
                    clipPos.xy += offset;
                }

                output.positionCS = clipPos;
                return output;
            }

            half4 fragOutline(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
