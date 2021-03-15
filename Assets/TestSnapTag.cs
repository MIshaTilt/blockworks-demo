using UnityEngine;

public class TestSnapTag : MonoBehaviour
{
    public SnapType type;
}

public enum SnapType
{
    Preview,
    Snap,
    Disconnect
}