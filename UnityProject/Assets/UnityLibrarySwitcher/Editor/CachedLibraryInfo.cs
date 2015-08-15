using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace UnityLibrarySwitcher
{
    public class CachedLibraryInfo
    {
        public string Branch;
        public BuildTarget Target;
        public string Path;
        public long Size;
        private Action _calculateAsync;

        public CachedLibraryInfo(string branch, BuildTarget target, string path)
        {
            Branch = branch;
            Target = target;
            Path = path;
        }

        public void ResetSize()
        {
            // the place to kill _calculateAsync
        }

        public void CalculateSize()
        {
            _calculateAsync = () => {
                Size = DirSize(Path);
            };
            _calculateAsync.BeginInvoke(_calculateAsync.EndInvoke, null);
        }

        public static long DirSize(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                throw new Exception("Couldn't calculate directory size. Path doesn't exist: " + dirPath);
            return DirSize(new DirectoryInfo(dirPath));
        }

        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return(size);
        }
    }
}