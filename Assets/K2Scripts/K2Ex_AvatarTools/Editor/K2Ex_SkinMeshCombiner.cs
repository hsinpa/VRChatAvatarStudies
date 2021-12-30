//#define DEBUGMODE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using System.Text.RegularExpressions;
using K2Scripts.MultiLanguageSystem;


namespace K2Ex_AvatarTools
{
    public class SkinMeshCombiner : EditorWindow
    {

        struct BlendShape
        {
            public string name;
            public int vertexOffset;
            public List<Vector3> deltaVerticies;
            public List<Vector3> deltaNormals;
            public List<Vector3> deltaTangents;




            public BlendShape(string name, List<Vector3> verticies, List<Vector3> normals, List<Vector3> tangents, int vertexOffset)
            {
                this.name = name;
                this.vertexOffset = vertexOffset;
                deltaVerticies = verticies;
                deltaNormals = normals;
                deltaTangents = tangents;
            }
        };

        private GameObject TargetObject;


        //GUI制御用
        private bool isInit = false;
        private Vector2 scrollpos = new Vector2(0, 0);

        private string[] optionpopup1_string;// = new string[] { "重複シェイプキーを合成する", "連番別名でシェイプキーを追加する" };  // 重複シェイプキー名があったらどうするかの選択肢
        private string[] optionpopup2_string;// = new string[] { "上書きで保存する", "連番別名で保存する" };  // 既にファイルが有った場合
        private string[] optionpopupLang_string = new string[] { "English(Machine)", "日本語/日本語のみ" , "日本語/日本語と英語混じり", "中文/简体（机器）", "中文/繁體（機械）", "한국 (기계)"};  // languageIdStringと対応させること

        private MultiLanguageTexts multistring;

        private string Lang = "JP"; // 言語選択(optionpopupLang_indexによって上書きされる。)

        //オプション
        private int optionpopup1_index = 0; // 重複シェイプキーがあったときの処理選択
        private int optionpopup2_index = 0; // メッシュファイルが有った時の処理
        private int optionpopupLang_index = 1; // 言語設定
        private string BakedMeshNaming = "Body";    // 出力Mesh名
        private string savingfolder = "Assets/";    // 出力フォルダ


        //Language
        private const string languagePath = "AT_SMC_Language";
        private string[] languageIdString = new String[]{"EN", "JP", "JP2", "CNK", "CNN", "KR"};


        //固定内部オプション
        private const string addfilenammeword = "_SMC";   //複製ファイルのオブジェクト名に付け加える語尾名。

        [MenuItem("AvatarTools/K2Ex_SkinMeshCombiner")]
        static void Open()
        {
            var setting = EditorWindow.GetWindow<SkinMeshCombiner>();

            // ウィンドウ設定
            setting.minSize = new Vector2(400, 450);
            setting.maxSize = new Vector2(500, 450);

            setting.titleContent = new GUIContent(nameof(SkinMeshCombiner));

        }


        private void MultiLanguageGUIRefresh()
        { // 一部のGUIのテキストは別管轄なので変数に直接代入する
            optionpopup1_string = new string[] { multistring.GetTransText("combine shape", Lang), multistring.GetTransText("add shape", Lang) };
            optionpopup2_string = new string[] { multistring.GetTransText("overwrite save", Lang), multistring.GetTransText("new save", Lang) };
        }

        private void MultiLanguageInit()
        {
            multistring = new MultiLanguageTexts(languagePath);

            // GUIの別用意メニューテキストに代入
            MultiLanguageGUIRefresh();
        }

        private void LoadInit() // Editor設定読み出し
        {
            optionpopupLang_index = EditorPrefs.GetInt("Language", optionpopupLang_index);
            optionpopup1_index = EditorPrefs.GetInt("optionpopup1_index", optionpopup1_index);
            optionpopup2_index = EditorPrefs.GetInt("optionpopup2_index", optionpopup2_index);
            savingfolder = EditorPrefs.GetString("savingfolder", savingfolder);
        }



