using UnityEngine;
using UnityEditor;

using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace しゃがみ置き換え
{
    // アセットを安全に読み込むためのクラス
    public class AssetLoader
    {
        public string guid = "";
        public string filter = "";
        public string subdir_name = "";

        // 初期化
        public AssetLoader(string guid, string filter = "", string subdir_name = "")
        {
            this.guid = guid;
            this.filter = filter;
            this.subdir_name = subdir_name;
        }

        // GUIDからアセットを返す、見つからなければファイル名で検索して返す
        public T Load<T>()
        {
            string path = AssetDatabase.GUIDToAssetPath(this.guid);
            if (path != "")
            {
                return (T)(object)AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            }
            else if (this.filter != "")
            {
                string dir = GetBaseDir();
                if (this.subdir_name != "")
                {
                    string new_dir = Path.Combine(dir, this.subdir_name).Replace(@"\", "/");
                    if (Directory.Exists(new_dir))
                    {
                        dir = new_dir;
                    }
                }

                string[] results = AssetDatabase.FindAssets(this.filter, new string[] { dir });
                results = results.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();
                results = results.OrderBy(x => Path.GetFileNameWithoutExtension(x).Length).ToArray();
                if (1 <= results.Length)
                {
                    return (T)(object)AssetDatabase.LoadAssetAtPath(results[0], typeof(Object));
                }
                else
                {
                    return default(T);
                }
            }
            return default(T);
        }

        // パスを返す
        public string GetAssetPath()
        {
            return AssetDatabase.GUIDToAssetPath(this.guid);
        }

        // 拡張子抜きのファイル名を返す
        public string GetFileNameWithoutExtension()
        {
            return Path.GetFileNameWithoutExtension(GetAssetPath());
        }

        // このスクリプトのパスを取得
        public string ShowTraceLog([CallerFilePath] string path = "")
        {
            return path;
        }

        // ベースのディレクトリパスを取得
        public string GetBaseDir()
        {
            string[] splited = Regex.Split(ShowTraceLog(), @"[\\/]Assets[\\/]", RegexOptions.IgnoreCase);
            string[] dir_names = Regex.Split(splited[splited.Length - 1], @"[\\/]", RegexOptions.IgnoreCase);
            if (2 <= dir_names.Length)
            {
                return Path.Combine("Assets", dir_names[0]).Replace(@"\", "/");
            }
            return "Assets";
        }
    }
}
