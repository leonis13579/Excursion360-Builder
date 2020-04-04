using Excursion360_Builder.Shared.States.Items.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Excursion360_Builder.Runtime.Markers
{
    public class FieldItemMarker : Marker
    {
        public FieldItem fieldItem;
        public override string Title => fieldItem.title;

        public override void HandleInteract()
        {
            Debug.Log(Title);
        }
    }
}
