/*
    AvatarTools VerEx.0.5.0
    BindPoseTransformer

    本スクリプト(BindPoseTransformer)は空々こん(twitter: @kuukuukon)が作成したものです。
    MIT Licence です。

    https://kuukuukon.booth.pm/items/2014610
    AvatarTools VerExの一部として同梱しています。
*/


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

    public class K2Ex_BindPoseTransformer : EditorWindow
    {


        // -- var For GUI =========================================================================================
        #region VarForGUI
        // General

        // Window Config
        static Vector2 windowSizeMin = new Vector2(380, 615);
        static Vector2 windowSizeMax = new Vector2(600, 850);

        // Gui制御用(非保存)
        //Vector2 scrollPosition = new Vector2(0,0);

        SkinnedMeshRenderer selectedTargetMesh;     // 処理用とは別に持つ。
        int selectedTargetBoneIndex = 0;
        List<string> popupBoneList;
        Transform selectedRepObject;
        string savingfilename = "";
        string originalMeshName = "";

        bool isLockByTransEdit = false;


        // オプション(保存)
        bool optHelpShow_BPT = false;
        bool optHelpShow_Save = false;
        bool optHelpShow_Rep = false;
        bool optRep_PoseUpdate = false;
        string savingfolder = "Assets/";
        bool optAddNameBPT = true;
        bool optAddNameMesh = true;
        bool optAddNameNumber = true;
        bool optDiffName = true;
        bool optAutoSave = true;
        bool optSaveCheck = true;

        // オプション（非保存)


        #endregion



        // === Event ================================================================================================
        #region Event
        [MenuItem("AvatarTools/K2Ex_BindPoseTransformer")]
        static void Open()
        {

            var window = EditorWindow.GetWindow<K2Ex_BindPoseTransformer>();

            // ウィンドウ設定
            window.minSize = windowSizeMin;
            window.maxSize = windowSizeMax;

            window.titleContent = new GUIContent(nameof(K2Ex_BindPoseTransformer));

            window.Setup();
        }


        // ウィンドウを閉じたときの終了処理
        void OnDestroy()
        {
            // オブジェクトを出してたら削除
            if(backupSkinnedMesh) UnityEngine.Object.DestroyImmediate(backupSkinnedMesh);   // バックアップオブジェクトの削除
            if(poseTransformer || isLockByTransEdit) PoseTransformEditCancel();

            // 設定情報をセーブする
            EditorPrefsSave();
        }

        // Updateのようなもの。頻度が少ない？
        void OnInspectorUpdate()
        {
            if(isLockByTransEdit) {
                PoseTransformEditing(); // Pose変形編集中の更新処理
                Repaint();    // 変形処理中のみ描画更新する
            }
        }


        #endregion

        // === Funtion for GUI ===================================================================================
        #region FuntionForGUI
        public void Setup()
        {
            // その他初期化処理(Setupが実行されない状態で始まったとき用に分離)
            Init();
        }

        private void Init()
        {
            // Editor設定情報読み込み
            EditorPrefsLoad();

            // 言語関係読み込み
            //MultiLanguageInit();


            //isInit = true;  // フラグ立て
        }

        private void EditorPrefsSave()
        {
            EditorPrefs.SetBool("optHelpShow_BPT", optHelpShow_BPT);
            EditorPrefs.SetBool("optHelpShow_Save", optHelpShow_Save);
            EditorPrefs.SetBool("optHelpShow_Rep", optHelpShow_Rep);
            EditorPrefs.SetBool("optRep_PoseUpdate", optRep_PoseUpdate);

            EditorPrefs.SetString("savingfolder", savingfolder);
            EditorPrefs.SetBool("optAddNameBPT", optAddNameBPT);
            EditorPrefs.SetBool("optAddNameMesh", optAddNameMesh);
            EditorPrefs.SetBool("optAddNameNumber", optAddNameNumber);
            EditorPrefs.SetBool("optDiffName", optDiffName);
            EditorPrefs.SetBool("optAutoSave", optAutoSave);
            EditorPrefs.SetBool("optSaveCheck", optSaveCheck);
        }

        private void EditorPrefsLoad()
        {
            optHelpShow_BPT = EditorPrefs.GetBool("optHelpShow_BPT", optHelpShow_BPT);
            optHelpShow_Save = EditorPrefs.GetBool("optHelpShow_Save", optHelpShow_Save);
            optHelpShow_Rep = EditorPrefs.GetBool("optHelpShow_Rep", optHelpShow_Rep);
            optRep_PoseUpdate = EditorPrefs.GetBool("optRep_PoseUpdate", optRep_PoseUpdate);

            savingfolder = EditorPrefs.GetString("savingfolder", savingfolder);
            optAddNameBPT = EditorPrefs.GetBool("optAddNameBPT", optAddNameBPT);
            optAddNameMesh = EditorPrefs.GetBool("optAddNameMesh", optAddNameMesh);
            optAddNameNumber = EditorPrefs.GetBool("optAddNameNumber", optAddNameNumber);
            optDiffName = EditorPrefs.GetBool("optDiffName", optDiffName);
            optAutoSave = EditorPrefs.GetBool("optAutoSave", optAutoSave);
            optSaveCheck = EditorPrefs.GetBool("optSaveCheck", optSaveCheck);
        }

        // GUI状態から保存Assetのフルパスを取得する
        private string GetFullPath()
        {
            var originalpath = savingfolder + originalMeshName + ".asset";

            var fullpath = savingfolder;
            if(optAddNameBPT) fullpath += "BPT_";
            if(optAddNameMesh && selectedTargetMesh) fullpath += selectedTargetMesh.gameObject.name;
            if(optAddNameMesh && savingfilename.Length>0) fullpath += "_";
            fullpath += savingfilename + ".asset";
            if( optAddNameNumber || (optDiffName && fullpath == originalpath) ) fullpath = AssetDatabase.GenerateUniqueAssetPath(fullpath);
            return fullpath;
        }


        #endregion



        // === GUI ================================================================================================
        #region GUI


        private void OnGUI()
        {


            EditorGUILayout.LabelField("　対象選択　", (GUIStyle)"AppToolbar");
            if(!isLockByTransEdit) using ( new GUILayout.VerticalScope("GroupBox") )
            {
                EditorGUILayout.LabelField("本ツールは、対象Skinnedメッシュの形状をボーン単位で編集します。");
                EditorGUILayout.Space();

                using ( new GUILayout.HorizontalScope() )
                {
                    bool isSet = false , isRemove = false;
                    var beforeMesh = selectedTargetMesh;
                    selectedTargetMesh = EditorGUILayout.ObjectField("対象にしたいSkinnedMesh", selectedTargetMesh,  typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
                    isSet = selectedTargetMesh!=null && selectedTargetMesh != beforeMesh;   // 以前と別の有効なものになった。セットされた。（セットからセットへ、があり得る）
                    isRemove = selectedTargetMesh==null && beforeMesh!=null; // 有効からNullにかわった。解除された。

                    //処理
                    if(isSet)
                    {
                        originalMeshName = selectedTargetMesh.sharedMesh.name;
                        popupBoneList =  TargetMeshIsSet(selectedTargetMesh, out selectedTargetBoneIndex);
                    }

                    if(isRemove)
                    {
                        TargetMeshIsRemove();
                        popupBoneList = null;
                    }
                }
                using ( new GUILayout.HorizontalScope() )
                {
                    EditorGUILayout.PrefixLabel("対象にしたいBone");
                    if(selectedTargetMesh!=null && popupBoneList!=null)
                    {
                        var iCheck = selectedTargetBoneIndex;
                        selectedTargetBoneIndex = EditorGUILayout.Popup(selectedTargetBoneIndex, popupBoneList.ToArray());
                        if(selectedTargetBoneIndex != iCheck) selectedTargetBoneIndex = SelectedBoneIndex(selectedTargetBoneIndex);
                    }
                    else
                        EditorGUILayout.Popup(0, new string[] {"","先にMeshを選択してください"});
                }
                using ( new GUILayout.HorizontalScope() )
                {
                    EditorGUILayout.LabelField("");
                    if( GUILayout.Button("セット時の状態にリセット", GUILayout.ExpandWidth(true)) )
                    {
                        ResetSkinnedMesh();
                        popupBoneList =  TargetMeshIsSet(selectedTargetMesh, out selectedTargetBoneIndex,false);    // ボーン一覧が変更されている可能性があるので更新
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("操作ロック中");
            }

            EditorGUILayout.LabelField("　BindPose変形　", (GUIStyle)"AppToolbar");
            using ( new GUILayout.VerticalScope("GroupBox") )
            {
                optHelpShow_BPT = EditorGUILayout.Foldout(optHelpShow_BPT,"対象メッシュの対象ボーンの内部的なゼロ位置を変更します。", true);
                if(optHelpShow_BPT)
                {
                    EditorGUILayout.LabelField("編集開始ボタンを押すと出現し自動選択されるオブジェクト");
                    EditorGUILayout.LabelField("\"PoseTransformer\"を自由に移動回転拡縮をして変形させてください");
                }
                using ( new GUILayout.HorizontalScope() )
                {
                    if(!isLockByTransEdit)
                    {
                        if(GUILayout.Button("編集開始")) PoseTransformEditStart();
                    }
                    else
                    {
                        if(GUILayout.Button("完了"))
                        {
                            PoseTransformEditEnd();
                            if(optAutoSave) SaveAsset(GetFullPath(), selectedTargetMesh);
                        }
                        if(GUILayout.Button("キャンセル")) PoseTransformEditCancel();
                    }

                }
                if(isLockByTransEdit)
                {
                }
            }

            EditorGUILayout.LabelField("　Bone置換　", (GUIStyle)"AppToolbar");
            if(!isLockByTransEdit) using ( new GUILayout.VerticalScope("GroupBox") )
            {
                optHelpShow_Rep = EditorGUILayout.Foldout(optHelpShow_Rep, "対象ボーンの役割を別のGameObjectに置換えします。", true);
                if(optHelpShow_Rep)
                {
                    EditorGUILayout.LabelField("置換につかうGameObjectは、別のBoneを指定するか、");
                    EditorGUILayout.LabelField("空のオブジェクトを作成する等して指定してください。");
                    EditorGUILayout.LabelField("BindPoseの更新がOFFの場合、");
                    EditorGUILayout.LabelField("元の対象Boneとの位置のズレに応じて変形します。");
                }

                EditorGUILayout.Space();

                selectedRepObject = EditorGUILayout.ObjectField("置換に使用するBoneやObject", selectedRepObject,  typeof(Transform), true) as Transform;
                optRep_PoseUpdate = EditorGUILayout.ToggleLeft("BindPoseを更新する(元のメッシュの位置を保持する)", optRep_PoseUpdate);
                if(GUILayout.Button("置換する"))
                {
                    RepBone(selectedRepObject, optRep_PoseUpdate);

                    // 置換後の処理
                    popupBoneList =  TargetMeshIsSet(selectedTargetMesh, out selectedTargetBoneIndex,false);   // ボーン配列が変わっているはずなので変更する
                    if(optAutoSave && optRep_PoseUpdate) SaveAsset(GetFullPath(), selectedTargetMesh);   // オートセーブ ボーンポーズを更新しないときはセーブ不要
                }

            }
            else
            {
                EditorGUILayout.LabelField("操作ロック中");
            }

            /*
            EditorGUILayout.LabelField("　クリーンアップ　", (GUIStyle)"AppToolbar");
            if(!isLockByTransEdit) using ( new GUILayout.VerticalScope("GroupBox") )
            {

            }
            else
            {
                EditorGUILayout.LabelField("操作ロック中");
            }
            */



            EditorGUILayout.LabelField("　保存　", (GUIStyle)"AppToolbar");
            if(!isLockByTransEdit) using ( new GUILayout.VerticalScope("GroupBox") )
            {
                optHelpShow_Save = EditorGUILayout.Foldout(optHelpShow_Save, "Meshファイルの変更をAssetとして保存します。", true);
                if(optHelpShow_Save)
                {
                    EditorGUILayout.LabelField("VRChatなどで使用する場合、保存する必要があります。");
                    EditorGUILayout.LabelField("セット時のオリジナル名や他と同じ名になる場合、自動で別名になります。");
                    EditorGUILayout.LabelField("　※各種、同名回避の設定をOFFにする場合は注意してください。");
                    EditorGUILayout.LabelField("　自動上書きされ、同一のファイルを使用する他メッシュに影響がでます。");
                    EditorGUILayout.LabelField("　※ボーン置換は、BindPoseの更新をしない限りはセーブは不要です");
                    EditorGUILayout.LabelField("　（ボーン置換はメッシュの変更ではなくコンポーネントの設定変更のため）");
                }

                EditorGUILayout.Space();

                using ( new GUILayout.HorizontalScope() )
                {
                    EditorGUILayout.LabelField($"保存するフォルダ名：{savingfolder}");
                    if (GUILayout.Button("フォルダ選択"))   //, GUILayout.Width(20), GUILayout.Height(14)))
                    {
                        var path = EditorUtility.OpenFolderPanel("保存するフォルダ選択", savingfolder, "");
                        if (string.IsNullOrEmpty(path))
                        {
                        }
                        else
                        {
                            savingfolder = path;
                            var match = Regex.Match(savingfolder, @"Assets/.*");
                            savingfolder = match.Value + "/";
                            if (savingfolder == "/") savingfolder = "Assets/";
                        }
                    }

                }

                using ( new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("保存するファイル名：");//, GUILayout.Width(position.width - 143));
                    savingfilename = EditorGUILayout.TextField(savingfilename, GUILayout.Width(position.width - 280));
                }

                using ( new GUILayout.HorizontalScope() )
                {
                    EditorGUILayout.LabelField("名前に", GUILayout.Width(position.width - 225));
                    optAddNameBPT = EditorGUILayout.ToggleLeft("「BPT_」を追加", optAddNameBPT, GUILayout.Width(90));
                    optAddNameMesh = EditorGUILayout.ToggleLeft("Mesh名を追加", optAddNameMesh, GUILayout.Width(90));
                }
                using ( new GUILayout.HorizontalScope() )
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(position.width - 382));
                    optDiffName = EditorGUILayout.ToggleLeft("オリジナルと同一の名前を避ける", optDiffName, GUILayout.Width(160));
                    optAddNameNumber = EditorGUILayout.ToggleLeft("同名ファイルがあれば連番名を追加", optAddNameNumber, GUILayout.Width(180));
                }
                /*
                using ( new GUILayout.HorizontalScope() )
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(position.width - 225));
                    
                }
                */


                EditorGUILayout.Space();


                EditorGUILayout.LabelField($"出力パス：{GetFullPath()}");

                using ( new GUILayout.HorizontalScope() )
                {
                    if( GUILayout.Button("保存") ) SaveAsset(GetFullPath(), selectedTargetMesh);
                }

                using ( new GUILayout.HorizontalScope() )
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(position.width - 280));
                    optAutoSave = EditorGUILayout.ToggleLeft("Meshの変更がされたときに上記設定で自動保存", optAutoSave);
                }

                using ( new GUILayout.HorizontalScope() )
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(position.width - 280));
                    optSaveCheck = EditorGUILayout.ToggleLeft("保存時に同名ファイルが存在する場合、確認する", optSaveCheck);
                }



            }
            else
            {
                EditorGUILayout.LabelField("操作ロック中");
            }

            EditorGUILayout.LabelField("　" + AvatarAssembler.version);

        }



        #endregion


        // === Var For Process ==========================================================================================
        #region VarForProcess
        // 対象物
        SkinnedMeshRenderer targetMesh;
        List<string> boneList;
        int targetBoneIndex = 0;
        Transform targetBone;

        // 操作オブジェ
        GameObject poseTransformer;

        // 内部処理用
        GameObject backupSkinnedMesh;    // 戻す用
        Mesh originalMesh;  // 戻す用
        Mesh newMesh;
        Matrix4x4[] newBindposes;


        #endregion


        // === Process ==================================================================================================
        #region Process

        // TargetMeshがGUIにセットされた時
        private List<string> TargetMeshIsSet(SkinnedMeshRenderer selTargetMesh, out int selIndex, bool Backup = true)
        {
            // バックアップの準備
            if(Backup)
            {
                if(backupSkinnedMesh) UnityEngine.Object.DestroyImmediate(backupSkinnedMesh);  // すでにあったら削除
                backupSkinnedMesh = new GameObject("BPT_BackupObject Dont Delete me");
                UnityEditorInternal.ComponentUtility.CopyComponent(selTargetMesh);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(backupSkinnedMesh);
                backupSkinnedMesh.SetActive(false);
            }

            targetMesh = selTargetMesh;    // Processのほうの変数へコピー
            // boneListリスト生成処理
            boneList = new List<string>();
            foreach (var bone in targetMesh.bones)
            {
                boneList.Add(bone.gameObject.name);
            }


            targetBoneIndex = SelectedBoneIndex(targetBoneIndex);    // 選択ボーン更新

            selIndex = targetBoneIndex;
            return boneList;
        }

        // TargetMeshがGUIより解除された時
        private void TargetMeshIsRemove()
        {
            if(backupSkinnedMesh) UnityEngine.Object.DestroyImmediate(backupSkinnedMesh);  // バックアップオブジェクトがあったら削除
            targetMesh = null;
            boneList = null;
        }

        // selectedTargetBoneIndexが変更された。あとMeshが変更されたときも呼ぶ
        private int SelectedBoneIndex(int selIndex)
        {
            if(selIndex<0 || targetMesh==null || targetMesh.bones.Length <= selIndex)
            {
                targetBoneIndex = 0;
                targetBone = null;
            }
            else
            {
                targetBoneIndex = selIndex;
                targetBone = targetMesh.bones[targetBoneIndex];
            }
            return targetBoneIndex;
        }

        // SkinnedMeshのResetボタンが押された
        private void ResetSkinnedMesh()
        {
            if(backupSkinnedMesh==null || targetMesh==null)
            {
                // セットされてない又はバックアップが無い状態で押された
                EditorUtility.DisplayDialog("警告", "セットされていない、又はバックアップオブジェクトが有りません。", "OK");
                return;
            }

            UnityEditorInternal.ComponentUtility.CopyComponent(backupSkinnedMesh.GetComponent<SkinnedMeshRenderer>());
            UnityEditorInternal.ComponentUtility.PasteComponentValues(targetMesh);

            // 上のコピペだけでは完全なコピーはされない？手動でスクリプトで一部のデータを移す
            targetMesh.bones = backupSkinnedMesh.GetComponent<SkinnedMeshRenderer>().bones;
        }



        // 変形開始ボタンが押された。
        private void PoseTransformEditStart()
        {
            if(targetMesh == null)
            {
                EditorUtility.DisplayDialog("Error", "対象メッシュが選択されていません", "OK");
                return;
            }
            if(targetBone == null)
            {
                EditorUtility.DisplayDialog("Error", "Bone指定がされていません。", "OK");
                return;
            }

            // 編集用オブジェクトを生成
            poseTransformer = new GameObject("PoseTransformer");
            GameObjectUtility.SetParentAndAlign(poseTransformer, targetBone.gameObject);
            Selection.activeGameObject = poseTransformer;

            // 処理用変数の準備
            newMesh = Instantiate(targetMesh.sharedMesh);
            newBindposes = targetMesh.sharedMesh.bindposes.Clone() as Matrix4x4[];


            // メッシュ一時置き換え
            originalMesh = targetMesh.sharedMesh;
            targetMesh.sharedMesh = newMesh;


            isLockByTransEdit = true;   // 編集中フラグを建てる
        }

        // 編集中に連続して呼び出される
        private void PoseTransformEditing()
        {
            if(!poseTransformer){
                PoseTransformEditEnd();
                return; // 編集オブジェクトが消失したら終了処理
            }

            var mt = Matrix4x4.Rotate(Quaternion.Inverse(targetBone.transform.rotation)) * Matrix4x4.Translate(-targetBone.transform.position) * poseTransformer.transform.localToWorldMatrix;
            newBindposes[targetBoneIndex] = mt * targetBone.transform.worldToLocalMatrix * targetMesh.localToWorldMatrix;
            targetMesh.sharedMesh.bindposes = newBindposes;

        }


        // 変形終了ボタンが押された
        private void PoseTransformEditEnd()
        {
            if(poseTransformer)
            {
                UnityEngine.Object.DestroyImmediate(poseTransformer);   // 削除
            }


            // セーブはここではしない

            // newMesh はすでにSkinnedMeshRendererにセットされている

            isLockByTransEdit = false; // 編集中フラグを下ろす
        }

        private void PoseTransformEditCancel()
        {
            if(poseTransformer)
            {
                UnityEngine.Object.DestroyImmediate(poseTransformer);   // 削除
            }

            // キャンセル処理
            targetMesh.sharedMesh = originalMesh;   // オリジナルを戻す。


            isLockByTransEdit = false; // 編集中フラグを下ろす
        }


        // 置換の実行
        private void RepBone(Transform repobject, bool option_poseupdate)
        {
            if(repobject == null || targetBone == null)
            {
                EditorUtility.DisplayDialog("Error", "Meshが選ばれてない、Boneが選ばれてない、置換に使うオブジェクトが選ばれてない、のどれかです。", "");
                return;
            }

            Transform [] bones = targetMesh.bones.Clone() as Transform[];
            bones[targetBoneIndex] = repobject;
            targetMesh.bones = bones;

            if(option_poseupdate)
            {
                var newMesh = Instantiate(targetMesh.sharedMesh);
                var newBindposes = newMesh.bindposes.Clone() as Matrix4x4[];
                newBindposes[targetBoneIndex] = repobject.worldToLocalMatrix * targetMesh.localToWorldMatrix;
                newMesh.bindposes = newBindposes;
                targetMesh.sharedMesh = newMesh;
            }


        }



        // セーブ処理
        private void SaveAsset(string path, SkinnedMeshRenderer saveSkinnedMesh)
        {
            if(optSaveCheck && path != AssetDatabase.GenerateUniqueAssetPath(path) )
            {
                if(!EditorUtility.DisplayDialog("確認", "同名のファイルが存在します。上書きしてよろしいですか？\n※他に同じファイルを使用するMeshがあった場合、同じ影響が出ます。", "上書き保存", "キャンセル") ) return;
            }
            AssetDatabase.CreateAsset(saveSkinnedMesh.sharedMesh, path);
            AssetDatabase.SaveAssets();
        }



        #endregion

    }

}