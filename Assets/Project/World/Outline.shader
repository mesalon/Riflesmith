Shader "Custom/Outline" {
    Properties {
			[HDR] _Color("Color", Color) = (1,1,1,1)
			[HDR] _AltColor("Alt Color", Color) = (1,1,1,1)
			_Thickness("Thickness", Int) = 1
			_Threshold("Threshold", Float) = 0.5
			[Toggle] _UseAlt("Use Alt Color", Int) = 0
    }
    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" }
				Pass {
						Tags { "LightMode" = "DepthNormals" }
						ZWrite On
						ColorMask 0
						HLSLPROGRAM
						#pragma vertex vert
						#pragma fragment frag
						#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
						struct Attributes { float4 positionOS : POSITION; };
						struct Varyings { float4 positionHCS : SV_POSITION; };
						Varyings vert(Attributes IN) {
							Varyings OUT;
							OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
							return OUT;
						}
						half frag(Varyings IN) : SV_Target { return 0; }
						ENDHLSL
				}
        Pass {
						Tags { "LightMode" = "Outlines" }
						Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
						#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            CBUFFER_START(UnityPerMaterial)
							float4 _Color;
							float4 _AltColor;
							uint _Thickness;
							float _Threshold;
							int _UseAlt;
            CBUFFER_END

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN, float4 posCS : SV_POSITION) : SV_Target {
							float2 uv = posCS;
							float t = _Thickness;
							float tl = LinearEyeDepth(LoadSceneDepth(uv + float2(-t,  t)), _ZBufferParams);
							float tc = LinearEyeDepth(LoadSceneDepth(uv + float2( 0,  t)), _ZBufferParams);
							float tr = LinearEyeDepth(LoadSceneDepth(uv + float2( t,  t)), _ZBufferParams);
							float cl = LinearEyeDepth(LoadSceneDepth(uv + float2(-t,  0)), _ZBufferParams);
							float cr = LinearEyeDepth(LoadSceneDepth(uv + float2( t,  0)), _ZBufferParams);
							float bl = LinearEyeDepth(LoadSceneDepth(uv + float2(-t, -t)), _ZBufferParams);
							float bc = LinearEyeDepth(LoadSceneDepth(uv + float2( 0, -t)), _ZBufferParams);
							float br = LinearEyeDepth(LoadSceneDepth(uv + float2( t, -t)), _ZBufferParams);
							float Gx = tl - tr + (cl - cr) * 2 + bl - br;
							float Gy = tl - bl + (tc - bc) * 2 + tr - br;
							float edge = sqrt(Gx*Gx + Gy*Gy);
							return (_UseAlt == 1 ? _AltColor : _Color) * step(_Threshold, edge);
						}
						ENDHLSL
				}
		}
}
