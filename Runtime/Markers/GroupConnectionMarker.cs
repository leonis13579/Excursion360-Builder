using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GroupConnectionMarker : Marker
{

    /// <summary>
    /// Connection group to which this marker is assigned
    /// </summary>
    public GroupConnection groupConnection;

    public override string Title => groupConnection.title;

    public override void HandleInteract()
    {
    }
}

