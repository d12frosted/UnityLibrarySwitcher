using UnityEditor;

namespace UnityLibrarySwitcher
{
    public class CachedLibraryInfo
    {
        public string Branch;
        public BuildTarget Target;
        public long Size;

        public CachedLibraryInfo(string branch, BuildTarget target)
        {
            Branch = branch;
            Target = target;
        }
    }
}