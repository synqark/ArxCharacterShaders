using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace AxCharacterShaders.Generator
{
    public class AxGenerator
    {
        // 対象とするシェーダー名のキーワード
        // フォルダ以下のシェーダー一覧から対象キーワードで絞った後、除外キーワードによって一部を対象から外す
        public string[] includeShaderNames;

        // 除外するシェーダー名のキーワード
        public string[] excludeShaderNames;

        // シェーダー名のプレフィクス
        public string namePrefix;

        // シェーダーカテゴリの挿入位置とプレフィクス
        public KeyValuePair<int, string> variationName;

        // ファイル名のプレフィクス
        public string fileNamePrefix;

        // 置換キーワードと置換内容の配列
        public Dictionary<string, string> replaceCodes;

        public void run(bool isCheckOnly = false)
        {
            // 対象シェーダーファイルを収集
            var shaders = AssetDatabase.FindAssets("t:Shader", new string[]{"Assets/ArxCharacterShaders/Shaders"});

            // includeShaderNamesを含み、excludeShaderNamesを含まないシェーダーに絞る
            var targets = shaders.Where(item => {
                var path = AssetDatabase.GUIDToAssetPath(item);
                var filename = Path.GetFileName(path);
                return includeShaderNames.Any(i => filename.Contains(i)) && !excludeShaderNames.Any(i => filename.Contains(i));
            });

            if (isCheckOnly){
                foreach (var shader in targets) {
                    var path = AssetDatabase.GUIDToAssetPath(shader);
                    var filename = Path.GetFileName(path);
                    Debug.Log(filename);
                }
                return;
            }

            // 改変を開始
            foreach (var shader in targets) {
                var path = AssetDatabase.GUIDToAssetPath(shader);
                var filename = Path.GetFileName(path);

                // 変換後のファイルパスを作成
                var destFilename = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + this.fileNamePrefix + filename;

                // シェーダーコードの変換
                var shaderCode = LoadTextFileFromDisk(path);
                var destShaderName = "";
                for (var line = 0; line < shaderCode.Count ; line++) {

                    // シェーダー名の変更
                    if (string.IsNullOrEmpty(destShaderName) && shaderCode[line].StartsWith("Shader")) {
                        // シェーダー名を分解
                        var pathinfile = shaderCode[line].Split('"')[1];
                        var shaderNameArray = new List<string>(pathinfile.Split('/'));

                        // カテゴリが定義されていれば所定の位置に追加
                        if (this.variationName.Key > 0) {
                            shaderNameArray.Insert(variationName.Key, variationName.Value);
                        }

                        // シェーダー名を決定
                        shaderNameArray.Insert(shaderNameArray.Count-1, this.namePrefix + shaderNameArray.Last());
                        shaderNameArray.RemoveAt(shaderNameArray.Count-1);
                        destShaderName = string.Join("/", shaderNameArray);

                        // コードに反映
                        var newShaderNameLine = $"Shader \"{destShaderName}\"{shaderCode[line].Split('"')[2]}";
                        shaderCode.RemoveAt(line);
                        shaderCode.Insert(line, newShaderNameLine);
                    }

                    // キーワードの置換
                    if (shaderCode[line].Contains("AXCS_GENERATOR")) {
                        var keyword = shaderCode[line].Substring(shaderCode[line].IndexOf(":")+1);
                        // 先頭についているスペースとタブ文字数を取得
                        var spacing = 0;
                        foreach(var c in shaderCode[line]) {
                            if (!Char.IsWhiteSpace(c)) break;
                            spacing++;
                        }
                        var whitespace = new string(' ', spacing);

                        if (replaceCodes.ContainsKey(keyword)) {
                            var code = replaceCodes[keyword];
                            var lines = code.Split(new[]{"\r\n","\n","\r"}, StringSplitOptions.None);
                            shaderCode.RemoveAt(line);
                            shaderCode.InsertRange(line, lines.Select(text => whitespace + text.Trim()));
                        }
                    }
                }
                // Debug.Log(string.Join("\r\n", shaderCode));

                Debug.Log("ファイルパス：" + destFilename + Environment.NewLine + "シェーダー名：" + destShaderName);

                // 変換結果を出力

                SaveTextfileToDisk(shaderCode, destFilename);
                // AssetDatabase.ImportAsset(destFilename, ImportAssetOptions.ForceUpdate);
                // シェーダーをReimport
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("AXCS/GenerateTest_3")]
        public static void generateOutlineVariation()
        {
            var outlineGenerator = new AxGenerator(){
                includeShaderNames = new string[]{"Opaque", "Cutout", "Stencil", "EmissiveFreak"},
                excludeShaderNames = new string[]{"Fade", "Outline"},
                namePrefix = "",
                fileNamePrefix = "Outline_",
            };
            outlineGenerator.replaceCodes = new Dictionary<string, string>();
            outlineGenerator.replaceCodes.Add(
                "OUTLINE_USE_GEOM",
                "#pragma geometry geom"
            );
            outlineGenerator.replaceCodes.Add(
                "OUTLINE_SHADER_MODEL",
                "#pragma target 4.0"
            );

            outlineGenerator.replaceCodes.Add(
                "OUTLINE_PROPERTIES",
                $@"// Outline
                _OutlineWidth (""[Outline] Width"", Range(0, 20)) = 0.1
                _OutlineMask (""[Outline] Outline Mask"", 2D) = ""white"" {{}}
                _OutlineCutoffRange (""[Outline] Cutoff Range"", Range(0, 1)) = 0.5
                _OutlineColor (""[Outline] Color"", Color) = (0,0,0,1)
                _OutlineTexture (""[Outline] Texture"", 2D) = ""white"" {{}}
                _OutlineShadeMix (""[Outline] Shade Mix"", Range(0, 1)) = 0
                _OutlineTextureColorRate (""[Outline] Texture Color Rate"", Range(0, 1)) = 0.05
                _OutlineWidthMask (""[Outline] Outline Width Mask"", 2D) = ""white"" {{}}
                [AXCSToggle]_OutlineUseColorShift(""[Outline] Use Outline Color Shift"", Int) = 0
                [PowerSlider(2.0)]_OutlineHueShiftFromBase(""[Outline] Hue Shift From Base"", Range(-0.5, 0.5)) = 0
                _OutlineSaturationFromBase(""[Outline] Saturation From Base"", Range(0, 2)) = 1
                _OutlineValueFromBase(""[Outline] Value From Base"", Range(0, 2)) = 1"
            );

            outlineGenerator.replaceCodes.Add(
                "OUTLINE_PROPERTIES_OPAQUE",
                $@"// Outline
                _OutlineWidth (""[Outline] Width"", Range(0, 20)) = 0.1
                _OutlineColor (""[Outline] Color"", Color) = (0,0,0,1)
                _OutlineTexture (""[Outline] Texture"", 2D) = ""white"" {{}}
                _OutlineShadeMix (""[Outline] Shade Mix"", Range(0, 1)) = 0
                _OutlineTextureColorRate (""[Outline] Texture Color Rate"", Range(0, 1)) = 0.05
                _OutlineWidthMask (""[Outline] Outline Width Mask"", 2D) = ""white"" {{}}
                [AXCSToggle]_OutlineUseColorShift(""[Outline] Use Outline Color Shift"", Int) = 0
                [PowerSlider(2.0)]_OutlineHueShiftFromBase(""[Outline] Hue Shift From Base"", Range(-0.5, 0.5)) = 0
                _OutlineSaturationFromBase(""[Outline] Saturation From Base"", Range(0, 2)) = 1
                _OutlineValueFromBase(""[Outline] Value From Base"", Range(0, 2)) = 1"
            );

            outlineGenerator.replaceCodes.Add(
                "OUTLINE_DEFINE",
                $@"#define AXCS_OUTLINE"
            );



            outlineGenerator.variationName = new KeyValuePair<int, string>(1, "_Outline");

            outlineGenerator.run();
        }

        [MenuItem("AXCS/GenerateTest")]
        public static void runTest()
        {
            // 対象シェーダーファイルを収集
            var shaders = AssetDatabase.FindAssets("t:Shader", new string[]{"Assets/ArxCharacterShaders/Shaders"});
            foreach (var shader in shaders){
                var path = AssetDatabase.GUIDToAssetPath(shader);
                var filename = Path.GetFileName(path);

                // Debug.Log(path);
                if (path.Contains("Assets/ArxCharacterShaders/Shaders/Cutout.shader")) {
                    // シェーダーコードの変換
                    var result = LoadTextFileFromDisk(path);
                    for (var line = 0; line < result.Count ; line++) {
                        // シェーダー名の変更

                        // キーワードの置換
                        if (result[line].Contains("AXCS_GENERATOR")) {
                            var keyword = result[line].Substring(result[line].IndexOf(":")+1);
                            // 先頭についているスペースとタブ文字数を取得
                            var spacing = 0;
                            foreach(var c in result[line]) {
                                if (!Char.IsWhiteSpace(c)) break;
                                spacing++;
                            }
                            var whitespace = new string(' ', spacing);

                            if(keyword == "OUTLINE_USE_GEOM") {
                                result.RemoveAt(line);
                                result.InsertRange(line, new string[]{whitespace + "a", whitespace + "b"});
                            }
                        }
                    }
                    // 変換結果を出力
                    var exportPath = "Assets/fugafuga.shader";
                    SaveTextfileToDisk(result, exportPath);
                    AssetDatabase.ImportAsset(exportPath, ImportAssetOptions.ForceUpdate);
                    // シェーダーをReimport
                }
            }

            // 対象シェーダーごとに変換処理
                // シェーダーファイルの行配列を回す
                // 定型文が埋め込まれている行を見つけた場合に、定型文の無いように対応する定義済みのコードを差し替え（挿入）
                // 変換が完了したシェーダーファイルを 〇〇_もともとのシェーダー名.shader で再出力する。 同名のシェーダーがある場合は上書きする。

            // シェーダーディレクトリを再インポートする。
        }

		public static List<string> LoadTextFileFromDisk( string pathName )
		{
			var result = new List<string>();
            if ( !string.IsNullOrEmpty( pathName ) && File.Exists( pathName ) )
            {
                StreamReader fileReader = null;
                try
                {
                    fileReader = new StreamReader( pathName );
                    while (!fileReader.EndOfStream){
                        result.Add(fileReader.ReadLine());
                    }
                }
                catch ( Exception e )
                {
                    Debug.LogException( e );
                }
                finally
                {
                    if( fileReader != null)
                        fileReader.Close();
                }
            }
			return result;
		}

		public static void SaveTextfileToDisk( List<string> shaderBody, string pathName)
		{
			// Write to disk
			StreamWriter fileWriter = new StreamWriter( pathName );
			try
			{
                foreach(var line in shaderBody) {
				    fileWriter.WriteLine( line );
                }
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
			finally
			{
				fileWriter.Close();
			}
		}
    }
}