#if UNITY_EDITOR
using System;
using System.IO;

using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

class ScriptedBuilds
{
    //Called from command line only
    static void PerformHeadlessBuild()
    {
        // As a fallback use <project root>/BUILD as output path
        string buildPath = Path.Combine(Application.dataPath, "BUILD");
        BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        string buildName = "B2DServer.exe";


        // read in command line arguments e.g. add "-buildPath some/Path" if you want a different output path 
        var args = Environment.GetCommandLineArgs();

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildPath")
            {
                buildPath = args[i + 1];
            }
            else if (args[i] == "-targetLinux64")
            {
                buildTarget = BuildTarget.StandaloneLinux64;
                buildName = "B2DServer.x86_64";
            }
            else if (args[i] == "-targetWin64")
            {
                buildTarget = BuildTarget.StandaloneWindows64;
            }
            else if (args[i] == "-targetWin32")
            {
                buildTarget = BuildTarget.StandaloneWindows;
            }
            else if (args[i] == "-targetOSX")
            {
                buildTarget = BuildTarget.StandaloneOSX;
                buildName = "B2DServerOSX";
            }
        }

        // if the output folder doesn't exist create it now
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        Debug.Log("Buid Path = " + buildPath);

        BuildReport report = BuildPipeline.BuildPlayer(

            // Simply use the scenes from the build settings
            // see https://docs.unity3d.com/ScriptReference/EditorBuildSettings-scenes.html
            EditorBuildSettings.scenes,

            // pass on the output folder
            buildPath + "/" + buildName,

            // Build for Linux 64 bit
            buildTarget,

            // Use Headless mode
            // see https://docs.unity3d.com/ScriptReference/BuildOptions.EnableHeadlessMode.html
            // and make the build fail for any error
            // see https://docs.unity3d.com/ScriptReference/BuildOptions.StrictMode.html
            BuildOptions.StrictMode
        );

        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }
}
#endif