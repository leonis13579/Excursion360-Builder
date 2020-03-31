using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ConnectionMarker : Marker
{
    /// <summary>
    /// Connection to which this marker is assigned
    /// </summary>
    public Connection connection;

    public override string Title => connection.GetDestenationTitle();

    public override void HandleInteract()
    {
        if (connection.rotationAfterStepAngleOverridden)
        {
            var current = Camera.main.transform.localEulerAngles;
            Camera.main.transform.localEulerAngles = new Vector3(current.x, connection.rotationAfterStepAngle, current.z);
        }
        Tour.Instance.StartTransition(connection.Destination);
    }
}

