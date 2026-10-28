Shader "Custom/MyGizmo" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Tags {
			"RenderPipeline" = "UniversalPipeline"
				"Queue" = "Overlay"
				"RenderType" = "Transparent"
		}

		ZTest Always
			ZWrite Off

			Pass {
				HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

					CBUFFER_START(UnityPerMaterial)
					float4 _Color;
				CBUFFER_END

					struct Attributes {
						float4 positionOS : POSITION;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

				struct Varyings {
					float4 positionHCS : SV_POSITION;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				Varyings vert (Attributes input) {
					Varyings output = (Varyings)0;
					UNITY_SETUP_INSTANCE_ID(input);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

					output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
					return output;
				}

				half4 frag (Varyings input) : SV_Target {
					return _Color;
				}
				ENDHLSL
			}
	}
}
