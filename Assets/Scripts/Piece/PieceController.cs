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
    public CameraController cameraController; // 視点連動に使用

    [Header("Prefabs")]
    public GameObject blockPrefab;

    PieceDefinition currentDef;
    Vector3Int origin;
    List<Vector3Int> localCells = new();
    List<GameObject> blockObjects = new();

    float fallTimer;
    bool fastFall;
    bool hasPiece;

    void Start()
    {
        // Inspector 未設定の場合は自動取得
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
        // カメラ視点に合わせた移動軸を取得
        GetMoveAxesForCamera(
            gravityManager.Current,
            cameraController,
            out Vector3Int left, out Vector3Int right,
            out Vector3Int up,   out Vector3Int down);

        if (Input.GetKeyDown(KeyCode.LeftArrow))  TryMove(left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryMove(right);
        if (Input.GetKeyDown(KeyCode.UpArrow))    TryMove(up);
        if (Input.GetKeyDown(KeyCode.DownArrow))  TryMove(down);

        // ピース回転（Y軸: Q=反時計 / E=時計、X軸: Z=前傾 / X=後傾）
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
            if (!TryMove(gravityManager.GravityVector))
                Lock();
        }
    }

    // 90度スナップ回転。範囲外なら即キャンセル（壁キックなし）
    void TryRotate(System.Func<Vector3Int, Vector3Int> rot)
    {
        var rotated = new List<Vector3Int>();
        foreach (var c in localCells) rotated.Add(rot(c));

        var world = new List<Vector3Int>();
        foreach (var c in rotated) world.Add(origin + c);

        if (board.CanPlace(world))
        {
            localCells = rotated;
            RedrawPiece();
            ghostPiece?.UpdateGhost(this);
        }
    }

    bool TryMove(Vector3Int delta)
    {
        var newCells = GetWorldCells(origin + delta);
        if (board.CanPlace(newCells))
        {
            origin += delta;
            RedrawPiece();
            ghostPiece?.UpdateGhost(this);
            return true;
        }
        return false;
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
        currentDef  = def;
        origin      = spawnPos;
        localCells  = new List<Vector3Int>(def.cells);

        if (!board.CanPlace(GetWorldCells(origin))) return false;

        hasPiece  = true;
        fallTimer = 0f;
        RedrawPiece();
        ghostPiece?.UpdateGhost(this);
        return true;
    }

    // キューブ回転アニメ中にピースをCubeRootへ一時的に接続
    public void AttachToCubeRoot(Transform cubeRoot)
    {
        foreach (var go in blockObjects)
            if (go != null) go.transform.SetParent(cubeRoot, true);
    }

    public void DetachFromCubeRoot()
    {
        foreach (var go in blockObjects)
            if (go != null) go.transform.SetParent(transform, true);
        // 回転後にグリッド座標へ再スナップ
        RedrawPiece();
        ghostPiece?.UpdateGhost(this);
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

    void RedrawPiece()
    {
        DestroyPieceObjects();
        foreach (var c in GetWorldCells(origin))
        {
            if (!board.InBounds(c)) continue;
            var go = Instantiate(blockPrefab, transform);
            go.transform.localPosition = new Vector3(c.x, c.y, c.z);
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material       = new Material(mr.sharedMaterial);
                mr.material.color = currentDef.color;
            }
            blockObjects.Add(go);
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
    // カメラの右・上ベクトルをグリッド軸にスナップして返す
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

        // 右移動軸: カメラ right を重力面に投影
        Vector3 projRight = Vector3.ProjectOnPlane(camRight, gravVec);
        if (projRight.sqrMagnitude < 0.01f)
            projRight = Vector3.ProjectOnPlane(camUp, gravVec);
        projRight = projRight.normalized;

        // 奥行き移動軸: カメラ forward を重力面に投影
        // 真上・真下視点では forward が重力と平行になるため、
        // フォールバックとしてカメラ up を使う
        Vector3 projFwd = Vector3.ProjectOnPlane(camFwd, gravVec);
        if (projFwd.sqrMagnitude < 0.01f)
            projFwd = Vector3.ProjectOnPlane(camUp, gravVec);
        projFwd = projFwd.normalized;

        Vector3Int snapRight = SnapToAxis(projRight);
        Vector3Int snapFwd   = SnapToAxis(projFwd);

        // snapRight と snapFwd が同じ軸になった場合は直交する別軸に補正
        if (snapRight == snapFwd || snapRight == -snapFwd)
            snapFwd = new Vector3Int(snapRight.z, snapRight.x, snapRight.y);

        right = snapRight;
        left  = -snapRight;
        up    = snapFwd;
        down  = -snapFwd;
    }

    // ベクトルを最も近い 6 軸方向（±X/Y/Z）に丸める
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
