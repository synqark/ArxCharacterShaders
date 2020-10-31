float4 frag(
    #ifdef AXCS_OUTLINE
        g2f i
    #else
        VertexOutput i
    #endif
    ,  bool isFrontFace : SV_IsFrontFace
    ) : SV_Target
{
    UNITY_VERTEX_OUTPUT_STEREO(i);

    // 表裏・アウトライン
    fixed faceSign = isFrontFace ? 1 : -1;
    bool isOutline = i.color.a;

    // アウトラインの裏面は常に削除
    clip(1 - isOutline + isFrontFace - 0.001);

    // 後の計算に使う変数を予め定義
    float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir * lerp(1, faceSign, _DoubleSidedFlipBackfaceNormal));
    float3 viewDirection = normalize(UnityWorldSpaceViewDir(i.posWorld.xyz));
    float3 normalLocal = UnpackScaleNormal(tex2D(REF_BUMPMAP,TRANSFORM_TEX(i.uv0, REF_BUMPMAP)), REF_BUMPSCALE);
    float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
    float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz + float3(0, +0.0000000001, 0));
    float3 lightColor = _LightColor0.rgb;
    float3 halfDirection = normalize(viewDirection+lightDirection);
    float3 cameraSpaceViewDir = mul((float3x3)unity_WorldToCamera, viewDirection);

    #if !defined(SHADOWS_SCREEN)
        float attenuation = 1;
    #else
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.posWorld.xyz);
    #endif

    // 拡散色（基本色）を決定
    // テクスチャカラー×プロパティカラー×頂点カラー（頂点ブレンドを設定している場合）
    float4 _MainTex_var = UNITY_SAMPLE_TEX2D(REF_MAINTEX, TRANSFORM_TEX(i.uv0, REF_MAINTEX));
    float3 Diffuse = (_MainTex_var.rgb*REF_COLOR.rgb);
    Diffuse = lerp(Diffuse, Diffuse * i.color, _VertexColorBlendDiffuse);

    // Cutoutの場合、アルファが決定したのでここでクリップする。
    #ifdef AXCS_CUTOUT
        clip((_MainTex_var.a * REF_COLOR.a) - _CutoutCutoutAdjust);
    #endif

    #ifdef AXCS_OUTLINE
        // 描画しているものがアウトラインの場合
        if (isOutline) {
            // アウトラインクリップ処理
            #if defined(AXCS_CUTOUT) || defined(AXCS_FADE)
                float _OutlineMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_OutlineMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _OutlineMask)).r;
                clip(_OutlineMask_var.r - _OutlineCutoffRange);
            #endif
            // アウトライン用のテクスチャと色を使う場合は、その割合でDiffuseを更新する
            float4 _OutlineTexture_var = UNITY_SAMPLE_TEX2D_SAMPLER(_OutlineTexture, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _OutlineTexture));
            float3 outlineColor = lerp(float3(_OutlineColor.rgb * _OutlineTexture_var.rgb), Diffuse, _OutlineTextureColorRate);
            if (_OutlineUseColorShift) {
                float3 Outline_Diff_HSV = CalculateHSV(outlineColor, _OutlineHueShiftFromBase, _OutlineSaturationFromBase, _OutlineValueFromBase);
                Diffuse = Outline_Diff_HSV;
            } else {
                Diffuse = outlineColor;
            }
        }
    #endif

    // 空間光をサンプリング
    float3 ShadeSH9Plus = GetSHLength();
    float3 ShadeSH9Minus = ShadeSH9(float4(0,0,0,1)) * _ShadowAmbientIntensity;

    // もっとも明るい部分を取得
    float3 directLightingShadowFactor = ShadeSH9Plus * lerp(_ShadowAmbientIntensity, 1, saturate(lightColor));
    float3 directLighting = saturate(directLightingShadowFactor + lightColor);
    // もっとも暗い部分を取得
    float3 indirectLighting = saturate(ShadeSH9Minus);

    // 最も明るい部分と最も暗い部分をそれぞれグレースケールに変換し、その差を取得
    float3 grayscale_vector = grayscale_vector_node();
    float grayscalelightcolor = dot(lightColor,grayscale_vector);
    float bottomIndirectLighting = dot(ShadeSH9Minus,grayscale_vector);
    float topIndirectLighting = dot(ShadeSH9Plus,grayscale_vector);
    float lightDifference = (topIndirectLighting+grayscalelightcolor)-bottomIndirectLighting;

    // 陰と光のグラデーション強度を決定
    fixed _ShadowborderBlur_var = UNITY_SAMPLE_TEX2D_SAMPLER(_ShadowborderBlurMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _ShadowborderBlurMask)).r * _ShadowborderBlur;
    float ShadowborderMin = saturate(_Shadowborder - _ShadowborderBlur_var/2);//この場合saturateはコスト０でmaxより軽量です
    float ShadowborderMax = saturate(_Shadowborder + _ShadowborderBlur_var/2);//この場合saturateはコスト０でminより軽量です

    // 計算済みの「明るい部分」「暗い部分」「それらの差」を利用し、現在描画しているピクセルの明るさがどれだけなのかを０（最も暗い）～１（最も明るい）で正規化して取得する
    // →受光強度とする
    float grayscaleDirectLighting2 = ((dot(lightDirection,normalDirection)*0.5+0.5)*grayscalelightcolor) + dot(ShadeSH9Normal( normalDirection ),grayscale_vector);
    float remappedLight2 = ((grayscaleDirectLighting2-bottomIndirectLighting)/lightDifference);
    float directContribution = 1.0 - ((1.0 - saturate(( (saturate(remappedLight2) - ShadowborderMin)) / (ShadowborderMax - ShadowborderMin))));

    // （若干おまじない）周囲のオブジェクトから受けた「影」の明るさを補正する。
    float selfShade = saturate(dot(lightDirection,normalDirection)+1+_OtherShadowAdjust);
    float otherShadow = saturate(saturate(mad(attenuation,2 ,-1))+mad(_OtherShadowBorderSharpness,-selfShade,_OtherShadowBorderSharpness));
    float tmpDirectContributionFactor0 = saturate(dot(lightColor,grayscale_for_light())*1.5);
    directContribution = lerp(0, directContribution, saturate(1-(mad(tmpDirectContributionFactor0,-otherShadow,tmpDirectContributionFactor0))));

    // 設定されていれば、受光強度をステップ化する
    directContribution = lerp(directContribution, min(1,floor(directContribution * _ShadowSteps) / (_ShadowSteps - 1)), _ShadowUseStep);

    // 各種プロパティから受光強度を直接補正する
    // １：裏面専用の倍率を受光強度に乗算
    float additionalContributionMultiplier = lerp(_DoubleSidedBackfaceLightIntensity, 1, isFrontFace);

    // ２：ShadeCapでLightShutterを使っている場合、受光強度にマスク処理を施す
    if (_ShadowCapBlendMode == 2) {
        float3 normalDirectionShadowCap = normalize(mul( float3(normalLocal.r*_ShadowCapNormalMix,normalLocal.g*_ShadowCapNormalMix,normalLocal.b), tangentTransform )); // Perturbed normals
        float2 transformShadowCap = ComputeTransformCap(cameraSpaceViewDir,normalDirectionShadowCap);
        float4 _ShadowCapTexture_var =  UNITY_SAMPLE_TEX2D_SAMPLER(_ShadowCapTexture, REF_MAINTEX, TRANSFORM_TEX(transformShadowCap, _ShadowCapTexture));
        float4 _ShadowCapBlendMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_ShadowCapBlendMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _ShadowCapBlendMask));
        additionalContributionMultiplier *= saturate(1.0 - mad(_ShadowCapBlendMask_var.rgb,-_ShadowCapTexture_var.rgb,_ShadowCapBlendMask_var.rgb)*_ShadowCapBlend);
    }
    // 受光強度に反映
    directContribution *= additionalContributionMultiplier;


    // 頂点ライティングの情報を計算----------------------------------------------
    // 受光強度の補正はこちらにも反映する
    float3 coloredLight_sum = float3(0,0,0);
    fixed _PointShadowborderBlur_var = UNITY_SAMPLE_TEX2D_SAMPLER(_PointShadowborderBlurMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _PointShadowborderBlurMask)).r * _PointShadowborderBlur;
    float VertexShadowborderMin = saturate(-_PointShadowborderBlur_var*0.5 + _PointShadowborder);
    float VertexShadowborderMax = saturate( _PointShadowborderBlur_var*0.5 + _PointShadowborder);
    float4 directContributionVertex = 1.0 - ((1.0 - saturate(( (saturate(i.ambientAttenuation) - VertexShadowborderMin)) / (VertexShadowborderMax - VertexShadowborderMin))));
    directContributionVertex = lerp(directContributionVertex, min(1,floor(directContributionVertex * _PointShadowSteps) / (_PointShadowSteps - 1)), _PointShadowUseStep) * additionalContributionMultiplier;
    //ベクトル演算を減らしつつ、複数のスカラー演算を一つのベクトル演算にまとめました。
    //現代のPC向けGPUはほぼ100%がスカラー型であり、ベクトル演算は基本的にその次元数分ALU負荷が倍増します。
    //複数の掛け算は基本的にスカラーを左に寄せるだけでベクトル演算が減って最適化に繋がります。
    float4 tmpColoredLightFactorAttenuated = directContributionVertex * i.ambientAttenuation;
    float4 tmpColoredLightFactorIndirect = mad(i.ambientIndirect,-_PointShadowStrength,i.ambientIndirect);
    float3 coloredLight_0 = max(tmpColoredLightFactorAttenuated.r ,tmpColoredLightFactorIndirect.r) * i.lightColor0;
    float3 coloredLight_1 = max(tmpColoredLightFactorAttenuated.g ,tmpColoredLightFactorIndirect.g) * i.lightColor1;
    float3 coloredLight_2 = max(tmpColoredLightFactorAttenuated.b ,tmpColoredLightFactorIndirect.b) * i.lightColor2;
    float3 coloredLight_3 = max(tmpColoredLightFactorAttenuated.a ,tmpColoredLightFactorIndirect.a) * i.lightColor3;
    coloredLight_sum = (coloredLight_0 + coloredLight_1 + coloredLight_2 + coloredLight_3) * _PointAddIntensity;


    // 受光強度を線形ではなく多少ゆるやかにしてみたかった
    // TODO:この時点のdirectContributionにShadowRampテクスチャを適用してみたい。
    // directContribution = 1 - (1 - directContribution) * (1 - directContribution);

    // 受光強度の計算が終わったので空間色を確定
    float3 finalLight = lerp(indirectLighting,directLighting,directContribution)+coloredLight_sum;

    float3 toonedMap = float3(0,0,0);
    {
        // Contributionが0の地点の色（影色）の決定
        float3 ShadeMap = float3(0,0,0);
        if (_ShadowPlanBUseCustomShadowTexture) {
            float4 _ShadowPlanBCustomShadowTexture_var = UNITY_SAMPLE_TEX2D_SAMPLER(_ShadowPlanBCustomShadowTexture, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _ShadowPlanBCustomShadowTexture));
            float3 shadowCustomTexture = _ShadowPlanBCustomShadowTexture_var.rgb * _ShadowPlanBCustomShadowTextureRGB.rgb;
            ShadeMap = shadowCustomTexture;
        } else {
            float3 Diff_HSV = CalculateHSV(Diffuse, _ShadowPlanBHueShiftFromBase, _ShadowPlanBSaturationFromBase, _ShadowPlanBValueFromBase);
            ShadeMap = Diff_HSV;
        }


        float _ShadowStrengthMask_var = tex2D(_ShadowStrengthMask, TRANSFORM_TEX(i.uv0, _ShadowStrengthMask));
        float shadowStrength = _ShadowStrengthMask_var * _ShadowStrength;

        float3 lightColorWhenShaded = directLighting + lerp(0, indirectLighting - directLighting, shadowStrength); // ３

        float3 finalLightColor = lerp(lightColorWhenShaded, directLighting, directContribution)+coloredLight_sum; // ５

        float3 finslBaseColor = lerp(ShadeMap, Diffuse, directContribution); // ７

        // finalLight = lerp(ShadeMap,directLighting,directContribution)+coloredLight_sum;
        // toonedMap = (Diffuse*directContribution + (ShadeMap * (1-directContribution)))*finalLight;
        toonedMap = finslBaseColor * finalLightColor; // ８
        /*

        １．サンプリングした最大の光の色 : directLighting
            TODO:光の影響を受けたくない場合に「Lerp(directLighting, 1, 利用者がUnlitにさせたい割合（1＝完全Unlit）)」をする。していいかどうかは要確認

        ２．サンプリングした最大の陰の色 : indirectLighting
        Ａ．サンプリングした重要度の低いポイントライトの色：coloredLight_sum
        ３．陰に対して陰の色をどの程度適用するか：[１] + lerp(0, [２] - [１], ShadowStrength) // ...あれ、ShadowStrengthってここでは？上の処理いらん気がしてきた
        　　　ShadowStrengthがゼロの場合、陰に対しても最大の光が決定される。
        ４．その地点にどの程度の光が降っているか（０～１で正規化済み）：directContribution
        ５．その地点に提供すべき光の色：lerp([３], [１], [４]) + [Ａ]
        ６．利用者が陰となるべき部分に適用したいテクスチャの色　: ShadeMap(ベースカラーから計算済み)
        ７．その地点で使用すべきテクスチャの色：lerp([６], Diffuse, [４])
        ８．その地点であるべき色：[５]＊[７]

        + Shadow Rampを指定できる何か

        空間にあるGIとライトで勝手に計算する価：
        directLighting
        indirectLighting
        directContribution

        利用者に弄ってもらいたいプロパティ：
        ShadowStrength：陰の適用度
        ShadeMap：影となるべき部分に適用したいテクスチャの色

        TODO：
        １．サンプリングした光の色もlerp(1, hoge, fuga)でコントロールできるようにしたい。望めばUnlitみたいにできるということ。
        ２．上の計算式に影２を作る場合、どうやって作るべきか考える。ShadeMapの計算プロセスに介入すればいける？
        ３．Addパスでの検討。Addは所詮Addなので、Diffuse*directLighting*directContributionの結果で終わりな気もする。

        デフォルトの影の色といいかんじに陰色を混ぜる場合： lerp(ShadeMap, indirectLighting, どれだけ空間が作った影色を適用したいかバリュー) → mixedShadeとする
        ベースカラー：Diffuse

        案１：
        directLightingAdjusted = lerp(1, directLIghting, lighterAffectedValue(0-1));
        indirectLightingAdjusted = lerp(1, directLIghting, darkerAffectedValue(0-1));
        ColorWhenLighted = Diffuse;
        ColorWhenShaded = lerp(Diffuse, ShadeValue, CustomShadeAffectiveValue(0-1));
        directCOntribution = directCOntribution(そのまま)

        */

        #ifdef AXCS_DEBUG_CONTRIBUTION
            toonedMap = directContribution;
        #endif
        #ifdef AXCS_DEBUG_LIGHTCOLOR
            toonedMap = finalLightColor;
        #endif
    }

    float3 tmpToonedMapFactor = (Diffuse+(Diffuse*coloredLight_sum));
    #ifdef AXCS_OUTLINE
        // アウトラインであればShadeMixを反映
        toonedMap = lerp(toonedMap, (toonedMap * _OutlineShadeMix + mad(tmpToonedMapFactor,-_OutlineShadeMix,tmpToonedMapFactor)), isOutline);
    #endif

    // 裏面であればHSVShiftを反映
    if(_DoubleSidedBackfaceUseColorShift) {
        toonedMap = lerp(toonedMap, CalculateHSV(toonedMap, _DoubleSidedBackfaceHueShiftFromBase, _DoubleSidedBackfaceSaturationFromBase, _DoubleSidedBackfaceValueFromBase), !isFrontFace);
        Diffuse = lerp(Diffuse, CalculateHSV(Diffuse, _DoubleSidedBackfaceHueShiftFromBase, _DoubleSidedBackfaceSaturationFromBase, _DoubleSidedBackfaceValueFromBase), !isFrontFace);
    }

    float3 ReflectionMap = float3(0,0,0);
    float3 specular = float3(0,0,0);
    float3 matcap = float3(0,0,0);
    float3 RimLight = float3(0,0,0);
    float3 shadowcap = float3(1000,1000,1000);

    #if !defined(AXCS_REFRACTED) && defined(AXCS_OUTLINE)
    if (!isOutline) {
    #endif
        // オプション：Reflection
        if (_UseReflection) {
            float3 normalDirectionReflection = normalize(mul( float3(normalLocal.rg*_ReflectionNormalMix,normalLocal.b), tangentTransform ));
            float reflNdotV = abs(dot( normalDirectionReflection, viewDirection ));
            float _ReflectionSmoothnessMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_ReflectionReflectionMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _ReflectionReflectionMask));
            float reflectionSmoothness = _ReflectionReflectionPower*_ReflectionSmoothnessMask_var;
            float perceptualRoughnessRefl = 1.0 - reflectionSmoothness;
            float3 reflDir = reflect(-viewDirection, normalDirectionReflection);
            float roughnessRefl = SmoothnessToRoughness(reflectionSmoothness);
            float3 indirectSpecular = float3(0,0,0);
            if (_UseReflectionProbe) {
                indirectSpecular = GetIndirectSpecular(lightColor, lightDirection,
                    normalDirectionReflection, viewDirection, reflDir, attenuation, roughnessRefl, i.posWorld.xyz
                );
                if (any(indirectSpecular.xyz) == 0) indirectSpecular = GetIndirectSpecularCubemap(_ReflectionCubemap, _ReflectionCubemap_HDR, reflDir, roughnessRefl);
            } else {
                indirectSpecular = GetIndirectSpecularCubemap(_ReflectionCubemap, _ReflectionCubemap_HDR, reflDir, roughnessRefl);
            }
            float3 specularColorRefl = reflectionSmoothness;
            float specularMonochromeRefl;
            float3 diffuseColorRefl = Diffuse;
            diffuseColorRefl = DiffuseAndSpecularFromMetallic( diffuseColorRefl, specularColorRefl, specularColorRefl, specularMonochromeRefl );
            specularMonochromeRefl = 1.0-specularMonochromeRefl;
            half grazingTermRefl = saturate( reflectionSmoothness + specularMonochromeRefl );
            #ifdef UNITY_COLORSPACE_GAMMA
                half surfaceReduction = 1.0-0.28*roughnessRefl*perceptualRoughnessRefl;
            #else
                half surfaceReduction = rcp(roughnessRefl*roughnessRefl + 1.0);
            #endif
            indirectSpecular *= FresnelLerp (specularColorRefl, grazingTermRefl, reflNdotV);
            indirectSpecular *= surfaceReduction *lerp(float3(1,1,1), finalLight,_ReflectionShadeMix);
            float reflSuppress = _ReflectionSuppressBaseColorValue * reflectionSmoothness;
            toonedMap = lerp(toonedMap,mad(toonedMap,-surfaceReduction,toonedMap), reflSuppress);
            ReflectionMap = indirectSpecular*lerp(float3(1,1,1), finalLight,_ReflectionShadeMix);
        }

        // オプション：Gloss
        if(_UseGloss) {
            float glossNdotV = abs(dot( normalDirection, viewDirection ));
            float _GlossBlendMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_GlossBlendMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _GlossBlendMask));
            float gloss = _GlossBlend * _GlossBlendMask_var;
            float perceptualRoughness = 1.0 - gloss;
            float roughness = perceptualRoughness * perceptualRoughness;
            float specPow = exp2( gloss * 10.0+1.0);
            float NdotL = saturate(dot( normalDirection, lightDirection ));
            float LdotH = saturate(dot(lightDirection, halfDirection));
            float3 specularColor = _GlossPower;
            float specularMonochrome;
            float3 diffuseColor = Diffuse;
            diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
            specularMonochrome = 1.0-specularMonochrome;
            float NdotH = saturate(dot( normalDirection, halfDirection ));
            float VdotH = saturate(dot( viewDirection, halfDirection ));
            float visTerm = SmithJointGGXVisibilityTerm( NdotL, glossNdotV, roughness );
            float normTerm = GGXTerm(NdotH, roughness);
            float specularPBL = (visTerm*normTerm) * UNITY_PI;
            #ifdef UNITY_COLORSPACE_GAMMA
                specularPBL = sqrt(max(1e-4h, specularPBL));
            #endif
            specularPBL = max(0, specularPBL * NdotL);
            #if defined(_SPECULARHIGHLIGHTS_OFF)
                specularPBL = 0.0;
            #endif
            specularPBL *= any(specularColor) ? 1.0 : 0.0;
            float3 attenColor = attenuation * _LightColor0.xyz;
            float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
            half grazingTerm = saturate( gloss + specularMonochrome );
            specular = attenuation * directSpecular * _GlossColor.rgb;
        }

        // オプション：MatCap
        if (_MatcapBlendMode < 3) {
            float3 normalDirectionMatcap = normalize(mul( float3(normalLocal.r*_MatcapNormalMix,normalLocal.g*_MatcapNormalMix,normalLocal.b), tangentTransform )); // Perturbed normals
            float2 transformMatcap = ComputeTransformCap(cameraSpaceViewDir,normalDirectionMatcap);
            float4 _MatcapTexture_var = UNITY_SAMPLE_TEX2D_SAMPLER(_MatcapTexture, REF_MAINTEX, TRANSFORM_TEX(transformMatcap, _MatcapTexture));
            float4 _MatcapBlendMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_MatcapBlendMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _MatcapBlendMask));
            matcap = ((_MatcapColor.rgb*_MatcapTexture_var.rgb)*_MatcapBlendMask_var.rgb*_MatcapBlend) * lerp(float3(1,1,1), finalLight,_MatcapShadeMix);
        }

        // オプション：Rim
        if (_UseRim) {
            float _RimBlendMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_RimBlendMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _RimBlendMask));
            float4 _RimTexture_var = UNITY_SAMPLE_TEX2D_SAMPLER(_RimTexture, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _RimTexture));

            float rimNdotV = abs(dot( normalDirection, viewDirection ));
            float oneMinusRimNdotV = 1 - rimNdotV; // 0:正面 ~ 1:真横
            float value = (oneMinusRimNdotV - _RimBlendStart) / (_RimBlendEnd - _RimBlendStart);
            float rimPow3 = value*value*value;
            float rimPow5 = Pow5(value);
            float valueTotal = min(1, lerp(value, lerp(rimPow3, rimPow5, max(0, _RimPow-1)), min(1,_RimPow)));

            RimLight = (
                    lerp( _RimTexture_var.rgb, Diffuse, _RimUseBaseTexture )
                    * valueTotal
                    * _RimBlend
                    * _RimColor.rgb
                    * _RimBlendMask_var
                    * lerp(float3(1,1,1), finalLight,_RimShadeMix)
                );
        }

        // オプション:ShadeCap
        if (_ShadowCapBlendMode < 2) {
            float3 normalDirectionShadowCap = normalize(mul( float3(normalLocal.r*_ShadowCapNormalMix,normalLocal.g*_ShadowCapNormalMix,normalLocal.b), tangentTransform )); // Perturbed normals
            float2 transformShadowCap = ComputeTransformCap(cameraSpaceViewDir, normalDirectionShadowCap);
            float4 _ShadowCapTexture_var =  UNITY_SAMPLE_TEX2D_SAMPLER(_ShadowCapTexture, REF_MAINTEX, TRANSFORM_TEX(transformShadowCap, _ShadowCapTexture));
            float4 _ShadowCapBlendMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_ShadowCapBlendMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _ShadowCapBlendMask));
            shadowcap = (1.0 - mad(_ShadowCapBlendMask_var.rgb,-(_ShadowCapTexture_var.rgb),_ShadowCapBlendMask_var.rgb)*_ShadowCapBlend);
        }
    #if !defined(AXCS_REFRACTED) && defined(AXCS_OUTLINE)
    }
    #endif

    float3 finalcolor2 = toonedMap+ReflectionMap + specular;

    // ShadeCapのブレンドモード
    if (_ShadowCapBlendMode == 0) { // Darken
        finalcolor2 = min(finalcolor2, shadowcap);
    } else if  (_ShadowCapBlendMode == 1) { // Multiply
        finalcolor2 = finalcolor2 * shadowcap;
    }

    // MatCapのブレンドモード
    if (_MatcapBlendMode == 0) { // Add
        finalcolor2 = finalcolor2 + matcap;
    } else if (_MatcapBlendMode == 1) { // Lighten
        finalcolor2 = max(finalcolor2, matcap);
    } else if (_MatcapBlendMode == 2) { // Screen
        finalcolor2 = 1-(1-finalcolor2) * (1-matcap);
    }

    // 屈折
    #ifdef AXCS_REFRACTED
        float refractionValue = pow(1.0-saturate(dot(normalDirection, viewDirection)),_RefractionFresnelExp);
        float2 sceneUVs = (i.grabUV) + ((refractionValue*_RefractionStrength) * mul(unity_WorldToCamera, float4(normalDirection,0) ).xyz.rgb.rg);
        float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
    #endif


    // Emission Parallax
    float3 emissionParallax = float3(0,0,0);
    if(_UseEmissionParallax) {
        float _EmissionParallaxDepthMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionParallaxDepthMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _EmissionParallaxDepthMask)).r;
        float2 emissionParallaxTransform = _EmissionParallaxDepth * (_EmissionParallaxDepthMask_var - _EmissionParallaxDepthMaskInvert) * mul(tangentTransform, viewDirection).xy + i.uv0;
        float _EmissionMask_var =  UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionParallaxMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _EmissionParallaxMask)).r;
        float3 _EmissionParallaxTex_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionParallaxTex, REF_MAINTEX, TRANSFORM_TEX(emissionParallaxTransform, _EmissionParallaxTex)).rgb * _EmissionParallaxColor.rgb;
        emissionParallax = _EmissionParallaxTex_var * _EmissionMask_var;
    }

    #ifdef AXCS_EMISSIVE_FREAK
        float time = _Time.r;

        float2 emissiveFreak1uv = i.uv0 + float2(fmod(_EmissiveFreak1U * time, 1.0 / _EmissiveFreak1Tex_ST.x), fmod(time * _EmissiveFreak1V, 1.0 / _EmissiveFreak1Tex_ST.y));
        float _EmissiveFreak1DepthMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissiveFreak1DepthMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _EmissiveFreak1DepthMask)).r;
        float2 emissiveFreak1Transform = _EmissiveFreak1Depth * (_EmissiveFreak1DepthMask_var - _EmissiveFreak1DepthMaskInvert) * mul(tangentTransform, viewDirection).xy + emissiveFreak1uv;
        float _EmissiveFreak1Mask_var =  UNITY_SAMPLE_TEX2D_SAMPLER(_EmissiveFreak1Mask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _EmissiveFreak1Mask)).r;
        float3 _EmissiveFreak1Tex_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissiveFreak1Tex, REF_MAINTEX, TRANSFORM_TEX(emissiveFreak1Transform, _EmissiveFreak1Tex)).rgb * _EmissiveFreak1Color.rgb;
        float emissiveFreak1Breathing = cos(_EmissiveFreak1Breathing*time) * 0.5 + 0.5;//sin(x+π/2)=cos(x)
        float emissiveFreak1BlinkOut = 1 - ((_EmissiveFreak1BlinkOut*time) % 1.0);
        float emissiveFreak1BlinkIn = (_EmissiveFreak1BlinkIn*time) % 1.0;
        float emissiveFreak1Hue = (_EmissiveFreak1HueShift*time) % 1.0;
        _EmissiveFreak1Tex_var = CalculateHSV(_EmissiveFreak1Tex_var, emissiveFreak1Hue, 1.0, 1.0);
        float3 emissiveFreak1 = _EmissiveFreak1Tex_var * _EmissiveFreak1Mask_var;
        emissiveFreak1 = lerp(emissiveFreak1, emissiveFreak1 * emissiveFreak1Breathing, _EmissiveFreak1BreathingMix);
        emissiveFreak1 = lerp(emissiveFreak1, emissiveFreak1 * emissiveFreak1BlinkOut, _EmissiveFreak1BlinkOutMix);
        emissiveFreak1 = lerp(emissiveFreak1, emissiveFreak1 * emissiveFreak1BlinkIn, _EmissiveFreak1BlinkInMix);

        float2 emissiveFreak2uv = i.uv0 + float2(fmod(_EmissiveFreak2U * time, 1.0 / _EmissiveFreak2Tex_ST.x), fmod(time * _EmissiveFreak2V, 1.0 / _EmissiveFreak2Tex_ST.y));
        float _EmissiveFreak2DepthMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissiveFreak2DepthMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _EmissiveFreak2DepthMask)).r;
        float2 emissiveFreak2Transform = _EmissiveFreak2Depth * (_EmissiveFreak2DepthMask_var - _EmissiveFreak2DepthMaskInvert) * mul(tangentTransform, viewDirection).xy + emissiveFreak2uv;
        float _EmissiveFreak2Mask_var =  UNITY_SAMPLE_TEX2D_SAMPLER(_EmissiveFreak2Mask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _EmissiveFreak2Mask)).r;
        float3 _EmissiveFreak2Tex_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissiveFreak2Tex, REF_MAINTEX, TRANSFORM_TEX(emissiveFreak2Transform, _EmissiveFreak2Tex)).rgb * _EmissiveFreak2Color.rgb;
        float emissiveFreak2Breathing = cos(_EmissiveFreak2Breathing*time) * 0.5 + 0.5;//sin(x+π/2)=cos(x)
        float emissiveFreak2BlinkOut = 1 - ((_EmissiveFreak2BlinkOut*time) % 1.0);
        float emissiveFreak2BlinkIn = (_EmissiveFreak2BlinkIn*time) % 1.0;
        float emissiveFreak2Hue = (_EmissiveFreak2HueShift*time) % 1.0;
        _EmissiveFreak2Tex_var = CalculateHSV(_EmissiveFreak2Tex_var, emissiveFreak2Hue, 1.0, 1.0);
        float3 emissiveFreak2 = _EmissiveFreak2Tex_var * _EmissiveFreak2Mask_var;
        emissiveFreak2 = lerp(emissiveFreak2, emissiveFreak2 * emissiveFreak2Breathing, _EmissiveFreak2BreathingMix);
        emissiveFreak2 = lerp(emissiveFreak2, emissiveFreak2 * emissiveFreak2BlinkOut, _EmissiveFreak2BlinkOutMix);
        emissiveFreak2 = lerp(emissiveFreak2, emissiveFreak2 * emissiveFreak2BlinkIn, _EmissiveFreak2BlinkInMix);

    #else
        float3 emissiveFreak1 = float3(0,0,0);
        float3 emissiveFreak2 = float3(0,0,0);
    #endif

    // Emissive合成・FinalColor計算
    float3 _Emission = tex2D(REF_EMISSIONMAP,TRANSFORM_TEX(i.uv0, REF_EMISSIONMAP)).rgb *REF_EMISSIONCOLOR.rgb;
    _Emission = _Emission + emissionParallax + emissiveFreak1 + emissiveFreak2;
    float3 emissive = max( lerp(_Emission.rgb, _Emission.rgb * i.color, _VertexColorBlendEmissive) , RimLight) * !isOutline;
    float3 finalColor = emissive + finalcolor2;

    #ifdef AXCS_FADE
        fixed _AlphaMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, REF_MAINTEX, TRANSFORM_TEX(i.uv0, _AlphaMask)).r;
        #ifdef AXCS_REFRACTED
            fixed4 finalRGBA = fixed4(lerp(sceneColor, finalColor, (_MainTex_var.a*REF_COLOR.a*_AlphaMask_var)),1);
        #else
            fixed4 finalRGBA = fixed4(finalColor,(_MainTex_var.a*REF_COLOR.a*_AlphaMask_var));
        #endif
        if (_UseProximityOverride) {
            float overrideDistance = _ProximityOverrideBegin - _ProximityOverrideEnd;
            float overrideFactor = 1.0 - clamp( (distance( i.posWorld , _WorldSpaceCameraPos ) - _ProximityOverrideEnd) / overrideDistance , 0.0 , 1.0 ).x;
            finalRGBA = lerp(finalRGBA, fixed4(lerp(_ProximityOverrideColor.rgb, finalRGBA.rgb, _ProximityOverrideAlphaOnly), _ProximityOverrideColor.a), overrideFactor);
        }
        UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
        return finalRGBA;
    #else
        if (_UseProximityOverride) {
            float overrideDistance = _ProximityOverrideBegin - _ProximityOverrideEnd;
            float overrideFactor = 1.0 - clamp( (distance( i.posWorld , _WorldSpaceCameraPos ) - _ProximityOverrideEnd) / overrideDistance , 0.0 , 1.0 ).x;
            fixed4 finalRGBA = fixed4(lerp(finalColor, _ProximityOverrideColor.xyz, overrideFactor),1);
            UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
            return finalRGBA;
        } else {
            fixed4 finalRGBA = fixed4(finalColor,1);
            UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
            return finalRGBA;
        }
    #endif
}
