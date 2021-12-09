// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Light"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
        _NormalMap ("NormalMap", 2D) = "white" {}
        _ShininessMap ("ShininessMap", 2D) = "white" {}
        
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float worldnormal : TEXCOORD5;
                float3 view : TEXCOORD2;
                float3 tangent : TEXCOORD3;
                float binormal : TEXCOORD4;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _NormalMap;
            sampler2D _ShininessMap;
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
                
                o.tangent = mul(unity_ObjectToWorld, v.tangent);
                o.binormal = cross(o.normal, o.tangent) * v.tangent;
                
                o.view = mul((float3x3)UNITY_MATRIX_MV, o.normal); 
                                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
//                 float worldnormal = normalize(i.normal);
//                 float3 worldtangent = normalize(i.tangent);
//                 float3 worldbinormal = normalize(i.binormal);
//                 
//                 float3x3 rotmtx = (worldtangent, worldbinormal, worldnormal);
//                 
//                 float4 shininess = tex2D(_ShininessMap, i.uv);
//                 
//                 float3 pixelnormal = tex2D(_NormalMap, i.uv).rgb;
//                 pixelnormal = normalize(pixelnormal * 2.0 - 1.0);
//                 
//                 pixelnormal = normalize(mul(rotmtx, pixelnormal));
//                 
//                 float3 vertexnormal = normalize(i.normal);
//                 float3 tolight = normalize(_WorldSpaceLightPos0.xyz);
//                 
//                 
//                 float3 reflectionvector = reflect(tolight, pixelnormal);
//                 float3 viewdir = float3(0.0, 0.0, 1.0);
//                 
//                 float3 halfvector = normalize(tolight + viewdir)
//                 
//                 float intensity = max(0.0, dot(pixelnormal, tolight));
//                 
//                 float specularity = pow(max(0.0, dot(reflectionvector, viewdir)), _SpecPower);
//                 
//                 float4 lightColor = float4(1.0, 1.0, 1.0, 0.0);
//                 
//                 float2 texCoords = i.view.xy * 0.5 + 0.5;
//                 float4 reflection = tex2D(_ReflectionMap, texCoords);
//                 
//                 float4 color = reflection * 0.2 + _Albedo * intensity + (specularity * shininess) * lightColor;
                float4 color = float4(0,0,0,1);
                return color;
            }
            ENDCG
        }
    }
}
