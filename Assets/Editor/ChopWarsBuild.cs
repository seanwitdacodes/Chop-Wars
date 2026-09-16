#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class ChopWarsBuild
{
    private static string[] EnabledScenes()
    {
        string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length != 6 || !scenes[0].EndsWith("MainMenu.unity"))
            throw new InvalidOperationException("Enable all six game scenes, with MainMenu first, before building.");
        return scenes;
    }

    [MenuItem("Tools/Chop Wars/Build macOS")]
    public static void BuildMac()
    {
        string[] scenes = EnabledScenes();
        Directory.CreateDirectory("Builds");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Chop Wars.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        });
        File.WriteAllText("Logs/ChopWarsBuild.txt",
            $"Unity {Application.unityVersion}\nResult: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\n");
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("macOS build failed. See Logs/ChopWarsBuild.txt and Unity Console.");
        Debug.Log("Chop Wars macOS build succeeded: Builds/Chop Wars.app");
    }

    [MenuItem("Tools/Chop Wars/Build Web")]
    public static void BuildWeb()
    {
        string[] scenes = EnabledScenes();
        const string output = "Builds/Web";
        Directory.CreateDirectory(output);
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/ChopWarsWebBuild.txt",
            $"Unity {Application.unityVersion}\nResult: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\n");
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("Web build failed. See Logs/ChopWarsWebBuild.txt and Unity Console.");
        PolishWebShell(output);
        Debug.Log("Chop Wars Web build succeeded: " + output);
    }

    private static void PolishWebShell(string output)
    {
        string cssPath = Path.Combine(output, "TemplateData", "style.css");
        File.AppendAllText(cssPath, @"

html, body { width: 100%; height: 100%; overflow: hidden; background: radial-gradient(circle at top, #3f2413 0%, #170b06 72%); }
#unity-container.unity-desktop { width: min(100vw, calc(100vh * 1.6)); height: min(100vh, calc(100vw / 1.6)); }
#unity-canvas { width: 100% !important; height: 100% !important; display: block; box-shadow: 0 18px 70px rgba(0,0,0,.55); }
#unity-footer { display: none !important; }
");
    }
}
#endif
