using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hsinpa.Mesh.Editor {
    [CustomEditor(typeof(SkinMeshBoneCombiner))]

    public class SkinMeshBoneCombinerEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SkinMeshBoneCombiner myTarget = (SkinMeshBoneCombiner)target;
            if (GUILayout.Button("Smark bone bind"))
            {
                myTarget.SmartSetSkinBone();
            }
        }
    }
}