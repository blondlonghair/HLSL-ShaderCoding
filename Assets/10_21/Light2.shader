Shader "Unlit/Light"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Ambient ("Ambient", Color) = (.25, .5, .5, 1)
        _Albedo ("Albedo", Color) = (.25, .5, .5, 1)
        _SpecPower ("SpecPower", Float) = 2
        _ReflectionMap ("ReflectionMap", 2D) = "white" {}
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _ReflectionMap;
            float4 _MainTex_ST;
            float4 _Ambient;
            float4 _Albedo;
            float _SpecPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = i.normal;
                float3 tolight = _WorldSpaceLightPos0.xyz;
                
                float3 reflectionvector = reflect(tolight, normal);
                float3 viewdir = float3(0.0, 0.0, 1.0);
                
                float intensity = max(0.0, dot(normal, tolight));
                
                if (intensity >0.9)
                {
                    intensity = 1.0;
                }
                else if (intensity > 0.2)
                {
                    intensity = 0.5;
                }
                else
                {
                    intensity = 0.0f;
                }
                
                float specularity = pow(max(0.0, dot(reflectionvector, viewdir)), _SpecPower);
                
                float4 lightColor = float4(1.0, 1.0, 1.0, 0.0);
                
                float4 color = _Ambient + _Albedo * intensity + specularity * lightColor;
                return color;
            }
            ENDCG
        }
    }
}
