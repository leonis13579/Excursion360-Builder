using System;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.SceneRenderers
{
    public class ViewDirectionRenderer
    {
        private State targetViewState;
        private Func<float> getAngle;
        private Action<float> setAngle;

        public UnityEngine.Object CurrentEditableObject { get; private set; }

        public void SetEditing(
            State stateTo, 
            Func<float> getAngle, 
            Action<float> setAngle,
            UnityEngine.Object currentEditableObject)
        {
            targetViewState = stateTo;
            this.getAngle = getAngle;
            this.setAngle = setAngle;
            CurrentEditableObject = currentEditableObject;
        }

        public void ClearEditing()
        {
            targetViewState = default;
            this.getAngle = default;
            this.setAngle = default;
            CurrentEditableObject = default;
        }

        public ViewDirectionRenderer()
        {
            Selection.selectionChanged += () => targetViewState = default;
        }

        internal void Render(SceneView obj)
        {
            if (!targetViewState || EditorApplication.isPlaying)
            {
                return;
            }

            var destinationPosition = targetViewState.transform.position;

            if (InteractionHelper.GetStateClickPoint(targetViewState, out var quaternion))
            {
                setAngle(quaternion.eulerAngles.y);
            }

            var middleAngle = getAngle();
            var leftAngle = middleAngle - 20;
            var rightAngle = middleAngle + 20;


            var leftPosition = destinationPosition + Quaternion.Euler(0, leftAngle, 0) * Vector3.forward;
            var leftUpPosition = leftPosition + Vector3.up;
            var leftDownPosition = leftPosition - Vector3.up;


            var middlePosition = destinationPosition + Quaternion.Euler(0, middleAngle, 0) * Vector3.forward;
            var rightPosition = destinationPosition + Quaternion.Euler(0, rightAngle, 0) * Vector3.forward;
            var rightUpPosition = rightPosition + Vector3.up;
            var rightDownPosition = rightPosition - Vector3.up;


            Handles.color = Color.green;
            Handles.DrawLine(destinationPosition, leftPosition);
            Handles.DrawLine(leftUpPosition, leftDownPosition);

            Handles.DrawLine(destinationPosition, rightPosition);
            Handles.DrawLine(rightUpPosition, rightDownPosition);

            Handles.color = Color.white;
            Handles.DrawLine(destinationPosition, middlePosition);

        }
    }
}
