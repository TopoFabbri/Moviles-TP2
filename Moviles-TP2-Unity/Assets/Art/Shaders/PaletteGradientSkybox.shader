Shader "Custom/PaletteGradientSkybox"
{
    Properties
    {
        _ZenithColor ("Zenith Color", Color) = (0.141, 0.184, 0.329, 1)
        _HorizonColor ("Horizon Color", Color) = (0.071, 0.090, 0.165, 1)
        _NadirColor ("Nadir Color", Color) = (0.039, 0.051, 0.102, 1)
        _HorizonSharpness ("Horizon Sharpness", Range(0.5, 6.0)) = 2.2
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 dirWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _ZenithColor;
                half4 _HorizonColor;
                half4 _NadirColor;
                half _HorizonSharpness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.dirWS = input.positionOS.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half y = normalize(input.dirWS).y;
                half upT = pow(saturate(y), 1.0h / _HorizonSharpness);
                half downT = pow(saturate(-y), 1.0h / _HorizonSharpness);

                half3 col = lerp(_HorizonColor.rgb, _ZenithColor.rgb, upT);
                col = lerp(col, _NadirColor.rgb, downT);
                return half4(col, 1.0h);
            }
            ENDHLSL
        }
    }
}
