using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    [Header("References")]
    public Board board;
    public BoardRenderer boardRenderer;
    public GravityManager gravityManager;
    public PieceSpawner pieceSpawner;
    public FaceEliminator faceEliminator;
    public GhostPiece ghostPiece;

    [Header("Prefabs")]
    public GameObject blockPrefab;

    // 現在のピースデータ
    PieceDefinition currentDef;
    Vector3Int origin;                   // ピース基準座標（グリッド）
    List<Vector3Int> localCells = new(); // ローカルオフセット
    List<GameObject> blockObjects = new();

    float fallTimer;
    bool fastFall;
    bool hasPiece;

    void Update()
    {
        if (!hasPiece || GameManager.Instance?.State != GameState.Playing) return;

        HandleInput();
        HandleFall();
    }

    void HandleInput()
    {
        // 移動軸：重力方向以外の2軸
        GetMoveAxes(gravityManager.Current, out Vector3Int axisA, out Vector3Int axisB);

        if (Input.GetKeyDown(KeyCode.LeftArrow))  TryMove(-axisA);
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryMove( axisA);
        if (Input.GetKeyDown(KeyCode.UpArrow))    TryMove( axisB);
        if (Input.GetKeyDown(KeyCode.DownArrow))  TryMove(-axisB);

        fastFall = Input.GetKey(KeyCode.Space);
    }

    void HandleFall()
    {
        float interval = fastFall
            ? Mathf.Min(0.05f, GameManager.Instance.CurrentFallInterval())
            : GameManager.Instance.CurrentFallInterval();

        fallTimer += Time.deltaTime;
        if (fallTimer >= interval)
        {
            fallTimer = 0f;
            if (!TryMove(gravityManager.GravityVector))
                Lock();
        }
    }

    bool TryMove(Vector3Int delta)
    {
        var newCells = GetWorldCells(origin + delta);
        if (board.CanPlace(newCells))
        {
            ErasePieceFromRenderer();
            origin += delta;
            DrawPiece();
            ghostPiece?.UpdateGhost(this);
            return true;
        }
        return false;
    }

    bool TryMoveBool(Vector3Int delta)
    {
        var newCells = GetWorldCells(origin + delta);
        if (board.CanPlace(newCells))
        {
            ErasePieceFromRenderer();
            origin += delta;
            DrawPiece();
            ghostPiece?.UpdateGhost(this);
            return true;
        }
        return false;
    }

    void Lock()
    {
        // ボードに確定配置
        board.Place(GetWorldCells(origin));
        boardRenderer.FullRedraw();

        // ゴーストをクリア
        ghostPiece?.ClearGhost();

        // 面消去
        int cleared = faceEliminator.EliminateAll(board, gravityManager.Current);
        if (cleared > 0)
        {
            GameManager.Instance.AddScore(cleared);
            boardRenderer.FullRedraw();
        }

        hasPiece = false;
        DestroyPieceObjects();

        pieceSpawner.SpawnNext();
    }

    public bool Spawn(PieceDefinition def, Vector3Int spawnPos)
    {
        currentDef = def;
        origin = spawnPos;
        localCells = new List<Vector3Int>(def.cells);

        if (!board.CanPlace(GetWorldCells(origin))) return false;

        hasPiece = true;
        fallTimer = 0f;
        DrawPiece();
        ghostPiece?.UpdateGhost(this);
        return true;
    }

    // キューブ回転時にピースをCubeRootに一時接続
    public void AttachToCubeRoot(Transform cubeRoot)
    {
        foreach (var go in blockObjects)
            go.transform.SetParent(cubeRoot);
    }

    public void DetachFromCubeRoot()
    {
        foreach (var go in blockObjects)
            go.transform.SetParent(transform);
    }

    public List<Vector3Int> GetWorldCells(Vector3Int org)
    {
        var result = new List<Vector3Int>();
        foreach (var lc in localCells)
            result.Add(org + lc);
        return result;
    }

    public List<Vector3Int> CurrentWorldCells => GetWorldCells(origin);
    public Vector3Int Origin => origin;

    void DrawPiece()
    {
        DestroyPieceObjects();
        var cells = GetWorldCells(origin);
        foreach (var c in cells)
        {
            if (!board.InBounds(c)) continue;
            var go = Instantiate(blockPrefab, transform);
            go.transform.position = new Vector3(c.x, c.y, c.z);
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = new Material(mr.sharedMaterial);
                mr.material.color = currentDef.color;
            }
            blockObjects.Add(go);
        }
    }

    void ErasePieceFromRenderer()
    {
        DestroyPieceObjects();
    }

    void DestroyPieceObjects()
    {
        foreach (var go in blockObjects)
            if (go != null) Destroy(go);
        blockObjects.Clear();
    }

    // 重力方向以外の移動2軸を返す
    static void GetMoveAxes(GravityDirection gravity, out Vector3Int axisA, out Vector3Int axisB)
    {
        switch (gravity)
        {
            case GravityDirection.Down:
            case GravityDirection.Up:
                axisA = Vector3Int.right;
                axisB = new Vector3Int(0, 0, 1);
                break;
            case GravityDirection.Left:
            case GravityDirection.Right:
                axisA = Vector3Int.up;
                axisB = new Vector3Int(0, 0, 1);
                break;
            default:
                axisA = Vector3Int.right;
                axisB = Vector3Int.up;
                break;
        }
    }
}
