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
            } else
            {
                foreach (var pack in buildPacks)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(pack.Version);
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void FindBuildPacks()
        {
            buildPacks = Directory.GetFiles(packsLocation, "web-viewer-*.zip")
                .Select(p => new BuildPack
                {
                    Version = p
                })
                .ToList();
        }

        private static IEnumerator DownloadViewer(string folderPath)
        {
            try
            {
                string row;
                using (UnityWebRequest w = UnityWebRequest.Get("https://api.github.com/repos/RTUITLab/Excursion360-Web/releases/latest"))
                {
                    w.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    yield return w.SendWebRequest();

                    while (w.isDone == false)
                    {
                        yield return null;
                        EditorUtility.DisplayProgressBar("Downloading", "Downloading meta", w.downloadProgress);
                    }
                    row = w.downloadHandler.text;
                }
                var parsed = JsonUtility.FromJson<ReleaseResponse>(row);
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
                    File.WriteAllBytes(Path.Combine(folderPath, $"web-viewer-{parsed.tag_name}.zip"), w.downloadHandler.data);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
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
