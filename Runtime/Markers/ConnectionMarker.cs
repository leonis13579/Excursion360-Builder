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

    public override string Title => connection.Destination.title;

    public override void HandleInteract()
    {
        Tour.Instance.StartTransition(connection.Destination);
    }
}

