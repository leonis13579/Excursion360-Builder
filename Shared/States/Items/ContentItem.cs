using Excursion360_Builder.Shared.States.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ContentItem : MonoBehaviourStateItem
{
    public ContentType Type;

#if UNITY_EDITOR
    public bool isOpened;
#endif
}

