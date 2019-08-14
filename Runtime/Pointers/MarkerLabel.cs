using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/**
 * @brief Text billboard
 * 
 * Used in default pointers
 */
public class MarkerLabel : MonoBehaviour
{
    public string text
    {
        set
        {
            if (textMesh == null)
                return;

            textMesh.text = value;
        }
    }

    private TextMeshProUGUI textMesh;

    void Start()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Tour.Instance.transform.position);
    }
}
