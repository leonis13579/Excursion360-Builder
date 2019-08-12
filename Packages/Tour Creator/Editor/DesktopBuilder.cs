using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;

public class DesktopBuilder
{
    private const string BUILD_PATH = "Builds/Desktop/tour.exe";

    public static void Build()
    {
        if (!EditorSceneManager.EnsureUntitledSceneHasBeenSaved("Scene must be saved before creating build"))
            return;

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = new[] { EditorSceneManager.GetActiveScene().path },
            locationPathName = BUILD_PATH,
            target = BuildTarget.StandaloneWindows,
            options = BuildOptions.ShowBuiltPlayer
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
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
