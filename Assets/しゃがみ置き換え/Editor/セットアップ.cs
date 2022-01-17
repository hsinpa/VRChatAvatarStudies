using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Animations;
using System;

using VRC.SDK3.Avatars.ScriptableObjects;

namespace しゃがみ置き換え
{
    public class セットアップ : EditorWindow
    {
        // しゃがみ メニュー名
        string MENU_NAME_CROUCH = "しゃがみ置換";
        // しゃがみ メニュー
        AssetLoader MENU_CROUCH = new AssetLoader("358431ce821d9f04c95702b142cf8ee4", "_しゃがみ置き換えメニュー", "Menu");
        // しゃがみ アイコン
        AssetLoader MENU_ICON_CROUCH = new AssetLoader("8e75aca10d1b79742b58466ea659a0a5", "_しゃがみサブメニュー", "Icons");

        // 立ち メニュー名
        string MENU_NAME_STAND = "立ち置換";
        // 立ち メニュー
        AssetLoader MENU_STAND = new AssetLoader("2e00ae8ba36ce0049b6f2783e342e388", "_立ち置き換えメニュー", "Menu");
        // 立ち アイコン
        AssetLoader MENU_ICON_STAND = new AssetLoader("129b00c7938e29a46add836a5b25ffa8", "_立ちサブメニュー", "Icons");

        // 伏せ メニュー名
        string MENU_NAME_PRONE = "伏せ置換";
        // 伏せ メニュー
        AssetLoader MENU_PRONE = new AssetLoader("079a9c141e0a791468b3cb4f5d489cda", "_伏せ置き換えメニュー", "Menu");
        // 伏せ アイコン
        AssetLoader MENU_ICON_PRONE = new AssetLoader("b89f48036bf739d498a300fa38d1d4af", "_伏せサブメニュー", "Icons");

        // 仮のBaseコントローラー
        AssetLoader TEMPORARY_CONTROLLER = new AssetLoader("96934f7c7934d1a429b888d1b5c0cec0", "Locomotion (仮)", "Controller");
        // 仮のExpressionパラメーター
        AssetLoader TEMPORARY_PARAMETER = new AssetLoader("7b177f546296c064cb2aca151d036fef", "パラメーター (仮)", "Menu");
        // 仮のExpressionメニュー
        AssetLoader TEMPORARY_MENU = new AssetLoader("ca8cf63e1ae3fa24fa1734163e7db831", "メニュー (仮)", "Menu");
        // 項目をまとめるサブメニュー
        AssetLoader TEMPORARY_MENU_SUB = new AssetLoader("6297d18e6824fcf43a380cfb5a694ae1", "サブメニュー", "Menu");

        // BlendTree しゃがみ
        AssetLoader TEMPLATE_BLENDTREE_CROUCH = new AssetLoader("fdcdd0ce1af2df34588066dd5b7deb83", "_しゃがみ置き換えBlendTree", "Animations");
        // BlendTree 立ち
        AssetLoader TEMPLATE_BLENDTREE_STAND = new AssetLoader("3389e14c7d0c7304880266d7ce75281d", "_立ち置き換えBlendTree", "Animations");
        // BlendTree 伏せ
        AssetLoader TEMPLATE_BLENDTREE_PRONE = new AssetLoader("4c2893d3e0fffa347a1c16ee7a6d6955", "_伏せ置き換えBlendTree", "Animations");

        // しゃがみ置き換えに使うパラメータ名
        string PARAMETER_CROUCH_NAME = "CrouchReplace";
        // 立ち置き換えに使うパラメータ名
        string PARAMETER_STAND_NAME = "StandReplace";
        // 伏せ置き換えに使うパラメータ名
        string PARAMETER_PRONE_NAME = "ProneReplace";

        // デフォルトしゃがみのパス
        string DEFAULT_ANIM_CROUCH = "Assets/VRCSDK/Examples3/Animation/ProxyAnim/proxy_crouch_still.anim";
        // デフォルト立ちのパス
        string DEFAULT_ANIM_STAND = "Assets/VRCSDK/Examples3/Animation/ProxyAnim/proxy_stand_still.anim";
        // デフォルト伏せのパス
        string DEFAULT_ANIM_PRONE = "Assets/VRCSDK/Examples3/Animation/ProxyAnim/proxy_low_crawl_still.anim";

