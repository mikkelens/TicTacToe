Shader "Custom/Outline"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [HDR] 
        _EmissionColor("EmissionColor", Color) = (0,0,0)

        _OutlineColor ("OutlineColor", Color) = (0,0,0,0)
        _OutlineRadius ("OutlineRadius", Range(0,2)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        #pragma multi_compile_instancing

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;

        float3 _EmissionColor;

        float _OutlineRadius;


        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _OutlineColor)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            o.Albedo = c.rgb;

            o.Emission = c.rgb * _EmissionColor;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG

        pass
        {
            Cull front

            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"

            float _OutlineRadius;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _OutlineColor)
            UNITY_INSTANCING_BUFFER_END(Props)

            float4 Vert(float4 position : POSITION, float3 normal : NORMAL) : SV_POSITION
            {
                /*position.xyz += _OutlineRadius * normal;

                return UnityObjectToClipPos(position);*/
                return UnityObjectToClipPos(position * _OutlineRadius); //Works best with convex shapes
            }

            half4 Frag() : SV_Target
            {
                return UNITY_ACCESS_INSTANCED_PROP(Props, _OutlineColor);
            }

            ENDCG
        
        }
    }
}