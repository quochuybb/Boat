#ifndef MO_SHADOW_CASTER_PASS_INCLUDED
#define MO_SHADOW_CASTER_PASS_INCLUDED

float3 _LightDirection;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 uv     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
};

float4 GetShadowPositionHClip(Attributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}

Varyings vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);

    //output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    output.positionCS = GetShadowPositionHClip(input);
    return output;
}

half4 frag(Varyings input) : SV_TARGET
{
    // #ifdef _ALPHATEST_ON
    // half4 albedoAlpha =SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv)*_BaseColor;
    // half alpha=albedoAlpha.a;
    // clip(alpha - _Cutoff);
    // #endif
    
    return 0;
}

#endif
