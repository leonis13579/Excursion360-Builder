using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GroupConnectionMarker : Marker
{
    public GameObject buttonPrefab;
    public GameObject buttonsHolder;
    public int positionInitialDelta = 60;
    public int positionDelta = 40;
    /// <summary>
    /// Connection group to which this marker is assigned
    /// </summary>
    public GroupConnection groupConnection;

    public override string Title => groupConnection.title;

    protected override void Start()
    {
        base.Start();
        buttonsHolder.SetActive(false);
        GetComponentInChildren<Canvas>().worldCamera = Camera.current;
        for (int i = 0; i < groupConnection.states.Count; i++)
        {
            var newButton = Instantiate(buttonPrefab, buttonsHolder.transform);
            newButton.transform.localPosition =
                newButton.transform.localPosition
                 + Vector3.down * positionInitialDelta
                 + Vector3.down * (i + 1) * positionDelta;
            var targetState = groupConnection.states[i];
            newButton.GetComponentInChildren<TextMeshProUGUI>().SetText(targetState.title);
            newButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Tour.Instance.StartTransition(targetState);

                var targetAngle = groupConnection.rotationAfterStepAngles.FirstOrDefault(p => p.state == targetState)?.rotationAfterStepAngle;
                if (targetAngle.HasValue)
                {
                    var current = Camera.main.transform.localEulerAngles;
                    Camera.main.transform.localEulerAngles = new Vector3(current.x, targetAngle.Value, current.z);
                }
            });
        }
        var positionAfterButtonsDelta = positionInitialDelta + groupConnection.states.Count * positionDelta;
        for (int i = 0; i < groupConnection.infos.Count; i++)
        {
            var newButton = Instantiate(buttonPrefab, buttonsHolder.transform);
            newButton.GetComponent<Image>().color = Color.gray;
            newButton.transform.localPosition =
                newButton.transform.localPosition
                 + Vector3.down * positionAfterButtonsDelta
                 + Vector3.down * (i + 1) * positionDelta;
            var targetState = groupConnection.infos[i];
            newButton.GetComponentInChildren<TextMeshProUGUI>().SetText(targetState);
        }
    }

    public override void HandleInteract()
    {
        buttonsHolder.SetActive(!buttonsHolder.activeSelf);
    }
}

