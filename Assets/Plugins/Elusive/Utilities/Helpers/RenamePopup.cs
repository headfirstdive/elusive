#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Elusive.Utilities
{
    public class RenamePopup : EditorWindow
    {
        private string objectName;
        private static Action<string> acceptedCallback;

        public static void Init(Action<string> callback)
        {
            acceptedCallback = callback;
            RenamePopup window = ScriptableObject.CreateInstance<RenamePopup>();
            window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 250, 75);
            window.ShowPopup();
            window.Focus();
        }


        private void OnGUI()
        {
            EditorGUILayout.LabelField("Name your animation.", EditorStyles.centeredGreyMiniLabel);

            GUI.SetNextControlName("renameField");
            objectName = EditorGUILayout.TextField(objectName);
            GUI.FocusControl("renameField");

            if (GUILayout.Button("Accept")) Accept();
        }


        private void Accept()
        {
            acceptedCallback(objectName);
            this.Close();
        }
    }
}
#endif