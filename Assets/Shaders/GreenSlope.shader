Shader "Golf/GreenSlope"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.2, 0.6, 0.2, 1.0)
        _SlopeDirection ("Slope Direction", Vector) = (1, 0, 0, 0)
        _SlopeStrength ("Slope Strength", Range(0, 1)) = 0.2
        _GridScale ("Grid Scale", Float) = 10
        _GridColor ("Grid Color", Color) = (0.15, 0.5, 0.15, 0.3)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _SlopeDirection;
                float _SlopeStrength;
                float _GridScale;
                float4 _GridColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 baseColor = texColor * _Color;

                // Slope visualization: lines perpendicular to slope direction
                float2 slopeDir = normalize(_SlopeDirection.xz);
                float slopeDot = dot(IN.positionWS.xz, slopeDir);
                float slopeLines = frac(slopeDot * _GridScale);
                slopeLines = smoothstep(0.45, 0.5, slopeLines) - smoothstep(0.5, 0.55, slopeLines);

                // Blend slope lines with base color
                half4 finalColor = lerp(baseColor, _GridColor, slopeLines * _SlopeStrength * 0.5);

                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
