Shader "Custom/AvgBloom" {
	Properties{
		_MainTex("Texture", 2D) = "clear" {}
	}

		CGINCLUDE
#include "UnityCG.cginc"

	sampler2D _MainTex, _SourceTex;
	float4 _MainTex_TexelSize;
	float4 _SourceTex_TexelSize;

	float4 _addColor_pos;
	half4 _addColor;

	struct VertexData {
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct Interpolators {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	Interpolators VertexProgram(VertexData v) {
		Interpolators i;
		i.pos = UnityObjectToClipPos(v.vertex);
		i.uv = v.uv;
		return i;
	}

	half4 Sample(float2 uv) {
		return tex2D(_MainTex, uv).rgba;
	}

	#define AddIfNotTranparent(X) (X) // ((X).a < 0.0001 ? float4(0,0,0,0) : (X))
	half4 MAdd(half4 a, half4 b)
	{
		half4 c;
		if      (a.a < 0.01) return b;
		else if (b.a < 0.01) return a;

		half alphaSum = (a.a + b.a);

		if (alphaSum <= 0.01) return half4(0, 0, 0, 0);
		c.r = (a.r * a.a + b.r * b.a) / alphaSum;
		c.g = (a.g * a.a + b.g * b.a) / alphaSum;
		c.b = (a.b * a.a + b.b * b.a) / alphaSum;
		c.a = alphaSum;

		return c;
	}

	half4 SampleBlur(float2 uv, float delta) {
		float2 o = _MainTex_TexelSize.xy * delta;
		half4 c = MAdd(
			MAdd(Sample(float2(uv.x + o.x, uv.y)), Sample(float2(uv.x - o.x, uv.y))),
			MAdd(Sample(float2(uv.x, uv.y + o.y)), Sample(float2(uv.x, uv.y - o.y)))
		);
		c.a /= 4;
		return c;
	}


	ENDCG

		SubShader{
			Cull Off
			ZTest Always
			ZWrite Off

			Pass { // 0 ClipAlpha
				CGPROGRAM
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram

					half4 FragmentProgram(Interpolators i) : SV_Target {
						half4 c = Sample(i.uv);
						if (c.a > 0.3)
						{
							c.a /= 2;
							return c;
						}
							
						else
							return half4(0, 0, 0, 0);
					}
				ENDCG
			}

			Pass { // 1 Blur
				CGPROGRAM
					#pragma vertex VertexProgram
					#pragma fragment FragmentProgram

					half4 FragmentProgram(Interpolators i) : SV_Target {
						return SampleBlur(i.uv, 1);
					}
				ENDCG
			}
			Pass { // 2 Custom Subtractive
				CGPROGRAM
					#pragma vertex VertexProgram
					#pragma fragment FragmentProgram

					half4 FragmentProgram(Interpolators i) : SV_Target {
						half4 ogColor = tex2D(_SourceTex, i.uv);
						half4 subColor = Sample(i.uv);
						if(ogColor.a <= 0 || subColor.a <= 0) return ogColor;
						subColor.a = ogColor.a - subColor.a;
						return subColor;
					}
				ENDCG
			}
			Pass { // 3 Custom additive
				CGPROGRAM
					#pragma vertex VertexProgram
					#pragma fragment FragmentProgram

					half4 FragmentProgram(Interpolators i) : SV_Target {
						half4 a = tex2D(_SourceTex, i.uv);
						half4 b = Sample(i.uv);

						return MAdd(a, b);
					}
				ENDCG
			}
			Pass { // 4 alpha clipping
				CGPROGRAM
					#pragma vertex VertexProgram
					#pragma fragment FragmentProgram

					half4 FragmentProgram(Interpolators i) : SV_Target {
						half4 a = Sample(i.uv);
						a.a = (a.a * 10 > 1) ? 1: a.a * 10;
						return a;
					}
				ENDCG
			}
			Pass { // 5 color shit
				CGPROGRAM
					#pragma vertex VertexProgram
					#pragma fragment FragmentProgram

					half4 FragmentProgram(Interpolators i) : SV_Target {
						float2 crclSize = _MainTex_TexelSize.xy / _SourceTex_TexelSize.xy;
						float2 crclPos = (i.uv - _addColor_pos.xy) / crclSize;

						if (abs(crclPos.x) + abs(crclPos.y) < 0.1) return _addColor;
						else return Sample(i.uv);
					/*

						half4 c = tex2D(_SourceTex, i.uv);//; +_addColor;
						if (c.a = 0) c = Sample(i.uv);
						return c;
					*/
						
					}
				ENDCG
			}

	}
}