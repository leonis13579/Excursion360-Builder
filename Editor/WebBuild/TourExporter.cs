using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Packages.tour_creator.Editor.WebBuild;

#if UNITY_EDITOR

using UnityEditor;
using Exported = Packages.tour_creator.Editor.Protocol;

public class TourExporter
{
    public static void ExportTour(string viewerLocation, string folderPath)
    {
        try
        {
            UnpackViewer(viewerLocation, folderPath);
            if (!CopyLogo(folderPath, out var logoFileName))
            {
                return;
            }
            CreateConfigFile(folderPath, logoFileName);


            Exported.Tour tour = new Exported.Tour();

            // Find first state
            if (Tour.Instance == null)
            {
                EditorUtility.DisplayDialog("Error", "There is no tour object on this scene!", "Ok");
                return;
            }

            tour.colorSchemes = Tour.Instance.colorSchemes.Select(cs => cs.color).ToArray();

            State firstState = Tour.Instance.firstState;
            if (firstState == null)
            {
                EditorUtility.DisplayDialog("Error", "First state is not selected!", "Ok");
                return;
            }

            // Find all states
            State[] states = GameObject.FindObjectsOfType<State>();
            if (states.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "There is no states on this scene to export!", "Ok");
                return;
            }
            Debug.Log($"Finded {states.Length} states");
            // Pre process states
            UpdateProcess(0, states.Length, "Exporting", "");

            Dictionary<int, string> stateIds = new Dictionary<int, string>();

            for (int i = 0; i < states.Length; ++i)
            {
                var state = states[i];

                TextureSource textureSource = state.GetComponent<TextureSource>();
                if (textureSource == null)
                {
                    EditorUtility.DisplayDialog("Error", "State has no texture source!", "Ok");
                    return;
                }

                Exported.State exportedState = new Exported.State
                {
                    id = "state_" + i,
                    title = state.title,
                    url = $"{Tour.Instance.linkPrefix}{textureSource.Export(folderPath, "state_" + i)}",
                    type = textureSource.GetSourceType().ToString().ToLower(),
                    rotation = state.transform.rotation.eulerAngles,
                    pictureRotation = state.transform.rotation
                };

                stateIds.Add(state.GetInstanceID(), exportedState.id);
                tour.states.Add(exportedState);

                UpdateProcess(i + 1, states.Length, "Exporting", $"{i + 1}/{states.Length}: {state.title}");
            }

            // Assign first state id
            stateIds.TryGetValue(firstState.GetInstanceID(), out tour.firstStateId);

            // Process links
            for (int i = 0; i < states.Length; ++i)
            {
                var state = states[i];
                var exportedState = tour.states[i];

                Debug.Log(state.title);

                Connection[] connections = state.gameObject.GetComponents<Connection>();
                if (connections == null || connections.Length == 0)
                {
                    EditorUtility.DisplayDialog("Warning", $"State {state.title} does not have connections!", "Ok");
                    continue;
                }

                foreach (var connection in connections)
                {
                    Debug.Log($"{connection.origin.title} -- {connection.destination.origin.title}");
                    if (!stateIds.TryGetValue(connection.destination.origin.GetInstanceID(), out string otherId))
                    {
                        EditorUtility.DisplayDialog("Warning", $"State {state.title} linked to {connection.destination.origin.title}, but id not found", "Ok");
                        continue;
                    }

                    Vector3 direction = (connection.orientation * Vector3.forward).normalized;

                    exportedState.links.Add(new Exported.StateLink()
                    {
                        id = otherId,
                        rotation = connection.orientation,
                        colorScheme = connection.colorScheme
                    });
                }
            }

            // Serialize and write
            File.WriteAllText(folderPath + "/tour.json", JsonUtility.ToJson(tour, true));
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Error while exporting tour\n{ex.Message}\n{ex.StackTrace}", "Ok");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
        // Finish
    }

    private static void CreateConfigFile(string folderPath, string logoFileName)
    {
        var configuration = new Configuration
        {
            logoUrl = logoFileName,
            sceneUrl = ""
        };
        var stringConfig = JsonUtility.ToJson(configuration);
        File.WriteAllText(Path.Combine(folderPath, "config.json"), stringConfig);
    }

    private static bool CopyLogo(string folderPath, out string logoPath)
    {
        logoPath = "";
        string path = AssetDatabase.GetAssetPath(Tour.Instance.logoTexture);
        if (string.IsNullOrEmpty(path))
        {
            if (!EditorUtility.DisplayDialog("Внимание", "Вы не указали логотип для перехода между локациями. Уверены, что хотите оставить логотип по умолчанию?", "Да, продолжить", "Отмена"))
            {
                EditorUtility.DisplayDialog("Ошибка", "Операция отменена", "Ок");
                return false;
            }
            logoPath = "";
            return true;
        }
        string filename = "logo" + Path.GetExtension(path);
        Debug.Log(Path.Combine(folderPath, filename));
        File.Copy(path, Path.Combine(folderPath, filename));
        logoPath = filename;
        return true;
    }

    private static void UnpackViewer(string viewerLocation, string folderPath)
    {
        using (var fileStream = File.OpenRead(viewerLocation))
        using (var zipInputStream = new ZipInputStream(fileStream))
        {

            while (zipInputStream.GetNextEntry() is ZipEntry zipEntry)
            {
                var entryFileName = zipEntry.Name;
                var buffer = new byte[4096];

                var fullZipToPath = Path.Combine(folderPath, entryFileName);
                var directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0)
                    Directory.CreateDirectory(directoryName);

                if (Path.GetFileName(fullZipToPath).Length == 0)
                {
                    continue;
                }

                using (FileStream streamWriter = File.Create(fullZipToPath))
                {
                    StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                }
            }
        }
    }


    public static bool TryGetTargetFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog("Ошибка", "Выборанная директория не существует", "Ок");
            return false;
        }
        if (!EditorUtility.DisplayDialog("Внимание", "Все файлы в выбранной папке будут удалены, Вы уверены?", "Да, удалить", "Отмена"))
        {
            EditorUtility.DisplayDialog("Ошибка", "Операция отменена", "Ок");
            return false;
        }

        var files = Directory.GetFiles(folderPath);
        for (int i = 0; i < files.Length; i++)
        {
            var filePath = files[i];
            UpdateProcess(i, files.Length, "Удаление файлов", filePath);
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Ошибка", $"Неполучается удалить файл {filePath}\n{ex.Message}\n{ex.StackTrace}", "Ок");
                return false;
            }
        }

        return true;
    }

    static void UpdateProcess(int current, int target, string title, string message)
    {
        EditorUtility.DisplayProgressBar(title, message, current / (float)target);
    }
}

#endif
