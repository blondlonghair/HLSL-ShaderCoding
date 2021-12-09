Shader "Unlit/Homework"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FilterSize ("FilterSize", Color) = (1.0, 1.0, 1.0, 1.0)
        _Gamma ("Gamma", Float) = 1.0
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FilterSize;
            float _Gamma;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float param = 0.5 + 0.4 * sin(_Time.y);
                
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float4 invert = float4(1,1,1,1) - col;
                
                float lum = (col.r * 0.3) + (col.g * 0.59) + (col.b * 0.11);//(col.r + col.g + col.b) / 3.0;
                float4 luminescence = float4(lum, lum, lum, lum);
                
                col = pow(col, float4(_Gamma, _Gamma, _Gamma, _Gamma) * 2.2);
                
//                 col =* _FilterSize;
                
                return col;
            }
            ENDCG
        }
    }
}
