Shader "Unlit/vertexShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float3 objectSpaceNormals : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                
                float spike = sin(v.vertex.x * 8.0 + _Time * 8.0) + 
                              sin(v.vertex.y * 8.0 + _Time * 15.2) + 
                              sin(v.vertex.z * 8.0 + _Time * 20.5);
                
                spike = spike * 0.4 + 0.6;
                
                o.vertex = UnityObjectToClipPos(v.vertex * spike);
                o.objectSpaceNormals = normalize(UnityObjectToViewPos(v.vertex * spike));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 N = normalize(i.objectSpaceNormals);
                
                float3 red = float3(1.0, 0.6, 0.6);
                float3 green = float3(0.6, 1.0, 0.6);
                float3 blue = float3(0.6, 0.6, 1.0);
                
                float3 color = N.x * red + N.y * blue;
                
                return col + color.rgbb;
            }
            ENDCG
        }
    }
}
