using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateItem : MonoBehaviour
{
    public StateItemType Type { get; }
    public new string name = "no name";
    public Quaternion orientation = Quaternion.identity;

#if UNITY_EDITOR
    public bool isFolded;
#endif
    void Start()
    {
    }

    void Update()
    {
        base.name = this.name;
    }
}
