Shader "Custom/PlanetShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert 

        // Use shader model 3.0 target, to get nicer looking lighting
       // #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 pos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void vert (inout appdata_base v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.pos = v.vertex.xyz;
        }

        #define PI 3.141592653589793
        
        inline float2 RadialCoords(float3 a_coords)
        {
            float3 a_coords_n = normalize(a_coords);
            float lon = atan2(a_coords_n.z, a_coords_n.x);
            float lat = acos(a_coords_n.y);
            float2 sphereCoords = float2(lon, lat) * (1.0 / PI);
            return float2(sphereCoords.x * 0.5 + 0.5, 1 - sphereCoords.y);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, RadialCoords(IN.pos)) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
}
