#ifndef MO_FORWARD_PASS_INCLUDED
#define MO_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    
    float2 uv           : TEXCOORD0;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS                   : SV_POSITION;
    float3 positionWS                   : TEXCOORD0;
    
    float2 uv               : TEXCOORD1;

	//half3 vertexSH                  : TEXCOORD2;
	
    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z

	half  fogFactor                 : TEXCOORD7;
	
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct MySurfaceData
{
	half3 normalTS;
};

struct MyInputData
{
    float3 positionWS;
    half3 normalWS;
	half3x3 tangentToWorld;
   
    float4 shadowCoord;

	//half3 bakedGI;

    half3 viewDirectionWS;

	half3 reflectionDirWS;
};

MySurfaceData InitSurfaceData(Varyings input)
{
    MySurfaceData surfaceData=(MySurfaceData)0;
    
    surfaceData.normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv),_BumpScale);
	
    return surfaceData;
}

MyInputData InitInputData(Varyings input, inout MySurfaceData surfaceData)
{
    MyInputData inputData=(MyInputData)0;
    
    inputData.positionWS = input.positionWS;
    
    inputData.viewDirectionWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);

	inputData.tangentToWorld=half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz);
    inputData.normalWS = TransformTangentToWorld(surfaceData.normalTS,inputData.tangentToWorld);
    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = SafeNormalize(inputData.viewDirectionWS);

	//inputData.bakedGI=input.vertexSH;
	
	inputData.shadowCoord=TransformWorldToShadowCoord(input.positionWS);

	inputData.reflectionDirWS=reflect(-inputData.viewDirectionWS,inputData.normalWS);

    return inputData;
}

half CalcFresnel(half3 n, half3 v, float power, float scale, float bias) {
    float nv = saturate(dot(n, v));
    float fresnel = pow(max(0.0001,nv), power);
    return 1-saturate(fresnel * scale + bias);
}

real3 MyDecodeHDR(real4 encodedIrradiance, real4 decodeInstructions)
{
    // Take into account texture alpha if decodeInstructions.w is true(the alpha value affects the RGB channels)
    real alpha = max(decodeInstructions.w * (encodedIrradiance.a - 1.0) + 1.0, 0.0);

    // If Linear mode is not supported we can skip exponent part
    return (decodeInstructions.x * PositivePow(alpha, decodeInstructions.y)) * encodedIrradiance.rgb;
}

//blinn-phong
half3 CalcSpecular(MyInputData inputData, MySurfaceData surfaceData,Light light)
{
    half3 n=inputData.normalWS;
    half3 v=inputData.viewDirectionWS;
    half3 l=light.direction;
    half3 h = SafeNormalize(l+v);
   
    half smoothness = exp2(10*_SpecularSmoothness+1); 
    half specular=pow(max(0.0001,dot(n,h)),smoothness);
   
    half3 specularCol =  _SpecularColor.rgb*specular*_SpecularIntensity*light.color;
    specularCol*=light.shadowAttenuation*light.distanceAttenuation;
    return specularCol;
}



