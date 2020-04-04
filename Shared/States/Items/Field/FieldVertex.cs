using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Excursion360_Builder.Shared.States.Items.Field
{
    /// <summary>
    /// Vertex of <see cref="FieldItem"/>
    /// </summary>
    [Serializable]
    public class FieldVertex : IStateItem
    {
        public int index;
        [SerializeField]
        private Quaternion orientation;

        public Quaternion Orientation { get => orientation; set => orientation = value; }
    }
}
