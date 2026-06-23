#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    public static void BuildWindows()
    {
        EnsureTextMeshProResources();
        Directory.CreateDirectory("Build");

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
            locationPathName = Path.Combine("Build", "xiuxian-unity.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        UnityEngine.Debug.Log($"Build completed: result={summary.result}, output={summary.outputPath}, size={summary.totalSize} bytes");

        EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    private static void EnsureTextMeshProResources()
    {
        if (UnityEngine.Resources.Load<TMPro.TMP_Settings>("TMP Settings") != null)
            return;

        string resourcesPackage = Directory
            .GetFiles(Path.Combine("Library", "PackageCache"), "TMP Essential Resources.unitypackage", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (string.IsNullOrEmpty(resourcesPackage) || !File.Exists(resourcesPackage))
            throw new FileNotFoundException("TMP Essential Resources package was not found in Library/PackageCache.");

        UnityEngine.Debug.Log($"Importing TMP Essential Resources from {resourcesPackage}");
        AssetDatabase.ImportPackage(resourcesPackage, false);
        AssetDatabase.Refresh();
    }
}
#endif
