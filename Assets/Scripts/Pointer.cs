using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    public LayerMask markersLayer = 1 << 9;
    public MarkerLabel markerLabelPrefab;

    [HideInInspector]
    public Ray currentRay;

    private Marker _lastHoveredMarker = null;
    private MarkerLabel _markerLabel;

    public void Awake()
    {
        _markerLabel = Instantiate(markerLabelPrefab, transform);
        _markerLabel.gameObject.SetActive(false);
    }

    public bool HoverCheck(out Connection connection)
    {
        connection = null;

        RaycastHit hit;
        bool intersects = Physics.Raycast(currentRay, out hit, 10.0f, markersLayer);

        Marker marker = null;
        if (intersects)
            marker = hit.collider.gameObject.GetComponent<Marker>();

        if (_lastHoveredMarker && _lastHoveredMarker != marker)
            _lastHoveredMarker.Hovered = false;

        _lastHoveredMarker = marker;

        if (marker == null)
        {
            _markerLabel.gameObject.SetActive(false);
            return false;
        }

        _lastHoveredMarker.Hovered = true;
        connection = marker.connection;

        _markerLabel.gameObject.SetActive(true);
        _markerLabel.Text = connection.destination.state.title;

        var origin = PlayerState.Instance.transform.position;
        var direction = hit.point - origin;
        _markerLabel.transform.position = origin + direction.normalized * 0.9f + Vector3.up * 0.02f;

        return true;
    }
}