        // アバター一覧を格納
        List<GameObject> avatar_obs = new List<GameObject>();
        // 選択中のアバター番号
        int selected_avatar_index = 0;

        // 立ち置き換えの有効
        bool enable_stand_replace = true;
        // 伏せ置き換えの有効
        bool enable_prone_replace = true;

        // メニュー追加モード
        int submenu_mode = 1;

        // アニメ遷移時間モード
        int duration_mode = 0;

        // 詳細の表示をするか
        bool show_details = true;

        // Auto-Footstepsをオフにするか
        bool disable_auto_footsteps = true;

        // スクロール位置
        Vector2 scroll = Vector2.zero;

        // メニューへの項目の追加とウィンドウ作成
        [MenuItem("しゃがみ置き換え/セットアップ")]
        private static void Create()
        {
            // ウィンドウ生成
            var window = GetWindow<セットアップ>("しゃがみ置き換え セットアップ");
            window.position = new Rect(0, 0, 500, 850);
        }

        // GUI描画
        private void OnGUI()
        {
            try
            {
                Draw();

            }
            catch (Exception e)
            {
                GUILayout.Label("セットアップGUIを表示する際にエラーが発生しました。");
                GUILayout.TextField(e.ToString());
            }
        }
        void Draw()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            // Toggleなどのラベルが狭くなってしまうのを防止
            float label_width = 180;
            EditorGUIUtility.labelWidth = label_width;

