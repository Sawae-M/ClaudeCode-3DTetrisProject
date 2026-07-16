using System.Collections.Generic;
using UnityEngine;

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

        if (ghostBlockPrefab == null) return;

        Vector3Int gravity    = gravityManager.GravityVector;
        Vector3Int testOrigin = piece.Origin;

        // 落下できる限り進める
        while (true)
        {
            Vector3Int next  = testOrigin + gravity;
            var        cells = piece.GetWorldCells(next);
            if (board.CanPlace(cells))
                testOrigin = next;
            else
                break;
        }

        // 現在位置と同じ（着地済み）なら非表示
        if (testOrigin == piece.Origin) return;

        foreach (var c in piece.GetWorldCells(testOrigin))
        {
            if (!board.InBounds(c)) continue;
            var go = Instantiate(ghostBlockPrefab, transform);
            // PieceController と同じ親空間 (transform = PieceController) に合わせる
            go.transform.localPosition = new Vector3(c.x, c.y, c.z);
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
