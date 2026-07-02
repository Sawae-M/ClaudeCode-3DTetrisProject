using UnityEngine;

public enum GravityDirection { Down, Up, Left, Right, Forward, Back }

public class GravityManager : MonoBehaviour
{
    public static GravityManager Instance { get; private set; }

    public GravityDirection Current { get; private set; } = GravityDirection.Down;

    // 重力ベクトル（ワールド座標系、ピースが「落ちる」方向）
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

    public void SetDirection(GravityDirection dir)
    {
        Current = dir;
    }

    // キューブ回転に合わせて重力方向を変換
    // axis: X軸回転(1,0,0) or Y軸(0,1,0) or Z軸(0,0,1), angle: +90 or -90
    public void RotateGravity(Vector3 axis, float angle)
    {
        var q = Quaternion.AngleAxis(angle, axis);
        var v = q * GravityDirToVector3(Current);
        Current = Vector3ToGravityDir(v);
    }

    static Vector3 GravityDirToVector3(GravityDirection d) => d switch
    {
        GravityDirection.Down    => Vector3.down,
        GravityDirection.Up      => Vector3.up,
        GravityDirection.Left    => Vector3.left,
        GravityDirection.Right   => Vector3.right,
        GravityDirection.Forward => Vector3.forward,
        GravityDirection.Back    => Vector3.back,
        _ => Vector3.down
    };

    static GravityDirection Vector3ToGravityDir(Vector3 v)
    {
        v = v.normalized;
        float threshold = 0.9f;
        if (v.y < -threshold) return GravityDirection.Down;
        if (v.y >  threshold) return GravityDirection.Up;
        if (v.x < -threshold) return GravityDirection.Left;
        if (v.x >  threshold) return GravityDirection.Right;
        if (v.z >  threshold) return GravityDirection.Forward;
        return GravityDirection.Back;
    }
}
