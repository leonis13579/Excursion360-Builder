using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.Extensions
{
    public static class ConnectionExtensions
    {

        public static Vector3 GetOriginPosition(this Connection connection) => connection.transform.position + connection.Orientation * Vector3.forward;
        public static Vector3 GetDestinationPosition(this Connection connection, float ifCleanLength = 1f) => 
            connection.Destination ?
            connection.Destination.transform.position :
            connection.GetOriginPosition() + Vector3.up * ifCleanLength;

        public static Connection GetBackConnection(this Connection connection) => 
            connection.Destination ?
            connection.Destination.GetComponents<Connection>().FirstOrDefault(c => c.Destination == connection.Origin) :
            default;

        public static GroupConnection GetBackGroupConnection(this Connection connection) =>
            connection.Destination ?
            connection.Destination.GetComponents<GroupConnection>().FirstOrDefault(gc => gc.states.Any(s => s == connection.Origin)) :
            default;
    }
}