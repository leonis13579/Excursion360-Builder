using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class Styles
{
    static Styles()
    {
        ToggleButtonStyleNormal = "Button";
        ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
        ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
    }
    public static readonly GUIStyle ToggleButtonStyleNormal;
    public static readonly GUIStyle ToggleButtonStyleToggled;
}

