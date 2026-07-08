Shader "Custom/FullscreenOutline" {   
		Properties {
			_Strength("Strength", Range(0, 1)) = 0
		}
    SubShader {
        HLSLINCLUDE
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
					#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        ENDHLSL

				
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite Off Cull Off
        Pass {
            Name "Outline"

            HLSLPROGRAM
            
						CBUFFER_START(UnityPerMaterial)
							float _Strength;
						CBUFFER_END

            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag (Varyings input) : SV_Target {
							float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord).rgba;
							float4 depth = float4(0, LinearEyeDepth(SampleSceneDepth(input.texcoord), _ZBufferParams) / 50, 0, 1);
							return lerp(color, depth, _Strength);
						}

						ENDHLSL
				}
		}
}
