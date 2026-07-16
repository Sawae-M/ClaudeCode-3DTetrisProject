using UnityEngine;

public enum GravityDirection { Down, Up, Left, Right, Forward, Back }

public class GravityManager : MonoBehaviour
{
    public static GravityManager Instance { get; private set; }

    public GravityDirection Current { get; private set; } = GravityDirection.Down;

    public Vector3Int GravityVector => Current switch
    {
        GravityDirection.Down    => new Vector3Int( 0, -1,  0),
        GravityDirection.Up      => new Vector3Int( 0,  1,  0),
        GravityDirection.Left    => new Vector3Int(-1,  0,  0),
        GravityDirection.Right   => new Vector3Int( 1,  0,  0),
        GravityDirection.Forward => new Vector3Int( 0,  0,  1),
        GravityDirection.Back    => new Vector3Int( 0,  0, -1),
        _ => Vector3Int.zero
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 重力は常に Down 固定。視点切り替えでは変化しない。
    public void SetFromView(int viewIndex) { }
}