        private void Init() // 初めてOnGUIが読み込まれたときに呼び出す。
        {
            isInit = true;  //フラグ立てとく
            LoadInit();
        }



        private void OnGUI()
        {
            if (!isInit) Init();
            if (multistring == null) MultiLanguageInit();// 言語設定が初期化されてなかったら初期化。

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"  {multistring.GetTransText("Basic", Lang)}  ", (GUIStyle)"AppToolbar");
            EditorGUILayout.Space();

            GUILayout.BeginVertical((GUIStyle)"GroupBox");

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(multistring.GetTransText("TargetObject", Lang));
            TargetObject = EditorGUILayout.ObjectField(TargetObject, typeof(GameObject), true) as GameObject;
            GUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!TargetObject);
            if (GUILayout.Button(multistring.GetTransText("Combine!", Lang)))
            {
                if (!Check()) return;
                Combine();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("  " + multistring.GetTransText("Option", Lang) + "  ", (GUIStyle)"AppToolbar");
            scrollpos = GUILayout.BeginScrollView(scrollpos, false, false, (GUIStyle)"PreHorizontalScrollbar",(GUIStyle)"PreVerticalScrollbar", (GUIStyle)"GroupBox",
                GUILayout.Width(position.width - 20), GUILayout.Height(position.height - 215));
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField(multistring.GetTransText("Shapekey settings", Lang), (GUIStyle)"AppToolbar");
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(multistring.GetTransText("duplicate names shape", Lang));
                    var isChange = optionpopup1_index;
                    optionpopup1_index = EditorGUILayout.Popup(optionpopup1_index, optionpopup1_string);
                    if( isChange != optionpopup1_index ) EditorPrefs.SetInt("optionpopup1_index",optionpopup1_index);
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();


                EditorGUILayout.LabelField(multistring.GetTransText("baked mesh setting", Lang), (GUIStyle)"AppToolbar");
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(multistring.GetTransText("baked mesh name", Lang));
                    BakedMeshNaming = EditorGUILayout.TextField(BakedMeshNaming);
                }


                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField(multistring.GetTransText("baked mesh file setting", Lang), (GUIStyle)"AppToolbar");
                using (new EditorGUILayout.HorizontalScope())
                {

                    if (GUILayout.Button(multistring.GetTransText("Output Folder", Lang), GUILayout.Width(100)))
                    {
                        var path = EditorUtility.OpenFolderPanel(multistring.GetTransText("Output Folder", Lang), savingfolder, "");
                        if (string.IsNullOrEmpty(path))
                        {
                        }
                        else
                        {
                            savingfolder = path;
                            var match = Regex.Match(savingfolder, @"Assets/.*");
                            savingfolder = match.Value + "/";
                            if (savingfolder == "/") savingfolder = "Assets/";
                            EditorPrefs.SetString("savingfolder", savingfolder);
                        }
                    }

                    EditorGUILayout.LabelField($"[ {savingfolder} ]");

                }
                EditorGUILayout.Space();

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(multistring.GetTransText("same name", Lang));
                    var isChange = optionpopup2_index;
                    optionpopup2_index = EditorGUILayout.Popup(optionpopup2_index, optionpopup2_string);
                    if( isChange != optionpopup2_index ) EditorPrefs.SetInt("optionpopup2_index",optionpopup2_index);
                }

            }
            GUILayout.EndScrollView();



            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"   {AvatarAssembler.toolname}  {AvatarAssembler.version}");

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Check WEB to Update", GUILayout.Width(position.width - 260)))
                    {
                        Application.OpenURL(AvatarAssembler.url);
                    }
                    //if (GUILayout.Button("出来ること出来ないこと", GUILayout.Width(position.width / 2 - 10)))
                    //{
                    //    Application.OpenURL(documenturl);
                    //}
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

        }

        private bool Check()
        {
            var skinMeshRendererList = TargetObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (!skinMeshRendererList.Any())
            {
                EditorUtility.DisplayDialog("ERROR", "対象のオブジェクトにSkinnedMeshRendererが存在しません。", "OK");
                return false;
            }

            if (BakedMeshNaming == "")
            {  // 名前変更するようにするならここで変な記号とか使ってないかチェックスべきだけどもめんどくさいのでチェック無し
                BakedMeshNaming = "Body";
                EditorUtility.DisplayDialog("注意", "ベイクした後のメッシュ名が空欄です。\n初期値”Body”に戻しました。", "OK");
            }


            var checkScalebase = skinMeshRendererList.First().rootBone.localScale.x != 1 || skinMeshRendererList.First().rootBone.parent.localScale.x != 1 || TargetObject.transform.localScale.x != 1;
            if (checkScalebase)
            {
                int option = EditorUtility.DisplayDialogComplex("ATTENTION",
                    TargetObject.name + "の単位Scaleが１ではありません。実行すると失敗する可能性があります。\n\n" +
                    "もしBlenderでFBX出力したものでこの問題が出た場合、Exportする前に全オブジェクト選択しCtrl+Aで適用(全トランスフォームまたは回転と拡縮)し、\n" +
                    "さらにFBXExportする時の設定の「スケールの適用」を「FBX単位スケール」にする等をしてください。",
                    "無視して実行",
                    "キャンセル",
                    ""
                    );
                if (option == 1) return false;
            }


            return true;
        }

        private void Combine()
        {
            String objectName = TargetObject.name;
#if DEBUGMODE
        Debug.Log("Combine start");
#endif

            // 元を壊さないよう複製
            GameObject newObject = Instantiate(TargetObject, new Vector3(3.0f, 0.0f, 0.0f), Quaternion.identity);

            SkinnedMeshRenderer skinnedMesh = newObject.GetComponentInChildren<SkinnedMeshRenderer>();

            Transform rootBone = skinnedMesh.rootBone;


            // ボーンリストの取得
            // Hipsオブジェクト以下のボーン構造を全取得
            // ウェイトが入っていないエンプティオブジェクトも関係なしに取得する

            List<Transform> boneList = GetAllChildren.GetAll(skinnedMesh.rootBone.gameObject).Select(e => e.transform).ToList();
            boneList.Insert(0, skinnedMesh.rootBone); // 先頭にHipsボーン追加


            // バインドポーズは事前に生成
            List<Matrix4x4> bindPoseList = new List<Matrix4x4>();
            foreach (var bone in boneList)
            {
                var bindPose = bone.worldToLocalMatrix * skinnedMesh.rootBone.parent.localToWorldMatrix;
                bindPoseList.Add(bindPose);
            }


            // 子オブジェクトからスキンメッシュを持っているオブジェクトのリストを取得
            var skinMeshRendererList = newObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            // クロスコンポーネント持ちは除外する
            skinMeshRendererList = skinMeshRendererList.Where(item => item.gameObject.GetComponent<Cloth>() == null).ToArray();

            List<Material> materialList = new List<Material>(); // マテリアルリスト
            List<Vector3> verticesList = new List<Vector3>(); // 頂点リスト
            List<Vector3> normalsList = new List<Vector3>(); // ノーマルリスト
            List<Vector4> tangentsList = new List<Vector4>(); // タンジェントリスト
            List<Vector2> uvsList = new List<Vector2>(); // UVリスト
            List<BlendShape> blendShapesList = new List<BlendShape>();
            List<List<int>> subMeshList = new List<List<int>>(); // サブメッシュ(三角形)リスト
            List<BoneWeight> boneWeightsList = new List<BoneWeight>(); // ボーンウェイトリスト



            foreach (var skinMesh in skinMeshRendererList)
            {
#if DEBUGMODE
            Debug.Log("skinMesh: " + skinMesh.name);
#endif
                var srcMesh = skinMesh.sharedMesh;


                // 元のスキンメッシュのボーンインデックスと結合メッシュオブジェクトのボーンインデックスの対応付け
                var boneIndexMatchDictionary = new Dictionary<int, int>();
                for (int i = 0; i < skinMesh.bones.Length; ++i)
                {
                    Transform srcBone = skinMesh.bones[i];
                    var dstBone = boneList.Select((trans, index) => new { trans, index }).Where(e => String.Equals(e.trans.name, srcBone.name)).First();

                    boneIndexMatchDictionary.Add(i, dstBone.index);
                }


                // マテリアルリストの更新
                var materialIndexMatchDictionary = new Dictionary<int, int>();

                for (int i = 0; i < skinMesh.sharedMaterials.Length; ++i)
                {
                    Material srcMaterial = skinMesh.sharedMaterials[i];
                    var targetMat = materialList.Select((mat, index) => new { mat, index }).Where(e => e.mat.Equals(srcMaterial));

                    if (!targetMat.Any())
                    {
                        // 同一マテリアルが存在しない場合
                        // マテリアルリストに追加
                        materialList.Add(srcMaterial);

                        // サブメッシュリストに新たなサブメッシュを作成する
                        subMeshList.Add(new List<int>());

                        // Srcマテリアルのインデックスと結合メッシュオブジェクトのマテリアルのインデックスの対応
                        materialIndexMatchDictionary.Add(i, materialList.Count() - 1);

                    }
                    else
                    {
                        // 同一マテリアルが存在する場合
                        materialIndexMatchDictionary.Add(i, targetMat.First().index);
                    }
                }

                // メッシュコピー開始

                int indexOffset = verticesList.Count();

                // 頂点コピー
                foreach (var vertex in srcMesh.vertices)
                {
                    verticesList.Add(vertex);
                }

                // ノーマルのコピー
                foreach (var normal in srcMesh.normals)
                {
                    normalsList.Add(normal);
                }
                // タンジェントのコピー
                foreach (var tangent in srcMesh.tangents)
                {
                    tangentsList.Add(tangent);
                }
                // UV座標のコピー
                foreach (var uv in srcMesh.uv)
                {
                    uvsList.Add(uv);
                }

                // blendShapeのコピー
                //同名のシェイプキーが存在するときにエラーが発生する
#if DEBUGMODE
            Debug.Log("srcMesh.blendShapeCount : " + srcMesh.blendShapeCount);
#endif
                // ソースMeshからシェイプキーデータ取得
                for (int i = 0; i < srcMesh.blendShapeCount; ++i)
                {
#if DEBUGMODE
                Debug.Log($"Shape No.{i} name : {srcMesh.GetBlendShapeName(i)}    ,    FrameCount : {srcMesh.GetBlendShapeFrameCount(i)}");
#endif
                    int vertexNum = srcMesh.vertexCount;

                    Vector3[] deltaVartices = new Vector3[vertexNum];
                    Vector3[] deltaNormals = new Vector3[vertexNum];
                    Vector3[] deltaTangets = new Vector3[vertexNum];

                    srcMesh.GetBlendShapeFrameVertices(i, 0, deltaVartices, deltaNormals, deltaTangets);

                    string blendShapeName = srcMesh.GetBlendShapeName(i);

                    BlendShape blendShape = new BlendShape(blendShapeName, deltaVartices.ToList(), deltaNormals.ToList(), deltaTangets.ToList(), indexOffset); // 元のMesh名も記録する。同名ボーンが存在した時用

                    blendShapesList.Add(blendShape);
                }


                // 三角形インデックスのコピー
                for (int subMeshIndex = 0; subMeshIndex < srcMesh.subMeshCount; ++subMeshIndex)
                {
                    var triangles = srcMesh.GetTriangles(subMeshIndex);
                    foreach (var triangle in triangles)
                    {
                        subMeshList[materialIndexMatchDictionary[subMeshIndex]].Add(triangle + indexOffset);
                    }
                }

                // ボーンウェイトのコピー
                foreach (var boneWeight in srcMesh.boneWeights)
                {
                    BoneWeight weight = new BoneWeight();
                    weight.boneIndex0 = boneIndexMatchDictionary[boneWeight.boneIndex0];
                    weight.boneIndex1 = boneIndexMatchDictionary[boneWeight.boneIndex1];
                    weight.boneIndex2 = boneIndexMatchDictionary[boneWeight.boneIndex2];
                    weight.boneIndex3 = boneIndexMatchDictionary[boneWeight.boneIndex3];

                    weight.weight0 = boneWeight.weight0;
                    weight.weight1 = boneWeight.weight1;
                    weight.weight2 = boneWeight.weight2;
                    weight.weight3 = boneWeight.weight3;

                    boneWeightsList.Add(weight);
                }

                // 参照元のオブジェクトを削除
                GameObject.DestroyImmediate(skinMesh.gameObject);
            }

            // 新たなメッシュを作成
            GameObject generateMeshObject = new GameObject(BakedMeshNaming);    // 元 Body 指定
            generateMeshObject.transform.parent = newObject.transform;
            generateMeshObject.transform.localPosition = Vector3.zero;

            SkinnedMeshRenderer combinedSkinMesh = generateMeshObject.AddComponent<SkinnedMeshRenderer>();
            Mesh combinedMesh = new Mesh();

            combinedMesh.name = objectName;
            combinedSkinMesh.rootBone = rootBone;
            combinedMesh.RecalculateBounds(); // バウンズは再生成

            combinedMesh.SetVertices(verticesList);
            combinedMesh.SetNormals(normalsList);
            combinedMesh.SetTangents(tangentsList);
            combinedMesh.SetUVs(0, uvsList);

#if DEBUGMODE
        Debug.Log($"SubMeshList.Count : {subMeshList.Count()}");
#endif
            combinedMesh.subMeshCount = subMeshList.Count();
            for (int i = 0; i < subMeshList.Count(); ++i)
            {
                combinedMesh.SetTriangles(subMeshList[i].ToArray(), i);
#if DEBUGMODE
            Debug.Log($"SubMeshListNumber  : {i}/{subMeshList[i].Count()}");
#endif
            }



            // BlendShapeの設定、事前処理。

            //delta配列を保持しておきたい。処理後のあとは渡すだけの配列に置き換える

            var processedBlendShapeList = new List<BlendShape>();

            foreach (var blendShape in blendShapesList)
            {
                int offset = blendShape.vertexOffset;
                int vertexSize = blendShape.deltaVerticies.Count();

                List<Vector3> deltaVerticies = new List<Vector3>();
                List<Vector3> deltaNormals = new List<Vector3>();
                List<Vector3> deltaTangents = new List<Vector3>();

                for (int i = 0; i < verticesList.Count(); ++i)
                {
                    if (i >= offset && i < offset + vertexSize)
                    {
                        deltaVerticies.Add(blendShape.deltaVerticies[i - offset]);
                        deltaNormals.Add(blendShape.deltaNormals[i - offset]);
                        deltaTangents.Add(blendShape.deltaTangents[i - offset]);
                    }
                    else
                    {
                        deltaVerticies.Add(Vector3.zero);
                        deltaNormals.Add(Vector3.zero);
                        deltaTangents.Add(Vector3.zero);
                    }
                }

                var newblendshape = blendShape;
                newblendshape.deltaVerticies = deltaVerticies;
                newblendshape.deltaNormals = deltaNormals;
                newblendshape.deltaTangents = deltaTangents;
                processedBlendShapeList.Add(newblendshape);
            }

            // そしてblendShapeListに戻す
            blendShapesList = processedBlendShapeList;
            var renamedBlendShapeList = new List<BlendShape>();
            var processednameList = new List<string>();
            foreach (var blendShape in blendShapesList)  // これならもうforでやっちゃってもいいきがしないでもない
            {

                if (processednameList.Where(val => blendShape.name == val).Any()) break;  // 処理済みネームリストに既に同じ名前があったら処理をスキップ
                processednameList.Add(blendShape.name); // 自分の名前を処理済みに入れておく。

                var sameblendshapelist = blendShapesList.Where(val => val.name == blendShape.name); //自身含む、自身と同じ名前のリストを生成

                if (sameblendshapelist.Count() > 1)
                {       // 同じ名前が１つよりおおい、つまり自身以外にも居た場合
                        // 同名シェイプキー存在時の処理
                    if (optionpopup1_index == 1)
                    { // Modeスイッチ  0:合成 1:別名追加
                      // 別名で追加

                        for (var i = 0; i < sameblendshapelist.Count(); i++)
                        {

                            var newblendshape = sameblendshapelist.ElementAt(i);
                            newblendshape.name = $"{blendShape.name}{(i < 1 ? "" : (i + 1).ToString())}";   // 無印,2,3,4,...,nと続く
                            renamedBlendShapeList.Add(newblendshape);
                        }


                    }
                    else
                    {
                        // 同名で合成追加
                        var newconbineshape = blendShape;// 先頭にあるものに合成する


                        for (var i = 1; i < sameblendshapelist.Count(); i++)
                        {

                            List<Vector3> deltaVerticies = new List<Vector3>();
                            List<Vector3> deltaNormals = new List<Vector3>();
                            List<Vector3> deltaTangents = new List<Vector3>();

                            for (int ii = 0; ii < blendShape.deltaVerticies.Count; ++ii)
                            {
                                deltaVerticies.Add(newconbineshape.deltaVerticies[ii] + sameblendshapelist.ElementAt(i).deltaVerticies[ii]);
                                deltaNormals.Add(newconbineshape.deltaNormals[ii] + sameblendshapelist.ElementAt(i).deltaNormals[ii]);
                                deltaTangents.Add(newconbineshape.deltaTangents[ii] + sameblendshapelist.ElementAt(i).deltaTangents[ii]);
                            }
                            newconbineshape.deltaVerticies = deltaVerticies;
                            newconbineshape.deltaNormals = deltaNormals;
                            newconbineshape.deltaTangents = deltaTangents;

                        }
                        renamedBlendShapeList.Add(newconbineshape);


                    }
                }
                else
                {
                    // 同名のシェイプキーが存在しなかった時の処理
                    renamedBlendShapeList.Add(blendShape);  // 自身をそのまま追加
                }
            }
            blendShapesList = renamedBlendShapeList;    // そして元のListに戻す。

            // ブレンドシェイプの設定
            foreach (var blendShape in blendShapesList)
            {
                //combinedMesh.AddBlendShapeFrame(blendShape.name, 100, deltaVerticies.ToArray(), deltaNormals.ToArray(), deltaTangents.ToArray());
                combinedMesh.AddBlendShapeFrame(blendShape.name, 100, blendShape.deltaVerticies.ToArray(), blendShape.deltaNormals.ToArray(), blendShape.deltaTangents.ToArray());
            }

            combinedMesh.boneWeights = boneWeightsList.ToArray();
            combinedMesh.bindposes = bindPoseList.ToArray();

            combinedSkinMesh.sharedMesh = combinedMesh;
            combinedSkinMesh.sharedMaterials = materialList.ToArray();

            combinedSkinMesh.bones = boneList.ToArray(); // 結合スキンメッシュのボーンリストを設定


            // 複製物への変更内容

            // セーブ
            var filepath = savingfolder + objectName + addfilenammeword + ".asset";
            if (optionpopup2_index == 0)
            {   // 0:上書き保存
                AssetDatabase.CreateAsset(combinedMesh, filepath);

            }
            else
            {  // 1:連番追加保存（判定スキップ）
                AssetDatabase.CreateAsset(combinedMesh, AssetDatabase.GenerateUniqueAssetPath(filepath));
            }

            AssetDatabase.SaveAssets();


        }
    }

}