            // アバター一覧を取得
            avatar_obs.Clear();
            foreach (GameObject ob in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var comp in ob.GetComponents<Component>())
                {
                    try
                    {
                        if (ob.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>() != null)
                        {
                            avatar_obs.Add(ob);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            // アバター無いじゃん
            if (avatar_obs.Count == 0)
            {
                GUILayout.Label("シーン内にアバターが見つかりません。アバターをアップロードできる状態にしてからこのUIを開いてください。");
                return;
            }
            // アバター選択
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("アバターを指定してください：", GUILayout.Width(label_width));
                selected_avatar_index = EditorGUILayout.Popup(selected_avatar_index, avatar_obs.Select(x => x.name).ToArray());
                EditorGUILayout.EndHorizontal();
            }

            // 選択中のアバター情報
            var avatar_ob = avatar_obs[selected_avatar_index];
            var vrc_comp = avatar_ob.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            // すでにセットアップが実行された後かどうか
            bool is_setuped = false;

            GUILayout.Label("");

            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("「しゃがみ置き換え」を有効にする", true);
                EditorGUI.EndDisabledGroup();
                
                enable_stand_replace = EditorGUILayout.Toggle("「立ち置き換え」を有効にする", enable_stand_replace);
                
                enable_prone_replace = EditorGUILayout.Toggle("「伏せ置き換え」を有効にする", enable_prone_replace);

                GUILayout.Label("");

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Exメニュー項目追加モード：", GUILayout.Width(label_width));
                submenu_mode = EditorGUILayout.Popup(submenu_mode, new string[] { "直下に項目を追加 (1～3つ圧迫)", "サブメニューにまとめる (1つ圧迫)" });
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            GUILayout.Label("");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("立ち⇔伏せなどのアニメ遷移時間：", GUILayout.Width(label_width));
            duration_mode = EditorGUILayout.Popup(duration_mode, new string[] { "変更しない", "デフォルト (0.5)", "速い (0.25)", "高速 (0.1)", "一瞬 (0.0)" });
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("");

            disable_auto_footsteps = EditorGUILayout.Toggle("「Auto-Footstep」をオフにする", disable_auto_footsteps);
            EditorGUILayout.HelpBox("チェックを入れたままにすることを推奨。足の向きがアニメーション通りになります。", MessageType.Info);

            GUILayout.Label("");

            // 詳細を表示
            {
                //show_details = EditorGUILayout.Toggle("詳細を表示", show_details);
                if (show_details)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);

                    // Baseコントローラー関係
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        GUILayout.Label("Base (Locomotion) コントローラー");

                        var base_layer = vrc_comp.baseAnimationLayers[0];
                        List<string> info_text = new List<string>();
                        AnimatorController target_controller = null;

                        // コントローラーがすでに設定されている
                        if (vrc_comp.customizeAnimationLayers && base_layer.animatorController != null && !base_layer.isDefault)
                        {
                            info_text.Add("○ すでにBaseコントローラーが割り当てられています。");

                            AnimatorControllerLayer target_layer = null;
                            var controller = base_layer.animatorController as AnimatorController;
                            foreach (var layer in controller.layers)
                            {
                                if (layer.name == "Locomotion")
                                {
                                    target_layer = layer;
                                    break;
                                }
                            }

                            if (target_layer != null)
                            {
                                info_text.Add("○ BaseコントローラーにLocomotionレイヤーが見つかりました。");

                                BlendTree target_tree = null;
                                foreach (var state in target_layer.stateMachine.states)
                                {
                                    if (state.state.name == "Crouching" && state.state.motion is BlendTree)
                                    {
                                        target_tree = state.state.motion as BlendTree;

                                        is_setuped = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target_tree.children[0].motion)) == TEMPLATE_BLENDTREE_CROUCH.guid;
                                    }
                                }

                                if (target_tree != null)
                                {
                                    info_text.Add("○ LocomotionレイヤーにCrouchingステートが見つかりました。");
                                    info_text.Add("ここにしゃがみ置き換え用Blend Treeを割り当てます。");
                                    EditorGUILayout.HelpBox(string.Join("\n", info_text), MessageType.Info);
                                }
                                else
                                {
                                    info_text.Add("✕ Crouchingステートが見つかりません。");
                                    info_text.Add("このスクリプトではセットアップできません、中止します。");
                                    EditorGUILayout.HelpBox(string.Join("\n", info_text), MessageType.Error);
                                    return;
                                }
                            }
                            else
                            {
                                info_text.Add("✕ Locomotionレイヤーが見つかりません。");
                                info_text.Add("このスクリプトではセットアップできません、中止します。");
                                EditorGUILayout.HelpBox(string.Join("\n", info_text), MessageType.Error);
                                return;
                            }

                            target_controller = base_layer.animatorController as AnimatorController;
                        }
                        // コントローラーがまだ設定されていない
                        else
                        {
                            info_text.Add("△ Playable LayersにBaseが設定されていません。");
                            info_text.Add("仮のコントローラーを割り当てます。");
                            EditorGUILayout.HelpBox(string.Join("\n", info_text), MessageType.Info);

                            target_controller = TEMPORARY_CONTROLLER.Load<AnimatorController>();
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("使用Baseコントローラー：", GUILayout.Width(label_width));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(target_controller, typeof(AnimatorController), false);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("BlendTree しゃがみ：", GUILayout.Width(label_width));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(TEMPLATE_BLENDTREE_CROUCH.Load<BlendTree>(), typeof(BlendTree), false);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();

                        if (enable_stand_replace)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("BlendTree 立ち：", GUILayout.Width(label_width));
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(TEMPLATE_BLENDTREE_STAND.Load<BlendTree>(), typeof(BlendTree), false);
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }

                        if (enable_prone_replace)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("BlendTree 伏せ：", GUILayout.Width(label_width));
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(TEMPLATE_BLENDTREE_PRONE.Load<BlendTree>(), typeof(BlendTree), false);
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.Label("");

                    // Expressionパラメーター関係
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        GUILayout.Label("Expressionパラメーター");

                        List<string> info_text = new List<string>();

                        VRCExpressionParameters parameters = null;
                        if (vrc_comp.customExpressions && vrc_comp.expressionParameters != null)
                        {
                            info_text.Add("○ すでにExpressionパラメーターが割り当てられています。");
                            info_text.Add("ここにパラメーター項目を追加します。");
                            EditorGUILayout.HelpBox(string.Join("\n", info_text), MessageType.Info);

                            parameters = vrc_comp.expressionParameters;
                        }
                        else
                        {
                            info_text.Add("△ Expressionパラメーターが設定されていません。");
                            info_text.Add("仮のExpressionパラメーターを割り当てます。");
                            EditorGUILayout.HelpBox(string.Join("\n", info_text), MessageType.Info);

                            parameters = TEMPORARY_PARAMETER.Load<VRCExpressionParameters>();
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("使用Expressionパラメーター：", GUILayout.Width(label_width));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(parameters, typeof(VRCExpressionParameters), false);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("パラメーター名 しゃがみ：", GUILayout.Width(label_width));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.TextArea(PARAMETER_CROUCH_NAME);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();

                        if (enable_stand_replace)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("パラメーター名 立ち：", GUILayout.Width(label_width));
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.TextArea(PARAMETER_STAND_NAME);
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }

