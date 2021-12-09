// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ColorMixing"
{
    Properties
    {
            _MainTex ("Main Texture", 2D) = "White" {}
            _SubTex ("Sub Texture", 2D) = "White" {}
            _MixValue ("MixValue", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv_1 : TEXCOORD0;
                float2 uv_2 : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv_1 : TEXCOORD0;
                float2 uv_2 : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _MainTex_ST;
            float4 _SubTex_ST;

            sampler2D _MainTex;
            sampler2D _SubTex;
            float _MixValue;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv_1 = TRANSFORM_TEX(v.uv_1, _MainTex);
                o.uv_2 = TRANSFORM_TEX(v.uv_2, _SubTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
//                 sampler2D h = lerp(_MainTex, _SubTex, 0.5);
                sampler2D h =  _SubTex;

                float4 outputTex = tex2D(h, float2(i.uv_1.x, i.uv_1.y));
                
                outputTex += _Time.y;
                
                return outputTex;
            }
            ENDCG
        }
    }
}
