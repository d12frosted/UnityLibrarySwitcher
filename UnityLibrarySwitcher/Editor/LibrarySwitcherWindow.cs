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
            var widths = new float[] { 0.42f, 0.26f, 0.24f }.Select(x => x * Screen.width).ToArray();
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
                    GUILayout.Label("unknown size", GUILayout.Width(widths[2]));
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

        #region Size calculation

        public static long DirSize(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                throw new Exception("Couldn't calculate directory size. Path doesn't exist: " + dirPath);
            return DirSize(new DirectoryInfo(dirPath));
        }

        public static long DirSize(DirectoryInfo d)
        {
            long Size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += DirSize(di);
            }
            return(Size);
        }

        /*
        public static long DirSize(string sourceDir, bool recurse)
        {
            long size = 0;
            string[] fileEntries = Directory.GetFiles(sourceDir);

            foreach (string fileName in fileEntries)
            {
                Interlocked.Add(ref size, (new FileInfo(fileName)).Length);
            }

            if (recurse)
            {
                string[] subdirEntries = Directory.GetDirectories(sourceDir);

                Parallel.For<long>(0, subdirEntries.Length, () => 0, (i, loop, subtotal) =>
                        {
                            if ((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                            {
                                subtotal += DirSize(subdirEntries[i], true);
                                return subtotal;
                            }
                            return 0;
                        },
                                   (x) => Interlocked.Add(ref size, x)
                                   );
            }
            return size;
        }
        */

        #endregion
    }
}