using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.Extensions
{
    static class GroupConnectionExtensions
    {
        public static Vector3 GetOriginPosition(this GroupConnection connection) => connection.transform.position + connection.Orientation * Vector3.forward;
    }
}
