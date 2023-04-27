using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JUtils.Console
{
    [CustomEditor(typeof(Console))]
    public class ConsoleInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
                base.OnInspectorGUI();
            else
                EditorGUILayout.HelpBox("Changing console values from inspector at runtime is not supported.", MessageType.Info);
        }
    }

}
