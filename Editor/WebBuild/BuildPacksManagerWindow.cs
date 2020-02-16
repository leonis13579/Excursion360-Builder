using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Packages.tour_creator.Editor.WebBuild;
using Packages.tour_creator.Editor.WebBuild.GitHubAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Packages.Excursion360_Builder.Editor.WebBuild
{
    class BuildPacksManagerWindow : EditorWindow
    {
        private string packsLocation;
        private List<BuildPack> buildPacks = new List<BuildPack>();

        private string[] buildPackTags = Array.Empty<string>();
        private int selectedbuildTagNum = 0;

        private string outFolderPath;


        private void OnEnable()
        {
            packsLocation = Application.dataPath + "/Tour creator";
            if (!Directory.Exists(packsLocation))
            {
                Directory.CreateDirectory(packsLocation);
            }
            FindBuildPacks();
        }

        private void OnGUI()
        {
            DrawInstalledList();
        }

        private void DrawInstalledList()
        {
            if (GUILayout.Button("Download last viewer version"))
            {
                StartBackgroundTask(DownloadViewer(packsLocation));
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Available viewers", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh"))
            {
                FindBuildPacks();
            }

            GUILayout.EndHorizontal();
            if (buildPacks.Count == 0)
            {
                GUILayout.Label("No web viewers", EditorStyles.label);
            }
            else
            {
                foreach (var pack in buildPacks)
                {
                    pack.IsFolded = EditorGUILayout.Foldout(pack.IsFolded, pack.Version);
                    if (pack.IsFolded)
                    {
                        RenderPack(pack);
                    }
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Exporting excursion", EditorStyles.boldLabel);
            selectedbuildTagNum = EditorGUILayout.Popup("Viewer version", selectedbuildTagNum, buildPackTags);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Target path");
            outFolderPath = EditorGUILayout.TextField(outFolderPath);
            if (GUILayout.Button("..."))
            {
                outFolderPath = EditorUtility.OpenFolderPanel("Select folder to export", outFolderPath, "");
                Repaint();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Export"))
            {
                if (!TourExporter.TryGetTargetFolder(outFolderPath))
                {
                    return;
                }
                TourExporter.ExportTour(buildPacks[selectedbuildTagNum].Location, outFolderPath);
            }
        }

        private void RenderPack(BuildPack pack)
        {
            EditorGUI.indentLevel++;
            switch (pack.Status)
            {
                case BuildPackStatus.NotLoaded:
                    StartBackgroundTask(DownloadReleaseInfo(pack.Id, p =>
                    {
                        pack.PublishDate = p.PublishedAt;
                        pack.Status = BuildPackStatus.Loaded;
                    }, e => pack.Status = BuildPackStatus.LoadingError));
                    pack.Status = BuildPackStatus.Loading;
                    break;
                case BuildPackStatus.Loading:
                    EditorGUILayout.LabelField($"Downloading...");
                    break;
                case BuildPackStatus.Loaded:
                    EditorGUILayout.LabelField($"Publish date: {pack.PublishDate:yyyy-MM-dd}");
                    if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), "Remove"))
                    {
                        File.Delete(pack.Location);
                        FindBuildPacks();
                    }
                    break;
                case BuildPackStatus.LoadingError:
                    EditorGUILayout.LabelField($"Download error");
                    break;
                default:
                    EditorGUILayout.LabelField($"Unexpected error");
                    break;
            }
            EditorGUI.indentLevel--;
        }

        private void FindBuildPacks()
        {
            buildPacks = Directory.GetFiles(packsLocation, "web-viewer-*-*.zip")
                .Select(path => (path, match: Regex.Match(path, @"web-viewer-(?<tag>\S+)-(?<id>\d+).zip")))
                .Select(b => new BuildPack
                {
                    Id = int.Parse(b.match.Groups["id"].Value),
                    Version = b.match.Groups["tag"].Value,
                    Location = b.path
                })
                .ToList();
            buildPackTags = buildPacks.Select(p => p.Version).ToArray();
            selectedbuildTagNum = buildPackTags
                .Select((tag, i) => (tag, i))
                .OrderBy(s => s.tag)
                .FirstOrDefault()
                .i;
        }

        private IEnumerator DownloadReleaseInfo(int releaseId, Action<ReleaseResponse> done, Action<string> error)
            => DownloadReleaseInfo(releaseId.ToString(), done, error);
        private IEnumerator DownloadReleaseInfo(string releaseId, 
            Action<ReleaseResponse> done,
            Action<string> error)
        {
            try
            {
                string row;
                using (UnityWebRequest w = UnityWebRequest.Get("https://api.github.com/repos/RTUITLab/Excursion360-Web/releases/" + releaseId))
                {
                    w.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    yield return w.SendWebRequest();

                    while (w.isDone == false)
                    {
                        yield return null;
                        EditorUtility.DisplayProgressBar("Downloading", $"Fetching release information {releaseId}", w.downloadProgress);
                    }
                    row = w.downloadHandler.text;
                    if (w.isHttpError)
                    {
                        error(row);
                    }
                }
                var parsed = JsonUtility.FromJson<ReleaseResponse>(row);
                done(parsed);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private IEnumerator DownloadViewer(string folderPath)
        {
            try
            {
                ReleaseResponse parsed = null;
                string errorMessage = null;
                var downloadingTask = DownloadReleaseInfo(
                    "latest",
                    r => parsed = r,
                    e => errorMessage = e);
                while (downloadingTask.MoveNext())
                {
                    yield return downloadingTask.Current;
                }
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                    yield break;
                }
                var targetLink = parsed.assets.FirstOrDefault(a => a.name == "build.zip");
                if (targetLink == null)
                {
                    EditorUtility.DisplayDialog("Error", "No needed asset in latest release", "Ok");
                    yield break;
                }

                using (UnityWebRequest w = UnityWebRequest.Get(targetLink.browser_download_url))
                {
                    w.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    yield return w.SendWebRequest();

                    while (w.isDone == false)
                    {
                        yield return null;
                        EditorUtility.DisplayProgressBar("Downloading", "Downloading viewer", w.downloadProgress);
                    }
                    File.WriteAllBytes(Path.Combine(folderPath, $"web-viewer-{parsed.tag_name}-{parsed.id}.zip"), w.downloadHandler.data);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                FindBuildPacks();
            }
        }

        public static void StartBackgroundTask(IEnumerator update)
        {
            EditorApplication.CallbackFunction closureCallback = null;

            closureCallback = () =>
            {
                try
                {
                    if (update.MoveNext() == false)
                    {
                        EditorApplication.update -= closureCallback;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    EditorApplication.update -= closureCallback;
                }
            };

            EditorApplication.update += closureCallback;
        }
    }
}
