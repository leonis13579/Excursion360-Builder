using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Text billboard
/// <para>Used in default pointers</para>
/// </summary>
public class MarkerLabel : MonoBehaviour
{
    public void SetText(string text)
    {
        if (textMesh == null)
            return;

        textMesh.text = text;
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
