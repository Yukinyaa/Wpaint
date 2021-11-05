Shader "Custom/PaintableSprite" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _OriginalTex ("Original", 2D) = "white" {}
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Cull Off
	Lighting Off
	ZWrite Off
	Blend One OneMinusSrcAlpha

    SubShader {
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _OriginalTex;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                #ifdef SOFTPARTICLES_ON
                float4 projPos : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.color = v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            
            void RGV2HSV(half3 In, out half3 Out)
            {
                half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                half4 P = lerp(half4(In.bg, K.wz), half4(In.gb, K.xy), step(In.b, In.g));
                half4 Q = lerp(half4(P.xyw, In.r), half4(In.r, P.yzx), step(P.x, In.r));
                half D = Q.x - min(Q.w, Q.y);
                half E = 1e-10;
                Out = float3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), Q.x);
            }

            void HSV2RGB(half3 In, out half3 Out)
            {
                half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                half3 P = abs(frac(In.xxx + K.xyz) * 6.0 - K.www);
                Out = In.z * lerp(K.xxx, saturate(P - K.xxx), In.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half4 origin = tex2D(_OriginalTex, i.texcoord);
                half4 painted = tex2D(_MainTex, i.texcoord);
                half3 originHSV, paintedHSV;

                if (painted.a == 0) painted = half4(1, 1, 1, 1);

                RGV2HSV(origin.rgb.xyz, originHSV);
                RGV2HSV(painted.rgb.xyz, paintedHSV);
                
                half3 col3;
                paintedHSV.z = originHSV.z;
                HSV2RGB(paintedHSV, col3);
                
                half4 col = half4(col3, 1);
                
                return col;
            }
            ENDCG
        }
    }
}
}