using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CumLoader
{
    internal static class AssemblyGenerator
    {
        internal static bool HasGeneratedAssembly = false;

        internal static void Check()
        {
            if (!Imports.IsIl2CppGame() || Initialize())
                HasGeneratedAssembly = true;
            else
                CumLoaderBase.UNLOAD(false);
        }

        private static bool Initialize()
        {
            string GeneratorProcessPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Imports.GetGameDirectory(), "CumLoader"), "Dependencies"), "AssemblyGenerator"), "CumLoader.AssemblyGenerator.exe");
            if (!File.Exists(GeneratorProcessPath))
            {
                CumLogger.LogError("CumLoader.AssemblyGenerator.exe does not Exist!");
                return false;
            }
            var generatorProcessInfo = new ProcessStartInfo(GeneratorProcessPath);
            generatorProcessInfo.Arguments = $"\"{CumLoaderBase.UnityVersion}\" {"\"" + Regex.Replace(Imports.GetGameDirectory(), @"(\\+)$", @"$1$1") + "\""} {"\"" + Regex.Replace(Imports.GetGameDataDirectory(), @"(\\+)$", @"$1$1") + "\""} {(Force_Regenerate() ? "true" : "false")} {(string.IsNullOrEmpty(Force_Version_Unhollower()) ? "" : Force_Version_Unhollower())}";
            generatorProcessInfo.UseShellExecute = false;
            generatorProcessInfo.RedirectStandardOutput = true;
            generatorProcessInfo.CreateNoWindow = true;
            Process process = null;
            try { process = Process.Start(generatorProcessInfo); } catch (Exception e) { CumLogger.LogError(e.ToString()); CumLogger.LogError("Unable to Start Assembly Generator!"); return false; }
            var stdout = process.StandardOutput;
            while (!stdout.EndOfStream)
                CumLogger.Log(stdout.ReadLine());
            while (!process.HasExited)
                Thread.Sleep(100);
            if (process.ExitCode != 0)
            {
                CumLogger.Native_ThrowInternalError($"Assembly Generator exited with code {process.ExitCode}");
                return false;
            }
            if (Imports.IsDebugMode())
                CumLogger.Log($"Assembly Generator ran Successfully!");
            return true;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static bool Force_Regenerate();
        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private extern static string Force_Version_Unhollower();
    }
}
