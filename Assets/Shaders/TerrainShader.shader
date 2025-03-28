Shader "Custom/TerrainShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PlainsTex ("Plains Texture", 2D) = "white" {}
        _ForestTex ("Forest Texture", 2D) = "white" {}
        _DesertTex ("Desert Texture", 2D) = "white" {}
        _MountainTex ("Mountain Texture", 2D) = "white" {}
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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _PlainsTex;
            sampler2D _ForestTex;
            sampler2D _DesertTex;
            sampler2D _MountainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 plainsColor = tex2D(_PlainsTex, i.uv);
                fixed4 forestColor = tex2D(_ForestTex, i.uv);
                fixed4 desertColor = tex2D(_DesertTex, i.uv);
                fixed4 mountainColor = tex2D(_MountainTex, i.uv);

                fixed4 finalColor = lerp(plainsColor, forestColor, i.color.g);
                finalColor = lerp(finalColor, desertColor, i.color.r);
                finalColor = lerp(finalColor, mountainColor, i.color.b);

                return finalColor;
            }
            ENDCG
        }
    }
} 