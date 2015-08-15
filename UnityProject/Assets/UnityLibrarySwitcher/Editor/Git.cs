using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityLibrarySwitcher
{
    public class Git
    {
        public static List<string> GetListOfBranches()
        {
            var result = MonoBasher.ExecuteCommand("git branch --no-color");
            if (result.ExitCode != 0)
            {
                throw new Exception("Couldn't get the list of branches with error message: " + result.StandartError);
            }
            return result.StandartOutputLines.Select(x => x.Substring(2).TrimEnd('\n', '\r')).ToList();
        }

        public static string GetCurrentBranch()
        {
            var result = MonoBasher.ExecuteCommand("git rev-parse --abbrev-ref HEAD");
            if (result.ExitCode != 0)
            {
                throw new Exception("Couldn't get the name of current branch with error message: " + result.StandartError);
            }
            return result.StandartOutput;
        }

        public static void SwitchBranchTo(string branchTo)
        {
            var result = MonoBasher.ExecuteCommand("git checkout " + branchTo);
            if (result.ExitCode != 0)
            {
                throw new System.Exception(string.Format("Couldn't switch to branch {0}. Message:\n{1}", branchTo, result.StandartError));
            }
        }
    }
}
