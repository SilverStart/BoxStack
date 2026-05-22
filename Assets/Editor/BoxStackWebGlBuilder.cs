using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BoxStackWebGlBuilder
{
    private const string DefaultOutputPath = "webgl";

    public static void Build()
    {
        string outputPath = GetArgument("-outputPath") ?? DefaultOutputPath;
        bool development = HasArgument("-development");

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No enabled scenes found in EditorBuildSettings.");
        }

        Directory.CreateDirectory(outputPath);

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        BuildPlayerOptions buildOptions = new()
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = development ? BuildOptions.Development : BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        Console.WriteLine($"BoxStack WebGL build result: {summary.result}");
        Console.WriteLine($"Output path: {Path.GetFullPath(outputPath)}");
        Console.WriteLine($"Total size: {summary.totalSize}");
        Console.WriteLine($"Total time: {summary.totalTime}");

        if (summary.result != BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }

    private static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static bool HasArgument(string name)
    {
        return Environment.GetCommandLineArgs()
            .Any(arg => string.Equals(arg, name, StringComparison.OrdinalIgnoreCase));
    }
}
