using UnityEngine;
using UnityEditor;
using System;

namespace AxCharacterShaders.AxTips
{
    public class AxTipWindow : EditorWindow
    {
        Vector2 scrPos = Vector2.zero;
        AxcsTip tipShowing;

        public static void ShowTips(Func<AxcsTip> func)
        {
            AxTipWindow window = (AxTipWindow)EditorWindow.GetWindow(typeof(AxTipWindow));
            window.tipShowing = func();
            window.titleContent = new GUIContent("AXCS_Tips");
            window.Show();
        }
        [MenuItem("AXCS/Tips")]
        public static void ShowInitial()
        {
            ShowTips(() => new Initial());
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        void OnGUI()
        {
            scrPos = EditorGUILayout.BeginScrollView(scrPos);

            var texure = AssetDatabase.LoadAssetAtPath<Texture2D>( AssetDatabase.GUIDToAssetPath( "ebc83669f167a5346b3608ef9d1c91e0" ));
            if(this.tipShowing != null)  {
                EditorGUILayout.LabelField(this.tipShowing.Title, EditorStyles.boldLabel);
                EditorGUILayout.TextArea(this.tipShowing.Description, EditorStyles.label);
                // if(this.tipShowing.imageUrl != ""){
                //     // EditorGUILayout.LabelField(new GUIContent(texure), new GUILayoutOption[]{
                //     //     GUILayout.Width(600),
                //     //     GUILayout.Height(100),
                //     // });
                // }
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                foreach (var item in this.tipShowing.properties)
                {
                    for(var i = 0; i< item.indentOffset; i++) EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(item.name, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    var style = EditorStyles.label;
                    style.wordWrap = true;
                    EditorGUILayout.TextArea(item.content, style);
                    EditorGUI.indentLevel--;
                    for(var i = 0; i< item.indentOffset; i++) EditorGUI.indentLevel--;
                }
            }



            EditorGUILayout.EndScrollView();
        }
    }

    public abstract class AxcsTip
    {
        public string Title;
        public string Description;
        public Property[] properties = new Property[]{};
        public string imageUrl;
        public struct Property
        {
            public string name;
            public string content;
            public int indentOffset;

            public Property(string name, string content, int indentOffset = 0)
            {
                this.name = name;
                this.content = content;
                this.indentOffset = indentOffset;
            }
        }
    }

    class Initial : AxcsTip
    {
        public Initial()
        {
            Title = "Tips";
            Description = "カテゴリタイトルの右端をクリックすると説明が表示されます。";
        }
    }
    class AxcsGenerator : AxcsTip
    {
        public AxcsGenerator()
        {
            Title = "Generator";
            Description = "「Opaque, Fade, Cutout, FadeRefracted」の4種類のシェーダーから、「StencilReader, StencilWriter, EmissiveFreak, Outline」のバリエーションを作ることができます。 全てのシェーダーを再生成する場合は、メニューの番号順に実行してください。 自動生成されるシェーダーのプロパティに手を加えたい場合は、アセット内の [ArxCharacterShaders\\Editor\\Generator\\AxGenerator.cs] 内に定義されている書き換えコードを修正してください。";
            properties = new Property[] {
                new Property("1.GenerateStencilWriter", "CutoutからStencilWriter_Cutoutを作成します。"),
                new Property("2.GenerateStencilReader", "Cutout, Fade, FadeRefractedからStencilReader_〇〇を作成します。"),
                new Property("※StencilReader_DoubleFadeについて", "DoubleFadeは自動生成の対象ではないため、手動で整える必要があります。 StencilReader_Fadeを参考に修正してください。", 1),
                new Property("3.GenerateEmissiveFreak", "EmissiveFreak, Outline以外のシェーダーからEmissiveFreak_〇〇を作成します。"),
                new Property("4.GenerateOutline", "Fade, Outline以外のシェーダーからOutline_〇〇を作成します。"),
            };
        }
    }
    class Common : AxcsTip
    {
        public Common()
        {
            Title = "Common";
            Description = "マテリアルの基本となる色の設定ができます。";
            properties = new Property[]{
                new Property("Main Texture", "マテリアルの基本色をテクスチャ*カラーで指定。 ここで設定された色は影や周囲の色の影響を受ける。"),
                new Property("Normal Map", "マテリアルの凹凸を表現するテクスチャを指定。 また指定されたテクスチャの強度をスライダーで設定。"),
                new Property("Emission", "マテリアルが発光している色をテクスチャ*カラーで設定。 ここで指定された色は影や周囲の色の影響を受けない。"),
                new Property("Cull", "どの面を隠すかを設定。「None」で両面表示。"),
                new Property("Flip backface normal", "裏面の法線を反転するかどうか。 両面に設定したいメッシュが鉄などといった光を通さないものの場合に指定。", 1),
                new Property("Backface Light intensity", "裏面の受光量を乗算で指定。 スカートの裏側など、必然的に暗い部分に関して倍率を下げる。", 1),
                new Property("Use backface color shift", "裏面の色をベースカラーから変更したいときに指定。", 1),
                new Property("ZWrite", "（arktoon/Fade を指定時に表示） 透明に関する設定。意図しない限り基本的にはONにしておく。"),
            };
        }
    }
    class ProximityColorOverride : AxcsTip
    {
        public ProximityColorOverride()
        {
            Title = "Proximity Color Override";
            Description = "カメラからの距離に応じて、指定された色または透明度に変更していくカテゴリです。";
            properties = new Property[]{
                new Property("Color", "色または透明度を指定"),
                new Property("Begin(far)", "変色を開始するカメラからの距離を設定"),
                new Property("End(near)", "変色を終了する（完全に指定された色になる）カメラからの距離を設定"),
            };
        }
    }
    class NonRegisteredProperties : AxcsTip
    {
        public NonRegisteredProperties()
        {
            Title = "Non Registered Properties";
            Description = "シェーダーのプロパティを独自で追加した場合など、一旦このカテゴリに出てきます。";
        }
    }
    class Advanced : AxcsTip
    {
        public Advanced()
        {
            Title = "Advanced / Experimental";
            Description = "使われる頻度が低いか、実験的な機能をこっそり置いていたりしましたが、最近は使われてません。";
        }
    }

    class ParallaxedEmission : AxcsTip
    {
        public ParallaxedEmission()
        {
            Title = "Parallaxed Emission";
            Description = "視差のあるEmissionテクスチャを貼り付けることができます。";
            properties = new Property[]{
                new Property("Texture & Color", "表示させたいテクスチャと色"),
                new Property("TexCol Mask", "Parallaxed Emissionを適用する場所を、白黒のテクスチャで指定"),
                new Property("Parallax Depth & Mask", "視差の深度を設定。 正の値だと手前にくるようになり、負の値だと奥に行ったような表現。 Parallax Depth Maskを指定することで、スライダーで指定したDepthの値に乗算してマスキング可。"),
                new Property("Invert Depth Mask", "深度を逆に設定"),
            };
        }
    }

    class EmissiveFreak : AxcsTip
    {
        public EmissiveFreak()
        {
            Title = "Emissive Freak";
            Description = "アニメーション付きの発光テクスチャを2枚まで使用できる、特殊バリエーションです。";
            properties = new Property[]{
                new Property("Texture & Color", "発行させたいテクスチャと色"),
                new Property("TexCol Mask", "テクスチャと色にマスクをしたい場合に指定するテクスチャ。 マスクテクスチャはアニメーション対象外"),
                new Property("Scroll U", "U方向のテクスチャスクロール速度"),
                new Property("Scroll V", "V方向のテクスチャスクロール速度"),
                new Property("Depth & Mask", "視差効果を使う場合、その深度とマスク"),
                new Property("Invert Depth Mask", "深度を逆に設定"),
                new Property("Breathing", "じわっと浮かび上がり、じわっと消えるアニメーション設定"),
                new Property("Blink Out", "突然浮かび、じわっと消えるアニメーション設定"),
                new Property("Blink In", "じわっと浮かび、突然消えるアニメーション設定"),
                new Property("Hue Shift", "色相変更アニメーションの設定"),
            };
        }
    }
    class Shading : AxcsTip
    {
        public Shading()
        {
            Title = "Shading / Shadow";
            Description = "マテリアルの陰に関する調整ができます。 陰の強さだけではなく、陰の諧調、また陰にかかったテクスチャの色などを指定できます。";
            properties = new Property[]{
                new Property("Border & Blur", "メッシュの光と陰の境界に関する調整項目"),
                new Property("Border", "陰の境目となる地点を指定。", 1),
                new Property("Blur & Mask", "影の境目をどの程度グラデーションさせるかを指定。またマスクテクスチャで部位ごとに陰のグラデーション度合いを指定可", 1),
                new Property("Use Step", "陰のグラデーションをポスタライズ（n諧調化）する場合にチェック", 1),
                new Property("Step", "諧調の数", 2),
                new Property("Default color shading", "Sceneの情報を利用した陰色の設定"),
                new Property("Strength & mask", "陰色の強さを指定。 Default color shadingを利用したくない場合は0を指定", 1),
                new Property("Custom color shading", "独自の色を用いた陰色の設定"),
                new Property("Use Texture", "陰用のテクスチャがある場合はチェック", 1),
                new Property("Texture", "陰用のテクスチャ", 2),
                new Property("RGB Multiply", "陰用のテクスチャに乗算する色", 2),
                new Property("Hue Shift / Saturation / Value", "陰用のテクスチャを使用しない場合、CommonのMain Textureで指定した色にHSV効果を適用したものを陰色として使用。 Cutsom color shadingを使用したくない場合は 0 / 1 / 1 を指定", 1),
                new Property("Ambient Light", "空間光（Environment Light / Light Probe等の情報）をどの程度利用するか"),
                new Property("Intensity", "1で空間光をそのまま使用～0で利用しない（真っ黒）", 1)
            };
        }
    }
    class Outline : AxcsTip
    {
        public Outline()
        {
            Title = "Outline";
            Description = "アウトラインバリエーションを使っている場合、アウトラインの太さ・色情報をこのカテゴリから指定できます。";
            properties = new Property[]{
                new Property("Width & Mask", "アウトラインの幅を指定。 またマスクテクスチャで部位ごとの太さを指定。"),
                new Property("Cutoff Mask / Range", "アウトラインを描画しない部分をカットオフマスクとして指定。 Rangeで境目の微調整。"),
                new Property("Texture & Color", "アウトラインの色を指定。"),
                new Property("Base Color Mix", "アウトラインの色にCommon/Main Textureの色をどの程度混ぜるかを指定", 1),
                new Property("Use Color Shift", "Texture&Color, Base Color Mixの結果の色にHSV変換を適用したい場合にチェック", 1),
                new Property("Shadow mix", "Shading / Shadowカテゴリで指定した「陰」を、どの程度アウトラインに適用するかを指定"),
            };
        }
    }
    class Gloss : AxcsTip
    {
        public Gloss()
        {
            Title = "Gloss";
            Description = "リアルタイムライトに対する「つや」を表現できます。";
            properties = new Property[]{
                new Property("Smoothness & Mask", "艶の強さを設定。高いほど光が集まり、強く反射。 Maskを指定することでメッシュの一部のみ設定が可能です。"),
                new Property("Metallic", "メタリック感を設定。 高いほど、メッシュの色が光に混ざる"),
                new Property("Color", "反射する光の色を乗算"),
            };
        }
    }
    class MatCap : AxcsTip
    {
        public MatCap()
        {
            Title = "MatCap";
            Description = "メッシュに対して、予め「ライティングされた光源」（＝MaterialCapture, MatCap）テクスチャを設定できます。";
            properties = new Property[]{
                new Property("Blend Mode", "MatCapの色をどのように合成するかを設定。"),
                new Property("Unused", "MatCapを使用しない", 1),
                new Property("Add", "加算。 既存の色にMatCapの色を加える。", 1),
                new Property("Lighten", "比較(明)。既存の色とMatCapの色を比較し、明るい方を表示", 1),
                new Property("Screen", "スクリーン合成。 既存の色とMatCapの色でスクリーン合成", 1),
                new Property("Blend & Mask","Blend Modeで設定した方法でどの程度ブレンドするかを設定します。 マスクテクスチャを使うことで、メッシュの部分ごとのブレンドを制御できます （黒色：0　～　白色：Blendの値）"),
                new Property("Texture & Color", "MatCapテクスチャを指定"),
                new Property("Normal Map mix","MatCapを着色する際、NormalMapをどの程度考慮するかをスライダーで指定"),
                new Property("Shadow mix","Shading / Shadowカテゴリで指定した「陰」の部分に効果を反映させたくない場合にスライダーで指定"),
            };
        }
    }
    class Reflection : AxcsTip
    {
        public Reflection()
        {
            Title = "Reflection";
            Description = "メッシュに対して鏡面反射を設定します。";
            properties = new Property[]{
                new Property("Use Reflection Probe", "Sceneのリフレクションを使用する場合に設定。 シーンのリフレクションが無い場所ではCubemapが使用される。"),
                new Property("Smoothness & Mask", "艶の強さを設定。高いほど光が集まり、強く反射。 Maskを指定することでメッシュの一部のみ設定が可能です。"),
                new Property("Cubemap（Fallback）", "反射するCubemapテクスチャを設定します。 Use Reflection Probeが有効の場合は、そちらが優先。"),
                new Property("Normal Map mix", "リフレクションに対してNormalMapをどの程度考慮するかをスライダーで設定"),
                new Property("Shadow mix","Shading / Shadowカテゴリで指定した「陰」の部分に効果を反映させたくない場合にスライダーで指定"),
            };
        }
    }
    class Rim : AxcsTip
    {
        public Rim()
        {
            Title = "Rim";
            Description = "メッシュのリム（ヘリ、枠）に対する光を指定できます（リムライトと呼ばれます）";
            properties = new Property[]{
                new Property("Blend Start", "リム効果を開始する位置を指定（スライダーが高いほど、側面に寄る）"),
                new Property("Blend End", "リム効果が最大になる位置を指定（スライダーが高いほど、側面に寄る）"),
                new Property("Power type", "BlendStart～BlendEndまでのリム効果に与える指数"),
                new Property("Blend & Mask", "リム効果の強さとマスクテクスチャ。"),
                new Property("Texture & Color", "リム効果の色またはテクスチャ"),
                new Property("Use Base Color", "Common/MainTextureで指定した色を加算させたい場合にチェック", 1),
                new Property("Shadow mix","Shading / Shadowカテゴリで指定した「陰」の部分に効果を反映させたくない場合にスライダーで指定"),
            };
        }
    }
    class ShadeCap : AxcsTip
    {
        public ShadeCap()
        {
            Title = "ShadeCap";
            Description = "メッシュに対して、予め「ライティングされた陰」テクスチャを設定できます。 MatCapの暗い版です。造語です。";
            properties = new Property[]{
                new Property("Blend Mode", "ShadeCapの色をどのように合成するかを設定。"),
                new Property("Unused", "ShadeCapを使用しない", 1),
                new Property("Darken", "比較(暗)。既存の色とShadeCapの色を比較し、暗い方を表示", 1),
                new Property("Multiply", "乗算。 既存の色にShadeCapの色を乗算する。", 1),
                new Property("Light Shutter", "受光マスク。 黒の部分をShading / Shadow カテゴリで指定した「陰」扱いにする。 黒いテクスチャを指定すると、メッシュ全体が陰に入ったようになる。", 1),
                new Property("Blend & Mask","Blend Modeで設定した方法でどの程度ブレンドするかを設定します。 マスクテクスチャを使うことで、メッシュの部分ごとのブレンドを制御できます （黒色：0　～　白色：Blendの値）"),
                new Property("Texture & Color", "ShadeCapテクスチャを指定"),
                new Property("Normal Map mix","ShadeCapを着色する際、NormalMapをどの程度考慮するかをスライダーで指定"),
            };
        }
    }
    class StencilReader : AxcsTip
    {
        public StencilReader()
        {
            Title = "Stencil Reader";
            Description = "ステンシルバッファを読み取り、指定した計算結果に基づいて描画の有無を決めます。 Stencil Writerまたはその他ステンシル操作系のシェーダーの利用が前提です。";
            properties = new Property[]{
                new Property("Stencil Number", "ステンシルの値"),
                new Property("Compare Action", "Numberで指定した値とステンシルバッファを比較する式。 比較結果が「正しい」場合にマテリアルが描画される。 つまり、バッファの部分を抜きたい場合は「NotEqual」を指定する。"),
            };
        }
    }
    class StencilWriter : AxcsTip
    {
        public StencilWriter()
        {
            Title = "Stencil Writer";
            Description = "ステンシルバッファを書き込むシェーダーです。 Stencil Readerや、その他ステンシルを読み取るシェーダーの利用が前提になります。";
            properties = new Property[]{
                new Property("Stencil Number", "ステンシルの値"),
                new Property("Stencil Mask & Range", "メッシュの一部だけステンシルを書き込む場合はマスクテクスチャを指定。 スライダーで微調整"),
                new Property("Alpha(Dither)", "ステンシルバッファの透明度。 実際に透明度の概念はなく、ディザ加工で半透明を表現する。"),
            };
        }
    }
    class Refraction : AxcsTip
    {
        public Refraction()
        {
            Title = "Refraction";
            Description = "屈折を表現します。";
            properties = new Property[]{
                new Property("Fresnel Exp", "屈折へのフレネル効果。 側面のみを屈折させたい場合にスライダーで設定。"),
                new Property("Strength", "屈折する強度を設定。 "),
            };
        }
    }
    class AlphaMask : AxcsTip
    {
        public AlphaMask()
        {
            Title = "AlphaMask";
            Description = "アルファ値にマスクテクスチャを適用できます。 Commom/Main Textureで指定したテクスチャのアルファ情報が正しければ、本カテゴリを使う必要はありません。";
            properties = new Property[]{
                new Property("AlphaMask", "Common/MainTextureで指定した色のアルファ値に乗算するマスクテクスチャを指定。"),
            };
        }
    }
    class AlphaCutout : AxcsTip
    {
        public AlphaCutout()
        {
            Title = "Alpha Cutout";
            Description = "カットアウトの値を微調整します。";
            properties = new Property[]{
                new Property("Cutoff Adjust", "Common/MainTextureで指定した色のアルファ値と比較し、透明部分と判断するための値"),
            };
        }
    }
    class SecondaryCommon : AxcsTip
    {
        public SecondaryCommon()
        {
            Title = "Secondary Common";
            Description = "「2番目」の色を指定します。ステンシルの計算式を変えるなどして、Commonとは別の場所に別の色を指定したいときに使うはず。多分。";
            properties = new Property[]{
                new Property("Main Texture", "マテリアルの基本色をテクスチャ*カラーで指定。 ここで設定された色は影や周囲の色の影響を受ける。"),
                new Property("Normal Map", "マテリアルの凹凸を表現するテクスチャを指定。 また指定されたテクスチャの強度をスライダーで設定。"),
                new Property("Emission", "マテリアルが発光している色をテクスチャ*カラーで設定。 ここで指定された色は影や周囲の色の影響を受けない。"),
            };
        }
    }
    class DetailMap : AxcsTip
    {
        public DetailMap()
        {
            Title = "Detail Map";
            Description = "CommonおよびShading（テクスチャ利用中）で決定された色にオーバーレイで追加のテクスチャを指定します。 基本的にはStandardシェーダーのDetail Mapsを同じ用法を想定しています。";
            properties = new Property[]{
                //Mask
                //Detail map
                //Detail map (When shaded)
                //Normal map
            };
        }
    }
}