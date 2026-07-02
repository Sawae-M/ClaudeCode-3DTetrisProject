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

        // スポーン位置 = 重力方向の反対端、中央付近
        Vector3Int spawnPos = GetSpawnPosition(gravityManager.Current);

        bool ok = pieceController.Spawn(current, spawnPos);
        if (!ok)
        {
            GameManager.Instance.TriggerGameOver();
            return;
        }

        GameManager.Instance?.uiManager?.UpdateNextPiece(next);
    }

    // 重力方向ごとのスポーン位置（キューブの"入り口"中央）
    Vector3Int GetSpawnPosition(GravityDirection dir)
    {
        int c = Board.Size / 2; // 2
        return dir switch
        {
            GravityDirection.Down    => new Vector3Int(c, Board.Size - 1, c),
            GravityDirection.Up      => new Vector3Int(c, 0,              c),
            GravityDirection.Left    => new Vector3Int(Board.Size - 1, c, c),
            GravityDirection.Right   => new Vector3Int(0,              c, c),
            GravityDirection.Forward => new Vector3Int(c, c, 0),
            GravityDirection.Back    => new Vector3Int(c, c, Board.Size - 1),
            _ => new Vector3Int(c, Board.Size - 1, c)
        };
    }

    PieceDefinition PickRandom() => pieces[Random.Range(0, pieces.Length)];
}
