using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace Exported
{
    [Serializable]
    class SphericalDirection
    {
        public float f = 0.0f;
        public float o = 0.0f;
    }

    [Serializable]
    class StateLink : SphericalDirection
    {
        public string id = "state";
    }

    [Serializable]
    class State
    {
        public string id;
        public string title;
        public string url;
        public string type;
        public SphericalDirection viewDirection = new SphericalDirection();
        public Vector3 rotation = Vector3.zero;

        public List<StateLink> links = new List<StateLink>();
    }

    [Serializable]
    class Tour
    {
        public string firstStateId = "state";
        public List<State> states = new List<State>();
    }
}

public class TourExporter
{
    public static void ExportTour()
    {
        Exported.Tour tour = new Exported.Tour();

        // Select path
        string path = EditorUtility.OpenFolderPanel("Select folder for tour", "", "");
        if (path.Length == 0)
            return;

        // Find first state
        if (Tour.Instance == null)
        { 
            EditorUtility.DisplayDialog("Error", "There is no tour object on this scene!", "Ok");
            return;
        }

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

        // Pre process states
        UpdateProcess(0, states.Length);

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
                url = textureSource.Export(path, "state_" + i),
                type = textureSource.GetSourceType().ToString().ToLower(),
                rotation = state.transform.rotation.eulerAngles
            };

            stateIds.Add(state.GetInstanceID(), exportedState.id);
            tour.states.Add(exportedState);

            UpdateProcess(i + 1, states.Length);
        }

        // Assign first state id
        stateIds.TryGetValue(firstState.GetInstanceID(), out tour.firstStateId);

        // Process links
        for (int i = 0; i < states.Length; ++i)
        {
            var state = states[i];
            var exportedState = tour.states[i];

            Connection[] connections = state.gameObject.GetComponents<Connection>();
            if (connections == null)
                continue;

            foreach (var connection in connections)
            {
                if (!stateIds.TryGetValue(connection.origin.GetInstanceID(), out string otherId))
                {
                    continue;
                }

                Vector3 direction = (connection.orientation * Vector3.forward).normalized;

                exportedState.links.Add(new Exported.StateLink()
                {
                    id = otherId,
                    o = Mathf.Acos(direction.z),
                    f = Mathf.Atan2(direction.y, direction.x)
                });
            }
        }

        // Serialize and write
        File.WriteAllText(path + "/tour.json", JsonUtility.ToJson(tour));

        // Finish
        EditorUtility.ClearProgressBar();
    }

    static void UpdateProcess(int current, int target)
    {
        EditorUtility.DisplayProgressBar("Exporting", "Please wait...", (float)current/ (float)target);
    }
}

#endif
