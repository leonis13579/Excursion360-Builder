using UnityEngine;

public class Connection : MonoBehaviour
{
    public State state;
    public Connection destination;
    public Quaternion orientation = Quaternion.identity;

    void Start()
    {
        state = GetComponent<State>();
    }
}
