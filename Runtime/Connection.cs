using UnityEngine;

/**
 * @brief Represents connection of origin state to destination state
 * 
 *      ___                         ___
 *     /   \ <- orientation   ---> /   \
 *     \__\/                       \/__/
 * origin ^------------------------^ destination
 */

 
public class Connection : MonoBehaviour
{
    /**
     * @brief Origin of this connection
     */
    public State origin
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
     * @brief Orientation of marker in origin
     */
    public Quaternion orientation = Quaternion.identity;

    /**
     * @brief Linked connection
     */
    public Connection destination;

    private State _origin; /// Cache
}
