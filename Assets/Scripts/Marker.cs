using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief Physical representation of connection origin
 */
public class Marker : MonoBehaviour
{
    /**
     * @brief Connection to which this marker is assigned
     */
    public Connection connection;

    /**
     * @brief Affects animation varialbe "Hovered"
     */
    public bool hovered
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

            if (_animator != null)
                _animator.SetBool("Hovered", _hovered);
        }
    }

    private Animator _animator;
    private bool _hovered;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }
}
