using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace UnityLibrarySwitcher
{
    public class LibrarySwitcher : EditorWindow
    {
        private static string LibraryCacheDirectory = "LibrarySwitcherCache";
        private string CurrentBranchName;
        private List<string> Branches;
        private BuildTarget CurrentBuildTarget;

        [MenuItem ("Window/Library Switcher")]
        private static void ShowWindow ()
        {
            var window = EditorWindow.GetWindow(typeof(LibrarySwitcher));
            window.titleContent.text = "Library Switcher";
        }

        private void OnEnable()
        {
            var branchesResult = MonoBasher.ExecuteCommand("git branch --no-color");
            if (branchesResult.ExitCode != 0)
            {
                Debug.LogError(branchesResult.StandartError);
                return;
            }
            Branches = branchesResult.StandartOutputLines.Select(x => x.Substring(2)).ToList();

            var currentBranchResult = MonoBasher.ExecuteCommand("git rev-parse --abbrev-ref HEAD");
            if (currentBranchResult.ExitCode != 0)
            {
                Debug.LogError(currentBranchResult.StandartError);
                return;
            }
            CurrentBranchName = currentBranchResult.StandartOutput;

            if (!Directory.Exists(LibraryCacheDirectory))
            {
                Debug.LogFormat("Creating cache directory at '{0}'.", LibraryCacheDirectory);
                Directory.CreateDirectory(LibraryCacheDirectory);
            }
        }

        private void OnGUI()
        {
            var currentTarget = EditorUserBuildSettings.activeBuildTarget;

            EditorGUILayout.LabelField("Current target:", currentTarget.ToString());
            EditorGUILayout.LabelField("Current branch:", CurrentBranchName);
            EditorGUILayout.Separator();

            var target = (BuildTarget) EditorGUILayout.EnumPopup("Select target:", currentTarget);
            if (target != currentTarget)
            {
                OnTargetChange(currentTarget, target);
            }

            var currentBranchIndex = Branches.IndexOf(CurrentBranchName);
            var branchIndex = EditorGUILayout.Popup("Select branch:", currentBranchIndex, Branches.ToArray());
            if (branchIndex != currentBranchIndex)
            {
                OnBranchChange(Branches[branchIndex]);
            }
        }

        private void OnTargetChange(BuildTarget targetFrom, BuildTarget targetTo)
        {
            Debug.LogFormat("{0} => {1}", targetFrom.ToString(), targetTo.ToString());

            var libraryPathFrom = LibraryPath(CurrentBranchName, targetFrom);
            var libraryPathTo = LibraryPath(CurrentBranchName, targetTo);
            SwitchLibrary(libraryPathFrom, libraryPathTo);
            EditorUserBuildSettings.SwitchActiveBuildTarget(targetTo);
        }

        private void OnBranchChange(string branch)
        {
            Debug.LogFormat("{0} => {1}", CurrentBranchName, branch);

            var result = MonoBasher.ExecuteCommand("git checkout " + branch);
            if (result.ExitCode != 0)
            {
                throw new System.Exception(string.Format("Couldn't switch to branch {0}. Message:\n{1}", branch, result.StandartError));
            }

            // if (!Directory.Exists(BranchPath(branch)))
            // {
                // Directory.CreateDirectory(BranchPath(branch));
            // }

            var branchPath = BranchPath(CurrentBranchName);
            if (!Directory.Exists(branchPath))
            {
                Directory.CreateDirectory(branchPath);
            }

            var target = EditorUserBuildSettings.activeBuildTarget;
            var libraryPathFrom = LibraryPath(CurrentBranchName, target);
            var libraryPathTo = LibraryPath(branch, target);
            SwitchLibrary(libraryPathFrom, libraryPathTo);
            CurrentBranchName = branch;
        }

        private static void SwitchLibrary(string pathFrom, string pathTo)
        {
            if (Directory.Exists(pathTo))
            {
                FileUtil.MoveFileOrDirectory("Library", pathFrom);
                FileUtil.MoveFileOrDirectory(pathTo, "Library");
            }
            else
            {
                FileUtil.ReplaceDirectory("Library", pathFrom);
            }
        }

        private static string BranchPath(string branch)
        {
            return string.Format("{0}/{1}",
                                 LibraryCacheDirectory,
                                 branch.Replace('/', '_'));
        }

        private static string LibraryPath(string branch, BuildTarget target)
        {
            return string.Format("{0}/{1}",
                                 BranchPath(branch),
                                 target.ToString());
        }

    }

}