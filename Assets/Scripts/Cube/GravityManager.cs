using UnityEngine;

public enum GravityDirection { Down, Up, Left, Right, Forward, Back }

public class GravityManager : MonoBehaviour
{
    public static GravityManager Instance { get; private set; }

    public GravityDirection Current { get; private set; } = GravityDirection.Down;

    // カメラ視点インデックス → 重力方向
    // 横から見る視点（Front/Back/Left/Right）は常に Down で
    // 画面上から下へブロックが落ちる。
    // Top/Bottom 視点は Forward/Back でブロックが奥・手前へ落ちる。
    // 視点 0:Top  1:Bottom  2:Right  3:Left  4:Front  5:Back
    public static readonly GravityDirection[] ViewToGravity = {
        GravityDirection.Forward, // 0 Top    : 上から見下ろし → 奥へ落下
        GravityDirection.Back,    // 1 Bottom : 下から見上げ  → 手前へ落下
        GravityDirection.Down,    // 2 Right  : 右から見る    → 下へ落下
        GravityDirection.Down,    // 3 Left   : 左から見る    → 下へ落下
        GravityDirection.Down,    // 4 Front  : 正面から見る  → 下へ落下（デフォルト）
        GravityDirection.Down,    // 5 Back   : 背面から見る  → 下へ落下
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