half3 CalcOutsideCol(MyInputData inputData, MySurfaceData surfaceData)
{
    half3 p=inputData.positionWS;

	_MainLightShadowParams.x=1;
	Light mainLight = GetMainLight(inputData.shadowCoord);
    mainLight.shadowAttenuation=LerpWhiteTo(mainLight.shadowAttenuation,_CustomShadowStrength);

	half3 direct_MainLight_SpecularPart =CalcSpecular(inputData, surfaceData,mainLight);//CalcPBRSpecTerm(n, l, v, roughness,roughness2)*brdfSpecular*mainLightRadiance;
	
	half3 direct_SubLights_SpecularPart=0;
   
#ifdef _ADDITIONAL_LIGHTS
    int subLightCount = GetAdditionalLightsCount();
         		
	for (int lightIndex = 0u; lightIndex < subLightCount; ++lightIndex)
	{
		Light subLight = GetAdditionalLight(lightIndex, p, 1);
	  
		half3 direct_SubLight_SpecularPart =CalcSpecular(inputData, surfaceData,subLight);
		
		direct_SubLights_SpecularPart+= direct_SubLight_SpecularPart;
	}
#endif
   
    half3 reflectDir =inputData.reflectionDirWS;
    float4 reflectionCube = SAMPLE_TEXTURECUBE_LOD(_ReflectionCube, sampler_ReflectionCube, reflectDir, _ReflectionBlur);
   
    #ifdef _USE_HDR_REFLECTION
        half3 reflectionCol = MyDecodeHDR(reflectionCube, unity_SpecCube0_HDR);
    #else
        half3 reflectionCol = reflectionCube;
    #endif

    reflectionCol*=_ReflectionColor;

	reflectionCol*= mainLight.shadowAttenuation;
	
    half3 indirect_SpecularPart = reflectionCol* _ReflectionStrength;
	
	half3 color =   direct_MainLight_SpecularPart + direct_SubLights_SpecularPart+ indirect_SpecularPart;

	color=min(color,8);
	
	return color;
}

half3 CalcInsideFGCol(MyInputData inputData)
{
    half3 V=inputData.viewDirectionWS;
    half3 N=inputData.normalWS;
    V=V+N*_InsideFG_DissortStrength;
    half3 viewDirTS = TransformWorldToTangentDir(V, inputData.tangentToWorld);

    float2 uv=viewDirTS.xy*0.5+0.5;
   
    uv = uv * float2(_InsideFG_TilingU,_InsideFG_TilingV)+_Time.xx * _InsideFG_ScrollSpeed;
   
    half3 fgCol = SAMPLE_TEXTURE2D(_InsideFG_Tex, sampler_InsideFG_Tex, uv).rrr ;
    //fgCol=pow(max(0.0001,fgCol), _InsideFG_MaskPower);
    fgCol.rgb*= _InsideFG_Color;

    return fgCol;
}

half3 CalcInsideBGCol(MyInputData inputData)
{
    half3 V=inputData.viewDirectionWS;
    half3 N=inputData.normalWS;

    half3 N_V = dot(N, V) * V;
    half3 Offset = N-N_V ;
    Offset *=_InsideBG_DissortStrength;

    half3 VV=(V+Offset);

    half tt = SAMPLE_TEXTURECUBE(_InsideBG_CubeMap, sampler_InsideBG_CubeMap, VV).r;
    tt = pow(max(0.0001,tt), _InsideBG_Power);
    half3 col = lerp(_InsideBG_ColorA, _InsideBG_ColorB, tt);

    return col; 
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;
	
    output.positionWS =TransformObjectToWorld(input.positionOS);
    output.positionCS =TransformWorldToHClip(output.positionWS);
    
    output.uv = input.uv;
			    
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetWorldSpaceViewDir(output.positionWS);

    output.normal = half4(normalInput.normalWS, viewDirWS.x);
    output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);

	//output.vertexSH= SampleSH(normalInput.normalWS);

	output.fogFactor = ComputeFogFactor(output.positionCS.z);
    
    return output;
}

half4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    
    MySurfaceData surfaceData = InitSurfaceData(input);
    MyInputData inputData = InitInputData(input, surfaceData);
 
    half3 outsideCol= CalcOutsideCol(inputData, surfaceData);
   
    half3 insideFGCol = CalcInsideFGCol(inputData);

    half3 insideBGCol = CalcInsideBGCol(inputData);
  
    half fresnel = CalcFresnel(inputData.normalWS, inputData.viewDirectionWS, _FresnelPower, _FresnelScale, _FresnelBias);
    
    half3 finalCol =lerp(insideBGCol+insideFGCol,outsideCol,fresnel);
	
	finalCol.rgb = MixFog(finalCol.rgb, input.fogFactor);
   
    return half4(finalCol,1);
}

#endif
