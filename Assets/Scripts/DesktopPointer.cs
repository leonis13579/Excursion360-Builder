using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        currentRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        Connection connection;
        if (HoverCheck(out connection) && Input.GetMouseButtonUp(0))
        {
            PlayerState.Instance.StartTransition(connection.destination.state);
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
