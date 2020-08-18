using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AxCharacterShaders
{
    public class AxInspector : ShaderGUI
    {
        #region MaterialProperties
        MaterialProperty BaseTexture;
        MaterialProperty BaseColor;
        MaterialProperty Normalmap;
        MaterialProperty BumpScale;
        MaterialProperty EmissionMap;
        MaterialProperty EmissionColor;
        MaterialProperty AlphaMask;
        MaterialProperty BaseTextureSecondary;
        MaterialProperty BaseColorSecondary;
        MaterialProperty NormalmapSecondary;
        MaterialProperty BumpScaleSecondary;
        MaterialProperty EmissionMapSecondary;
        MaterialProperty EmissionColorSecondary;
        MaterialProperty UseEmissionParallax;
        MaterialProperty EmissionParallaxColor;
        MaterialProperty EmissionParallaxTex;
        MaterialProperty EmissionParallaxMask;
        MaterialProperty EmissionParallaxDepth;
        MaterialProperty EmissionParallaxDepthMask;
        MaterialProperty EmissionParallaxDepthMaskInvert;
        MaterialProperty Shadowborder;
        MaterialProperty ShadowborderBlur;
        MaterialProperty ShadowborderBlurMask;
        MaterialProperty ShadowStrength;
        MaterialProperty ShadowStrengthMask;
        MaterialProperty ShadowIndirectIntensity;
        MaterialProperty ShadowUseStep;
        MaterialProperty ShadowSteps;
        MaterialProperty PointAddIntensity;
        MaterialProperty PointShadowStrength;
        MaterialProperty PointShadowborder;
        MaterialProperty PointShadowborderBlur;
        MaterialProperty PointShadowborderBlurMask;
        MaterialProperty PointShadowUseStep;
        MaterialProperty PointShadowSteps;
        MaterialProperty CutoutCutoutAdjust;
        MaterialProperty ShadowPlanBDefaultShadowMix;
        MaterialProperty ShadowPlanBUseCustomShadowTexture;
        MaterialProperty ShadowPlanBHueShiftFromBase;
        MaterialProperty ShadowPlanBSaturationFromBase;
        MaterialProperty ShadowPlanBValueFromBase;
        MaterialProperty ShadowPlanBCustomShadowTexture;
        MaterialProperty ShadowPlanBCustomShadowTextureRGB;
        MaterialProperty UseGloss;
        MaterialProperty GlossBlend;
        MaterialProperty GlossBlendMask;
        MaterialProperty GlossPower;
        MaterialProperty GlossColor;
        MaterialProperty UseOutline;
        MaterialProperty OutlineWidth;
        MaterialProperty OutlineMask;
        MaterialProperty OutlineCutoffRange;
        MaterialProperty OutlineColor;
        MaterialProperty OutlineTexture;
        MaterialProperty OutlineShadeMix;
        MaterialProperty OutlineTextureColorRate;
        MaterialProperty OutlineWidthMask;
        MaterialProperty OutlineUseColorShift;
        MaterialProperty OutlineHueShiftFromBase;
        MaterialProperty OutlineSaturationFromBase;
        MaterialProperty OutlineValueFromBase;
        MaterialProperty MatcapBlendMode;
        MaterialProperty MatcapBlend;
        MaterialProperty MatcapTexture;
        MaterialProperty MatcapColor;
        MaterialProperty MatcapBlendMask;
        MaterialProperty MatcapNormalMix;
        MaterialProperty MatcapShadeMix;
        MaterialProperty UseReflection;
        MaterialProperty UseReflectionProbe;
        MaterialProperty ReflectionReflectionPower;
        MaterialProperty ReflectionReflectionMask;
        MaterialProperty ReflectionNormalMix;
        MaterialProperty ReflectionShadeMix;
        MaterialProperty ReflectionCubemap;
        MaterialProperty ReflectionSuppressBaseColorValue;
        MaterialProperty RefractionFresnelExp;
        MaterialProperty RefractionStrength;
        MaterialProperty UseRim;
        MaterialProperty RimBlend;
        MaterialProperty RimBlendMask;
        MaterialProperty RimShadeMix;
        MaterialProperty RimBlendStart;
        MaterialProperty RimBlendEnd;
        MaterialProperty RimPow;
        MaterialProperty RimColor;
        MaterialProperty RimTexture;
        MaterialProperty RimUseBaseTexture;
        MaterialProperty ShadowCapBlendMode;
        MaterialProperty ShadowCapBlend;
        MaterialProperty ShadowCapBlendMask;
        MaterialProperty ShadowCapNormalMix;
        MaterialProperty ShadowCapTexture;
        MaterialProperty StencilNumber;
        MaterialProperty StencilCompareAction;
        MaterialProperty StencilNumberSecondary;
        MaterialProperty StencilCompareActionSecondary;
        MaterialProperty StencilMaskTex;
        MaterialProperty StencilMaskAdjust;
        MaterialProperty StencilMaskAlphaDither;
        MaterialProperty Cull;
        MaterialProperty DoubleSidedFlipBackfaceNormal;
        MaterialProperty DoubleSidedBackfaceLightIntensity;
        MaterialProperty DoubleSidedBackfaceUseColorShift;
        MaterialProperty DoubleSidedBackfaceHueShiftFromBase;
        MaterialProperty DoubleSidedBackfaceSaturationFromBase;
        MaterialProperty DoubleSidedBackfaceValueFromBase;
        MaterialProperty ZWrite;
        MaterialProperty VertexColorBlendDiffuse;
        MaterialProperty VertexColorBlendEmissive;
        MaterialProperty OtherShadowBorderSharpness;
        MaterialProperty OtherShadowAdjust;
        MaterialProperty BackfaceColorMultiply;
        MaterialProperty EmissiveFreak1Tex;
        MaterialProperty EmissiveFreak1Mask;
        MaterialProperty EmissiveFreak1Color;
        MaterialProperty EmissiveFreak1U;
        MaterialProperty EmissiveFreak1V;
        MaterialProperty EmissiveFreak1Depth;
        MaterialProperty EmissiveFreak1DepthMask;
        MaterialProperty EmissiveFreak1DepthMaskInvert;
        MaterialProperty EmissiveFreak1Breathing;
        MaterialProperty EmissiveFreak1BreathingMix;
        MaterialProperty EmissiveFreak1BlinkOut;
        MaterialProperty EmissiveFreak1BlinkOutMix;
        MaterialProperty EmissiveFreak1BlinkIn;
        MaterialProperty EmissiveFreak1BlinkInMix;
        MaterialProperty EmissiveFreak1HueShift;
        MaterialProperty EmissiveFreak2Tex;
        MaterialProperty EmissiveFreak2Mask;
        MaterialProperty EmissiveFreak2Color;
        MaterialProperty EmissiveFreak2U;
        MaterialProperty EmissiveFreak2V;
        MaterialProperty EmissiveFreak2Depth;
        MaterialProperty EmissiveFreak2DepthMask;
        MaterialProperty EmissiveFreak2DepthMaskInvert;
        MaterialProperty EmissiveFreak2Breathing;
        MaterialProperty EmissiveFreak2BreathingMix;
        MaterialProperty EmissiveFreak2BlinkOut;
        MaterialProperty EmissiveFreak2BlinkOutMix;
        MaterialProperty EmissiveFreak2BlinkIn;
        MaterialProperty EmissiveFreak2BlinkInMix;
        MaterialProperty EmissiveFreak2HueShift;

        // TODO: そろそろShaderUtil.GetPropertiesで一括処理したい。
        // ただ、その場合は、カスタムインスペクタで定義していない追加のプロパティを、このファイルを弄らずに動的に表示できるようにしてあげたい（改変の負荷軽減のため）

        #endregion

        static bool IsShowAdvanced = false;
        static bool IsShowAlphaMask = false;
        static bool IsShowNonRegistered = false;
        GUIStyle style = new GUIStyle();


        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            Material material = materialEditor.target as Material;

            Shader shader = material.shader;

            // shader.nameによって調整可能なプロパティを制御する。
            bool isOpaque = shader.name.Contains("Opaque");
            bool isFade = shader.name.Contains("Fade");
            bool isCutout = shader.name.Contains("Cutout");
            bool isStencilWriter = shader.name.Contains("Stencil/Writer") || shader.name.Contains("StencilWriter");
            bool isStencilReader = shader.name.Contains("Stencil/Reader") || shader.name.Contains("StencilReader");
            bool isStencilReaderDouble = shader.name.Contains("Stencil/Reader/Double");
            bool isStencilWriterMask = shader.name.Contains("Stencil/WriterMask");
            bool isRefracted = shader.name.Contains("Refracted");
            bool isEmissiveFreak = shader.name.Contains("/EmissiveFreak/");
            bool isOutline = shader.name.Contains("/Outline/");

            // Clear regitered props
            ClearRegisteredPropertiesList();

            // FindProperties
            BaseTexture = FindAndRegisterProperties("_MainTex", props, false);
            BaseColor = FindAndRegisterProperties("_Color", props, false);
            Normalmap = FindAndRegisterProperties("_BumpMap", props, false);
            BumpScale = FindAndRegisterProperties("_BumpScale", props, false);
            EmissionMap = FindAndRegisterProperties("_EmissionMap", props, false);
            EmissionColor = FindAndRegisterProperties("_EmissionColor", props, false);
            AlphaMask = FindAndRegisterProperties("_AlphaMask", props, false);
            BaseTextureSecondary = FindAndRegisterProperties("_MainTexSecondary", props, false);
            BaseColorSecondary = FindAndRegisterProperties("_ColorSecondary", props, false);
            NormalmapSecondary = FindAndRegisterProperties("_BumpMapSecondary", props, false);
            BumpScaleSecondary = FindAndRegisterProperties("_BumpScaleSecondary", props, false);
            EmissionMapSecondary = FindAndRegisterProperties("_EmissionMapSecondary", props, false);
            EmissionColorSecondary = FindAndRegisterProperties("_EmissionColorSecondary", props, false);
            UseEmissionParallax = FindAndRegisterProperties("_UseEmissionParallax", props, false);
            EmissionParallaxColor = FindAndRegisterProperties("_EmissionParallaxColor", props, false);
            EmissionParallaxTex = FindAndRegisterProperties("_EmissionParallaxTex", props, false);
            EmissionParallaxMask = FindAndRegisterProperties("_EmissionParallaxMask", props, false);
            EmissionParallaxDepth = FindAndRegisterProperties("_EmissionParallaxDepth", props, false);
            EmissionParallaxDepthMask = FindAndRegisterProperties("_EmissionParallaxDepthMask", props, false);
            EmissionParallaxDepthMaskInvert = FindAndRegisterProperties("_EmissionParallaxDepthMaskInvert", props, false);
            CutoutCutoutAdjust = FindAndRegisterProperties("_CutoutCutoutAdjust", props, false);
            Shadowborder = FindAndRegisterProperties("_Shadowborder", props, false);
            ShadowborderBlur = FindAndRegisterProperties("_ShadowborderBlur", props, false);
            ShadowborderBlurMask = FindAndRegisterProperties("_ShadowborderBlurMask", props, false);
            ShadowStrength = FindAndRegisterProperties("_ShadowStrength", props, false);
            ShadowStrengthMask = FindAndRegisterProperties("_ShadowStrengthMask", props, false);
            ShadowIndirectIntensity = FindAndRegisterProperties("_ShadowIndirectIntensity", props, false);
            ShadowUseStep = FindAndRegisterProperties("_ShadowUseStep", props, false);
            ShadowSteps = FindAndRegisterProperties("_ShadowSteps", props, false);
            PointAddIntensity = FindAndRegisterProperties("_PointAddIntensity", props, false);
            PointShadowStrength = FindAndRegisterProperties("_PointShadowStrength", props, false);
            PointShadowborder = FindAndRegisterProperties("_PointShadowborder", props, false);
            PointShadowborderBlur = FindAndRegisterProperties("_PointShadowborderBlur", props, false);
            PointShadowborderBlurMask= FindAndRegisterProperties("_PointShadowborderBlurMask", props, false);
            PointShadowUseStep = FindAndRegisterProperties("_PointShadowUseStep", props, false);
            PointShadowSteps = FindAndRegisterProperties("_PointShadowSteps", props, false);
            ShadowPlanBDefaultShadowMix = FindAndRegisterProperties("_ShadowPlanBDefaultShadowMix", props, false);
            ShadowPlanBUseCustomShadowTexture = FindAndRegisterProperties("_ShadowPlanBUseCustomShadowTexture", props, false);
            ShadowPlanBHueShiftFromBase = FindAndRegisterProperties("_ShadowPlanBHueShiftFromBase", props, false);
            ShadowPlanBSaturationFromBase = FindAndRegisterProperties("_ShadowPlanBSaturationFromBase", props, false);
            ShadowPlanBValueFromBase = FindAndRegisterProperties("_ShadowPlanBValueFromBase", props, false);
            ShadowPlanBCustomShadowTexture = FindAndRegisterProperties("_ShadowPlanBCustomShadowTexture", props, false);
            ShadowPlanBCustomShadowTextureRGB = FindAndRegisterProperties("_ShadowPlanBCustomShadowTextureRGB", props, false);
            UseGloss = FindAndRegisterProperties("_UseGloss", props, false);
            GlossBlend = FindAndRegisterProperties("_GlossBlend", props, false);
            GlossBlendMask = FindAndRegisterProperties("_GlossBlendMask", props, false);
            GlossPower = FindAndRegisterProperties("_GlossPower", props, false);
            GlossColor = FindAndRegisterProperties("_GlossColor", props, false);
            UseOutline = FindAndRegisterProperties("_UseOutline", props, false);
            OutlineWidth = FindAndRegisterProperties("_OutlineWidth", props, false);
            OutlineMask = FindAndRegisterProperties("_OutlineMask", props, false);
            OutlineCutoffRange = FindAndRegisterProperties("_OutlineCutoffRange", props, false);
            OutlineColor = FindAndRegisterProperties("_OutlineColor", props, false);
            OutlineTexture = FindAndRegisterProperties("_OutlineTexture", props, false);
            OutlineShadeMix = FindAndRegisterProperties("_OutlineShadeMix", props, false);
            OutlineTextureColorRate = FindAndRegisterProperties("_OutlineTextureColorRate", props, false);
            OutlineWidthMask = FindAndRegisterProperties("_OutlineWidthMask", props, false);
            OutlineUseColorShift = FindAndRegisterProperties("_OutlineUseColorShift", props, false);
            OutlineHueShiftFromBase = FindAndRegisterProperties("_OutlineHueShiftFromBase", props, false);
            OutlineSaturationFromBase = FindAndRegisterProperties("_OutlineSaturationFromBase", props, false);
            OutlineValueFromBase = FindAndRegisterProperties("_OutlineValueFromBase", props, false);
            MatcapBlendMode = FindAndRegisterProperties("_MatcapBlendMode", props, false);
            MatcapBlend = FindAndRegisterProperties("_MatcapBlend", props, false);
            MatcapTexture = FindAndRegisterProperties("_MatcapTexture", props, false);
            MatcapColor = FindAndRegisterProperties("_MatcapColor", props, false);
            MatcapBlendMask = FindAndRegisterProperties("_MatcapBlendMask", props, false);
            MatcapNormalMix = FindAndRegisterProperties("_MatcapNormalMix", props, false);
            MatcapShadeMix = FindAndRegisterProperties("_MatcapShadeMix", props, false);
            UseReflection = FindAndRegisterProperties("_UseReflection", props, false);
            UseReflectionProbe = FindAndRegisterProperties("_UseReflectionProbe", props, false);
            ReflectionReflectionPower = FindAndRegisterProperties("_ReflectionReflectionPower", props, false);
            ReflectionReflectionMask = FindAndRegisterProperties("_ReflectionReflectionMask", props, false);
            ReflectionNormalMix = FindAndRegisterProperties("_ReflectionNormalMix", props, false);
            ReflectionShadeMix = FindAndRegisterProperties("_ReflectionShadeMix", props, false);
            ReflectionCubemap = FindAndRegisterProperties("_ReflectionCubemap", props, false);
            ReflectionSuppressBaseColorValue = FindAndRegisterProperties("_ReflectionSuppressBaseColorValue", props, false);
            RefractionFresnelExp = FindAndRegisterProperties("_RefractionFresnelExp", props, false);
            RefractionStrength = FindAndRegisterProperties("_RefractionStrength", props, false);
            UseRim = FindAndRegisterProperties("_UseRim", props, false);
            RimBlend = FindAndRegisterProperties("_RimBlend", props, false);
            RimBlendMask = FindAndRegisterProperties("_RimBlendMask", props, false);
            RimShadeMix = FindAndRegisterProperties("_RimShadeMix", props, false);
            RimBlendStart = FindAndRegisterProperties("_RimBlendStart", props, false);
            RimBlendEnd = FindAndRegisterProperties("_RimBlendEnd", props, false);
            RimPow = FindAndRegisterProperties("_RimPow", props, false);
            RimColor = FindAndRegisterProperties("_RimColor", props, false);
            RimTexture = FindAndRegisterProperties("_RimTexture", props, false);
            RimUseBaseTexture = FindAndRegisterProperties("_RimUseBaseTexture", props, false);
            ShadowCapBlendMode = FindAndRegisterProperties("_ShadowCapBlendMode", props, false);
            ShadowCapBlend = FindAndRegisterProperties("_ShadowCapBlend", props, false);
            ShadowCapBlendMask = FindAndRegisterProperties("_ShadowCapBlendMask", props, false);
            ShadowCapNormalMix = FindAndRegisterProperties("_ShadowCapNormalMix", props, false);
            ShadowCapTexture = FindAndRegisterProperties("_ShadowCapTexture", props, false);
            StencilNumber = FindAndRegisterProperties("_StencilNumber", props, false);
            StencilMaskTex = FindAndRegisterProperties("_StencilMaskTex", props, false);
            StencilMaskAdjust = FindAndRegisterProperties("_StencilMaskAdjust", props, false);
            StencilMaskAlphaDither = FindAndRegisterProperties("_StencilMaskAlphaDither", props, false);
            StencilCompareAction = FindAndRegisterProperties("_StencilCompareAction", props, false);
            StencilNumberSecondary = FindAndRegisterProperties("_StencilNumberSecondary", props, false);
            StencilCompareActionSecondary = FindAndRegisterProperties("_StencilCompareActionSecondary", props, false);
            Cull = FindAndRegisterProperties("_Cull", props, false);
            DoubleSidedFlipBackfaceNormal = FindAndRegisterProperties("_DoubleSidedFlipBackfaceNormal", props, false);
            DoubleSidedBackfaceLightIntensity = FindAndRegisterProperties("_DoubleSidedBackfaceLightIntensity", props, false);
            DoubleSidedBackfaceUseColorShift = FindAndRegisterProperties("_DoubleSidedBackfaceUseColorShift", props, false);
            DoubleSidedBackfaceHueShiftFromBase = FindAndRegisterProperties("_DoubleSidedBackfaceHueShiftFromBase", props, false);
            DoubleSidedBackfaceSaturationFromBase = FindAndRegisterProperties("_DoubleSidedBackfaceSaturationFromBase", props, false);
            DoubleSidedBackfaceValueFromBase = FindAndRegisterProperties("_DoubleSidedBackfaceValueFromBase", props, false);
            VertexColorBlendDiffuse = FindAndRegisterProperties("_VertexColorBlendDiffuse", props, false);
            VertexColorBlendEmissive = FindAndRegisterProperties("_VertexColorBlendEmissive", props, false);
            OtherShadowBorderSharpness = FindAndRegisterProperties("_OtherShadowBorderSharpness", props, false);
            OtherShadowAdjust = FindAndRegisterProperties("_OtherShadowAdjust", props, false);
            ZWrite = FindAndRegisterProperties("_ZWrite", props, false);

            EmissiveFreak1Tex = FindAndRegisterProperties("_EmissiveFreak1Tex", props, false);
            EmissiveFreak1Mask = FindAndRegisterProperties("_EmissiveFreak1Mask", props, false);
            EmissiveFreak1Color = FindAndRegisterProperties("_EmissiveFreak1Color", props, false);
            EmissiveFreak1U = FindAndRegisterProperties("_EmissiveFreak1U", props, false);
            EmissiveFreak1V = FindAndRegisterProperties("_EmissiveFreak1V", props, false);
            EmissiveFreak1Depth = FindAndRegisterProperties("_EmissiveFreak1Depth", props, false);
            EmissiveFreak1DepthMask = FindAndRegisterProperties("_EmissiveFreak1DepthMask", props, false);
            EmissiveFreak1DepthMaskInvert = FindAndRegisterProperties("_EmissiveFreak1DepthMaskInvert", props, false);
            EmissiveFreak1Breathing = FindAndRegisterProperties("_EmissiveFreak1Breathing", props, false);
            EmissiveFreak1BreathingMix = FindAndRegisterProperties("_EmissiveFreak1BreathingMix", props, false);
            EmissiveFreak1BlinkOut = FindAndRegisterProperties("_EmissiveFreak1BlinkOut", props, false);
            EmissiveFreak1BlinkOutMix = FindAndRegisterProperties("_EmissiveFreak1BlinkOutMix", props, false);
            EmissiveFreak1BlinkIn = FindAndRegisterProperties("_EmissiveFreak1BlinkIn", props, false);
            EmissiveFreak1BlinkInMix = FindAndRegisterProperties("_EmissiveFreak1BlinkInMix", props, false);
            EmissiveFreak1HueShift = FindAndRegisterProperties("_EmissiveFreak1HueShift", props, false);

            EmissiveFreak2Tex = FindAndRegisterProperties("_EmissiveFreak2Tex", props, false);
            EmissiveFreak2Mask = FindAndRegisterProperties("_EmissiveFreak2Mask", props, false);
            EmissiveFreak2Color = FindAndRegisterProperties("_EmissiveFreak2Color", props, false);
            EmissiveFreak2U = FindAndRegisterProperties("_EmissiveFreak2U", props, false);
            EmissiveFreak2V = FindAndRegisterProperties("_EmissiveFreak2V", props, false);
            EmissiveFreak2Depth = FindAndRegisterProperties("_EmissiveFreak2Depth", props, false);
            EmissiveFreak2DepthMask = FindAndRegisterProperties("_EmissiveFreak2DepthMask", props, false);
            EmissiveFreak2DepthMaskInvert = FindAndRegisterProperties("_EmissiveFreak2DepthMaskInvert", props, false);
            EmissiveFreak2Breathing = FindAndRegisterProperties("_EmissiveFreak2Breathing", props, false);
            EmissiveFreak2BreathingMix = FindAndRegisterProperties("_EmissiveFreak2BreathingMix", props, false);
            EmissiveFreak2BlinkOut = FindAndRegisterProperties("_EmissiveFreak2BlinkOut", props, false);
            EmissiveFreak2BlinkOutMix = FindAndRegisterProperties("_EmissiveFreak2BlinkOutMix", props, false);
            EmissiveFreak2BlinkIn = FindAndRegisterProperties("_EmissiveFreak2BlinkIn", props, false);
            EmissiveFreak2BlinkInMix = FindAndRegisterProperties("_EmissiveFreak2BlinkInMix", props, false);
            EmissiveFreak2HueShift = FindAndRegisterProperties("_EmissiveFreak2HueShift", props, false);

            EditorGUIUtility.labelWidth = 0f;

            EditorGUI.BeginChangeCheck();
            {
                // Common
                UIHelper.ShurikenHeader("Common");
                UIHelper.DrawWithGroup(() => {
					UIHelper.DrawWithGroup(() => {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Main Texture", "Base Color Texture (RGB)"), BaseTexture, BaseColor);
                        materialEditor.TextureScaleOffsetPropertyIndent(BaseTexture);
					});
                    UIHelper.DrawWithGroup(() => {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "Normal Map (RGB)"), Normalmap, BumpScale);
                        materialEditor.TextureScaleOffsetPropertyIndent(Normalmap);
                    });
                    UIHelper.DrawWithGroup(() => {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Emission", "Emission (RGB)"), EmissionMap, EmissionColor);
                        materialEditor.TextureScaleOffsetPropertyIndent(EmissionMap);
                    });

                    UIHelper.DrawWithGroup(() => {
                        materialEditor.ShaderProperty(Cull, "Cull");
                        var culling = Cull.floatValue;
                        if(culling < 2){
                            EditorGUI.indentLevel ++;
                            materialEditor.ShaderProperty(DoubleSidedFlipBackfaceNormal, "Flip backface normal");
                            materialEditor.ShaderProperty(DoubleSidedBackfaceLightIntensity, "Backface Light Intensity");
                            materialEditor.ShaderProperty(DoubleSidedBackfaceUseColorShift, "Use Backface Color Shift");
                            var backfaceColorShift = DoubleSidedBackfaceUseColorShift.floatValue;
                            if(backfaceColorShift > 0) {
                                EditorGUI.indentLevel ++;
                                materialEditor.ShaderProperty(DoubleSidedBackfaceHueShiftFromBase, "Hue Shift");
                                materialEditor.ShaderProperty(DoubleSidedBackfaceSaturationFromBase, "Saturation");
                                materialEditor.ShaderProperty(DoubleSidedBackfaceValueFromBase, "Value");
                                EditorGUI.indentLevel --;
                            }
                            EditorGUI.indentLevel --;
                        }

                        if(isFade) materialEditor.ShaderProperty(ZWrite, "ZWrite");
                    });
                });

                // Secondary Common
                if(isStencilReaderDouble) {
                    UIHelper.ShurikenHeader("Secondary Common");
                    UIHelper.DrawWithGroup(() => {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Main Texture", "Base Color Texture (RGB)"), BaseTextureSecondary, BaseColorSecondary);
                        materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "Normal Map (RGB)"), NormalmapSecondary, BumpScaleSecondary);
                        materialEditor.TexturePropertySingleLine(new GUIContent("Emission", "Emission (RGB)"), EmissionMapSecondary, EmissionColorSecondary);
                    });
                }

                // AlphaMask
                if(isFade){
                    IsShowAlphaMask = UIHelper.ShurikenFoldout("AlphaMask", IsShowAlphaMask);
                    if (IsShowAlphaMask) {
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.ShaderProperty(AlphaMask, "Alpha Mask");
                        });
                    }
                }

                // Refraction
                if(isRefracted){
                    UIHelper.ShurikenHeader("Refraction");
                    UIHelper.DrawWithGroup(() => {
                        materialEditor.ShaderProperty(RefractionFresnelExp, "Fresnel Exp");
                        materialEditor.ShaderProperty(RefractionStrength, "Strength");
                    });
                }

                // Alpha Cutout
                if(isCutout){
                    UIHelper.ShurikenHeader("Alpha Cutout");
                    UIHelper.DrawWithGroup(() => {
                        materialEditor.ShaderProperty(CutoutCutoutAdjust, "Cutoff Adjust");
                    });
                }

                // Shadow
                UIHelper.ShurikenHeader("Shadow");
                UIHelper.DrawWithGroup(() => {
                    materialEditor.ShaderProperty(Shadowborder, "Border");

                    UIHelper.DrawWithGroup(() => {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Strength & Mask", "Strength and Mask Texture"), ShadowStrengthMask, ShadowStrength);
                        materialEditor.TextureScaleOffsetPropertyIndent(ShadowStrengthMask);
                    });

                    UIHelper.DrawWithGroup(() => {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Blur & Mask", "Blur and Mask Texture"), ShadowborderBlurMask, ShadowborderBlur);
                        materialEditor.TextureScaleOffsetPropertyIndent(ShadowborderBlurMask);
                    });

                    UIHelper.DrawWithGroup(() => {
                        materialEditor.ShaderProperty(ShadowUseStep, "Use Steps");
                        var useStep = ShadowUseStep.floatValue;
                        if(useStep > 0)
                        {
                            EditorGUI.indentLevel ++;
                            ShadowSteps.floatValue = EditorGUILayout.IntSlider(
                                new GUIContent("Steps"),
                                (int)ShadowSteps.floatValue,
                                (int)ShadowSteps.rangeLimits.x,
                                (int)ShadowSteps.rangeLimits.y)
                            ;
                            EditorGUI.indentLevel --;
                        }
                    });

                    UIHelper.DrawWithGroup(() => {

                        {
                            EditorGUILayout.HelpBox(
                                "[Strength] max is recommended for using custom shade." + Environment.NewLine + "Custom Shadeの使用時は[Strength]を最大値に設定することを推奨", MessageType.Info);
                            UIHelper.DrawWithGroup(() => {
                                EditorGUILayout.LabelField("1st shade", EditorStyles.boldLabel);
                                EditorGUI.indentLevel ++;
                                materialEditor.ShaderProperty(ShadowPlanBUseCustomShadowTexture, "Use Shade Texture");
                                var useShadeTexture = ShadowPlanBUseCustomShadowTexture.floatValue;
                                if(useShadeTexture > 0)
                                {
                                    materialEditor.ShaderProperty(ShadowPlanBCustomShadowTexture, "Shade Texture");
                                    materialEditor.ShaderProperty(ShadowPlanBCustomShadowTextureRGB, "Shade Texture RGB");
                                }
                                else
                                {
                                    materialEditor.ShaderProperty(ShadowPlanBHueShiftFromBase, "Hue Shift");
                                    materialEditor.ShaderProperty(ShadowPlanBSaturationFromBase, "Saturation");
                                    materialEditor.ShaderProperty(ShadowPlanBValueFromBase, "Value");
                                }
                                EditorGUI.indentLevel --;
                            });
                        }
                    });
                });

                // Gloss
                UIHelper.ShurikenHeader("Gloss");
                materialEditor.DrawShaderPropertySameLIne(UseGloss);
                var isEnabledGloss = UseGloss.floatValue;
                if(isEnabledGloss > 0)
                {
                    UIHelper.DrawWithGroup(() => {
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Smoothness & Mask", "Smoothness and Mask Texture"), GlossBlendMask, GlossBlend);
                            materialEditor.TextureScaleOffsetPropertyIndent(GlossBlendMask);
                        });
                        materialEditor.ShaderProperty(GlossPower, "Metallic");
                        materialEditor.ShaderProperty(GlossColor, "Color");
                    });
                }

                // Outline
                if(!isRefracted && isOutline) {
                    UIHelper.ShurikenHeader("Outline");
                    materialEditor.DrawShaderPropertySameLIne(UseOutline);
                    var useOutline = UseOutline.floatValue;
                    if(useOutline > 0)
                    {
                        UIHelper.DrawWithGroup(() => {
                            UIHelper.DrawWithGroup(() => {
                                materialEditor.TexturePropertySingleLine(new GUIContent("Width & Mask", "Width and Mask Texture"), OutlineWidthMask, OutlineWidth);
                                materialEditor.TextureScaleOffsetPropertyIndent(OutlineWidthMask);
                            });
                            UIHelper.DrawWithGroup(() => {
                                if(!isOpaque) {
                                        materialEditor.TexturePropertySingleLine(new GUIContent("Cutoff Mask & Range", "Cutoff Mask Texture & Range"), OutlineMask, OutlineCutoffRange);
                                        materialEditor.TextureScaleOffsetPropertyIndent(OutlineMask);
                                }else{
                                    EditorGUILayout.LabelField("Cutoff Mask & Range","Unavailable in Opaque", EditorStyles.centeredGreyMiniLabel);
                                }
                            });
                            UIHelper.DrawWithGroup(() => {
                                materialEditor.TexturePropertySingleLine(new GUIContent("Texture & Color", "Texture and Color"), OutlineTexture, OutlineColor);
                                materialEditor.TextureScaleOffsetPropertyIndent(OutlineTexture);
                                materialEditor.ShaderProperty(OutlineTextureColorRate,"Base Color Mix");
                                materialEditor.ShaderProperty(OutlineUseColorShift, "Use Color Shift");
                                var isEnabledOutlineColorShift = OutlineUseColorShift.floatValue;
                                if(isEnabledOutlineColorShift > 0) {
                                    EditorGUI.indentLevel ++;
                                    materialEditor.ShaderProperty(OutlineHueShiftFromBase, "Hue Shift");
                                    materialEditor.ShaderProperty(OutlineSaturationFromBase, "Saturation");
                                    materialEditor.ShaderProperty(OutlineValueFromBase, "Value");
                                    EditorGUI.indentLevel --;
                                }
                            });
                            materialEditor.ShaderProperty(OutlineShadeMix,"Shadow mix");
                        });
                    }
                }

                // MatCap
                UIHelper.ShurikenHeader("MatCap");
                materialEditor.DrawShaderPropertySameLIne(MatcapBlendMode);
                var useMatcap = MatcapBlendMode.floatValue;
                if(useMatcap != 3) // Not 'Unused'
                {
                    UIHelper.DrawWithGroup(() => {
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Blend & Mask", "Blend and Mask Texture"), MatcapBlendMask, MatcapBlend);
                            materialEditor.TextureScaleOffsetPropertyIndent(MatcapBlendMask);
                        });
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Texture & Color", "Color and Texture"), MatcapTexture, MatcapColor);
                            materialEditor.TextureScaleOffsetPropertyIndent(MatcapTexture);
                        });
                        materialEditor.ShaderProperty(MatcapNormalMix, "Normal Map mix");
                        materialEditor.ShaderProperty(MatcapShadeMix,"Shadow mix");
                    });
                }

                // Reflection
                UIHelper.ShurikenHeader("Reflection");
                materialEditor.DrawShaderPropertySameLIne(UseReflection);
                var useReflection = UseReflection.floatValue;
                if(useReflection > 0)
                {
                    UIHelper.DrawWithGroup(() => {
                        materialEditor.ShaderProperty(UseReflectionProbe,"Use Reflection Probe");
                        var useProbe = UseReflectionProbe.floatValue;
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Smoothness & Mask", "Smoothness and Mask Texture"), ReflectionReflectionMask, ReflectionReflectionPower);
                            materialEditor.TextureScaleOffsetPropertyIndent(ReflectionReflectionMask);
                        });
                        UIHelper.DrawWithGroup(() => {
                            var cubemapLabel = "Cubemap";
                            if(useProbe > 0) {
                                cubemapLabel += "(fallback)";
                            }
                            materialEditor.TexturePropertySingleLine(new GUIContent(cubemapLabel, cubemapLabel), ReflectionCubemap);
                            materialEditor.TextureScaleOffsetPropertyIndent(ReflectionCubemap);
                        });
                        materialEditor.ShaderProperty(ReflectionNormalMix,"Normal Map mix");
                        materialEditor.ShaderProperty(ReflectionShadeMix, "Shadow mix");
                        materialEditor.ShaderProperty(ReflectionSuppressBaseColorValue,"Suppress Base Color");
                    });
                }

                // Rim Light
                UIHelper.ShurikenHeader("Rim");
                materialEditor.DrawShaderPropertySameLIne(UseRim);
                var useRim = UseRim.floatValue;
                if(useRim > 0)
                {
                    UIHelper.DrawWithGroup(() => {

                        materialEditor.ShaderProperty(RimBlendStart,"Blend Start");
                        materialEditor.ShaderProperty(RimBlendEnd,"Blend End");
                        materialEditor.ShaderProperty(RimPow,"Power type");

                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Blend & Mask", "Blend and Mask Texture"), RimBlendMask, RimBlend);
                            materialEditor.TextureScaleOffsetPropertyIndent(RimBlendMask);
                        });
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Texture & Color", "Texture and Color"), RimTexture, RimColor);
                            materialEditor.TextureScaleOffsetPropertyIndent(RimTexture);
                            materialEditor.ShaderProperty(RimUseBaseTexture,"Use Base Color");
                        });
                        materialEditor.ShaderProperty(RimShadeMix,"Shadow mix");
                    });
                }

                // Shade Cap
                UIHelper.ShurikenHeader("Shade Cap");
                materialEditor.DrawShaderPropertySameLIne(ShadowCapBlendMode);
                var useShadowCap = ShadowCapBlendMode.floatValue;
                if(useShadowCap != 3) // Not 'Unused'
                {
                    UIHelper.DrawWithGroup(() => {
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Blend & Mask", "Blend and Mask Texture"), ShadowCapBlendMask, ShadowCapBlend);
                            materialEditor.TextureScaleOffsetPropertyIndent(ShadowCapBlendMask);
                        });
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Texture", "Texture"), ShadowCapTexture);
                            materialEditor.TextureScaleOffsetPropertyIndent(ShadowCapTexture);
                        });
                        materialEditor.ShaderProperty(ShadowCapNormalMix,"Normal Map mix");
                    });
                }

                // Stencil Writer
                if(isStencilWriter)
                {
                    UIHelper.ShurikenHeader("Stencil Writer");
                    UIHelper.DrawWithGroup(() => {
                        materialEditor.ShaderProperty(StencilNumber,"Stencil Number");
                        if(isStencilWriterMask) {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Stencil Mask & Range", "Stencil Mask and Range"), StencilMaskTex, StencilMaskAdjust);
                            materialEditor.TextureScaleOffsetPropertyIndent(StencilMaskTex);
                        }
                        if(isStencilWriterMask) materialEditor.ShaderProperty(StencilMaskAlphaDither, "Alpha(Dither)");
                    });
                }

                // Stencil Reader
                if(isStencilReader)
                {
                    UIHelper.ShurikenHeader("Stencil Reader");
                    if(isStencilReaderDouble) {
                        UIHelper.DrawWithGroup(() => {
                            UIHelper.DrawWithGroup(() => {
                                EditorGUILayout.LabelField("Primary", EditorStyles.boldLabel);
                                EditorGUI.indentLevel++;
                                materialEditor.ShaderProperty(StencilNumber,"Number");
                                materialEditor.ShaderProperty(StencilCompareAction,"Compare Action");
                                EditorGUI.indentLevel--;
                            });
                            UIHelper.DrawWithGroup(() => {
                                EditorGUILayout.LabelField("Secondary", EditorStyles.boldLabel);
                                EditorGUI.indentLevel++;
                                materialEditor.ShaderProperty(StencilNumberSecondary,"Number");
                                materialEditor.ShaderProperty(StencilCompareActionSecondary,"Compare Action");
                                EditorGUI.indentLevel--;
                            });
                        });
                    } else {
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.ShaderProperty(StencilNumber,"Number");
                            materialEditor.ShaderProperty(StencilCompareAction,"Compare Action");
                        });
                    }
                }

                // Parallax Emission
                UIHelper.ShurikenHeader("Parallaxed Emission");
                materialEditor.DrawShaderPropertySameLIne(UseEmissionParallax);
                var useEmissionPara = UseEmissionParallax.floatValue;
                if(useEmissionPara > 0){
                    UIHelper.DrawWithGroup(() => {
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Texture & Color", "Texture and Color"), EmissionParallaxTex, EmissionParallaxColor);
                            materialEditor.TextureScaleOffsetPropertyIndent(EmissionParallaxTex);
                        });
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("TexCol Mask", "Texture and Color Mask"), EmissionParallaxMask);
                            materialEditor.TextureScaleOffsetPropertyIndent(EmissionParallaxMask);
                        });
                        UIHelper.DrawWithGroup(() => {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Depth & Mask", "Depth and Mask Texture"), EmissionParallaxDepthMask, EmissionParallaxDepth);
                            materialEditor.TextureScaleOffsetPropertyIndent(EmissionParallaxDepthMask);
                            materialEditor.ShaderProperty(EmissionParallaxDepthMaskInvert, "Invert Depth Mask");
                        });
                    });
                }

                // Scrolled Emission
                if(isEmissiveFreak)
                {
                    UIHelper.ShurikenHeader("Emissive Freak");
                    UIHelper.DrawWithGroup(() => {
                        UIHelper.DrawWithGroup(() => {
                            EditorGUILayout.LabelField("1st", EditorStyles.boldLabel);
                            UIHelper.DrawWithGroup(() => {
                                materialEditor.TexturePropertySingleLine(new GUIContent("Texture & Color", "Texture and Color"), EmissiveFreak1Tex, EmissiveFreak1Color);
                                materialEditor.TextureScaleOffsetPropertyIndent(EmissiveFreak1Tex);
                                materialEditor.TexturePropertySingleLine(new GUIContent("TexCol Mask", "Texture and Color Mask"), EmissiveFreak1Mask);
                                materialEditor.TextureScaleOffsetPropertyIndent(EmissiveFreak1Mask);
                                UIHelper.DrawWithGroup(() => {
                                    materialEditor.ShaderProperty(EmissiveFreak1U, "Scroll U");
                                    materialEditor.ShaderProperty(EmissiveFreak1V, "Scroll V");
                                });
                                UIHelper.DrawWithGroup(() => {
                                    materialEditor.TexturePropertySingleLine(new GUIContent("Depth & Mask", "Depth and Mask Texture"), EmissiveFreak1DepthMask, EmissiveFreak1Depth);
                                    materialEditor.TextureScaleOffsetPropertyIndent(EmissiveFreak1DepthMask);
                                    materialEditor.ShaderProperty(EmissiveFreak1DepthMaskInvert, "Invert Depth Mask");
                                });
                                UIHelper.DrawWithGroup(() => {
                                    materialEditor.ShaderProperty(EmissiveFreak1Breathing, "Breathing");
                                    materialEditor.ShaderProperty(EmissiveFreak1BreathingMix, "Breathing Mix");
                                    materialEditor.ShaderProperty(EmissiveFreak1BlinkOut, "Blink Out");
                                    materialEditor.ShaderProperty(EmissiveFreak1BlinkOutMix, "Blink Out Mix");
                                    materialEditor.ShaderProperty(EmissiveFreak1BlinkIn, "Blink In");
                                    materialEditor.ShaderProperty(EmissiveFreak1BlinkInMix, "Blink In Mix");
                                    materialEditor.ShaderProperty(EmissiveFreak1HueShift, "Hue Shift");
                                });
                            });
                        });

                        UIHelper.DrawWithGroup(() => {
                            EditorGUILayout.LabelField("2nd", EditorStyles.boldLabel);
                            UIHelper.DrawWithGroup(() => {
                                materialEditor.TexturePropertySingleLine(new GUIContent("Texture & Color", "Texture and Color"), EmissiveFreak2Tex, EmissiveFreak2Color);
                                materialEditor.TextureScaleOffsetPropertyIndent(EmissiveFreak2Tex);
                                materialEditor.TexturePropertySingleLine(new GUIContent("TexCol Mask", "Texture and Color Mask"), EmissiveFreak2Mask);
                                materialEditor.TextureScaleOffsetPropertyIndent(EmissiveFreak2Mask);
                                UIHelper.DrawWithGroup(() => {
                                    materialEditor.ShaderProperty(EmissiveFreak2U, "Scroll U");
                                    materialEditor.ShaderProperty(EmissiveFreak2V, "Scroll V");
                                });
                                UIHelper.DrawWithGroup(() => {
                                    materialEditor.TexturePropertySingleLine(new GUIContent("Depth & Mask", "Depth and Mask Texture"), EmissiveFreak2DepthMask, EmissiveFreak2Depth);
                                    materialEditor.TextureScaleOffsetPropertyIndent(EmissiveFreak2DepthMask);
                                    materialEditor.ShaderProperty(EmissiveFreak2DepthMaskInvert, "Invert Depth Mask");
                                });
                                UIHelper.DrawWithGroup(() => {
                                    materialEditor.ShaderProperty(EmissiveFreak2Breathing, "Breathing");
                                    materialEditor.ShaderProperty(EmissiveFreak2BreathingMix, "Breathing Mix");
                                    materialEditor.ShaderProperty(EmissiveFreak2BlinkOut, "Blink Out");
                                    materialEditor.ShaderProperty(EmissiveFreak2BlinkOutMix, "Blink Out Mix");
                                    materialEditor.ShaderProperty(EmissiveFreak2BlinkIn, "Blink In");
                                    materialEditor.ShaderProperty(EmissiveFreak2BlinkInMix, "Blink In Mix");
                                    materialEditor.ShaderProperty(EmissiveFreak2HueShift, "Hue Shift");
                                });
                            });
                        });
                    });
                }


                // Advanced / Experimental
                IsShowAdvanced = UIHelper.ShurikenFoldout("Advanced / Experimental (Click to Open)", IsShowAdvanced);
                if (IsShowAdvanced) {
                    UIHelper.DrawWithGroup(() => {
                        EditorGUILayout.HelpBox("These are some shade tweaking. no need to change usually." + Environment.NewLine + "ほとんどのケースで触る必要のないやつら。",MessageType.Info);
                        if (GUILayout.Button("Revert advanced params.")){
                            PointAddIntensity.floatValue = 1f;
                            PointShadowStrength.floatValue = 0.5f;
                            PointShadowborder.floatValue = 0.5f;
                            PointShadowborderBlur.floatValue = 0.01f;
                            PointShadowborderBlurMask.textureValue = null;
                            OtherShadowAdjust.floatValue = -0.1f;
                            OtherShadowBorderSharpness.floatValue = 3;
                            PointShadowUseStep.floatValue = 0;
                            PointShadowSteps.floatValue = 2;
                            ShadowIndirectIntensity.floatValue = 0.25f;
                            VertexColorBlendDiffuse.floatValue = 0f;
                            VertexColorBlendEmissive.floatValue = 0f;
                        }
                        UIHelper.DrawWithGroup(() => {
                            EditorGUILayout.LabelField("Directional Shadow", EditorStyles.boldLabel);
                            EditorGUI.indentLevel ++;
                            materialEditor.ShaderProperty(ShadowIndirectIntensity, "Indirect face Intensity (0.25)");
                            EditorGUI.indentLevel --;
                        });
                        UIHelper.DrawWithGroup(() => {
                            EditorGUILayout.LabelField("Vertex Colors", EditorStyles.boldLabel);
                            EditorGUI.indentLevel ++;
                            materialEditor.ShaderProperty(VertexColorBlendDiffuse, "Color blend to diffuse (def:0) ");
                            materialEditor.ShaderProperty(VertexColorBlendEmissive, "Color blend to emissive (def:0) ");
                            EditorGUI.indentLevel --;
                        });
                        UIHelper.DrawWithGroup(() => {
                            EditorGUILayout.LabelField("PointLights", EditorStyles.boldLabel);
                            EditorGUI.indentLevel ++;
                            materialEditor.ShaderProperty(PointAddIntensity, "Intensity (def:1)");
                            materialEditor.ShaderProperty(PointShadowStrength, "Shadow Strength (def:0.5)");
                            materialEditor.ShaderProperty(PointShadowborder, "Shadow Border (def:0.5)");
                            materialEditor.ShaderProperty(PointShadowborderBlur, "Shadow Border blur (def:0.01)");
                            materialEditor.ShaderProperty(PointShadowborderBlurMask, "Shadow Border blur Mask(def:none)");
                            materialEditor.ShaderProperty(PointShadowUseStep, "Use Shadow Steps");
                            var usePointStep = PointShadowUseStep.floatValue;
                            if(usePointStep > 0)
                            {
                                materialEditor.ShaderProperty(PointShadowSteps, " ");
                            }
                            EditorGUI.indentLevel --;
                        });
                        UIHelper.DrawWithGroup(() => {
                            EditorGUILayout.LabelField("Shade from other meshes", EditorStyles.boldLabel);
                            EditorGUI.indentLevel ++;
                            materialEditor.ShaderProperty(OtherShadowAdjust, "Adjust (def:-0.1)");
                            materialEditor.ShaderProperty(OtherShadowBorderSharpness, "Sharpness(def:3)");
                            EditorGUI.indentLevel --;
                        });
                    });
                }

                // Unregisteredprops
                IsShowNonRegistered = UIHelper.ShurikenFoldout("NonRegisteredProperties", IsShowNonRegistered);
                if(IsShowNonRegistered) {
                     DrawNonRegisteredProperties(materialEditor, props);
                }

                // Docs
                UIHelper.DrawWithGroupHorizontal(() => {
                    if(GUILayout.Button("How to use (Japanese)"))
                    {
                        System.Diagnostics.Process.Start("https://synqark.github.io/Arktoon-Shaders-Doc/");
                    }
                });
            }
            EditorGUI.EndChangeCheck();
        }

        List<string> registeredProperties = new List<string>();

        public void ClearRegisteredPropertiesList() {
            registeredProperties.Clear();
        }

        public MaterialProperty FindAndRegisterProperties(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory) {
            if (!registeredProperties.Contains(propertyName)) registeredProperties.Add(propertyName);
            return FindProperty(propertyName, properties, propertyIsMandatory);
        }

        public void DrawNonRegisteredProperties(MaterialEditor materialEditor, MaterialProperty[] props) {

            Material material = materialEditor.target as Material;
            var propCounts = ShaderUtil.GetPropertyCount(material.shader);
            for(var i = 0; i < propCounts; ++i) {
                var shName = ShaderUtil.GetPropertyName(material.shader, i);
                if (!registeredProperties.Contains(shName)) {
                    MaterialProperty p = FindProperty(shName, props, false);
                    materialEditor.ShaderProperty(p, shName);
                }
            }
        }
    }

    static class UIHelper
    {
        static int HEADER_HEIGHT = 22;

        public static void DrawShaderPropertySameLIne(this MaterialEditor editor, MaterialProperty property) {
            Rect r = EditorGUILayout.GetControlRect(true,0,EditorStyles.layerMaskField);
            r.y -= HEADER_HEIGHT;
            r.height = MaterialEditor.GetDefaultPropertyHeight(property);
            editor.ShaderProperty(r, property, " ");
        }

        private static Rect DrawShuriken(string title, Vector2 contentOffset) {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.margin = new RectOffset(0, 0, 8, 0);
            style.font = new GUIStyle(EditorStyles.boldLabel).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = HEADER_HEIGHT;
            style.contentOffset = contentOffset;
            var rect = GUILayoutUtility.GetRect(16f, HEADER_HEIGHT, style);
            GUI.Box(rect, title, style);
            return rect;
        }
        public static void ShurikenHeader(string title)
        {
            DrawShuriken(title,new Vector2(6f, -2f));
        }
        public static bool ShurikenFoldout(string title, bool display)
        {
            var rect = DrawShuriken(title,new Vector2(20f, -2f));
            var e = Event.current;
            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint) {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition)) {
                display = !display;
                e.Use();
            }
            return display;
        }
        public static void Vector2Property(MaterialProperty property, string name)
        {
            EditorGUI.BeginChangeCheck();
            Vector2 vector2 = EditorGUILayout.Vector2Field(name,new Vector2(property.vectorValue.x, property.vectorValue.y),null);
            if (EditorGUI.EndChangeCheck())
                property.vectorValue = new Vector4(vector2.x, vector2.y);
        }
        public static void Vector4Property(MaterialProperty property, string name)
        {
            EditorGUI.BeginChangeCheck();
            Vector4 vector4 = EditorGUILayout.Vector2Field(name,property.vectorValue,null);
            if (EditorGUI.EndChangeCheck())
                property.vectorValue = vector4;
        }
        public static void Vector2PropertyZW(MaterialProperty property, string name)
        {
            EditorGUI.BeginChangeCheck();
            Vector2 vector2 = EditorGUILayout.Vector2Field(name,new Vector2(property.vectorValue.x, property.vectorValue.y),null);
            if (EditorGUI.EndChangeCheck())
                property.vectorValue = new Vector4(vector2.x, vector2.y);
        }
        public static void TextureScaleOffsetPropertyIndent(this MaterialEditor editor, MaterialProperty property)
        {
            EditorGUI.indentLevel ++;
            editor.TextureScaleOffsetProperty(property);
            EditorGUI.indentLevel --;
        }
        public static void DrawWithGroup(Action action)
        {
            EditorGUILayout.BeginVertical( GUI.skin.box );
            action();
            EditorGUILayout.EndVertical();
        }
        public static void DrawWithGroupHorizontal(Action action)
        {
            EditorGUILayout.BeginHorizontal( GUI.skin.box );
            action();
            EditorGUILayout.EndHorizontal();
        }
    }
}