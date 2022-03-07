Shader "Hidden/Pixelation"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _PixelDensity("PixelDensity", int) = 1
        _AspektRatio("AspektRatio", float) = 1.
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            int _PixelDensity;
            float _AspektRatio;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 PS = float2(_PixelDensity * _AspektRatio, _PixelDensity);
                return tex2D(_MainTex, round(i.uv * PS) / PS);
            }
            ENDCG
        }
    }
}