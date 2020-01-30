using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateItem : MonoBehaviour
{
    /// <summary>
    /// Orientation of item in state
    /// </summary>
    public Quaternion orientation = Quaternion.identity;

#if UNITY_EDITOR
    public bool isOpened;
#endif
}
