using UnityEngine;

// グリッドの全ブロックを描画・更新する
public class BoardRenderer : MonoBehaviour
{
    [Header("References")]
    public Board board;
    public Transform cubeRoot;

    [Header("Prefabs")]
    public GameObject blockPrefab;
    public GameObject gridLinePrefab;   // 任意：ワイヤーフレーム表示用

    [Header("Materials")]
    public Material placedMaterial;
    public Material gridMaterial;

    GameObject[,,] blockObjects;

    public void Initialize()
    {
        blockObjects = new GameObject[Board.Size, Board.Size, Board.Size];

        // ブロック用オブジェクトをプール生成
        for (int x = 0; x < Board.Size; x++)
        for (int y = 0; y < Board.Size; y++)
        for (int z = 0; z < Board.Size; z++)
        {
            var go = Instantiate(blockPrefab, cubeRoot);
            go.transform.localPosition = new Vector3(x, y, z);
            go.SetActive(false);
            blockObjects[x, y, z] = go;
        }

        DrawGridLines();
    }

    // ボードの状態に合わせてブロック表示を更新
    public void FullRedraw()
    {
        for (int x = 0; x < Board.Size; x++)
        for (int y = 0; y < Board.Size; y++)
        for (int z = 0; z < Board.Size; z++)
        {
            blockObjects[x, y, z].SetActive(board.GetCell(x, y, z));
        }
    }

    public void SetCell(Vector3Int pos, bool active)
    {
        if (board.InBounds(pos))
            blockObjects[pos.x, pos.y, pos.z].SetActive(active);
    }

    void DrawGridLines()
    {
        if (gridLinePrefab == null) return;

        int n = Board.Size + 1; // 6本の線
        // X方向
        for (int y = 0; y < n; y++)
        for (int z = 0; z < n; z++)
            CreateLine(new Vector3(-0.5f, y - 0.5f, z - 0.5f), Vector3.right, Board.Size);

        // Y方向
        for (int x = 0; x < n; x++)
        for (int z = 0; z < n; z++)
            CreateLine(new Vector3(x - 0.5f, -0.5f, z - 0.5f), Vector3.up, Board.Size);

        // Z方向
        for (int x = 0; x < n; x++)
        for (int y = 0; y < n; y++)
            CreateLine(new Vector3(x - 0.5f, y - 0.5f, -0.5f), Vector3.forward, Board.Size);
    }

    void CreateLine(Vector3 start, Vector3 dir, float len)
    {
        var go = Instantiate(gridLinePrefab, cubeRoot);
        go.transform.localPosition = start + dir * (len / 2f);
        go.transform.localScale    = new Vector3(
            dir.x != 0 ? len : 0.05f,
            dir.y != 0 ? len : 0.05f,
            dir.z != 0 ? len : 0.05f);
    }
}
