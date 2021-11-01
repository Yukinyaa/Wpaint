Shader "Custom/Bloom" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex, _SourceTex;
		float4 _MainTex_TexelSize;

		half4 _Filter;

		half _Intensity;

		struct VertexData {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct Interpolators {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		Interpolators VertexProgram (VertexData v) {
			Interpolators i;
			i.pos = UnityObjectToClipPos(v.vertex);
			i.uv = v.uv;
			return i;
		}

		half4 Sample (float2 uv) {
			return tex2D(_MainTex, uv).rgba;
		}

		half4 SampleBlur (float2 uv, float delta) {
			float3 proj = UNITY_PROJ_COORD(uv);
			half4 top = tex2D(_MainTex, proj + float2(0, -_MainTex_TexelSize[1]));
			half4 bot = tex2D(_MainTex, proj + float2(0, _MainTex_TexelSize[1]));
			half4 left = tex2D(_MainTex, proj + float2(-_MainTex_TexelSize[0], 0));
			half4 right = tex2D(_MainTex, proj + float3(_MainTex_TexelSize[0], 0));
			half4 center = tex2D(_MainTex, proj);
			return (top + bot + left + right + center * 2) / 6;
		}

		half3 Prefilter (half3 c) {
			half brightness = max(c.r, max(c.g, c.b));
			half soft = brightness - _Filter.y;
			soft = clamp(soft, 0, _Filter.z);
			soft = soft * soft * _Filter.w;
			half contribution = max(soft, brightness - _Filter.x);
			contribution /= max(brightness, 0.00001);
			return c * contribution;
		}

	ENDCG

	SubShader {
		Cull Off
		ZTest Always
		ZWrite Off

		Pass { // 0
			CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				half4 FragmentProgram (Interpolators i) : SV_Target {
					half4 c = Sample(i.uv);
					if (c.a > 0.1)
						return Sample(c - half4(0, 0, 0, 0.1));
					else
						return half4(0, 0, 0, 0);

				}
			ENDCG
		}

		Pass { // 1
			CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				half4 FragmentProgram (Interpolators i) : SV_Target {
					return half4(SampleBox(i.uv, 1), 1);
				}
			ENDCG
		}

		Pass { // 2
			Blend One One

			CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				half4 FragmentProgram (Interpolators i) : SV_Target {
					return half4(SampleBox(i.uv, 0.5), 1);
				}
			ENDCG
		}

		Pass { // 3 custom additive
			CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				half4 FragmentProgram (Interpolators i) : SV_Target {
					half4 c = tex2D(_SourceTex, i.uv);
					c.rgb += _Intensity * SampleBox(i.uv, 0.5);
					return c;
				}
			ENDCG
		}

		Pass { // 4
			CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

				half4 FragmentProgram (Interpolators i) : SV_Target {
					return half4(_Intensity * SampleBox(i.uv, 0.5), 1);
				}
			ENDCG
		}
	}
}