using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public Connection connection;

    public bool Hovered
    {
        get
        {
            return _hovered;
        }
        set
        {
            if (_hovered == value)
                return;

            _hovered = value;
            _animator.SetBool("Hovered", _hovered);
        }
    }

    private Animator _animator;
    private bool _hovered;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
}
