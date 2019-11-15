using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Packages.tour_creator.Editor.WebBuild;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.WSA;
using UnityScript.Steps;

namespace Packages.tour_creator.Editor
{
    public class WebBuilder
    {
        public static void Build(string targetFolder)
        {
            try
            {
                StartBackgroundTask<string>(DownloadViewer(targetFolder));
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static IEnumerator DownloadViewer(string folderPath)
        {
            try
            {
                string row;
                using (UnityWebRequest w = UnityWebRequest.Get("https://api.github.com/repos/ITLabRTUMIREA/Excursion360-Web/releases/latest"))
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
                    var archiveStream = new MemoryStream(w.downloadHandler.data);
                    using var zipInputStream = new ZipInputStream(archiveStream);
                    while (zipInputStream.GetNextEntry() is ZipEntry zipEntry)
                    {
                        var entryFileName = zipEntry.Name;
                        var buffer = new byte[4096];

                        // Manipulate the output filename here as desired.
                        var fullZipToPath = Path.Combine(folderPath, entryFileName);
                        var directoryName = Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);

                        // Skip directory entry
                        if (Path.GetFileName(fullZipToPath).Length == 0)
                        {
                            continue;
                        }

                        // Unzip file in buffered chunks. This is just as fast as unpacking
                        // to a buffer the full size of the file, but does not waste memory.
                        // The "using" will close the stream even if an exception occurs.
                        using FileStream streamWriter = File.Create(fullZipToPath);
                        StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                    }
                }

            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

        }

        public static void StartBackgroundTask<T>(IEnumerator update, Action<T> end = null, Action<Exception> err = null)
        {
            EditorApplication.CallbackFunction closureCallback = null;

            closureCallback = () =>
            {
                try
                {
                    if (update.MoveNext() == false)
                    {
                        end?.Invoke((T)update.Current);
                        EditorApplication.update -= closureCallback;
                    }
                }
                catch (Exception ex)
                {
                    err?.Invoke(ex);
                    Debug.LogException(ex);
                    EditorApplication.update -= closureCallback;
                }
            };

            EditorApplication.update += closureCallback;
        }
    }
}
