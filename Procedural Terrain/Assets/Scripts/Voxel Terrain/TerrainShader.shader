Shader "Custom/TerrainShader"
{
    Properties
    {
        _TexArr("Textures", 2DArray) = "" {}

        _MainTex ("Texture", 2D) = "white" {}
        _WallTex("WallTexture", 2D) = "white" {}
        _TexScale("Texture Scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5
        #pragma require 2darray

        sampler2D _MainTex;
        sampler2D _WallTex;
        UNITY_DECLARE_TEX2DARRAY(_TexArr);
        float _TexScale;
        float _TriplanarBlendSharpness;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_TexArr;
        };

        float inverseLerp(float a, float b, float value)
        {
            return saturate((value - a) / (b - a));
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            float3 scaledWorldPos = IN.worldPos / _TexScale;
            float3 pWeight = abs(IN.worldNormal);
            pWeight /= pWeight.x + pWeight.y + pWeight.z;

            int texIndex = floor(IN.uv_TexArr.x + 0.1);


            //float3 x1 = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(scaledWorldPos.x, scaledWorldPos.z, 0));
            //float3 x2 = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(scaledWorldPos.x, scaledWorldPos.z, 1));
            float3 projected;
     
            projected = float3(scaledWorldPos.y, scaledWorldPos.z, texIndex);
            float3 xP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.x;

            projected = float3(scaledWorldPos.x, scaledWorldPos.z, texIndex);
            float3 yP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.y;

            projected = float3(scaledWorldPos.x, scaledWorldPos.y, texIndex);
            float3 zP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.z;

            float3 textureColor = xP + yP + zP ;

            o.Albedo = textureColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
