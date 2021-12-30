using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace K2Scripts.MultiLanguageSystem
{

    public class MultiLanguageTexts
    {
        // 構造体定義
        public struct OneLanguageString
        {
            public int index;    // message番号。任意で付けれる。番号かタイトルどちらかを必ず指定しなければならない。両方でも良い
            public string title;        // 検索用タイトル。英語限定。任意
            public string country;  // JP EN 等
            public string text;
            public OneLanguageString(int index,string title, string country, string text){
                this.index = index;
                this.title = title;
                this.country = country;
                this.text = text;
            }
        };



        // 変数
        public List<OneLanguageString> multiLanguageStrings;    // パブリック関数のAddTextを使用せず直接こっちにAddしてもよい。何だったらちょくでWhereしても




        // エラー用定数
        static string ERROR_NOTFOUNDTEXT = "ERROR_NOTFOUNDTEXT";
        static string ERROR_NOTFOUNDCOUNTRY = "ERROR_NOTFOUNDCOUNTRY";
        static string ERROR_NOTFOUNDINDEX = "ERROR_NOTFOUNDINDEX";
        static string ERROR_NOTFOUNDTITLE = "ERROR_NOTFOUNDTITLE";


        // コンストラクタ
        public MultiLanguageTexts(string path = null){
            if(path==null) multiLanguageStrings = new List<OneLanguageString>();
            else DatabaseLoad(path);
        }


        // public関数

        //テキストを追加する。message番号またはタイトル（どっちも検索用に任意のものを指定）を指定し、言語名(命名は任意)、翻訳テキストを指定する。
        public void AddText(int index, string country, string text){
            multiLanguageStrings.Add(new OneLanguageString(index,"",country,text));
        }
        public void AddText(string title, string country, string text){
            multiLanguageStrings.Add(new OneLanguageString(-1,title,country,text));
        }
        public void AddText(int index, string title, string country, string text){
            multiLanguageStrings.Add(new OneLanguageString(index,title,country,text));
        }


        // テキストをファイルから読み込ませたい。後ほどやる
        // public void AddTextFromFile()


        public string GetTransText(int index, string country, bool errortype = false){  // messageIndex式
            var matchtexts = multiLanguageStrings.Where( val => val.index == index && val.country == country);
            var res = "";

            //各種チェック
            if(matchtexts.Any()){  // 最低１つあったら
                foreach(var matchtext in matchtexts){   // 同じIndexに同じ言語指定のテキストが複数あった場合、合成する。改行を挟む。
                    res += $"{matchtext.text}\n";
                }
            }
            else { // 存在しなかったら
                if(errortype) {
                    Debug.LogError($"{ERROR_NOTFOUNDTEXT} : index={index} , country={country}");
                    return ERROR_NOTFOUNDTEXT;
                }
                //もう一回、言語指定無しで選び、最初にマッチしたものを選ぶ
                matchtexts = multiLanguageStrings.Where( val => val.index == index );
                if(!matchtexts.Any()){  // 別言語設定でなら存在したならば
                    res = matchtexts.First().text;
                    Debug.LogError($"{ERROR_NOTFOUNDCOUNTRY} : index={index} , country={country}"); // 指定言語が無かったら１番目を変わりに出すがあくまでERROR扱い。出力も簡易
                }
                else {   // 更に存在しなかったら
                    res = ERROR_NOTFOUNDINDEX;
                    Debug.LogError($"{ERROR_NOTFOUNDINDEX}&{ERROR_NOTFOUNDCOUNTRY} : index={index} , country={country}");
                }
            }
            return res;
        }


        public string GetTransText(string title, string country, bool errortype = false){  // title式
            var matchtexts = multiLanguageStrings.Where( val => val.title == title && val.country == country);
            var res = "";

            //各種チェック
            if(matchtexts.Any()){  // 最低１つあったら
                foreach(var matchtext in matchtexts){   // 同じIndexに同じ言語指定のテキストが複数あった場合、合成する。改行を挟む。
                    res += $"{matchtext.text}\n";
                }
                if(res!="") res = res.Remove(res.Length-1);   // resの最後に必ず\nが入るのでそれの削除
            }
            else { // 存在しなかったら
                if(errortype) {
                    Debug.LogError($"{ERROR_NOTFOUNDTEXT} : index={title} , country={country}");
                    return ERROR_NOTFOUNDTEXT;
                }
                //もう一回、言語指定無しで選び、最初にマッチしたものを選ぶ
                matchtexts = multiLanguageStrings.Where( val => val.title == title );
                if(!matchtexts.Any()){  // 別言語設定でなら存在したならば
                    res = matchtexts.First().text;
                    Debug.LogError($"{ERROR_NOTFOUNDCOUNTRY} : title={title} , country={country}"); // 指定言語が無かったら１番目を変わりに出すがあくまでERROR扱い。出力も簡易
                }
                else {   // 更に存在しなかったら
                    res = ERROR_NOTFOUNDINDEX;
                    Debug.LogError($"{ERROR_NOTFOUNDTITLE}&{ERROR_NOTFOUNDCOUNTRY} : title={title} , country={country}");
                }
            }
            return res;
        }


        public bool DatabaseLoad(string path)
        {
            multiLanguageStrings = new List<OneLanguageString>();


            var languagelist = DataBaseFileIO.DataBaseFileIO.ReadAsCVS(path);
            if(languagelist==null){
                //設定ファイルが無かった
                Debug.LogError("ERROR : MLS : 言語ファイルが有りませんでした NotFound Languagefile");
                return false;
            }

            foreach(var words in languagelist)
            {
                if(words.Length<4) continue;    // 要素が不足してたら無視する。
                if(words[0].IndexOf("//")==0) continue; // ←が先頭にあってもComment扱いで無視する
                var oneword = new MultiLanguageTexts.OneLanguageString(words[0]=="" ? -1 : int.Parse(words[0]), words[1], words[2], words[3]);
                multiLanguageStrings.Add(oneword);
            }
            return true;
        }


    }

}