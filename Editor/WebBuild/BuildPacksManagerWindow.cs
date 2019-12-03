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


        private Vector2 packsScrollPosition;
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
            if (GUILayout.Button("Скачать последнюю версию просмотрщика"))
            {
                StartBackgroundTask(DownloadViewer(packsLocation));
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Текущие версии web просмотрщиков", EditorStyles.boldLabel);
            if (GUILayout.Button("Обновить"))
            {
                FindBuildPacks();
            }

            GUILayout.EndHorizontal();
            if (buildPacks.Count == 0)
            {
                GUILayout.Label("Текущие версии web просмотрщиков отcутствуют", EditorStyles.label);
            }
            else
            {
                packsScrollPosition = GUILayout.BeginScrollView(packsScrollPosition);
                foreach (var pack in buildPacks)
                {
                    pack.IsFolded = EditorGUILayout.Foldout(pack.IsFolded, pack.Version);
                    if (pack.IsFolded)
                    {
                        EditorGUI.indentLevel++;
                        switch (pack.Status)
                        {
                            case BuildPackStatus.NotLoaded:
                                StartBackgroundTask(DownloadReleaseInfo(pack.Id, p =>
                                {
                                    pack.PublishDate = p.PublishedAt;
                                    pack.Status = BuildPackStatus.Loaded;
                                }));
                                pack.Status = BuildPackStatus.Loading;
                                break;
                            case BuildPackStatus.Loading:
                                EditorGUILayout.LabelField($"Загрузка...");
                                break;
                            case BuildPackStatus.Loaded:
                                EditorGUILayout.LabelField($"Опубликовано: {pack.PublishDate.ToString("yyyy-MM-dd")}");
                                break;
                            case BuildPackStatus.LoadingError:
                                EditorGUILayout.LabelField($"Ошибка при загрузке");
                                break;
                            default:
                                EditorGUILayout.LabelField($"Непредвиденная ошибка");
                                break;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                GUILayout.EndScrollView();
            }
        }

        private void FindBuildPacks()
        {
            buildPacks = Directory.GetFiles(packsLocation, "web-viewer-*-*.zip")
                .Select(fName => Regex.Match(fName, @"web-viewer-(?<tag>\S+)-(?<id>\d+).zip"))
                .Select(match => new BuildPack
                {
                    Id = int.Parse(match.Groups["id"].Value),
                    Version = match.Groups["tag"].Value
                })
                .ToList();
        }

        private IEnumerator DownloadReleaseInfo(int releaseId, Action<ReleaseResponse> done)
            => DownloadReleaseInfo(releaseId.ToString(), done);
        private IEnumerator DownloadReleaseInfo(string releaseId, Action<ReleaseResponse> done)
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
                        EditorUtility.DisplayProgressBar("Downloading", $"Получение информации о релизе {releaseId}", w.downloadProgress);
                    }
                    row = w.downloadHandler.text;
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
                var downloadingTask = DownloadReleaseInfo("latest", r => parsed = r);
                while (downloadingTask.MoveNext())
                {
                    yield return downloadingTask.Current;
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
