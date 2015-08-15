using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace UnityLibrarySwitcher
{
    public class LibrarySwitcherWindow : EditorWindow
    {
        private static LibrarySwitcher _model
        {
            get {
                if (m_model == null)
                {
                    m_model = new LibrarySwitcher();
                    m_model.UpdateListOfBranches();
                    m_model.UpdateCurrentBranchName();
                    m_model.UpdateCachedLibrariesInfo();
                }
                return m_model;
            }
        }
        private static LibrarySwitcher m_model;

        [MenuItem ("Window/Library Switcher")]
        private static void ShowWindow ()
        {
            var window = EditorWindow.GetWindow(typeof(LibrarySwitcherWindow));
            window.titleContent.text = "Library Switcher";
        }

        private void OnGUI()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Current target:", _model.CurrentBuildTarget.ToString());
            EditorGUILayout.LabelField("Current branch:", _model.CurrentBranchName);

            EditorGUILayout.Separator();

            var target = (BuildTarget) EditorGUILayout.EnumPopup("Select target:", _model.CurrentBuildTarget);
            _model.SwitchTargetTo(target);

            var branchIndex = EditorGUILayout.Popup("Select branch:", _model.Branches.IndexOf(_model.CurrentBranchName), _model.Branches.ToArray());
            _model.SwitchBranchTo(_model.Branches[branchIndex]);

            OnGUICachedLibraries();
        }

        private void OnGUICachedLibraries()
        {
            var widths = new float[] { 0.40f, 0.34f, 0.18f }.Select(x => x * Screen.width).ToArray();
            if (_model.CachedLibraries.Count > 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Branch", EditorStyles.boldLabel, GUILayout.Width(widths[0]));
                GUILayout.Label("Target", EditorStyles.boldLabel, GUILayout.Width(widths[1]));
                GUILayout.Label("Size", EditorStyles.boldLabel, GUILayout.Width(widths[2]));
                GUILayout.EndHorizontal();
            }

            foreach (var kvp in _model.CachedLibraries)
            {
                EditorGUILayout.Separator();
                foreach (var v in kvp.Value)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(v.Branch, GUILayout.Width(widths[0]));
                    GUILayout.Label(v.Target.ToString(), GUILayout.Width(widths[1]));
                    GUILayout.Label(v.Size == 0 ? "unknown size" : BytesToString(v.Size),
                                    GUILayout.Width(widths[2]));
                    GUILayout.EndHorizontal();
                }
            }
        }


        #region Size pretty printing

        static string[] sizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" }; //Longs run out around EB

        public static String BytesToString(long byteCount)
        {
            if (byteCount == 0)
                return "0" + sizeSuffixes[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + sizeSuffixes[place];
        }

        #endregion

    }
}