Shader "ArxCharacterShaders/_StencilReader/Fade" {
    Properties {
        // Double Sided
        [Enum(None,0, Front,1, Back,2)] _Cull("Cull", Int) = 2
        [AXCSToggle]_DoubleSidedFlipBackfaceNormal ("Flip backface normal", Float ) = 0
        _DoubleSidedBackfaceLightIntensity ("Backface Light intensity", Range(0, 2) ) = 0.5
        [AXCSToggle]_DoubleSidedBackfaceUseColorShift("Backface Use Color Shift", Int) = 0
        [PowerSlider(2.0)]_DoubleSidedBackfaceHueShiftFromBase("Backface Hue Shift From Base", Range(-0.5, 0.5)) = 0
        _DoubleSidedBackfaceSaturationFromBase("Backface Saturation From Base", Range(0, 2)) = 1
        _DoubleSidedBackfaceValueFromBase("Backface Value From Base", Range(0, 2)) = 1
        //
        [Enum(Off, 0, On, 1)]_ZWrite("ZWrite", Float) = 0
        // Common
        _MainTex ("[Common] Base Texture", 2D) = "white" {}
        _Color ("[Common] Base Color", Color) = (1,1,1,1)
        _BumpMap ("[Common] Normal map", 2D) = "bump" {}
        _BumpScale ("[Common] Normal scale", Range(0,2)) = 1
        _EmissionMap ("[Common] Emission map", 2D) = "white" {}
        [HDR]_EmissionColor ("[Common] Emission Color", Color) = (0,0,0,1)
        // Alpha Mask
        _AlphaMask ("[Alpha] AlphaMask", 2D ) = "white" {}
        // DetailMap
        _DetailMask ("[Detail] Mask texture", 2D) = "white" {}
        _DetailAlbedoMap ("[Detail] Albedo map ", 2D) = "gray" {}
        _DetailAlbedoScale ("[Detail] Albedo map scale", Range(0, 1)) = 1
        _DetailNormalMap ("[Detail] Normal map", 2D) = "bump" {}
        _DetailNormalMapScale ("[Detail] Normal scale", Range(0, 2)) = 1
        // Emission Parallax
        [AXCSToggle]_UseEmissionParallax ("[Emission Parallax] Use Emission Parallax", Int ) = 0
        _EmissionParallaxTex ("[Emission Parallax] Texture", 2D ) = "black" {}
        [HDR]_EmissionParallaxColor ("[Emission Parallax] Color", Color ) = (1,1,1,1)
        _EmissionParallaxMask ("[Emission Parallax] Emission Mask", 2D ) = "white" {}
        _EmissionParallaxDepth ("[Emission Parallax] Depth", Range(-1, 1) ) = 0
        _EmissionParallaxDepthMask ("[Emission Parallax] Depth Mask", 2D ) = "white" {}
        [AXCSToggle]_EmissionParallaxDepthMaskInvert ("[Emission Parallax] Invert Depth Mask", Float ) = 0
        // Shadow (received from DirectionalLight, other Indirect(baked) Lights, including SH)
        _Shadowborder ("[Shadow] border ", Range(0, 1)) = 0.6
        _ShadowborderBlur ("[Shadow] border Blur", Range(0, 1)) = 0.05
        _ShadowborderBlurMask ("[Shadow] border Blur Mask", 2D) = "white" {}
        _ShadowStrength ("[Shadow] Strength", Range(0, 1)) = 0.5
        _ShadowStrengthMask ("[Shadow] Strength Mask", 2D) = "white" {}
        _ShadowAmbientIntensity ("[Shadow] Ambient Intensity", Range(0, 1)) = 0.75
        // Shadow steps
        [AXCSToggle]_ShadowUseStep ("[Shadow] use step", Float ) = 0
        _ShadowSteps("[Shadow] steps between borders", Range(2, 10)) = 4
        // PointShadow (received from Point/Spot Lights as Pixel/Vertex Lights)
        _PointAddIntensity ("[PointShadow] Light Intensity", Range(0,1)) = 1
        _PointShadowStrength ("[PointShadow] Strength", Range(0, 1)) = 0.5
        _PointShadowborder ("[PointShadow] border ", Range(0, 1)) = 0.5
        _PointShadowborderBlur ("[PointShadow] border Blur", Range(0, 1)) = 0.01
        _PointShadowborderBlurMask ("[PointShadow] border Blur Mask", 2D) = "white" {}
        [AXCSToggle]_PointShadowUseStep ("[PointShadow] use step", Float ) = 0
        _PointShadowSteps("[PointShadow] steps between borders", Range(2, 10)) = 2
        // Plan B
        [AXCSToggle] _ShadowPlanBUseCustomShadowTexture ("[Plan B] Use Custom Shadow Texture", Int ) = 0
        [PowerSlider(2.0)]_ShadowPlanBHueShiftFromBase ("[Plan B] Hue Shift From Base", Range(-0.5, 0.5)) = 0
        _ShadowPlanBSaturationFromBase ("[Plan B] Saturation From Base", Range(0, 2)) = 1
        _ShadowPlanBValueFromBase ("[Plan B] Value From Base", Range(0, 2)) = 1
        _ShadowPlanBCustomShadowTexture ("[Plan B] Custom Shadow Texture", 2D) = "black" {}
        _ShadowPlanBCustomShadowDetailMap ("[Plan B] Custom Shadow Detail Texture ", 2D) = "gray" {}
        _ShadowPlanBCustomShadowTextureRGB ("[Plan B] Custom Shadow Texture RGB", Color) = (1,1,1,1)
        // Gloss
        [AXCSToggle]_UseGloss ("[Gloss] Enabled", Int) = 0
        _GlossBlend ("[Gloss] Smoothness", Range(0, 1)) = 0.5
        _GlossBlendMask ("[Gloss] Smoothness Mask", 2D) = "white" {}
        _GlossPower ("[Gloss] Metallic", Range(0, 1)) = 0.5
        _GlossColor ("[Gloss] Color", Color) = (1,1,1,1)
        // MatCap
        [Enum(Add,0, Lighten,1, Screen,2, Unused,3)] _MatcapBlendMode ("[MatCap] Blend Mode", Int) = 3
        _MatcapBlend ("[MatCap] Blend", Range(0, 3)) = 1
        _MatcapBlendMask ("[MatCap] Blend Mask", 2D) = "white" {}
        _MatcapNormalMix ("[MatCap] Normal map mix", Range(0, 2)) = 1
        _MatcapShadeMix ("[MatCap] Shade Mix", Range(0, 1)) = 0
        _MatcapTexture ("[MatCap] Texture", 2D) = "black" {}
        _MatcapColor ("[MatCap] Color", Color) = (1,1,1,1)
        // Reflection
        [AXCSToggle]_UseReflection ("[Reflection] Enabled", Int) = 0
        [AXCSToggle]_UseReflectionProbe ("[Reflection] Use Reflection Probe", Int) = 1
        _ReflectionReflectionPower ("[Reflection] Reflection Power", Range(0, 1)) = 1
        _ReflectionReflectionMask ("[Reflection] Reflection Mask", 2D) = "white" {}
        _ReflectionNormalMix ("[Reflection] Normal Map Mix", Range(0,2)) = 1
        _ReflectionShadeMix ("[Reflection] Shade Mix", Range(0, 1)) = 0
        _ReflectionSuppressBaseColorValue ("[Reflection] Suppress Base Color", Range(0, 1)) = 1
        _ReflectionCubemap ("[Reflection] Cubemap", Cube) = "" {}
        // Rim
        [AXCSToggle]_UseRim ("[Rim] Enabled", Int) = 0
        _RimBlend ("[Rim] Blend", Range(0, 3)) = 1
        _RimBlendMask ("[Rim] Blend Mask", 2D) = "white" {}
        _RimShadeMix("[Rim] Shade Mix", Range(0, 1)) = 0
        _RimBlendStart("[Rim] Blend start", Range(0, 1)) = 0
        _RimBlendEnd("[Rim] Blend end", Range(0, 1)) = 1
        [Enum(Linear,0, Pow3,1, Pow5,2)] _RimPow ("[Rim] Power Type", Int) = 1
        [HDR]_RimColor ("[Rim] Color", Color) = (1,1,1,1)
        _RimTexture ("[Rim] Texture", 2D) = "white" {}
        [AXCSToggle] _RimUseBaseTexture ("[Rim] Use Base Texture", Float ) = 0
        // ShadowCap
        [Enum(Darken,0, Multiply,1, Light Shutter,2, Unused,3)] _ShadowCapBlendMode ("[ShadowCap] Blend Mode", Int) = 3
        _ShadowCapBlend ("[ShadowCap] Blend", Range(0, 3)) = 1
        _ShadowCapBlendMask ("[ShadowCap] Blend Mask", 2D) = "white" {}
        _ShadowCapNormalMix ("[ShadowCap] Normal map mix", Range(0, 2)) = 1
        _ShadowCapTexture ("[ShadowCap] Texture", 2D) = "white" {}
        // Stencil(Reader)
        _StencilNumber ("[StencilReader] Number", int) = 5
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareAction ("[StencilReader] Compare Action", int) = 6
        // vertex color blend
        _VertexColorBlendDiffuse ("[VertexColor] Blend to diffuse", Range(0,1)) = 0
        _VertexColorBlendEmissive ("[VertexColor] Blend to emissive", Range(0,1)) = 0
        // advanced tweaking
        _OtherShadowAdjust ("[Advanced] Other Mesh Shadow Adjust", Range(-0.2, 0.2)) = -0.1
        _OtherShadowBorderSharpness ("[Advanced] Other Mesh Shadow Border Sharpness", Range(1, 5)) = 3
        // AXCS_GENERATOR:EMISSIVE_FREAK_PROPERTIES
        // Proximity color override
        [AXCSToggle]_UseProximityOverride ("[ProximityOverride] Enabled", Int) = 0
        _ProximityOverrideBegin ("[ProximityOverride] Begin", Range(0.0, 1.0)) = 0.10
        _ProximityOverrideEnd ("[ProximityOverride] End", Range(0.0, 1.0)) = 0.01
        _ProximityOverrideColor ("[ProximityOverride] Override Color", Color) = (0,0,0,1)
        [AXCSToggle]_ProximityOverrideAlphaOnly ("[ProximityOverride] Alpha Only", Int) = 0
    }
    SubShader {
        Tags {
            "Queue"="Transparent-100"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull [_Cull]
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]

            Stencil {
                Ref [_StencilNumber]
                Comp [_StencilCompareAction]
            }

            CGPROGRAM


            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles
            #pragma target 3.0
            #define AXCS_FADE
            // AXCS_GENERATOR:EMISSIVE_FREAK_DEFINE

            #include "cginc/arkludeDecl.cginc"
            #include "cginc/arkludeOther.cginc"
            #include "cginc/arkludeVertGeom.cginc"
            #include "cginc/arkludeFrag.cginc"
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Cull [_Cull]
            Blend One One
            ZWrite [_ZWrite]

            Stencil {
                Ref [_StencilNumber]
                Comp [_StencilCompareAction]
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles
            #pragma target 3.0
            #define AXCS_FADE
            #define AXCS_ADD

            #include "cginc/arkludeDecl.cginc"
            #include "cginc/arkludeOther.cginc"
            #include "cginc/arkludeVertGeom.cginc"
            #include "cginc/arkludeAdd.cginc"
            ENDCG
        }

        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "SHADOWCASTER"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual
            Cull [_Cull]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------
            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "cginc/arkludeFadeShadowCaster.cginc"

            ENDCG
        }
    }
    FallBack "Standard"
    CustomEditor "AxCharacterShaders.AxInspector"
}
