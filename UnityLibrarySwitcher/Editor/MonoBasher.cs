using System.Collections.Generic;
using System.Linq;

namespace UnityLibrarySwitcher
{
    public class MonoBasher
    {
        private static string ShellPath = "/bin/bash";

        public static Result ExecuteCommand(string command)
        {
            return ExecuteCommand(command, ShellPath);
        }

        public static Result ExecuteCommand(string command, string shell)
        {
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = ShellPath;
            proc.StartInfo.Arguments = "-c \" " + command + " \"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            var result = new Result();
            result.StandartOutput = proc.StandardOutput.ReadToEnd().TrimEnd('\n', '\r');
            result.StandartError = proc.StandardError.ReadToEnd().TrimEnd('\n', '\r');
            proc.WaitForExit();
            result.ExitCode = proc.ExitCode;
            return result;
        }

        public class Result
        {
            public int ExitCode;
            public string StandartOutput;
            public string StandartError;

            public List<string> StandartOutputLines
            {
                get
                {
                    return Lines(StandartOutput);
                }
            }

            public List<string> StandartErrorLines
            {
                get
                {
                    return Lines(StandartError);
                }
            }

            public void Perform(System.Action<Result> onOutput, System.Action<Result> onError)
            {
                if (ExitCode == 0)
                    onOutput(this);
                else
                    onError(this);
            }

            public override string ToString()
            {
                return string.Format("ExitCode({0}), output({1}), error({2})", ExitCode.ToString(), StandartOutput ?? "null", StandartError ?? "null");
            }

            private static List<string> Lines(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return new List<string>();
                }
                return value.Split('\n').ToList();
            }
        }

    }
}