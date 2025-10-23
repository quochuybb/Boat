

Shader "BOXTANK/MobileCrystal"
{
    Properties
    {
        
        [Header(Bump)]
        [NoScaleOffset][NormalMap] _BumpMap("Normal Map", 2D) = "bump" {}
	    _BumpScale("Scale", Range(0,4)) = 1.0
        

        [Header(Total Fresnel)]
        _FresnelPower("Fresnel Power", Range(0.1, 10)) = 1
        _FresnelScale("Fresnel Scale", Range(0, 5)) = 1
        _FresnelBias("Fresnel Bias", Range(-1, 1)) = 0

        [Header(Outside Specular)]
        _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularSmoothness("Specular Smoothness", Range(0, 1)) = 0.5
        _SpecularIntensity("Specular Intensity", Range(0, 10)) = 1

        [Header(Outside Shadow)]
        _CustomShadowStrength("Custom Shadow Strength", Range(0, 1)) = 0.5

        [Header(Outside Reflection)]
        [NoScaleOffset]_ReflectionCube("Reflection Cube", CUBE) = "black" {}
        [Toggle(_USE_HDR_REFLECTION)]_UseHDRReflection("Use HDR Reflection", float) = 0
        _ReflectionColor("Reflection Color", Color) = (1, 1, 1, 1)
        _ReflectionStrength("Reflection Strength", Range(0, 1)) = 0.5
        _ReflectionBlur("Reflection Blur", Range(0, 7)) = 0

        [Header(Inside Foreground)]
        [NoScaleOffset]_InsideFG_Tex("Inside Foreground Tex(R:Mask)", 2D) = "black" {}
        _InsideFG_TilingU("Inside Foreground Tiling U", Range(0,3)) =1
        _InsideFG_TilingV("Inside Foreground Tiling V", Range(0,3)) =1
        //_InsideFG_MaskPower("Inside Foreground Mask Power", Range(0,10)) = 1
        [HDR]_InsideFG_Color("Inside Foreground Color", Color) = (0, 0, 0, 0)
        _InsideFG_ScrollSpeed("Inside Foreground Scroll Speed(X:uSpeed,Y:vSpeed)", Vector) = (0, 0, 0, 0)
        _InsideFG_DissortStrength("Inside Foreground Dissort Noise Strength", Range(0, 1)) = 0.5
       

        [Header(Inside Background)]
        [NoScaleOffset]_InsideBG_CubeMap("Inside BG Cube Map", CUBE) = "black" {}
        [HDR]_InsideBG_ColorA("Inside BG Color A", Color) = (0, 0, 0, 0)
        [HDR]_InsideBG_ColorB("Inside BG Color B", Color) = (0, 0, 1, 0)
        _InsideBG_Power("Inside BG Power", Range(0,10)) = 1
        _InsideBG_DissortStrength("Inside BG Dissort Strength", Range(-1, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "ShaderModel"="3.5"}
        LOD 0

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
			
        	Blend One Zero
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            
            #pragma target 3.5

            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
            //#pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #pragma multi_compile_instancing

            #pragma multi_compile_fog
            
            // -------------------------------------
            // Material Keywords
            
            #include "Libs/Input.hlsl"
            #include "Libs/ForwardPass.hlsl"
            
            ENDHLSL
        }
    	
    	Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            Blend One Zero
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            
            #pragma target 3.5

            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_instancing

            #include "Libs/Input.hlsl"
            #include "Libs/ShadowCasterPass.hlsl"
            
            ENDHLSL
        }
    }
	
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}