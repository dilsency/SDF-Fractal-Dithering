/*
 * Copyright (c) 2025 mattdevv (https://github.com/mattdevv)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

Shader "Unlit/Simple_FractalDithering"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        _Color1 ("Color 1", Color) = (0.2, 0.2, 0.09803921, 1)
        _Color2 ("Color 2", Color) = (0.89803921, 1, 1, 1)
        
        _InputExposure ("Input Exposure", float) = 1
        _InputOffset  ("Input Offset", float) = 0
        _Clamp("Value Clamp", Vector) = (0.2, 1, 0, 0)
        
        _Scale ("Scale", float) = 3.5
        _DotRadius ("Dot Radius", Range(0, 2)) = 0.8
        [KeywordEnum(Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8)] _Bayer ("Bayer Level",int) = 2
        [Toggle(QUANTIZE_DOTS)] _QuantizeDots ("Quantize Dots", float) = 0
        
        [KeywordEnum(Circle, Star, Moon, Heart, CoolS)] _Shape ("SDF Shape", int) = 0
        [KeywordEnum(None, Freq, UV, Cell, Bayer, SDF)] _Debug ("Debug Mode",int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma target 4.5
            
            #pragma shader_feature _DEBUG_NONE _DEBUG_FREQ _DEBUG_UV _DEBUG_CELL _DEBUG_BAYER _DEBUG_SDF
            #pragma shader_feature _BAYER_LEVEL1 _BAYER_LEVEL2 _BAYER_LEVEL3 _BAYER_LEVEL4 _BAYER_LEVEL5 _BAYER_LEVEL6 _BAYER_LEVEL7 _BAYER_LEVEL8
            #pragma shader_feature _SHAPE_CIRCLE _SHAPE_STAR _SHAPE_MOON _SHAPE_HEART _SHAPE_COOLS
            #pragma shader_feature QUANTIZE_DOTS

            #pragma shader_feature INVERT_A
            #pragma shader_feature INVERT_B

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "./Bayer.hlsl"
            #include "./runevision.hlsl"
            #include "./SDF.hlsl"
            
            #if   defined(_BAYER_LEVEL1)
                #define LEVEL                1
            #elif defined(_BAYER_LEVEL2)
                #define LEVEL                2
            #elif defined(_BAYER_LEVEL3)
                #define LEVEL                3
            #elif defined(_BAYER_LEVEL4)
                #define LEVEL                4
            #elif defined(_BAYER_LEVEL5)
                #define LEVEL                5
            #elif defined(_BAYER_LEVEL6)
                #define LEVEL                6
            #elif defined(_BAYER_LEVEL7)
                #define LEVEL                7
            #elif defined(_BAYER_LEVEL8)
                #define LEVEL                8
            #endif
            
            #define LEVEL_RESOLUTION         exp2(LEVEL)
            #define LEVEL_DOTCOUNT           (LEVEL_RESOLUTION * LEVEL_RESOLUTION)

            #define LEVEL_PREV               (LEVEL - 1)
            #define LEVEL_PREV_RESOLUTION    exp2(LEVEL_PREV)
            #define LEVEL_PREV_DOTCOUNT      (LEVEL_PREV_RESOLUTION * LEVEL_PREV_RESOLUTION)
            
            struct appdata
            {
                float4 vertex : POSITION;
                float4 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 clipPos    : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color1;
            float4 _Color2;
            float _Scale;
            float2 _Clamp;
            float _DotRadius;

            float _InputExposure;
            float _InputOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                VertexPositionInputs positions = GetVertexPositionInputs(v.vertex.xyz);
                VertexNormalInputs normals = GetVertexNormalInputs(v.normalOS.xyz, v.tangentOS);

                o.clipPos = positions.positionCS;
                o.positionWS = positions.positionWS;
                o.normalWS = normals.normalWS;
                
                o.uv = (v.uv * 2 - 1) * 100; // try to improve precision
                
                return o;
            }

            // this is how I was calculating freq initially
            float CalculateFreq_mattdevv(float4 screenPos)
            {
                float zdepth = LinearEyeDepth(screenPos.z, _ZBufferParams);
                return zdepth * exp2(-_Scale);
            }
            
            half4 frag (v2f i) : SV_Target
            {
                // surface properties
                float shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(i.positionWS));
                float shading = pow(saturate(dot(normalize(i.normalWS), _MainLightPosition.xyz)), .5);
                float albedo = dot(float3(0.299, 0.587, 0.114), tex2D(_MainTex, i.uv).rgb);

                // calculate brightness of fragment
                float brightness = min(shadow, shading) * albedo;
                brightness = saturate(brightness * _InputExposure + _InputOffset);
                brightness = clamp(brightness, _Clamp.x, _Clamp.y);

                // calculate 
                float4 frequencies = CalculateFrequency_Rune(i.uv, i.clipPos, ddx_fine(i.uv), ddy_fine(i.uv), LEVEL, _Scale);
                float logLevel = log2(frequencies.w / brightness);
                float floorLog = floor(logLevel);
                float fracLog = logLevel - floorLog; // same as frac(logLevel)

                // each tile contains N*N cells of Bayer pattern
                float2 tileUV = (frac(i.uv * exp2(-floorLog)));
                // each cell spans covers 1 dot but is offset by (0.5, 0.5)
                float2 cellUV = frac(tileUV * LEVEL_RESOLUTION) - 0.5;

                // Calculate 4 nearest Bayer samples for this cell
                uint2 cellCoord = (uint2)(tileUV * LEVEL_RESOLUTION);
                float4 bayer = float4(
                    GetBayerFromCoordLevel_Direct(cellCoord + uint2(0,0), LEVEL),
                    GetBayerFromCoordLevel_Direct(cellCoord + uint2(1,0), LEVEL),
                    GetBayerFromCoordLevel_Direct(cellCoord + uint2(0,1), LEVEL),
                    GetBayerFromCoordLevel_Direct(cellCoord + uint2(1,1), LEVEL));

                // number each bayer dot sequentially then subtract the count of dots on the level above. ie (N-1)^2
                float4 bayerMask = (bayer * LEVEL_DOTCOUNT) - LEVEL_PREV_DOTCOUNT; // only want the dots that weren't on prev level

                // create a 0-1 mask for each Bayer dot that controls if it can be seen (also scales the dot in)
                const float numNewDots = LEVEL_DOTCOUNT - LEVEL_PREV_DOTCOUNT;

                // each bayer dot is sequentially a number in range [1, N) according to its intensity value
                // subtract the number of dots on the next higher level (N-1)^2 so this many first dots have a negative value
                // bayer dots are visible when fracLog*numNewDots is greater than their value
                float invisible = numNewDots * (1-fracLog);
                float4 scales = (saturate(invisible - bayerMask));
                #ifdef QUANTIZE_DOTS
                scales = step(1,scales);
                #endif

                // scale the dots cell according to their bayer value so dots scale/appear with relation to fracLog
                float x = fracLog;
                x = (1-(pow(1-x, 0.5))); // found that doing this removed some of the banding, based on eye test
                float4 scalar = rcp((x * 0.5 + 0.5) * brightness * scales);
                float2 sample0 = (cellUV + float2(+0.5, +0.5)) * scalar.x;
                float2 sample1 = (cellUV + float2(-0.5, +0.5)) * scalar.y;
                float2 sample2 = (cellUV + float2(+0.5, -0.5)) * scalar.z;
                float2 sample3 = (cellUV + float2(-0.5, -0.5)) * scalar.w;

                // sample SDF at 4 corners of the grid
                // Each coordinates are scaled so dots not yet visible are infinitely small
                float4 SDFs = float4(
                    SDF(sample0, _DotRadius),
                    SDF(sample1, _DotRadius),
                    SDF(sample2, _DotRadius),
                    SDF(sample3, _DotRadius));
                
                float minSDF = min(min(SDFs.x, SDFs.y), min(SDFs.z, SDFs.w)) ;
                float dots = AA_SDF(minSDF);

                #ifdef _DEBUG_FREQ
                float4 d_color = frac(floor(logLevel) / 2) < 0.5 ? float4(1,0,0,1) : float4(0,1,0,1);
                return lerp(d_color, 0, dots/2);
                #endif
                
                #ifdef _DEBUG_UV
                return lerp(float4(tileUV, 0 , 0), 0, dots/2);
                #endif
                
                #ifdef _DEBUG_CELL
                return lerp(float4(cellUV, 0, 0), 0, dots/2);
                #endif
                
                #ifdef _DEBUG_BAYER
                return lerp(bayer, 0, dots/2);
                #endif

                #ifdef _DEBUG_SDF
                return minSDF;
                #endif

                return lerp(_Color1, _Color2, dots);
            }
            ENDHLSL
        }

        Pass
        {
	        Tags { "LightMode"="ShadowCaster" }
	        
	        ZWrite On
	        ZTest LEqual
	        
            HLSLPROGRAM
	        #pragma vertex ShadowPassVertex
	        #pragma fragment ShadowPassFragment
	        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
	        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
	        #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
	        
	        ENDHLSL
        }
    }
}
