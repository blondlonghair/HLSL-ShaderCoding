Shader "Unlit/BlendingHomework"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SecTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Value ("Value", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Zwrite On
            
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
            sampler2D _SecTex;
            float4 _Color;
            float _Value;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = _Color;
                float4 texcol = tex2D(_MainTex, i.uv);
                float4 tex2col = tex2D(_SecTex, i.uv);
                
                float intensity = (_Color.r + _Color.g + _Color.b) / 3.0;
                
                clip(lerp(texcol, tex2col, _Value) - sin(_Time.y));
                col.r = sin(_Time.y * 1.0);
                col.g = sin(_Time.y * 2.0);
                col.b = sin(_Time.y * 3.0);
                return col * texcol;
            }
            ENDCG
        }
    }
}
