using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.WebBuild
{
    class BuildPacksManagerWindow : EditorWindow
    {
        private string packsLocation;

        private void OnEnable()
        {
            packsLocation = Application.dataPath + "/Tour creator";
            if (!Directory.Exists(packsLocation))
            {
                Directory.CreateDirectory(packsLocation);
            }
        }


        private void OnGUI()
        {
            GUILayout.Label("Select one state for editing it", EditorStyles.boldLabel);
        }

    }
}
