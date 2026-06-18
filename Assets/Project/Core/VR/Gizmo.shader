Shader "Custom/Gizmo" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass {
						Tags { "LightMode" = "UniversalForward" }
						ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
								UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
								UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _Color;

            Varyings vert (Attributes IN) {
                Varyings OUT;
								UNITY_SETUP_INSTANCE_ID(IN);
								UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
								OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
								return OUT;
						}

            half4 frag (Varyings IN) : SV_Target {
								_Color.a = 0.25f;
                return _Color;
            }
            ENDHLSL
        }
        Pass {
						Tags { "Queue"="Transparent" }
						Blend SrcAlpha OneMinusSrcAlpha
						ZTest Greater
						ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
								UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
								UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _Color;

            Varyings vert (Attributes IN) {
                Varyings OUT;
								UNITY_SETUP_INSTANCE_ID(IN);
								UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
								OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
								return OUT;
						}

            half4 frag (Varyings IN) : SV_Target {
								_Color.a = 0.25f;
                return _Color;
            }
            ENDHLSL
        }
    }
}
