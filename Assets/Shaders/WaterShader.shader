Shader "Custom/WaterShader"
{
    Properties
    {
        _Color ("Color", Color) = (0.2, 0.5, 1, 0.5)
        _MainTex ("Texture", 2D) = "white" {}
        _WaveScale ("Wave Scale", Float) = 10
        _WaveHeight ("Wave Height", Float) = 0.5
        _WaveSpeed ("Wave Speed", Float) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _WaveScale;
            float _WaveHeight;
            float _WaveSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                
                // Wave effect
                float wave = sin(v.vertex.x * _WaveScale + _Time.y * _WaveSpeed) * 
                            cos(v.vertex.z * _WaveScale + _Time.y * _WaveSpeed) * _WaveHeight;
                v.vertex.y += wave;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), 3.0);
                
                fixed4 col = _Color;
                col.a = lerp(0.5, 0.8, fresnel);
                return col;
            }
            ENDCG
        }
    }
} 