using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mouse controlled pointer
/// </summary>
public class DesktopPointer : Pointer
{
    public float sensitivityX = 1f;
    public float sensitivityY = 1f;

    public float minimumX = -360f;
    public float maximumX = 360f;

    public float minimumY = -60f;
    public float maximumY = 60f;

    private float _rotationY = 0f;

    void Update()
    {
        if (HoverCheck(Camera.main.ScreenPointToRay(Input.mousePosition), out Marker marker) && Input.GetMouseButtonUp(0))
        {
            marker.HandleInteract();
        }
        else if (Input.GetMouseButton(0))
        {
            float rotationX = transform.localEulerAngles.y - Input.GetAxis("Mouse X") * sensitivityX;

            _rotationY -= Input.GetAxis("Mouse Y") * sensitivityY;
            _rotationY = Mathf.Clamp(_rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-_rotationY, rotationX, 0);
        }
    }
}
