using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Excursion360_Builder.Shared.States.Items
{
    public class MonoBehaviourStateItem : MonoBehaviour, IStateItem
    {
        [SerializeField]
        private Quaternion orientation;
        public Quaternion Orientation { get => orientation; set => orientation = value; }
    }
}
