using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief Base class for pointers
 * 
 * Something like this should be in child class:
 * @code{.cs}
 * void Update()
 * {
 *     currentRay = get ray from view direction/mouse/e.t.c;
 *     
 *     Connection connection;
 *     if (HoverCheck(out connection) && smth is pressed)
 *         ViewSphere.Instance.StartTransition(connection.destination.origin);
 * }
 * @endcode
 */
public class Pointer : MonoBehaviour
{
    public LayerMask markersLayer = 1 << 9; /// Layer used for raycasting

    public MarkerLabel markerLabelPrefab; /// Prefab, spawned on hover

    private Marker _lastHoveredMarker = null;
    private MarkerLabel _markerLabel;

    void Awake()
    {
        _markerLabel = Instantiate(markerLabelPrefab, transform);
        _markerLabel.gameObject.SetActive(false);
    }

    public bool HoverCheck(Ray ray, out Connection connection)
    {
        connection = null;

        bool intersects = Physics.Raycast(ray, out RaycastHit hit, 10.0f, markersLayer);

        Marker marker = null;
        if (intersects)
            marker = hit.collider.gameObject.GetComponent<Marker>();

        if (_lastHoveredMarker && _lastHoveredMarker != marker)
            _lastHoveredMarker.hovered = false;

        _lastHoveredMarker = marker;

        if (marker == null)
        {
            _markerLabel.gameObject.SetActive(false);
            return false;
        }

        _lastHoveredMarker.hovered = true;
        connection = marker.connection;

        _markerLabel.gameObject.SetActive(true);
        _markerLabel.text = connection.destination.origin.title;

        var origin = Tour.Instance.transform.position;
        var direction = hit.point - origin;
        _markerLabel.transform.position = origin + direction.normalized * 0.9f + Vector3.up * 0.02f;

        return true;
    }
}
