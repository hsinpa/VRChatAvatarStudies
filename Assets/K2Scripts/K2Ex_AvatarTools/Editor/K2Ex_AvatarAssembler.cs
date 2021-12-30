/*
    AvatarTools VerEx.0.5.1
    AvatarAssembler

    本スクリプトは作者である 黒鳥様 (twitter: @kurotori4423)の同スクリプト(Ver.0.2)を、
    MIT Licenseに基づき、空々こん(twitter: @kuukuukon)が改変・二次配布を行ったものです。
    ライセンスはオリジナルの MIT Licenseを継承しています。

    オリジナル
    https://kurotori.booth.pm/items/1564788
    AvatarTools Ver.0.2 (2020/10/01現在、本家最新版はVer.2.0です)
    制作：黒鳥 様

    改変版
    https://kuukuukon.booth.pm/items/2014610
    AvatarTools VerEx


*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEditor;

using System.Text.RegularExpressions;
using K2Scripts.MultiLanguageSystem;

namespace K2Ex_AvatarTools
{

    public class AvatarAssembler : EditorWindow
    {

        struct ComponentData
        {
            public List<Component> components;
            public GameObject target;
        };

        // バージョン情報

        public static string toolname = "改変版 AvatarTools";
        public static string version = "VerEx.0.5.1";
        public static string url = "https://kuukuukon.booth.pm/items/2014610";
        public static string documenturl = "https://docs.google.com/document/d/1RPhGIADnYbt24jl3vEcSYciKgMLRGG-6Li_8Z0H-tog/edit?usp=sharing";

        // 処理用
        private GameObject BaseObject;
        private Vector2 scrollPosition = new Vector2();

        //private List<Transform> BoneList = new List<Transform>();
        private List<GameObject> CombineObjects = new List<GameObject>();

        // K2Ex追加設定
        private string AddMatchingWordCombineSide = "";   // [設定情報対象]Combine側がアバター側のボーン名に別ワードを加えている時に指定する。
        private const string V = "Assets/";
        private string savingfolder = V;


        //GUI制御用(K2Ex)
        private bool isInit = false;    // 初期化処理したかどうか。してなかったらLoadInit()を実行する
        private bool foldout1 = false;
        private bool foldout2 = false;


        private Vector2 scrollpos = new Vector2(0, 0);
        private Vector2 scrollpos2 = new Vector2(0, 0);
        private bool optionToggleAvatarDup_bool = true;
        private bool optionToggleDupShift_bool = true;
        private bool optionToggleSaveingPath_bool = false;

        // 言語設定
        private MultiLanguageTexts multistring;
        private string Lang = "JP"; // 言語選択(optionpopupLang_indexによって上書きされる。)
        private int optionpopupLang_index = 1; // [設定情報対象]言語設定
        private string[] optionpopupLang_string = new string[] { "English(Machine)", "日本語/日本語のみ" , "日本語/日本語と英語混じり", "中文/简体（机器）", "中文/繁體（機械）", "한국 (기계)"};  // languageIdStringと対応させること
        private const string languagePath = "AT_SMC_Language";
        private string[] languageIdString = new String[]{"EN", "JP", "JP2", "CNK", "CNN", "KR"};



        [MenuItem("AvatarTools/K2Ex_AvatarAssembler")]
        static void Open()
        {

            var assembler = EditorWindow.GetWindow<AvatarAssembler>();

            // ウィンドウ設定
            assembler.minSize = new Vector2(380, 500);
            assembler.maxSize = new Vector2(600, 800);

            assembler.titleContent = new GUIContent(nameof(AvatarAssembler));

            assembler.Setup();
        }

        public void Setup()
        {
            CombineObjects.Add(null);

            // その他初期化処理(Setupが実行されない状態で始まったとき用に分離)
            Init();

        }

        void OnDestroy()
        {
            // ウィンドウを閉じたときの終了処理

            // 設定情報をセーブする
            EditorPrefsSave();
        }

        private void Init()
        {
            // Editor設定情報読み込み
            EditorPrefsLoad();

            // 言語関係読み込み
            MultiLanguageInit();


            isInit = true;  // フラグ立て
        }


        private void EditorPrefsSave()
        {
            EditorPrefs.SetInt("Language", optionpopupLang_index);

            EditorPrefs.SetBool("optionToggleAvatarDup_bool", optionToggleAvatarDup_bool);
            EditorPrefs.SetBool("optionToggleDupShift_bool", optionToggleDupShift_bool);
            EditorPrefs.SetBool("optionToggleSaveingPath_bool", optionToggleSaveingPath_bool);

            if(optionToggleSaveingPath_bool) EditorPrefs.SetString("savingfolder", savingfolder);
        }

        private void EditorPrefsLoad()
        {
            optionpopupLang_index = EditorPrefs.GetInt("Language", optionpopupLang_index);

            optionToggleAvatarDup_bool = EditorPrefs.GetBool("optionToggleAvatarDup_bool", optionToggleAvatarDup_bool);
            optionToggleDupShift_bool = EditorPrefs.GetBool("optionToggleDupShift_bool", optionToggleDupShift_bool);
            optionToggleSaveingPath_bool = EditorPrefs.GetBool("optionToggleSaveingPath_bool", optionToggleSaveingPath_bool);

            if(optionToggleSaveingPath_bool) {
                savingfolder = EditorPrefs.GetString("savingfolder", savingfolder);
                CheckSavefoloderValid(savingfolder);
            }
        }

        private void MultiLanguageInit()
        {
            multistring = new MultiLanguageTexts(languagePath);

            // GUIの別用意メニューテキストに代入
            MultiLanguageGUIRefresh();
        }
        private void MultiLanguageGUIRefresh()
        { // 一部のGUIのテキストは別管轄なので変数に直接代入する
            //optionpopup2_string = new string[] { multistring.GetTransText("overwrite save", Lang), multistring.GetTransText("new save", Lang) };
        }

        private void CheckSavefoloderValid(string inputpath)
        {
            // --- デバック用　入力をテストデータに置き換え ---
            //inputpath = "Assets/存在しないフォルダ";  // テスト文字1
            //inputpath = "f:/外部フォルダ";              // テスト文字2
            //inputpath = "g:/Assets/適当な外部のAssetsとなのつくフォルダ";   // テスト文字3
            //--------------------

            // プロジェクト内であった
            savingfolder = Regex.Match(inputpath.Replace("\\","/"), @"Assets/.*").Value;
            if( savingfolder.Length>0 && savingfolder[savingfolder.Length-1] != '/') savingfolder += "/"; // 末尾にスラッシュがなければ追加
            if (savingfolder.Length == 0) {
                savingfolder = V;
                EditorUtility.DisplayDialog("Error", "プロジェクト外部のフォルダが指定されています。パスを初期化します。", "OK");
            }
            //EditorUtility.DisplayDialog("Debug3",savingfolder,"OK");
            if(!AssetDatabase.IsValidFolder(savingfolder.Remove(savingfolder.Length - 1)))  // 末尾に/が入ってると認識してくれない(´・ω・｀)
            {
                // 存在しないフォルダだった
                savingfolder = V;
                EditorUtility.DisplayDialog("Error", "存在しない出力フォルダパスが指定されています。パスを初期化します。※他のプロジェクトと設定を共有しているためよく発生することがあります。", "OK");
            }
            /*
            else
            {
                // 存在した
                EditorUtility.DisplayDialog("Debug4", "ヨシッ", "OK");
            }
            */
            
        }



        private bool Check()
        {
            var baseObjectAnimator = BaseObject.GetComponent<Animator>();
            if (!baseObjectAnimator)
            {
                EditorUtility.DisplayDialog("ERROR", "ベースオブジェクトはモデルデータではありません。", "OK");
                return false;
            }

            var baseSkinnedMesh = BaseObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (!baseSkinnedMesh)
            {
                EditorUtility.DisplayDialog("ERROR", "ベースオブジェクトにSkinnedMeshRendererが存在しません。", "OK");
                return false;
            }

            var baseRootBone = baseSkinnedMesh.rootBone;
            if (!baseRootBone)
            {
                EditorUtility.DisplayDialog("ERROR", "ベースオブジェクトのRootBoneが存在しません。", "OK");
                return false;
            }

            var checkScalebase = baseRootBone.parent.parent.localScale.x != 1 || baseRootBone.parent.localScale.x != 1;
            if (checkScalebase)
            {
                int option = EditorUtility.DisplayDialogComplex("ATTENTION",
                    BaseObject.name + "の単位Scaleが１ではありません。実行すると失敗する可能性があります。\n\n" +
                    "もしBlenderでFBX出力したものでこの問題が出た場合、Exportする前に全オブジェクト選択しCtrl+Aで適用(全トランスフォームまたは回転と拡縮)し、\n" +
                    "さらにFBXExportする時の設定の「スケールの適用」を「FBX単位スケール」にする等をしてください。\n\n" +
                    $"{baseRootBone.parent.parent.name}:{baseRootBone.parent.parent.localScale.x} , {baseRootBone.parent.name}:{baseRootBone.parent.localScale.y}" +
                    "\n※0.999...倍等の微差なら無視して実行してください",
                    multistring.GetTransText("cOK",Lang),
                    multistring.GetTransText("cCancel",Lang),
                    ""
                    );
                if (option == 1) return false;
            }


            foreach (var combineObject in CombineObjects)
            {
                // アニメーターの有無による衣装のモデルデータ判別を廃止。モデルデータではなかったら下記のチェックでどこかで弾いてくれるはず
                //if(!combineObject.GetComponent<Animator>())
                //{
                //    combineObject.AddComponent<Animator>();
                //    EditorUtility.DisplayDialog("Attention", "衣装には空のAnimatorComponentが必要です\n" + combineObject.name + " に空のAnimatorComponentを\n自動で追加しました。もう一度実行してください。", "OK");
                //    return false;
                //}

                // ところどころ、conbineObject参照すべきところをbaseObjectを参照しているので修正した ↓

                // そのうち、メッシュ無しでDB等のComponentだけ移植とかも出来るようにしたい。そこらへん整えてそのうちこのチェック廃止
                var combineSkinnedMesh = combineObject.GetComponentInChildren<SkinnedMeshRenderer>();
                if (!combineSkinnedMesh)
                {
                    EditorUtility.DisplayDialog("ERROR", combineObject.name + "にSkinnedMeshRendererが存在しません。", "OK");
                    return false;
                }

                var combineRootBone = combineSkinnedMesh.rootBone;
                if (!combineRootBone)
                {
                    EditorUtility.DisplayDialog("ERROR", combineObject.name + "のRootBoneが存在しません。\n\n正しく衣装オブジェクトを指定している場合、SkinnedMeshRendererコンポーネント内のRootBone欄が外れているだけの可能性があります。Hips等を指定してください", "OK");
                    return false;
                }

                //combineArmatureの大きさ判定
                var checkScalecomb = combineRootBone.parent.parent.localScale.x != 1 || combineRootBone.parent.localScale.x != 1;
                if (checkScalecomb)
                {
                    int option = EditorUtility.DisplayDialogComplex("ATTENTION",
                        combineObject.name + "の単位Scaleが１ではありません。実行すると失敗する可能性があります。\n\n" +
                    "もしBlenderでFBX出力したものでこの問題が出た場合、Exportする前に全オブジェクト選択しCtrl+Aで適用(全トランスフォームまたは回転と拡縮)し、\n" +
                    "さらにFBXExportする時の設定の「スケールの適用」を「FBX単位スケール」にする等をしてください。\n\n" +
                    $"{combineRootBone.parent.parent.name}:{combineRootBone.parent.parent.localScale.x} , {combineRootBone.parent.name}:{combineRootBone.parent.localScale.y}" +
                    "\n※0.999...倍等の微差なら無視して実行してください",

                        multistring.GetTransText("cOK",Lang),
                        multistring.GetTransText("cCancel",Lang),
                        ""
                        );
                    if (option == 1) return false;
                }


                //Debug.Log($"PreStartCheck. baseroot is {baseRootBone.name}.  combineroot is {combineRootBone.name}");
                bool Rnamecheck1 = baseRootBone.name + AddMatchingWordCombineSide != combineRootBone.name;
                bool Rnamecheck2 = AddMatchingWordCombineSide + baseRootBone.name != combineRootBone.name;
                if (Rnamecheck1 && Rnamecheck2) // AddMatchingWordCombineSideを付けた状態で一致するか
                {
                    //AddMatchingWordCombineSideを付けても一致しなかったパターン。

                    var match1 = Regex.Match(combineRootBone.name, @"^" + baseRootBone.name);
                    var match2 = Regex.Match(combineRootBone.name, baseRootBone.name + @"$");
                    //Debug.Log("比較テスト：" + match2.ToString());

                    var namematch1 = baseRootBone.name == match1.ToString();
                    var namematch2 = baseRootBone.name == match2.ToString();
                    if (namematch1 || namematch2)
                    { // 差分抜いたらマッチした場合
                        var sabun = namematch1 ? combineRootBone.name.Remove(0, match1.Length) : combineRootBone.name.Remove(combineRootBone.name.Length - match2.Length);   // 差分格納

                        int option = EditorUtility.DisplayDialogComplex("ATTENTION",
                            combineObject.name + "とBaseオブジェクトのrootBoneの名前が違いますが、名前の差分を差し引くと一致します\n\n" +
                            $"Base: {baseRootBone.name }  Combine: {combineRootBone.name}  差分: {sabun}\n\n" +
                            "差分を差し引いて実行しますか？\n\n" +
                            $"※現在のAddMatchingWordCombineSideの値\n[ {AddMatchingWordCombineSide} ]を[ {sabun} ]に置換",
                            "差し引いて実行 (OK)",
                            "Cancel",
                            ""
                            );

                        //Debug.Log("Dailog Res:" + option);

                        switch (option)
                        {
                            // 差し引いて実行
                            case 0:
                                AddMatchingWordCombineSide = sabun;
                                break;//return true; ここで返さずに最後まで処理する

                            // キャンセル
                            case 1:
                                return false;

                            // 無視して実行(廃止。さすがに意味がないので。)
                            //case 2:
                            //    return true;

                            default:
                                Debug.LogError("Unrecognized option.");
                                break;
                        }

                    }
                    else
                    {  // 差分抜いてもマッチしなかった場合
                        EditorUtility.DisplayDialog("ERROR",
                            combineObject.name + "とBaseオブジェクトのボーン命名規則が違うため実行できません。\n" +
                            "Base: " + baseRootBone.name + " | Combine: " + combineRootBone.name, "OK"
                        );
                        return false;
                    }

                }

            }

            // 複製オプションがオフだった場合に警告を発する
            if(!optionToggleAvatarDup_bool)
            {
                int option = EditorUtility.DisplayDialogComplex("ATTENTION",
                    $"着せ替えを実行すると\n「{BaseObject.name}」の内容が変更されます。\n\n" +
                    $"よろしいですか？\n\n※バックアップをとっておくことをお勧めします\n",
                    "実行",
                    "キャンセル",
                    "複製設定をONにして実行"
                );
                switch (option)
                {
                    // 実行
                    case 0:
                        break;

                    // キャンセル
                    case 1:
                        return false;

                    // 複製設定をONにして実行
                    case 2:
                        optionToggleAvatarDup_bool = true;
                        break;

                    default:
                        Debug.LogError("Unrecognized option.");
                        break;
                }
 
            }




            return true;
        }

        private bool CheckCombineObjects()
        {
            if (!CombineObjects.Any()) return false;

            foreach (var combineObject in CombineObjects)
            {
                if (!combineObject) return false;
            }

            return true;
        }

        private void OnGUI()
        {
            if(!isInit) Init();
            if (multistring == null) MultiLanguageInit();// 言語設定が初期化されてなかったら初期化。

            // 基本項目 -------------------------------------------------------------------------------------------------
            EditorGUILayout.LabelField($"  {multistring.GetTransText("Basic", Lang)}  ", (GUIStyle)"AppToolbar");
            GUILayout.BeginVertical((GUIStyle)"GroupBox");
            scrollpos = GUILayout.BeginScrollView(scrollpos, false, true, GUILayout.Width(position.width-35), GUILayout.Height(115));
            {
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{multistring.GetTransText("Avatar", Lang)} {multistring.GetTransText("Object", Lang)}");
                BaseObject = EditorGUILayout.ObjectField(BaseObject, typeof(GameObject), true) as GameObject;
                GUILayout.EndHorizontal();

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{multistring.GetTransText("Clothing",Lang)} {multistring.GetTransText("Objects", Lang)}");
                    if (GUILayout.Button("+"))
                    {
                        CombineObjects.Add(null);
                    }
                    EditorGUI.BeginDisabledGroup(!CombineObjects.Any());
                    if (GUILayout.Button("-"))
                    {
                        CombineObjects.Remove(CombineObjects.Last());
                    }
                    EditorGUI.EndDisabledGroup();

                }

                for (int i = 0; i < CombineObjects.Count(); ++i)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"{multistring.GetTransText("Clothing",Lang)} {multistring.GetTransText("Object", Lang)}" + (i + 1));
                        CombineObjects[i] = EditorGUILayout.ObjectField(CombineObjects[i], typeof(GameObject), true) as GameObject;
                    }
                }

            }
            GUILayout.EndScrollView();


            EditorGUI.BeginDisabledGroup(!CheckCombineObjects() || !BaseObject);
            if (GUILayout.Button(multistring.GetTransText("Assemble", Lang)))
            {
                if (!Check()) return;
                Assemble();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.EndVertical();



            EditorGUILayout.Space();
            EditorGUILayout.Space();


            // 設定 ------------------------------------------------------------------------------
            EditorGUILayout.LabelField($"  {multistring.GetTransText("Option", Lang)}  ", (GUIStyle)"AppToolbar");
            GUILayout.BeginVertical((GUIStyle)"GroupBox");

            scrollpos2 = GUILayout.BeginScrollView(scrollpos2, false, false, GUILayout.Width(position.width-40), GUILayout.Height(position.height-330));


            //EditorGUILayout.LabelField("一般設定", (GUIStyle)"AppToolbar");
            optionToggleAvatarDup_bool = EditorGUILayout.ToggleLeft(multistring.GetTransText("DupAvatar",Lang), optionToggleAvatarDup_bool);
            optionToggleDupShift_bool = EditorGUILayout.ToggleLeft(multistring.GetTransText("DupShift",Lang), optionToggleDupShift_bool);



            //EditorGUILayout.LabelField("ボーン名の差分単語指定", (GUIStyle)"AppToolbar");
            using (new EditorGUILayout.HorizontalScope())
            {
                AddMatchingWordCombineSide = EditorGUILayout.TextField(multistring.GetTransText("BoneWord",Lang), AddMatchingWordCombineSide, GUILayout.Width(position.width-110));
                foldout1 = EditorGUILayout.Foldout(foldout1, "?");
            }
            if (foldout1)
            {
                var mes = 
                    multistring.GetTransText("HelpA1",Lang) + "\n\n" +
                    multistring.GetTransText("HelpA2",Lang) + "\n" +
                    multistring.GetTransText("HelpA3",Lang) + "\n\n" +
                    multistring.GetTransText("HelpA4",Lang);

                EditorGUILayout.HelpBox(mes, MessageType.None);
            }

            using (new EditorGUILayout.HorizontalScope())
            {

                EditorGUILayout.LabelField($"{multistring.GetTransText("Mesh", Lang)} {multistring.GetTransText("Output Folder", Lang)} : {savingfolder}", GUILayout.Width(position.width-130));

                if (GUILayout.Button("…", GUILayout.Width(20)))
                {
                    var path = EditorUtility.OpenFolderPanel("Select Output Folder", savingfolder, "").Replace("\\","/");
                    if (string.IsNullOrEmpty(path))
                    {
                        //savingfolder = V;
                        // Null = キャンセル されたときは何もしない
                    }
                    else
                    {
                        CheckSavefoloderValid(path);//pathをチェックしsave folderに上書きされる
                    }
                }
                foldout2 = EditorGUILayout.Foldout(foldout2, "?");


            }
            if (foldout2)
            {
                EditorGUILayout.HelpBox(multistring.GetTransText("HelpB1", Lang), MessageType.None);
            }

            optionToggleSaveingPath_bool = EditorGUILayout.ToggleLeft(multistring.GetTransText("opt_foldersave", Lang), optionToggleSaveingPath_bool);


            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            EditorGUILayout.LabelField($"{toolname}  {version}");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Check WEB to Update", GUILayout.Width(position.width / 2 - 25)))
                {
                    Application.OpenURL(url);
                }
                if (GUILayout.Button("出来ること出来ないこと", GUILayout.Width(position.width / 2 - 25)))
                {
                    Application.OpenURL(documenturl);
                }
            }


            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("   Language", GUILayout.Width(80));
                optionpopupLang_index = EditorGUILayout.Popup(optionpopupLang_index, optionpopupLang_string, GUILayout.Width(125));

                var beforechangeLang = Lang; // 変更検知用
                Lang = languageIdString[optionpopupLang_index];
                // GUIの別用意メニューテキストに代入
                MultiLanguageGUIRefresh();
                if(beforechangeLang != Lang) EditorPrefs.SetInt("Language", optionpopupLang_index);
            }


            /*
            GUILayout.BeginVertical();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            var tex = EditorGUIUtility.IconContent("PrefabNormal Icon");
            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
            for (int i = 0; i < 100; i++)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(tex, GUILayout.Width(20));
                if (GUILayout.Button("Button", "label")) Debug.Log("Label");
                GUILayout.EndHorizontal();
            }
            EditorGUIUtility.SetIconSize(Vector2.zero);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            */
        }

        private void DestroyChild(GameObject gObject)
        {
            foreach (Transform n in gObject.transform)
            {
                GameObject.DestroyImmediate(n.gameObject);
            }
        }

        private List<Transform> GetBoneList(Transform rootBone)
        {
            // ベースボーンからボーンリストを生成
            return GetAllChildren.GetAll(rootBone.gameObject).Select(e => e.transform).ToList();
        }

        private Transform FindBone(GameObject root, string name)
        {
            List<GameObject> list = GetAllChildren.GetAll(root);

            foreach (var child in list)
            {
                if (String.Equals(name, child.name)) return child.transform;
            }
            return null;
        }

        private bool MatchWordCheck(string wordA, string wordB)
        {
            return
                wordA + AddMatchingWordCombineSide == wordB ||
                AddMatchingWordCombineSide + wordA == wordB ||
                wordA == wordB;
        }


        // クロスコンポーネントの転送
        private void ClothComponentTransporter(Cloth cloth, GameObject root, Cloth orignal)
        {
            // カプセルコライダー移植
            var capusuleColliders = cloth.capsuleColliders;

            List<GameObject> objectList = new List<GameObject>();

            GetAllChildren.GetChildren(root, ref objectList);

            List<CapsuleCollider> newList = new List<CapsuleCollider>();

            foreach (var capusuleCollider in capusuleColliders)
            {
                string name = capusuleCollider.gameObject.name;

                var sameObjects = objectList.Where(item => MatchWordCheck(item.name, name) ); // AddMatchingWordCombineSideを追加

                if (sameObjects.Any())
                {
                    newList.Add(sameObjects.First().GetComponent<CapsuleCollider>());
                }
            }

            cloth.capsuleColliders = newList.ToArray();


            //球体コライダー移植
            var sphereColliders = cloth.sphereColliders;

            List<ClothSphereColliderPair> newListS = new List<ClothSphereColliderPair>();

            foreach (var sphereCollider in sphereColliders)
            {
                string namefirst = sphereCollider.first.gameObject.name;
                string namesecond = sphereCollider.second.gameObject.name;

                var sameObjectsfirst = objectList.Where(item => MatchWordCheck(item.name, namefirst) ); // AddMatchingWordCombineSideを追加
                var sameObjectssecond = objectList.Where(item => MatchWordCheck(item.name, namesecond) ); // AddMatchingWordCombineSideを追加

                var cscp = new ClothSphereColliderPair();
                if (sameObjectsfirst.Any())
                {
                    cscp.first = sameObjectsfirst.First().GetComponent<SphereCollider>();
                }
                if (sameObjectssecond.Any())
                {
                    cscp.second = sameObjectssecond.First().GetComponent<SphereCollider>();
                }
                newListS.Add(cscp);
            }

            cloth.sphereColliders = newListS.ToArray();



            //CCBR式ウェイトコピー
            var coefficients = cloth.coefficients;
            var originalcoefficients = orignal.coefficients;
            if (originalcoefficients.Length == coefficients.Length)
            {
                for (int i = 0, il = originalcoefficients.Length; i < il; ++i)
                {
                    coefficients[i].collisionSphereDistance = originalcoefficients[i].collisionSphereDistance;
                    coefficients[i].maxDistance = originalcoefficients[i].maxDistance;
                }
                cloth.coefficients = coefficients;
            }
            else Debug.LogError("Clothコピーの際にメッシュの頂点数が一致しませんでした。When copying cloth, the number of mesh vertices did not match.");


            // 他のParameterは複製時のParameter生成で問題ないハズ


        }


        /*
        // List全表示用デバック関数
            private void ShowListContentsInTheDebugLog<T>(List<T> list)
            {
                string log = "";

                foreach(var content in list.Select((val, idx) => new {val, idx}))
                {
                    if (content.idx == list.Count - 1)
                        log += content.val.ToString();
                    else
                        log += content.val.ToString() + ", ";
                }

                Debug.Log(log);
            }
        */

        // === メイン処理 =============================================================================================
        private void Assemble()
        {
            // スタードデバッグログ
            //Debug.Log("Start Assemble");
            //Debug.Log("AddWord :" + AddMatchingWordCombineSide);

            // ダイアログ選択肢処理用
            var dialogRes_Listloss = true;


            List<Transform> BoneList = new List<Transform>();



            // ベースオブジェクトの複製
            GameObject newBase;
            if( optionToggleAvatarDup_bool )    // 複製設定IF
            {
                
                newBase = Instantiate(BaseObject, BaseObject.transform.position + (optionToggleDupShift_bool ? Vector3.right*1.5f : Vector3.zero )  , Quaternion.identity);
            }
            else
            {
                newBase = BaseObject;   // 複製せずに処理。
            }

            {
                // ベースオブジェクトからRootであるボーンを検索
                var baseSkinnedMesh = newBase.GetComponentInChildren<SkinnedMeshRenderer>();
                var rootBone = baseSkinnedMesh.rootBone; //FindBone(newBase, "Hips");

                //Debug.Log("rootBone:" + rootBone);

                // ボーンリストを生成
                BoneList = GetBoneList(rootBone);
                BoneList.Insert(0, rootBone); // 先頭にRootボーン追加
                                              //BoneList.Insert(0,rootBone.parent); // さらに先頭にArmtureを追加　これをしないとArmatureにコンポーネントがついていた場合に取得できない。が、気にするほどでもないか？
            }

            //ShowListContentsInTheDebugLog<Transform>(BoneList);

            List<ComponentData> componentDataList = new List<ComponentData>();

            foreach (var combineObject in CombineObjects)
            {

                List<Transform> combineBonesList = new List<Transform>();
                {
                    // 結合オブジェクトからRootであるボーンを検索
                    Transform rootBone;
                    var combineSkinnedMesh = combineObject.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (combineSkinnedMesh)
                    { // スキニードメッシュRendererがあったら
                        rootBone = combineSkinnedMesh.rootBone;
                    }
                    else
                    {    // 無かったら
                        rootBone = FindBone(combineObject, "Hips");
                    }

                    // 結合ボーンリストを作成
                    combineBonesList = GetBoneList(rootBone);
                    combineBonesList.Insert(0, rootBone);      // Base同様、Rootボーンをリストに含ませる。無かった。
                                                               //combineBonesList.Insert(0,rootBone.parent);       // Armatureを含めるかどうか

                    //Debug.Log("Conbine RootBone:" + rootBone.name);
                    //Debug.Log("ListFirst:" + combineBonesList.First());
                    //ShowListContentsInTheDebugLog<Transform>(combineBonesList);
                }
                // ベースボーンリストにボーンを追加していく
                foreach (var combineBone in combineBonesList)
                {
                    // ベースボーンから同名のボーンを検索する
                    var sameBone = BoneList.Where(e => MatchWordCheck(e.name, combineBone.name) ); // AddMatchingWordCombineSide を追加した

                    if (!sameBone.Any())
                    {
                        // 同名のボーンが存在しない場合

                        // 親ボーンを検索する
                        var parentName = combineBone.parent.name;
                        //Debug.Log("Bonename:"+combineBone.name);
                        //Debug.Log("Parentname:"+parentName);
                        var targetParent = BoneList.Select((trans, index) => new { trans, index }).Where(e => MatchWordCheck(e.trans.name, parentName) );//AddMatchingWordCombineSideを追加


                        if (targetParent.Any())
                        {
                            // オブジェクトを追加する
                            //var boneObject = Instantiate(combineBone.gameObject, targetParent.First().trans);   // ここでコンポーネントごとコピーされる？しかし正常にはされてない様子
                            var boneObject = new GameObject(combineBone.name);  //空っぽから生成する

                            //boneObject.name = combineBone.name;
                            boneObject.transform.parent = targetParent.First().trans;   // 親子関係設定
                                                                                        //DestroyChild(boneObject); // 不要な子オブジェクトを削除する
                                                                                        //Debug.Log("NewAddedbone:" + boneObject.gameObject.name);

                            // ローカルトランスフォームをそろえる
                            boneObject.transform.localPosition = combineBone.localPosition;
                            boneObject.transform.localRotation = combineBone.localRotation;
                            boneObject.transform.localScale = combineBone.localScale;


                            // 親ボーンを発見した場合
                            // 必ず親ボーンの後に挿入されるようにする。
                            BoneList.Insert(targetParent.First().index + 1, boneObject.transform);


                            // コンポーネントの上書き処理用スタック
                            ComponentData componentData;
                            var components = combineBone.gameObject.GetComponents<Component>();

                            componentData.target = boneObject;
                            componentData.components = components.ToList();
                            componentDataList.Add(componentData);
                            // コンポーネントを正規のコピペする。追加式。
                            foreach (var compdata in componentData.components)
                            {
                                if (compdata.GetType() == typeof(Transform)) continue;//Transformの追加をスキップする（スキップしなくても２重にはならないぽい
                                UnityEditorInternal.ComponentUtility.CopyComponent(compdata);
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(boneObject);
                            }

                        }
                        else
                        {
                            Debug.LogError("Parent not found. : " + parentName);
                        }
                    }
                    else
                    {
                        // 同名のボーンが存在した場合
                        // コンポーネントの上書き処理用スタック
                        ComponentData componentData;
                        var components = combineBone.gameObject.GetComponents<Component>();

                        //コンポチェック(デバッグログ表示)
                        /*
                        string debugstring = "" + combineBone.name + ":";
                        foreach(var comp in components){
                            debugstring += comp.GetType() +" , ";
                        }
                        Debug.Log(debugstring);
                        */

                        componentData.target = sameBone.First().gameObject;
                        componentData.components = components.ToList();
                        componentDataList.Add(componentData);
                        // この段階ではまたコピーを行っていない？
                        //同名ボーンが無かったときはコンポごとオブジェコピーされたがここではされてないので書き込む。追加扱い。DBかつ同名ターゲットだった場合のみ上書き

                        // 先にBese側ダイナミックボーンリストを取得
                        var basedblist = componentData.target.GetComponents<DynamicBone>().ToList();

                        foreach (var compdata in componentData.components)
                        {
                            UnityEditorInternal.ComponentUtility.CopyComponent(compdata);
                            if (compdata is DynamicBone)
                            { // DBだったら
                                var combdb = compdata as DynamicBone;
                                // Rootが同じDBを探索
                                var samebasedbs = basedblist.Where(val => MatchWordCheck(val.m_Root.name, combdb.m_Root.name) );
                                if (samebasedbs.Any())
                                { // 同じものがあれば 値を上書き　同名のものは他にはない前提で処理をする。
                                    UnityEditorInternal.ComponentUtility.PasteComponentValues(samebasedbs.First());
                                }
                                else
                                { // 同じものが無かった
                                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(componentData.target);
                                }

                            }
                            else if (compdata is DynamicBoneColliderBase)
                            {     // DBコライダーだったら
                                var combdbc = compdata as DynamicBoneColliderBase;
                                // Bese側にDBコライダーがないかチェック。あったら問答無用で上書きする
                                var basedbc = componentData.target.GetComponent<DynamicBoneColliderBase>(); // １つしかない前提で処理する。
                                if (basedbc)
                                {//存在したら
                                    UnityEditorInternal.ComponentUtility.PasteComponentValues(basedbc);//上書き
                                }
                                else
                                {
                                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(componentData.target);//追加
                                }

                            }
                            else
                            {  // その他だったら
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(componentData.target);
                            }
                        }



                        //Debug.Log("Over,"+componentData.target.name);
                    }
                }

                List<GameObject> objectList = GetAllChildren.GetAll(newBase);
                //ShowListContentsInTheDebugLog<GameObject>(objectList);

                // コンポーネントの新規 ・・・はない。参照の処理
                foreach (var componentData in componentDataList)
                {
                    var components = componentData.components;
                    //Debug.Log("componetDataTargetName:"+componentData.target.name + "-------------------------------------------------------");

                    //base側コンポーネント配列
                    List<Component> basecomps = componentData.target.GetComponents<Component>().ToList();   // 2020/10/02 DynamicBone指定からComponet指定に変更
                    //                          ↑の旧配置場所は下のforeachの中かつ「GetComponent」だったため、
                    //                          １つのオブジェにDBが複数あっても１つめのDBにしか処理できずにバグが発生していた。


                    //foreach(var component in components)
                    foreach (var basecomp in basecomps)
                    {
                        var type = basecomp.GetType();

                        //Debug.Log("typeof:"+ type);

                        // ダイナミックボーンの参照切り替え-----------------------------------
                        if (type == typeof(DynamicBone))
                        {
                            //Debug.Log("ProcessingIn:"+ type);

                            DynamicBone basedynamicBone = basecomp as DynamicBone;

                            //if(!copy) {   コンポーネントは前段階ですべてすでにコピーされているのでチェック及び追加処理は削除
                            //    copy = componentData.target.AddComponent<DynamicBone>();
                            //    Debug.Log("DynamicBone Notting. Adding");
                            //} else Debug.Log("DynamicBone is Ready");

                            // 指定無しかどうかチェック
                            var rootname = "(空欄)";
                            if( basedynamicBone.m_Root==null)
                            {
                                if(dialogRes_Listloss) dialogRes_Listloss = EditorUtility.DisplayDialog("ATTENTION",
                                    $"{basedynamicBone.gameObject.name}のDynamicBoneコンポーネントのRootが空欄です。\n\n処理は続行されます。",
                                    "了解",
                                    "全て了解");
                                
                                // 空欄を追加
                                basedynamicBone.m_Root = null;

                            } else
                            {
                                basedynamicBone.m_Root = BoneList.Where(item => MatchWordCheck(item.name, basedynamicBone.m_Root.name) ).First();
                                rootname = basedynamicBone.m_Root.name;
                            }
                                    // AddMatchingWordCombineSideを追加
                                      //Debug.Log("RootBone:"+basedynamicBone.m_Root + "----------------------");

                            // コライダーリスト処理
                            var colliders = basedynamicBone.m_Colliders;
                            var newColliders = new List<DynamicBoneColliderBase>();
                            foreach (var collider in colliders)
                            {
                                //Debug.Log("OrignalDBCname:"+collider);
                                if( collider==null)
                                {
                                    if(dialogRes_Listloss) dialogRes_Listloss = EditorUtility.DisplayDialog("ATTENTION",
                                        $"{basedynamicBone.gameObject.name}のDynamicBoneコンポーネント(root:{rootname})" +
                                        "のコライダーリストに空欄があります。\n\n処理は続行されます。",
                                        "了解",
                                        "全て了解");
                                    
                                    // 空欄を追加
                                    newColliders.Add(collider);
                                    continue;
                                }
                                var sameObject = objectList.Where(item => MatchWordCheck(item.name, collider.name) );
                                    // AddMatchingWordCombineSideを追加
                                    //Debug.Log("SameObject is :" + sameObject.First());
                                if (sameObject.Any())
                                {
                                    newColliders.Add(sameObject.First().GetComponent<DynamicBoneColliderBase>());
                                    //Debug.Log("SameObjectmatch:Yes : coll:"+newColliders.Last() + "  , data:" +collider.GetComponent<DynamicBoneColliderBase>());// ここで一部検出できないものがある。
                                }// else Debug.Log("SameObjectmatch:No");
                            }
                            //Debug.Log("DBCCount:"+colliders.Count);
                            basedynamicBone.m_Colliders = newColliders;

                            // 除外リスト処理
                            var exclusions = basedynamicBone.m_Exclusions;
                            var newExclusions = new List<Transform>();
                            foreach (var exclusion in exclusions)
                            {
                                if( exclusion==null)
                                {
                                    if(dialogRes_Listloss) dialogRes_Listloss = EditorUtility.DisplayDialog("ATTENTION",
                                        $"{basedynamicBone.gameObject.name}のDynamicBoneコンポーネント(root:{rootname})" +
                                        "の排除(exclusion)リストに空欄があります。\n\n処理は続行されます。",
                                        "了解",
                                        "全て了解");
                                    
                                    // 空欄を追加
                                    newExclusions.Add(exclusion);
                                    continue;
                                }

                                var sameObject = objectList.Where(item => MatchWordCheck(item.name, exclusion.name) );
                                    // AddMatchingWordCombineSideを追加
                                if (sameObject.Any())
                                {
                                    newExclusions.Add(sameObject.First().transform);
                                }
                            }
                            basedynamicBone.m_Exclusions = newExclusions;


                            /* // ここらはすでにコピーされている
                            copy.m_Damping = dynamicBone.m_Damping;
                            copy.m_DampingDistrib = dynamicBone.m_DampingDistrib;
                            copy.m_DistanceToObject = dynamicBone.m_DistanceToObject;
                            copy.m_DistantDisable = dynamicBone.m_DistantDisable;
                            copy.m_Elasticity = dynamicBone.m_Elasticity;
                            copy.m_ElasticityDistrib = dynamicBone.m_ElasticityDistrib;
                            copy.m_EndLength = dynamicBone.m_EndLength;
                            copy.m_EndOffset = dynamicBone.m_EndOffset;
                            copy.m_Force = dynamicBone.m_Force;
                            copy.m_FreezeAxis = dynamicBone.m_FreezeAxis;
                            copy.m_Gravity = dynamicBone.m_Gravity;
                            copy.m_Inert = dynamicBone.m_Inert;
                            copy.m_InertDistrib = dynamicBone.m_InertDistrib;
                            copy.m_Radius = dynamicBone.m_Radius;
                            copy.m_RadiusDistrib = dynamicBone.m_RadiusDistrib;
                            //copy.m_ReferenceObject = dynamicBone.m_ReferenceObject;
                            copy.m_Stiffness = dynamicBone.m_Stiffness;
                            copy.m_StiffnessDistrib = dynamicBone.m_StiffnessDistrib;
                            copy.m_UpdateMode = dynamicBone.m_UpdateMode;
                            copy.m_UpdateRate = dynamicBone.m_UpdateRate;
                            */
                        }



                        // コンストレイント型全てのSource参照切り替え----------------------------------------
                        if (type == typeof(IConstraint) ||
                            type == typeof(PositionConstraint)  ||  type == typeof(RotationConstraint) ||
                            type == typeof(ScaleConstraint)     ||  type == typeof(ParentConstraint) ||
                            type == typeof(AimConstraint)       ||  type == typeof(LookAtConstraint)
                        )   // ちょっと指定が力技すぎる。IConstraintとしてまとめて指定できないものか。
                        {
                            //Debug.Log("ProcessingIn:"+ type);

                            IConstraint baseIConstraint = basecomp as IConstraint;


                            // Sourceリストの参照切り替え
                            var sources = new List<ConstraintSource>(); baseIConstraint.GetSources(sources);    // Source一覧取得
                            var newSources = new List<ConstraintSource>();
                            foreach (var source in sources)
                            {
                                //Debug.Log("OrignalSourceName:"+source);
                                if( source.sourceTransform==null)
                                {
                                    if(dialogRes_Listloss) dialogRes_Listloss = EditorUtility.DisplayDialog("ATTENTION",
                                        $"{basecomp.gameObject.name}のConstraint系コンポーネント" +
                                        "のSourceリストに空欄があります。\n\n処理は続行されます。",
                                        "了解",
                                        "全て了解");
                                    
                                    // 空欄を追加
                                    newSources.Add(source);
                                    continue;
                                }

                                var sameObject = objectList.Where(item => MatchWordCheck(item.name, source.sourceTransform.gameObject.name) );  // AddMatchingWordCombineSideを追加
                                    //Debug.Log("SameObject is :" + sameObject.First());
                                if (sameObject.Any())
                                {
                                    var newSource = new ConstraintSource(){sourceTransform = sameObject.First().transform , weight = source.weight};
                                    newSources.Add(newSource);


                                    //Debug.Log("SameObjectmatch:Yes : coll:"+newColliders.Last() + "  , data:" +collider.GetComponent<DynamicBoneColliderBase>());// ここで一部検出できないものがある。
                                }// else Debug.Log("SameObjectmatch:No");
                            }
                            baseIConstraint.SetSources(newSources);

                        }


                        // Aim コンストレイント型のアップオブジェクト参照切り替え----------------------------------------
                        if ( type == typeof(AimConstraint) )
                        {
                            //Debug.Log("ProcessingIn:"+ type);

                            AimConstraint baseAimConstraint = basecomp as AimConstraint;

                            //まず、オブジェクトアップモード、またはオブジェクトローテーションアップモードか
                            bool tempCheck = baseAimConstraint.worldUpType == AimConstraint.WorldUpType.ObjectUp || baseAimConstraint.worldUpType == AimConstraint.WorldUpType.ObjectRotationUp;

                            if( baseAimConstraint.worldUpObject == null )
                            {
                                if(dialogRes_Listloss && tempCheck ) dialogRes_Listloss = EditorUtility.DisplayDialog("ATTENTION",
                                    $"{baseAimConstraint.gameObject.name}のAimConstraintコンポーネントのワールドアップ(または回転)オブジェクトの指定が空欄です。\n\n処理は続行されます。",
                                    "了解",
                                    "全て了解");
                                
                                // 空欄を追加
                                //baseAimConstraint.worldUpObject = null;

                            }
                            else
                            {
                                // WorldUpObjectの参照切り替え
                                baseAimConstraint.worldUpObject = BoneList.Where(item => MatchWordCheck(item.name, baseAimConstraint.worldUpObject.gameObject.name) ).First();
                                // ↑一致するものがなかったらNULLを返してほしいけど、修正するのがめんどくさいので元のオブジェクトが帰ってくる。↓の処理は実行されることはない
                                if(baseAimConstraint.worldUpObject == null ) EditorUtility.DisplayDialog("ERROR",
                                    $"{baseAimConstraint.gameObject.name}のAimConstraintコンポーネントのワールドアップ(または回転)オブジェクト参照が引き継げませんでした。\nアバタールート階層、Armature階層に存在するオブジェクトは現在の仕様では参照できません。",
                                    "了解");
                            }


                        }


                        // Joint型の参照切り替え
                        if (type == typeof(Joint)           ||  type == typeof(ConfigurableJoint) ||
                            type == typeof(CharacterJoint)  ||  type == typeof(HingeJoint) ||
                            type == typeof(SpringJoint)     ||  type == typeof(FixedJoint)
                        )
                        {
                            //Debug.Log("ProcessingIn:"+ type);

                            Joint baseJoint = basecomp as Joint;

                            if( baseJoint.connectedBody==null)
                            {
                                if(dialogRes_Listloss) dialogRes_Listloss = EditorUtility.DisplayDialog("ATTENTION",
                                    $"{baseJoint.gameObject.name}のJoint系コンポーネントの接続先が空欄です。\n\n処理は続行されます。",
                                    "了解",
                                    "全て了解");
                                
                                // 空欄を追加
                                baseJoint.connectedBody = null;

                            } else
                            {
                                // connectedBodyの参照切り替え
                                baseJoint.connectedBody = BoneList.Where(item => MatchWordCheck(item.name, baseJoint.connectedBody.gameObject.name) ).First().gameObject.GetComponent<Rigidbody>();
                            }

                        }



                    }

                }

                componentDataList.Clear();



                // 下のメッシュ移植を、アバタールート直下のメッシュ以外のオブジェでも移植するようにしたい。

                // メッシュのボーンリスト情報の更新
                var CombineMeshs = combineObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var mesh in CombineMeshs)
                {
                    //Debug.Log("Meshname:" + mesh.gameObject.name);
                    var combinedMesh = Instantiate(mesh.gameObject);
                    combinedMesh.name = mesh.gameObject.name;
                    combinedMesh.transform.SetParent(newBase.transform);

                    combinedMesh.transform.localPosition = mesh.gameObject.transform.localPosition;
                    combinedMesh.transform.localRotation = mesh.gameObject.transform.localRotation;
                    combinedMesh.transform.localScale = mesh.gameObject.transform.localScale;

                    var skinnedMesh = combinedMesh.GetComponent<SkinnedMeshRenderer>();
                    skinnedMesh.sharedMesh = mesh.sharedMesh;


                    //ボーンウェイトから実際に使っているボーンのインデックスを列挙し、以下のバインドポーズ処理時にローテンション不一致ボーンがあっても、列挙外であれば無視する
                    //var boneweight = new List<BoneWeight>();
                    var usebone = new List<int>();
                    foreach (var boneweight in skinnedMesh.sharedMesh.boneWeights.ToList())
                    {
                        if (!(usebone.Where(val => val == boneweight.boneIndex0).Any()) && boneweight.weight0 != 0.0) usebone.Add(boneweight.boneIndex0);
                        if (!(usebone.Where(val => val == boneweight.boneIndex1).Any()) && boneweight.weight1 != 0.0) usebone.Add(boneweight.boneIndex1);
                        if (!(usebone.Where(val => val == boneweight.boneIndex2).Any()) && boneweight.weight2 != 0.0) usebone.Add(boneweight.boneIndex2);
                        if (!(usebone.Where(val => val == boneweight.boneIndex3).Any()) && boneweight.weight3 != 0.0) usebone.Add(boneweight.boneIndex3);
                    }
                    //Debug.Log("useing bone list");
                    //ShowListContentsInTheDebugLog<int>(usebone);




                    // バインドポーズ処理
                    int i = 0;
                    bool isNomatch = false;
                    List<Transform> bones = new List<Transform>();
                    List<Matrix4x4> lbindpose = new List<Matrix4x4>();
                    foreach (var bone in skinnedMesh.bones)
                    {
                        var target = BoneList.Where(e => MatchWordCheck(e.name, bone.name)).First();// AddMatchingWordCombineSideを追加
                        bones.Add(target);

                        // ここでボーンの回転違いがあればBonePoseを補正する
                        if (target.transform.localRotation != bone.transform.localRotation)
                        {
                            //Debug.Log("name:" + target.name + ".  TransT:" + target.transform.localRotation + ".  TransB:" + bone.transform.localRotation);
                            //Debug.Log("name:" + target.name + ".  ScaleT:" + target.transform.localScale + ".  ScaleB:" + bone.transform.localScale);
                            lbindpose.Add(target.transform.worldToLocalMatrix * combinedMesh.transform.localToWorldMatrix);
                            //isNomatch = true;// 一つでも一致しないボーンローテーションがあった
                            if (usebone.Where(val => val == i).Any())
                            {
                                //Debug.Log(""+ target.name + " is usebone");
                                isNomatch = true; // ボーンウェイトで実際につかわれてるボーンだったらtrueへ　これで不要な複製回避しないと、一部のメッシュが異常複製する。謎。謎of謎
                            }
                            else
                            {
                                //Debug.Log(""+ target.name + " is Notusebone");
                            }

                        }
                        else
                        { // 同じ場合
                            lbindpose.Add(target.transform.worldToLocalMatrix * combinedMesh.transform.localToWorldMatrix);
                        }

                        // ローテーション違いがあったものだけ計算し直せばいいので↑の行は全てにおいて計算しなおしていて無駄だけども、気にするほどでもない
                        //Debug.Log("bone:"+bone.name+"("+i+")     , matrix:"+ lbindpose + "("+lbindpose.Count+")");
                        ++i;
                    }




                    // 1つでも一致しないボーンローテーションがあったらmeshを複製する
                    if (isNomatch)
                    {
                        var newmesh = Instantiate(mesh.sharedMesh);
                        var newname = mesh.sharedMesh.name + "_AA";

                        // 複製物への変更内容
                        newmesh.bindposes = lbindpose.ToArray();

                        /*
                        //セーブ先チェック
                        if(!AssetDatabase.IsValidFolder(savingfolder))
                        {
                            //無効なフォルダパスだった
                            savingfolder = V;
                            EditorUtility.DisplayDialog("ATTENTION",
                                    $"出力フォルダーの指定が無効です。\n \"Assets/\"に出力します。",
                                    "了解");
                        }
                        */

                        // セーブ
                        AssetDatabase.CreateAsset(newmesh, AssetDatabase.GenerateUniqueAssetPath(savingfolder + newname + ".asset"));
                        AssetDatabase.SaveAssets();

                        // 複製したメッシュに置き換え
                        skinnedMesh.sharedMesh = newmesh;
                    }

                    skinnedMesh.bones = bones.ToArray();
                    skinnedMesh.rootBone = BoneList.First();

                    if (skinnedMesh.probeAnchor)
                    {
                        var probeanker = BoneList.Where(e => MatchWordCheck(e.name, skinnedMesh.probeAnchor.gameObject.name) );
                        if (probeanker.Any()) skinnedMesh.probeAnchor = probeanker.First();// else skinnedMesh.probeAnchor = null;
                                                                                           // ProbeAnkerは全部Base側のHipsにしちゃっていいんでなかろうか。とはいえ、Baseのほうのアンカーはいじっていないので統一できないのが悩ましい
                    }

                    /*
                                    // 複製されたメッシュのボーンリストを確認。順番はあっている
                                    var conblist = mesh.bones.ToList();
                                    var origlist = skinnedMesh.bones.ToList();

                                    string log = "複製先とオリジナルのボーンリスト順列チェック\n";

                                    foreach(var content in origlist.Select((val, idx) => new {val, idx}))
                                    {
                                        log += conblist.ElementAt(content.idx) + "\t\t" + content.val.ToString() + ",\n";
                                    }

                                    Debug.Log(log + "\n Count: " + conblist.Count + " , " + origlist.Count);
                    */


                    var cloth = combinedMesh.GetComponent<Cloth>();
                    if (cloth)
                    {
                        ClothComponentTransporter(cloth, newBase, mesh.gameObject.GetComponent<Cloth>());
                    }

                }
            }

        }

    }

}