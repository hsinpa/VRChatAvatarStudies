using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Hsinpa.TV;

namespace Hsinpa.TV.Editor {
    [CustomEditor(typeof(TelevisionManager))]

    public class TelevisionManagerEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TelevisionManager myTarget = (TelevisionManager)target;
            if (GUILayout.Button("Generate TV set"))
            {
                myTarget.GenerateTelevisionSet();
            }
        }
    }
}