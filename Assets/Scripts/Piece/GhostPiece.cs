using System.Collections.Generic;
using UnityEngine;

// 落下先を半透明で表示するゴーストピース
public class GhostPiece : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject ghostBlockPrefab;

    [Header("References")]
    public Board board;
    public GravityManager gravityManager;

    List<GameObject> ghostObjects = new();

    public void UpdateGhost(PieceController piece)
    {
        ClearGhost();

        Vector3Int gravity = gravityManager.GravityVector;
        Vector3Int testOrigin = piece.Origin;

        // 落下可能な限り進める
        while (true)
        {
            Vector3Int next = testOrigin + gravity;
            var cells = piece.GetWorldCells(next);
            if (board.CanPlace(cells))
                testOrigin = next;
            else
                break;
        }

        // 現在位置と同じなら表示しない
        if (testOrigin == piece.Origin) return;

        var ghostCells = piece.GetWorldCells(testOrigin);
        foreach (var c in ghostCells)
        {
            if (!board.InBounds(c)) continue;
            var go = Instantiate(ghostBlockPrefab, transform);
            go.transform.position = new Vector3(c.x, c.y, c.z);
            ghostObjects.Add(go);
        }
    }

    public void ClearGhost()
    {
        foreach (var go in ghostObjects)
            if (go != null) Destroy(go);
        ghostObjects.Clear();
    }
}
