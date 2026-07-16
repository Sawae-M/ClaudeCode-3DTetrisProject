using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("References")]
    public PieceController pieceController;
    public Board board;
    public GravityManager gravityManager;

    [Header("Piece Definitions")]
    public PieceDefinition[] pieces;

    PieceDefinition next;

    void Awake()
    {
        if (pieces == null || pieces.Length == 0)
        {
            Debug.LogError("PieceSpawner: pieces配列が空です。Inspectorでピースアセットを設定してください。");
            return;
        }
        next = PickRandom();
    }

    public PieceDefinition Next => next;

    public void SpawnNext()
    {
        if (pieces == null || pieces.Length == 0) return;

        var current = next;
        next = PickRandom();

        // ピースのセル範囲を考慮して入口面中央にスポーン
        Vector3Int spawnPos = ComputeSpawnOrigin(gravityManager.Current, current.cells);

        bool ok = pieceController.Spawn(current, spawnPos);
        if (!ok)
        {
            Debug.LogWarning($"[PieceSpawner] Spawn failed: piece={current.pieceName} pos={spawnPos} gravity={gravityManager.Current}");
            GameManager.Instance.TriggerGameOver();
            return;
        }

        GameManager.Instance?.uiManager?.UpdateNextPiece(next);
    }

    // ピースのセルオフセット群を考慮し、入口面の2軸で中央に揃えたスポーン原点を返す
    Vector3Int ComputeSpawnOrigin(GravityDirection dir, Vector3Int[] cells)
    {
        // 重力方向に垂直な2軸のmin/maxを取得して中央寄せする
        int minA = int.MaxValue, maxA = int.MinValue;
        int minB = int.MaxValue, maxB = int.MinValue;
        int minD = int.MaxValue, maxD = int.MinValue;

        foreach (var c in cells)
        {
            GetComponents(dir, c, out int a, out int b, out int d);
            if (a < minA) minA = a; if (a > maxA) maxA = a;
            if (b < minB) minB = b; if (b > maxB) maxB = b;
            if (d < minD) minD = d; if (d > maxD) maxD = d;
        }

        // 2軸を中央に揃えるオフセット
        int originA = (Board.Size - (maxA - minA + 1)) / 2 - minA;
        int originB = (Board.Size - (maxB - minB + 1)) / 2 - minB;

        // 重力軸: 入口面に最も近いセルを合わせる
        // entryFace=0（低端から入る）→ minD を合わせる
        // entryFace=Size-1（高端から入る）→ maxD を合わせる
        int entryFace = GetEntryFaceIndex(dir);
        int originD   = entryFace == 0 ? -minD : entryFace - maxD;

        return BuildVector(dir, originA, originB, originD);
    }

    // セルを重力方向に応じて (axisA成分, axisB成分, 深さ成分) に分解
    static void GetComponents(GravityDirection dir, Vector3Int c, out int a, out int b, out int d)
    {
        switch (dir)
        {
            case GravityDirection.Down:
            case GravityDirection.Up:      a = c.x; b = c.z; d = c.y; break;
            case GravityDirection.Left:
            case GravityDirection.Right:   a = c.y; b = c.z; d = c.x; break;
            default:                       a = c.x; b = c.y; d = c.z; break;
        }
    }

    // (a, b, depth) → Vector3Int（重力方向に応じた軸割り当て）
    static Vector3Int BuildVector(GravityDirection dir, int a, int b, int d)
    {
        return dir switch
        {
            GravityDirection.Down  or GravityDirection.Up      => new Vector3Int(a, d, b),
            GravityDirection.Left  or GravityDirection.Right   => new Vector3Int(d, a, b),
            _                                                   => new Vector3Int(a, b, d),
        };
    }

    // 入口面のインデックス（ピースが最初に現れる深さ位置）
    static int GetEntryFaceIndex(GravityDirection dir) => dir switch
    {
        GravityDirection.Down    => Board.Size - 1, // Y の最大側から入る
        GravityDirection.Up      => 0,
        GravityDirection.Left    => Board.Size - 1,
        GravityDirection.Right   => 0,
        GravityDirection.Forward => 0,
        GravityDirection.Back    => Board.Size - 1,
        _ => Board.Size - 1
    };

    PieceDefinition PickRandom() => pieces[Random.Range(0, pieces.Length)];
}
