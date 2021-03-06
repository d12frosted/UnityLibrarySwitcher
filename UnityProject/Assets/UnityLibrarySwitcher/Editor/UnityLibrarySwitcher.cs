using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace UnityLibrarySwitcher
{
    public class LibrarySwitcher
    {
        #region Settings

        public static readonly string LibraryCacheDirectory = "LibrarySwitcherCache";

        #endregion

        #region Public fields and properties

        public string CurrentBranchName
        {
            get
            {
                return m_currentBranchName;
            }
        }

        public BuildTarget CurrentBuildTarget
        {
            get
            {
                return EditorUserBuildSettings.activeBuildTarget;
            }
        }

        public List<string> Branches
        {
            get
            {
                return m_branches;
            }
        }

        public Dictionary<string, List<CachedLibraryInfo>> CachedLibraries
        {
            get
            {
                return m_cachedLibraries;
            }
        }

        #endregion

        #region Private fields and properties

        private string m_currentBranchName;
        private List<string> m_branches;
        private Dictionary<string, List<CachedLibraryInfo>> m_cachedLibraries;

        #endregion

        #region Update values

        public void UpdateListOfBranches()
        {
            m_branches = Git.GetListOfBranches();
        }

        public void UpdateCurrentBranchName()
        {
            m_currentBranchName = Git.GetCurrentBranch();
        }

        public void UpdateCachedLibrariesInfo()
        {
            m_cachedLibraries = new Dictionary<string, List<CachedLibraryInfo>>();

            SafeCreateDirectory(LibraryCacheDirectory);

            foreach (var branchInfo in new DirectoryInfo(LibraryCacheDirectory).GetDirectories())
            {
                var list = new List<CachedLibraryInfo>();
                foreach (var targetInfo in branchInfo.GetDirectories())
                {
                    if (!Enum.IsDefined(typeof(BuildTarget), targetInfo.Name))
                    {
                        Debug.LogErrorFormat("Found trash at '{0}'. Consider removing it.", targetInfo);
                    }
                    else
                    {
                        var target = (BuildTarget) Enum.Parse(typeof(BuildTarget), targetInfo.Name);
                        var path = LibraryPath(branchInfo.Name, target);
                        var info = new CachedLibraryInfo(branchInfo.Name, target, path);
                        info.CalculateSize();
                        list.Add(info);
                    }
                }
                m_cachedLibraries.Add(branchInfo.Name, list);
            }
        }

        #endregion

        #region Switch methods

        public void SwitchTargetTo(BuildTarget targetTo)
        {
            if (targetTo == CurrentBuildTarget) return;

            Debug.LogFormat("{0} => {1}", CurrentBuildTarget.ToString(), targetTo.ToString());

            var libraryPathFrom = LibraryPath(CurrentBranchName, CurrentBuildTarget);
            var libraryPathTo = LibraryPath(CurrentBranchName, targetTo);
            SwitchLibrary(libraryPathFrom, libraryPathTo);
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(targetTo))
            {
                Directory.Delete(libraryPathFrom, true);
                UpdateCachedLibrariesInfo();
            }
        }

        public void SwitchBranchTo(string branchTo)
        {
            if (branchTo == CurrentBranchName) return;

            Debug.LogFormat("{0} => {1}", CurrentBranchName, branchTo);

            Git.SwitchBranchTo(branchTo);

            var libraryPathFrom = LibraryPath(CurrentBranchName, CurrentBuildTarget);
            var libraryPathTo = LibraryPath(branchTo, CurrentBuildTarget);
            SwitchLibrary(libraryPathFrom, libraryPathTo);
            m_currentBranchName = branchTo;
        }

        private void SwitchLibrary(string pathFrom, string pathTo)
        {
            SafeCreateDirectory(LibraryCacheDirectory);
            SafeCreateDirectory(BranchPath(CurrentBranchName));

            if (Directory.Exists(pathTo))
            {
                FileUtil.MoveFileOrDirectory("Library", pathFrom);
                FileUtil.MoveFileOrDirectory(pathTo, "Library");
            }
            else
            {
                FileUtil.ReplaceDirectory("Library", pathFrom);
            }
            UpdateCachedLibrariesInfo();
        }

        #endregion

        #region Path helpers

        private static void SafeCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string BranchPath(string branch)
        {
            return string.Format("{0}/{1}",
                                 LibraryCacheDirectory,
                                 branch.Replace('/', '_'));
        }

        private static string LibraryPath(CachedLibraryInfo info)
        {
            return LibraryPath(info.Branch, info.Target);
        }

        private static string LibraryPath(string branch, BuildTarget target)
        {
            return string.Format("{0}/{1}",
                                 BranchPath(branch),
                                 target.ToString());
        }

        #endregion

    }

}