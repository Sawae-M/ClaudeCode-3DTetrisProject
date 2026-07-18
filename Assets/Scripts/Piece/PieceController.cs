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
    public CameraController cameraController;

    [Header("Prefabs")]
    public GameObject blockPrefab;

    PieceDefinition currentDef;
    Vector3Int origin;
    List<Vector3Int> localCells = new();
    // blockObjects[i] は localCells[i] に対応する固定サイズのプール
    // 回転・移動時は再生成せず SyncPieceVisuals() で座標・表示だけ更新する
    List<GameObject> blockObjects = new();

    float fallTimer;
    bool fastFall;
    bool hasPiece;

    void Start()
    {
        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();
    }

    void Update()
    {
        if (!hasPiece || GameManager.Instance?.State != GameState.Playing) return;

        HandleInput();
        HandleFall();
    }

    void HandleInput()
    {
        GetMoveAxesForCamera(
            gravityManager.Current,
            cameraController,
            out Vector3Int left, out Vector3Int right,
            out Vector3Int up,   out Vector3Int down);

        if (Input.GetKeyDown(KeyCode.LeftArrow))  TryMove(left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryMove(right);
        if (Input.GetKeyDown(KeyCode.UpArrow))    TryMove(up);
        if (Input.GetKeyDown(KeyCode.DownArrow))  TryMove(down);

        if (Input.GetKeyDown(KeyCode.Q)) TryRotate(c => new Vector3Int(-c.z,  c.y,  c.x));
        if (Input.GetKeyDown(KeyCode.E)) TryRotate(c => new Vector3Int( c.z,  c.y, -c.x));
        if (Input.GetKeyDown(KeyCode.Z)) TryRotate(c => new Vector3Int( c.x,  c.z, -c.y));
        if (Input.GetKeyDown(KeyCode.X)) TryRotate(c => new Vector3Int( c.x, -c.z,  c.y));

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
            if (!TryFall())
                Lock();
        }
    }

    // 重力方向への落下。入口側OOBセルは通過を許可し、出口側OOBで着地と判定する
    bool TryFall()
    {
        Vector3Int delta    = gravityManager.GravityVector;
        var        newCells = GetWorldCells(origin + delta);
        if (board.CanFallIn(newCells, gravityManager.Current))
        {
            origin += delta;
            SyncPieceVisuals();
            ghostPiece?.UpdateGhost(this);
            return true;
        }
        return false;
    }

    // プレイヤー操作による水平移動。全セルが境界内に収まる場合のみ許可
    bool TryMove(Vector3Int delta)
    {
        var newCells = GetWorldCells(origin + delta);
        if (board.CanPlace(newCells))
        {
            origin += delta;
            SyncPieceVisuals();
            ghostPiece?.UpdateGhost(this);
            return true;
        }
        return false;
    }

    // 90度スナップ回転。全セルが境界内に収まる場合のみ適用（壁キックなし）
    void TryRotate(System.Func<Vector3Int, Vector3Int> rot)
    {
        var rotated = new List<Vector3Int>(localCells.Count);
        foreach (var c in localCells) rotated.Add(rot(c));

        var world = new List<Vector3Int>(rotated.Count);
        foreach (var c in rotated) world.Add(origin + c);

        if (board.CanPlace(world))
        {
            localCells = rotated;
            // セル数が同じなので blockObjects は再生成せず座標だけ更新
            SyncPieceVisuals();
            ghostPiece?.UpdateGhost(this);
        }
    }

    void Lock()
    {
        board.Place(GetWorldCells(origin));
        ghostPiece?.ClearGhost();
        DestroyPieceObjects();

        boardRenderer.FullRedraw();

        int cleared = faceEliminator.EliminateAll(board, gravityManager.Current);
        if (cleared > 0)
        {
            GameManager.Instance.AddScore(cleared);
            boardRenderer.FullRedraw();
        }

        hasPiece = false;
        pieceSpawner.SpawnNext();
    }

    public bool Spawn(PieceDefinition def, Vector3Int spawnPos)
    {
        currentDef = def;
        origin     = spawnPos;

        // def.cells を明示的にループでコピーし、SO の配列を直接参照しない
        localCells = new List<Vector3Int>(def.cells.Length);
        foreach (var c in def.cells) localCells.Add(c);

        if (!board.CanSpawn(GetWorldCells(origin))) return false;

        // ピース数だけ GameObject を生成（以降は座標・表示の更新のみ）
        CreatePieceObjects();
        SyncPieceVisuals();

        hasPiece  = true;
        fallTimer = 0f;
        ghostPiece?.UpdateGhost(this);
        return true;
    }

    public void AttachToCubeRoot(Transform cubeRoot)
    {
        foreach (var go in blockObjects)
            if (go != null) go.transform.SetParent(cubeRoot, true);
    }

    public void DetachFromCubeRoot()
    {
        foreach (var go in blockObjects)
            if (go != null) go.transform.SetParent(transform, true);
        SyncPieceVisuals();
        ghostPiece?.UpdateGhost(this);
    }

    public List<Vector3Int> GetWorldCells(Vector3Int org)
    {
        var result = new List<Vector3Int>(localCells.Count);
        foreach (var lc in localCells)
            result.Add(org + lc);
        return result;
    }

    public List<Vector3Int> CurrentWorldCells => GetWorldCells(origin);
    public Vector3Int Origin => origin;

    // localCells と 1:1 対応する GameObject を生成する（Spawn 時のみ呼ぶ）
    void CreatePieceObjects()
    {
        DestroyPieceObjects();
        for (int i = 0; i < localCells.Count; i++)
        {
            var go = Instantiate(blockPrefab, transform);
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material       = new Material(mr.sharedMaterial);
                mr.material.color = currentDef.color;
            }
            go.SetActive(false);
            blockObjects.Add(go);
        }
    }

    // 各 blockObjects[i] の座標と表示状態を localCells[i] に基づいて同期する
    // グリッド内なら表示・座標更新、グリッド外なら非表示（入口通過中）
    void SyncPieceVisuals()
    {
        for (int i = 0; i < localCells.Count && i < blockObjects.Count; i++)
        {
            Vector3Int wc = origin + localCells[i];
            bool vis = board.InBounds(wc);
            blockObjects[i].SetActive(vis);
            if (vis)
                blockObjects[i].transform.localPosition = new Vector3(wc.x, wc.y, wc.z);
        }
    }

    void DestroyPieceObjects()
    {
        foreach (var go in blockObjects)
            if (go != null) Destroy(go);
        blockObjects.Clear();
    }

    // ─────────────────────────────────────────────────────────────
    // カメラ視点に合わせてキー→グリッド移動方向を決定
    // ─────────────────────────────────────────────────────────────
    static void GetMoveAxesForCamera(
        GravityDirection gravity,
        CameraController cam,
        out Vector3Int left, out Vector3Int right,
        out Vector3Int up,   out Vector3Int down)
    {
        Vector3 gravVec  = GravityDirToVector3(gravity);
        Vector3 camRight = cam != null ? cam.transform.right   : Vector3.right;
        Vector3 camFwd   = cam != null ? cam.transform.forward : Vector3.forward;
        Vector3 camUp    = cam != null ? cam.transform.up      : Vector3.up;

        Vector3 projRight = Vector3.ProjectOnPlane(camRight, gravVec);
        if (projRight.sqrMagnitude < 0.01f)
            projRight = Vector3.ProjectOnPlane(camUp, gravVec);
        projRight = projRight.normalized;

        Vector3 projFwd = Vector3.ProjectOnPlane(camFwd, gravVec);
        if (projFwd.sqrMagnitude < 0.01f)
            projFwd = Vector3.ProjectOnPlane(camUp, gravVec);
        projFwd = projFwd.normalized;

        Vector3Int snapRight = SnapToAxis(projRight);
        Vector3Int snapFwd   = SnapToAxis(projFwd);

        if (snapRight == snapFwd || snapRight == -snapFwd)
            snapFwd = new Vector3Int(snapRight.z, snapRight.x, snapRight.y);

        right = snapRight;
        left  = -snapRight;
        up    = snapFwd;
        down  = -snapFwd;
    }

    static Vector3Int SnapToAxis(Vector3 v)
    {
        float ax = Mathf.Abs(v.x), ay = Mathf.Abs(v.y), az = Mathf.Abs(v.z);
        if (ax >= ay && ax >= az) return new Vector3Int((int)Mathf.Sign(v.x), 0, 0);
        if (ay >= ax && ay >= az) return new Vector3Int(0, (int)Mathf.Sign(v.y), 0);
        return new Vector3Int(0, 0, (int)Mathf.Sign(v.z));
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
}
