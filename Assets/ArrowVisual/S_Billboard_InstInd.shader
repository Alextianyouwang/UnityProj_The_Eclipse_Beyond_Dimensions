Shader "Procedural/S_Billboard_InstInd"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        _MasterScale("MasterScale", Range(0,1)) = 1

        _ViewNormalBlend("_ViewNormalBlend", Range(0,1)) = 1
    }
    SubShader
    {
         Tags {"RenderType" = "Opaque" "Queue" = "Geometry"  

            "RenderPipeline" = "UniversalPipeline"}
         LOD 300
         Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}
            Cull off
            HLSLPROGRAM
            #pragma target 5.0

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT 


            #include "./HL_Billboard_InstInd.hlsl"

            ENDHLSL
        }

    }
        FallBack "Hidden/Universal Render Pipeline/FallbackError"

}
