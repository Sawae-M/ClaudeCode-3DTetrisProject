using UnityEngine;

public enum GravityDirection { Down, Up, Left, Right, Forward, Back }

public class GravityManager : MonoBehaviour
{
    public static GravityManager Instance { get; private set; }

    public GravityDirection Current { get; private set; } = GravityDirection.Down;

    // カメラ視点インデックス → 重力方向（カメラ方向の反対 = ブロック落下方向）
    // 視点 0:Top  1:Bottom  2:Right  3:Left  4:Front  5:Back
    public static readonly GravityDirection[] ViewToGravity = {
        GravityDirection.Down,    // 0 Top    : カメラが上 → 重力 下
        GravityDirection.Up,      // 1 Bottom : カメラが下 → 重力 上
        GravityDirection.Left,    // 2 Right  : カメラが右 → 重力 左
        GravityDirection.Right,   // 3 Left   : カメラが左 → 重力 右
        GravityDirection.Forward, // 4 Front  : カメラが手前 → 重力 奥（+Z）
        GravityDirection.Back,    // 5 Back   : カメラが奥 → 重力 手前（-Z）
    };

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

    // カメラ視点インデックスから重力を設定
    public void SetFromView(int viewIndex)
    {
        if (viewIndex >= 0 && viewIndex < ViewToGravity.Length)
            Current = ViewToGravity[viewIndex];
    }
}
