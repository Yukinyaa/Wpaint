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

            fixed4 frag (v2f i) : SV_Target
            {
                half4 origin = tex2D(_OriginalTex, i.texcoord);
                half4 painted = tex2D(_MainTex, i.texcoord);

                fixed intensity = origin.r + origin.g + origin.b;
                fixed sum = painted.r + painted.g + painted.b;
                
                half4 col;
                //Origin Intensity를 Painted의 RGB비율대로 복구
                col.r = (painted.r / sum) * intensity;
                col.g = (painted.g / sum) * intensity;
                col.b = (painted.b / sum) * intensity;
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}
}