                        if (enable_prone_replace)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("パラメーター名 伏せ：", GUILayout.Width(label_width));
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.TextArea(PARAMETER_PRONE_NAME);
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.Label("");

                    // Expressionメニュー関係
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        GUILayout.Label("Expressionメニュー");

                        List<string> info_text = new List<string>();

                        VRCExpressionsMenu menu = null;
                        if (vrc_comp.customExpressions && vrc_comp.expressionsMenu != null)
                        {
                            info_text.Add("○ すでにExpressionメニューが割り当てられています。");
                            info_text.Add("ここにサブメニューを追加します。");
                            EditorGUILayout.HelpBox(string.Join("\n", info_text), MessageType.Info);

                            menu = vrc_comp.expressionsMenu;
                        }
                        else
                        {
                            info_text.Add("△ Expressionメニューが設定されていません。");
                            info_text.Add("仮のExpressionメニューを割り当てます。");
                            EditorGUILayout.HelpBox(string.Join("\n", info_text), MessageType.Info);

                            menu = TEMPORARY_MENU.Load<VRCExpressionsMenu>();
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("使用Expressionメニュー：", GUILayout.Width(label_width));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(menu, typeof(VRCExpressionsMenu), false);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("サブメニュー しゃがみ：", GUILayout.Width(label_width));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(MENU_CROUCH.Load<VRCExpressionsMenu>(), typeof(VRCExpressionsMenu), false);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();

                        if (enable_stand_replace)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("サブメニュー 立ち：", GUILayout.Width(label_width));
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(MENU_STAND.Load<VRCExpressionsMenu>(), typeof(VRCExpressionsMenu), false);
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }

                        if (enable_prone_replace)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("サブメニュー 伏せ：", GUILayout.Width(label_width));
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(MENU_PRONE.Load<VRCExpressionsMenu>(), typeof(VRCExpressionsMenu), false);
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            GUILayout.Label("");

