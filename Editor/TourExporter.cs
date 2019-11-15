using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR

using UnityEditor;

namespace Exported
{
    [Serializable]
    class StateLink
    {
        public string id = "state";
        public Quaternion rotation;
        public int colorScheme;
    }

    [Serializable]
    class State
    {
        public string id;
        public string title;
        public string url;
        public string type;
        public Vector3 rotation;
        public Quaternion pictureRotation;
        public List<StateLink> links = new List<StateLink>();
    }

    [Serializable]
    class Tour
    {
        public string firstStateId = "state";
        public List<State> states = new List<State>();
        public Color[] colorSchemes;
    }
}

public class TourExporter
{
    private static string lastFolderPath = "";
    public static void ExportTour()
    {
        try
        {
            Exported.Tour tour = new Exported.Tour();

            if (!TryGetTargetFolder(out var path))
                return;

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
                    url = $"{Tour.Instance.linkPrefix}{textureSource.Export(path, "state_" + i)}",
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
            File.WriteAllText(path + "/tour.json", JsonUtility.ToJson(tour, true));
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

    public static bool TryGetTargetFolder(out string folderPath)
    {
        // Select path
        folderPath = lastFolderPath = EditorUtility.OpenFolderPanel("Select folder for tour", lastFolderPath, "");
        if (folderPath.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Operation cancelled", "Ok");
            return false;
        }

        if (!EditorUtility.DisplayDialog("Warning", "All content in folder will be deleted!", "Ok, delete", "Cancel"))
        {
            EditorUtility.DisplayDialog("Error", "Operation cancelled", "Ok");
            return false;
        }

        var files = Directory.GetFiles(folderPath);
        for (int i = 0; i < files.Length; i++)
        {
            var filePath = files[i];
            UpdateProcess(i, files.Length, "Delete old files", filePath);
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Can't delete file {filePath}\n{ex.Message}\n{ex.StackTrace}", "Ok");
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
