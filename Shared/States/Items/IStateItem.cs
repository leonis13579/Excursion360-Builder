using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Excursion360_Builder.Shared.States.Items
{
    public interface IStateItem
    {
        /// <summary>
        /// Orientation of item in state
        /// </summary>
        Quaternion Orientation { get; set; }
    }
}