            // 実行ボタン
            {
                EditorGUILayout.BeginHorizontal();

                string main_button_text = "セットアップ実行";
                if (is_setuped)
                {
                    main_button_text = "セットアップ更新";
                }

                // セットアップ実行/更新
                if (GUILayout.Button(main_button_text))
                {
                    try
                    {
                        Main();
                    }
                    catch (Exception e)
                    {
                        if (EditorUtility.DisplayDialog("エラーが発生しました。", e.ToString(), "クリップボードにコピー", "閉じる"))
                        {
                            EditorGUIUtility.systemCopyBuffer = e.ToString();
                        }
                    }
                }

                // セットアップ解除
                if (is_setuped)
                {
                    if (GUILayout.Button("セットアップ解除"))
                    {
                        try
                        {
                            Clear();
                        }
                        catch (Exception e)
                        {
                            if (EditorUtility.DisplayDialog("エラーが発生しました。", e.ToString(), "クリップボードにコピー", "閉じる"))
                            {
                                EditorGUIUtility.systemCopyBuffer = e.ToString();
                            }
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // メイン処理
        public void Main()
        {
            Clear();

            // 選択中のアバター情報
            var avatar_ob = avatar_obs[selected_avatar_index];
            var vrc_comp = avatar_ob.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            // コントローラー関係
            {
                var base_layer = vrc_comp.baseAnimationLayers[0];
                // コントローラーがすでに設定されている
                if (vrc_comp.customizeAnimationLayers && base_layer.animatorController != null && !base_layer.isDefault)
                {

                }
                // コントローラーがまだ設定されていない
                else
                {
                    vrc_comp.customizeAnimationLayers = true;
                    base_layer.isDefault = false;
                    base_layer.animatorController = TEMPORARY_CONTROLLER.Load<AnimatorController>();
                    vrc_comp.baseAnimationLayers[0] = base_layer;
                }

                var controller = base_layer.animatorController as AnimatorController;

                // 二重BlendTree構築開始
                foreach (var layer in controller.layers)
                {
                    if (layer.name != "Locomotion")
                    {
                        continue;
                    }
                    // ステートループ
                    foreach (var state in layer.stateMachine.states)
                    {
                        // 関数：BlendTree割り当て
                        void AddBlendTree(bool enable, ChildAnimatorState target, string state_name, AssetLoader template, string default_anim_path)
                        {
                            if (enable)
                            {
                                if (target.state.name == state_name && target.state.motion is BlendTree)
                                {
                                    var blend_tree = target.state.motion as BlendTree;
                                    for (int i = 0; i < blend_tree.children.Length; i++)
                                    {
                                        var children = blend_tree.children;
                                        if (blend_tree.children[i].position.magnitude < 0.01)
                                        {
                                            // 元のアニメを確保
                                            var pre_motion = children[i].motion;
                                            if (pre_motion == null)
                                            {
                                                pre_motion = AssetDatabase.LoadAssetAtPath<Motion>(default_anim_path);
                                            }

                                            var new_blendtree = template.Load<BlendTree>();
                                            children[i].motion = new_blendtree;
                                            blend_tree.children = children;

                                            // できるだけ元のアニメを再現する
                                            var sub_children = new_blendtree.children;
                                            sub_children[0].motion = pre_motion;
                                            new_blendtree.children = sub_children;
                                        }
                                    }
                                }
                            }
                        }

                        // しゃがみ
                        AddBlendTree(true, state, "Crouching", TEMPLATE_BLENDTREE_CROUCH, DEFAULT_ANIM_CROUCH);
                        // 立ち
                        AddBlendTree(enable_stand_replace, state, "Standing", TEMPLATE_BLENDTREE_STAND, DEFAULT_ANIM_STAND);
                        // 伏せ
                        AddBlendTree(enable_prone_replace, state, "Prone", TEMPLATE_BLENDTREE_PRONE, DEFAULT_ANIM_PRONE);

                        // ディレイ変更
                        if ((new string[] { "Crouching", "Standing", "Prone" }).Contains(state.state.name))
                        {
                            foreach (var transition in state.state.transitions)
                            {
                                if ((new string[] { "Crouching", "Standing", "Prone" }).Contains(transition.destinationState.name))
                                {
                                    if (duration_mode == 1)
                                    {
                                        transition.duration = 0.5f;
                                    }
                                    else if (duration_mode == 2)
                                    {
                                        transition.duration = 0.25f;
                                    }
                                    else if (duration_mode == 3)
                                    {
                                        transition.duration = 0.1f;
                                    }
                                    else if (duration_mode == 4)
                                    {
                                        transition.duration = 0.0f;
                                    }
                                }
                            }
                        }
                    }
                }

                // 関数：コントローラー内にパラメータがなければ追加
                void AddControllerParam(bool enable, AnimatorController target, string name)
                {
                    if (enable)
                    {
                        if (!target.parameters.Select(x => x.name).Contains(name))
                        {
                            AnimatorControllerParameter new_param = new AnimatorControllerParameter();
                            new_param.name = name;
                            new_param.type = AnimatorControllerParameterType.Float;
                            new_param.defaultFloat = 0.0f;

                            target.parameters = target.parameters.Append(new_param).ToArray();
                        }
                    }
                }

                // しゃがみ
                AddControllerParam(true, controller, PARAMETER_CROUCH_NAME);
                // 立ち
                AddControllerParam(enable_stand_replace, controller, PARAMETER_STAND_NAME);
                // 伏せ
                AddControllerParam(enable_prone_replace, controller, PARAMETER_PRONE_NAME);
            }

            // Expressionパラメーター関係
            {
                // パラメーターを取得、もしくは仮のを割り当て
                VRCExpressionParameters parameters = null;
                if (vrc_comp.customExpressions && vrc_comp.expressionParameters != null)
                {
                    parameters = vrc_comp.expressionParameters;
                }
                else
                {
                    vrc_comp.customExpressions = true;
                    parameters = TEMPORARY_PARAMETER.Load<VRCExpressionParameters>();
                    vrc_comp.expressionParameters = parameters;
                }

                // 関数：パラメーターに項目追加
                void AddPram(bool enable, VRCExpressionParameters target, string name)
                {
                    if (enable && !target.parameters.Select(x => x.name).Contains(name))
                    {
                        VRCExpressionParameters.Parameter param = new VRCExpressionParameters.Parameter();
                        param.name = name;
                        param.valueType = VRCExpressionParameters.ValueType.Float;
                        param.defaultValue = 0.0f;
                        param.saved = true;
                        target.parameters = target.parameters.Append(param).ToArray();
                    }
                }

                // しゃがみ
                AddPram(true, parameters, PARAMETER_CROUCH_NAME);
                // 立ち
                AddPram(enable_stand_replace, parameters, PARAMETER_STAND_NAME);
                // 伏せ
                AddPram(enable_prone_replace, parameters, PARAMETER_PRONE_NAME);

                // 変更保存
                EditorUtility.SetDirty(parameters);
            }

            // Expressionメニュー関係
            {
                // パラメーターを取得、もしくは仮のを割り当て
                VRCExpressionsMenu menu = null;
                if (vrc_comp.customExpressions && vrc_comp.expressionsMenu != null)
                {
                    menu = vrc_comp.expressionsMenu;
                }
                else
                {
                    vrc_comp.customExpressions = true;
                    menu = TEMPORARY_MENU.Load<VRCExpressionsMenu>();
                    vrc_comp.expressionsMenu = menu;
                }

                // サブメニューにまとめる場合
                if (submenu_mode == 1)
                {
                    var new_control = new VRCExpressionsMenu.Control();
                    new_control.name = MENU_NAME_CROUCH;
                    new_control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                    new_control.subMenu = TEMPORARY_MENU_SUB.Load<VRCExpressionsMenu>();
                    new_control.icon = MENU_ICON_CROUCH.Load<Texture2D>();
                    menu.controls = menu.controls.Append(new_control).ToList();

                    // 変更保存
                    EditorUtility.SetDirty(menu);

                    menu = TEMPORARY_MENU_SUB.Load<VRCExpressionsMenu>();
                }

                // 関数：メニューに項目追加
                void AddMenu(bool enable, VRCExpressionsMenu target, AssetLoader sub_menu, string name, AssetLoader icon)
                {
                    if (enable)
                    {
                        var new_control = new VRCExpressionsMenu.Control();
                        new_control.name = name;
                        new_control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                        new_control.subMenu = sub_menu.Load<VRCExpressionsMenu>();
                        new_control.icon = icon.Load<Texture2D>();
                        target.controls = target.controls.Append(new_control).ToList();
                    }
                }

                // 立ち
                AddMenu(enable_stand_replace, menu, MENU_STAND, MENU_NAME_STAND, MENU_ICON_STAND);
                // しゃがみ
                AddMenu(true, menu, MENU_CROUCH, MENU_NAME_CROUCH, MENU_ICON_CROUCH);
                // 伏せ
                AddMenu(enable_prone_replace, menu, MENU_PRONE, MENU_NAME_PRONE, MENU_ICON_PRONE);

                // 変更保存
                EditorUtility.SetDirty(menu);
            }

            // Auto-Footstepsの処理
            if (disable_auto_footsteps)
            {
                vrc_comp.autoFootsteps = false;
            }

            AssetDatabase.Refresh();
        }

        void Clear()
        {
            // 選択中のアバター情報
            var avatar_ob = avatar_obs[selected_avatar_index];
            var vrc_comp = avatar_ob.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            // コントローラー関係
            {
                var base_layer = vrc_comp.baseAnimationLayers[0];
                // コントローラーがすでに設定されている
                if (vrc_comp.customizeAnimationLayers && base_layer.animatorController != null && !base_layer.isDefault)
                {
                    var controller = base_layer.animatorController as AnimatorController;
                    // レイヤーループ

                    foreach (var layer in controller.layers)
                    {
                        if (layer.name != "Locomotion")
                        {
                            continue;
                        }

                        // ステートループ
                        foreach (var state in layer.stateMachine.states)
                        {
                            // 関数：ステートと名前がマッチしたら初期化
                            void RemoveBlendTree(ChildAnimatorState current_state, string match_name, AssetLoader template, string default_anim_path)
                            {
                                string GetAssetGUID(UnityEngine.Object ob)
                                {
                                    return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(ob));
                                }

                                if (current_state.state.name == match_name && current_state.state.motion is BlendTree)
                                {
                                    var blend_tree = current_state.state.motion as BlendTree;
                                    var children = blend_tree.children;
                                    for (int i = 0; i < children.Length; i++)
                                    {
                                        if (GetAssetGUID(children[i].motion) == template.guid)
                                        {
                                            children[i].motion = template.Load<BlendTree>().children[0].motion;
                                            if (children[i].motion == null)
                                            {
                                                children[i].motion = AssetDatabase.LoadAssetAtPath<Motion>(default_anim_path);
                                            }
                                            blend_tree.children = children;
                                        }
                                    }
                                }
                            }

                            // しゃがみ
                            RemoveBlendTree(state, "Crouching", TEMPLATE_BLENDTREE_CROUCH, DEFAULT_ANIM_CROUCH);
                            // 立ち
                            RemoveBlendTree(state, "Standing", TEMPLATE_BLENDTREE_STAND, DEFAULT_ANIM_STAND);
                            // 伏せ
                            RemoveBlendTree(state, "Prone", TEMPLATE_BLENDTREE_PRONE, DEFAULT_ANIM_PRONE);
                        }
                    }

                    // コントローラー内のパラメーターを初期化
                    controller.parameters = controller.parameters.Where(x => x.name != PARAMETER_CROUCH_NAME).ToArray();
                    controller.parameters = controller.parameters.Where(x => x.name != PARAMETER_STAND_NAME).ToArray();
                    controller.parameters = controller.parameters.Where(x => x.name != PARAMETER_PRONE_NAME).ToArray();
                }
            }

            // Expressionパラメーター関係
            {
                // 関数：パラメーターからこのエディタ拡張で追加した要素を削除
                void RemovePram(VRCExpressionParameters prams)
                {
                    prams.parameters = prams.parameters.Where(x => x.name != PARAMETER_CROUCH_NAME).ToArray();
                    prams.parameters = prams.parameters.Where(x => x.name != PARAMETER_STAND_NAME).ToArray();
                    prams.parameters = prams.parameters.Where(x => x.name != PARAMETER_PRONE_NAME).ToArray();

                    // 変更保存
                    EditorUtility.SetDirty(prams);
                }

                // 現在割り当てられているパラメーターをリセット
                if (vrc_comp.customExpressions && vrc_comp.expressionParameters != null)
                {
                    RemovePram(vrc_comp.expressionParameters);
                }
                // 念のため、仮割り当てパラメーターもリセット
                RemovePram(TEMPORARY_PARAMETER.Load<VRCExpressionParameters>());
            }

            // Expressionメニュー関係
            {
                // 関数：メニューからこのエディタ拡張で追加した要素を削除
                void RemoveMenu(VRCExpressionsMenu menu)
                {
                    string GetAssetGUID(UnityEngine.Object ob)
                    {
                        return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(ob));
                    }

                    menu.controls = menu.controls.Where(x => GetAssetGUID(x.subMenu) != TEMPORARY_MENU_SUB.guid).ToList();

                    menu.controls = menu.controls.Where(x => GetAssetGUID(x.subMenu) != MENU_CROUCH.guid).ToList();
                    menu.controls = menu.controls.Where(x => GetAssetGUID(x.subMenu) != MENU_STAND.guid).ToList();
                    menu.controls = menu.controls.Where(x => GetAssetGUID(x.subMenu) != MENU_PRONE.guid).ToList();

                    menu.controls = menu.controls.Where(x => !(x.type == VRCExpressionsMenu.Control.ControlType.SubMenu && x.subMenu == null && x.name.Contains("置換"))).ToList();

                    // 変更保存
                    EditorUtility.SetDirty(menu);
                }

                // 現在割り当てられているメニューをリセット
                if (vrc_comp.customExpressions && vrc_comp.expressionsMenu != null)
                {
                    RemoveMenu(vrc_comp.expressionsMenu);
                }
                // 念のため、仮割り当てメニューもリセット
                RemoveMenu(TEMPORARY_MENU.Load<VRCExpressionsMenu>());
                RemoveMenu(TEMPORARY_MENU_SUB.Load<VRCExpressionsMenu>());
            }
        }
    }
}
