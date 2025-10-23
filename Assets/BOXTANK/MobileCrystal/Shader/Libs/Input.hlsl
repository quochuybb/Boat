#ifndef MO_INPUT_INCLUDED
#define MO_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

///CBUFFER_START(UnityPerMaterial)
    float _BumpScale;

    //Fresnel
    float _FresnelPower;
    float _FresnelScale;
    float _FresnelBias;

    // Outside Specular
    half4 _SpecularColor;
    float _SpecularSmoothness;
    float _SpecularIntensity;

    // Outside Reflection
    float _ReflectionStrength;
    float _ReflectionBlur;
    half4 _ReflectionColor;

    // Outside Shadow
    float _CustomShadowStrength;

    // Inside Foreground
    float _InsideFG_TilingU;
    float _InsideFG_TilingV;
    //float _InsideFG_MaskPower;
    half4 _InsideFG_Color;
    float2 _InsideFG_ScrollSpeed;
    float _InsideFG_DissortStrength;

    // Inside Background
    float _InsideBG_Power;
    half4 _InsideBG_ColorA;
    half4 _InsideBG_ColorB;
    float _InsideBG_DissortStrength;

///CBUFFER_END

TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);

TEXTURECUBE(_ReflectionCube);   SAMPLER(sampler_ReflectionCube);

// Inside Foreground
TEXTURE2D(_InsideFG_Tex);                   SAMPLER(sampler_InsideFG_Tex);

// Inside Background
TEXTURECUBE(_InsideBG_CubeMap);             SAMPLER(sampler_InsideBG_CubeMap);

#endif
