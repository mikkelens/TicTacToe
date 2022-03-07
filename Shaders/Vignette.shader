Shader "Hidden/Vignette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VDistance("VDistance", float) = 1
        _VSmooth("VSmooth", float) = 0.5
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
            float _VDistance;
            float _VSmooth;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 vignette = smoothstep(_VDistance, _VDistance - (_VSmooth), length(i.uv.xy - 0.5));
                float4 tex = tex2D(_MainTex, i.uv) * vignette;
                return tex;
            }
            ENDCG
        }
    }
}
