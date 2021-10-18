using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;

namespace AxCharacterShaders
{
    public class AxCommon : ScriptableObject
    {
        static AxCommon instance;

        // ArxCharacterShaderが保管されているディレクトリパスを取得
        public static string GetBaseDir()
        {
            if (instance == null) instance = (AxCommon)ScriptableObject.CreateInstance(typeof(AxCommon));
            var obj = MonoScript.FromScriptableObject(instance);
            var path = AssetDatabase.GetAssetPath(obj);
            path = path.Substring(0,path.IndexOf("Editor/AxCommon.cs"));
            return path;
        }
    }

    [InitializeOnLoad]
    public class AxInitializer
    {
        static AxInitializer()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                RefreshKeywords();
            }
        }

        [MenuItem("AXCS/Refresh AXCS Materials")]
        private static void RefreshAXCSMats()
        {
            RefreshKeywords();
        }

        private static void RefreshKeywords() {
            List<Material> armat = new List<Material>();
            Renderer[] arrend = (Renderer[])Resources.FindObjectsOfTypeAll(typeof(Renderer));
            foreach (Renderer rend in arrend) {
                foreach (Material mat in rend.sharedMaterials) {
                    if (!armat.Contains (mat)) {
                        armat.Add (mat);
                    }
                }
            }
            foreach (Material mat in armat) {
                if (mat != null && mat.shader != null && mat.shader.name != null && mat.shader.name.StartsWith("ArxCharacterShaders/")) {
                    // シェーダーの再割り当てをして、適切なキーワードを生成する
                    mat.shader = mat.shader;
                }
            }
            Debug.Log("Refreshed AXCS Materials");
        }
    }

    public class AxDocs
    {
        private const string baseUrl = "https://synqark.github.io/AXCS_Docs/";
        private const string docPath = "3-documents/";

        public static void Open(string path)
        {
            Application.OpenURL(baseUrl + docPath + path);
        }
    }
}