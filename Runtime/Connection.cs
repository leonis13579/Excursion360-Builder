using Excursion360_Builder.Shared.States.Items;
using System.Linq;
using UnityEngine;

/**
 * @brief Represents connection of origin state to destination state
 * 
 *      ___                         ___
 *     /   \ <- orientation   ---> /   \
 *     \__\/                       \/__/
 * origin ^------------------------^ destination
 */

 
public class Connection : MonoBehaviourStateItem
{

    /// <summary>
    /// Origin of this connection
    /// </summary>
    public State Origin
    { 
        get
        {
            if (_origin == null)
                _origin = GetComponent<State>();

            return _origin;
        } 
    }

    [HideInInspector]
    public int colorScheme = 0;

    /**
     * @brief Linked connection
     */
    public State Destination;

    private State _origin; /// Cache

    public bool rotationAfterStepAngleOverridden;
    public float rotationAfterStepAngle;
    public string GetDestenationTitle() =>
            Destination ?
            Destination.title :
            "NO DESTINATION !!!";
}
