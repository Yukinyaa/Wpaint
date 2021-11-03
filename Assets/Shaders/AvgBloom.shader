Shader "Custom/AvgBloom" {
	Properties{
		_MainTex("Texture", 2D) = "clear" {}
	}

		CGINCLUDE
#include "UnityCG.cginc"
#define MAX(a,b) ((a) > (b) ? (a) : (b))
#define MIN(a,b) ((a) < (b) ? (a) : (b))

	sampler2D _MainTex, _SourceTex;
	float4 _MainTex_TexelSize;
	float4 _SourceTex_TexelSize;

	float4 _addColor_pos;
	half4 _addColor;
	float2 _blobSize;
	
	float _dspl_brush_size;
	float2 _dspl_from;
	float2 _dspl_to;
	
	float2 _rmvMat_pos;


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
		if      (a.a < 0.0001) return b;
		else if (b.a < 0.0001) return a;

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
	float IsInCircle(float2 uv, float2 pointInPixel, float size){
		float circlep = distance(uv, pointInPixel) / size;
		if(circlep <= 1)
			return circlep;
		else return -1;
	
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
						if (c.a > 0.03)
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
			Pass { // 4 alpha multiplication/clipping
				CGPROGRAM
					#pragma vertex VertexProgram
					#pragma fragment FragmentProgram

					half4 FragmentProgram(Interpolators i) : SV_Target {
						half4 a = Sample(i.uv);
						a.a = (a.a * 50 > 1) ? 1 : a.a * 50;
						return a;
					}
				ENDCG
			}
			Pass { // 5 add paint
				CGPROGRAM
					#pragma vertex VertexProgram
					#pragma fragment FragmentProgram

					half4 FragmentProgram(Interpolators i) : SV_Target {
						float2 crclSize = _MainTex_TexelSize.xx / _MainTex_TexelSize.xy;
						float2 crclPos = (i.uv - _addColor_pos.xy) * crclSize;

						if (length(crclPos.xy) < 0.1) return _addColor;
						else return Sample(i.uv);

						float c = IsInCircle(i.uv, _addColor_pos.xy, _blobSize);

						_addColor.a = 0.1;

						if (c > 0)
							return _addColor;
						else return Sample(i.uv);
						
					}
				ENDCG
			}
			Pass { // 6 displacement
				CGPROGRAM
					#pragma vertex VertexProgram
					#pragma fragment FragmentProgram

					float PaintToKeep(float amt)
					{
						float r = amt / 5;
						return r < 0.01 ? MIN(amt, 0.01) : r;
					}
					half4 FragmentProgram(Interpolators i) : SV_Target {
						float k = 0.314/60;
						float2x2 rotationMatrix = float2x2(cos(k), -sin(k), sin(k), cos(k));

						float2 crclSize = _MainTex_TexelSize.xx / _MainTex_TexelSize.xy;

						float2 from = (i.uv - _dspl_from.xy) * crclSize;
						float2 to = (i.uv - _dspl_to.xy) * crclSize;

						half4 c = Sample(i.uv);

						if (length(from.xy) < _dspl_brush_size)
							c.a = PaintToKeep(c.a);


						if (length(to.xy) < _dspl_brush_size)
						{
							half4 otherColor = Sample(mul((i.uv - _dspl_to.xy), rotationMatrix) + _dspl_from.xy);
							otherColor.a -= PaintToKeep(otherColor.a);
							otherColor.a = MAX(otherColor.a, 0);
							return MAdd(c, otherColor);
						}
						else
							return c;
					}
					ENDCG
			}


	}
}