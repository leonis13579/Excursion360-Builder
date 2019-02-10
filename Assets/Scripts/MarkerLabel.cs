using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MarkerLabel : MonoBehaviour
{
    public string Text
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
        transform.rotation = Quaternion.LookRotation(transform.position - PlayerState.Instance.transform.position);
    }
}
