using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

/*
    cvsな感じで読み書き？

    // path はResources名で（ファイル名から拡張子を除いたもの。フォルダ指定もなしにファイル名のみ）要Resourcesフォルダ。

*/



namespace K2Scripts.DataBaseFileIO
{
    public static class DataBaseFileIO
    {

        public static List<String[]> ReadAsCVS(string path)  // ファイルを読み込めなかったらnullを返す
        {
            var textasset = Resources.Load<TextAsset>(path);
            if(textasset==null) return null;
            var database = textasset.text.Split('\n');


            var res = new List<String[]>();

            for(var i=0; i<database.Length; i++){   // databaseを１行１行処理していく
                var linedata = database[i];

                var words = linedata.Split(',');    // これで「,」で区切られたものが配列に
                for(var l=0; l<words.Length; l++) words[l] = words[l].Trim();
                res.Add(words);
            }

            return res;
        }
/*
        public static void SaveAsCVS(IEnumerable<String[]> database, string path, bool isNewFullPath = false)
        {
            var text = "";
            foreach(var linedata in database)
            {
                var linetext = "";
                foreach(var words in linedata)
                {
                    linetext += words + ",";
                }
                if(linetext!="") linetext = linetext.Remove(linetext.Length-1);    //空じゃなければ、行の最後の","を取り除く

                text += linetext + "\n";
            }
            if(text!="") text = text.Remove(text.Length-1); //からじゃなければ最後の改行を取り除く

            if(isNewFullPath) AssetDatabase.CreateAsset(new TextAsset(text), path); // isNewフラグで、pathをProjectパスとして新規・上書きする
            else AssetDatabase.CreateAsset(new TextAsset(text), AssetDatabase.GetAssetPath(Resources.Load<TextAsset>(path).GetInstanceID())); // 否で、Resourcesとして上書きする

            AssetDatabase.SaveAssets();
        }
*/
    }
}