using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor
{
    class InteractionHelper
    {
        public static bool GetStateClickPoint(State state, out Quaternion quaternion)
        {
            quaternion = default;
            if (Event.current.type != EventType.MouseDown || Event.current.button != 0)
            {
                return false;
            }
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
            Event.current.Use();

            SphereCollider collider = state.GetComponent<SphereCollider>();
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            ray.origin -= ray.direction * collider.radius * 4;

            if (collider.Raycast(ray, out RaycastHit hit, 100.0f))
            {
                var toCenterDirection = state.transform.position - hit.point;
                var rightDirection = Vector3.Cross(toCenterDirection, ray.direction);
                var normal = Vector3.Cross(rightDirection, ray.direction);
                var hitPosition = state.transform.position + StateEditorWindow.ReflectDirection(toCenterDirection, normal);

                Undo.RecordObject(state, "Undo orientation change");

                quaternion = Quaternion.FromToRotation(Vector3.forward,
                    hitPosition - state.transform.position);
                return true;
            }
            return false;
        }
    }
}
