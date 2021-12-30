using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace YoyogiMori
{
    public static class YMToonExporter
    {
        private static string ShaderPath => "Assets/YOYOGI_MORI/YM_Toon";
        private static string[] TargetPaths => new string[] { ShaderPath };
        private static string FilePrefix => "ym_toon_v.";
        private static Regex VersionRegex => new Regex($"{FilePrefix}([0-9].+[0-9].?[a-z])");
        private static string VersionTxtFilePath => $"{ShaderPath}/Version.txt";

        [MenuItem("Yoyogimori/YMToon/Export #&E")]
        public static void Execute()
        {
            var exportPath = EditorUtility.SaveFilePanel("Export Folder", Application.dataPath, FilePrefix, "unitypackage");
            var fileName = Path.GetFileNameWithoutExtension(exportPath);
            var version = VersionRegex.Match(fileName).Value;

            if (!File.Exists(VersionTxtFilePath)) { File.Create(VersionTxtFilePath); }
            using (StreamWriter outputFile = new StreamWriter(VersionTxtFilePath)) { outputFile.WriteLine(version); }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssetDatabase.ExportPackage(TargetPaths, exportPath, ExportPackageOptions.Recurse);
        }
    }
}
