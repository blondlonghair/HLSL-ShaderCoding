Shader "Unlit/PostProcessing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Brightness ("Brightness", Float) = 1.0
        _Contrast ("Contrast", Float) = 1.0
        _Exposure ("Exposure", Float) = 1.0
        _Gamma ("Gamma", Float) = 1.0
        _Saturation ("Saturation", Float) = 1.0
        _Colorize ("Colorize", Color) = (1.0, 1.0, 1.0, 1.0)
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
            float _Brightness;
            float _Contrast;
            float _Exposure;
            float _Gamma;
            float _Saturation;
            float4 _Colorize;

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

                float c = param + 0.5;
                col = (col - float4(0.5, 0.5, 0.5, 0.5)) * _Contrast + float4(0.5, 0.5, 0.5, 0.5);
                
                col = col + float4(_Brightness,_Brightness,_Brightness,_Brightness);
                
                col = col * pow(2.0, _Exposure);
                
                col = pow(col, float4(_Gamma,_Gamma,_Gamma,_Gamma) * 2.2);
                
                col = lerp(luminescence, col, _Saturation);
                
//                 col *= _Colorize;
                
//                 float scanline = 0.5 + 0.5 * sin(i.uv.y * 1000.0);
//                 
//                 float2 center = float2(0.0, 0.0);
//                 float dist = dot(center, i.uv);
//                 
//                 dist = 1 - pow(dist, 2.0);
                
//                 //lum
//                 //shadow = 1 - lum
//                 float highlight = pow(lum, 4.0);
//                 float shadow = pow(1.0 - lum, 4.0);
//                 
//                 col += shadow * float4(0.6, 0.6, 1.0) * 0.1;
//                 col += highlight * float4(1.0, 1.0, 0.0, 1,.0) * 0.7;

                
                
                return col;// * scanline * dist;
            }
            ENDCG
        }
    }
}
