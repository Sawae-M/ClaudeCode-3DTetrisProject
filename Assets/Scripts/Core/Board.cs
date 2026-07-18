using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int Size = 5;

    // true = 埋まっている
    bool[,,] grid = new bool[Size, Size, Size];

    public void Initialize()
    {
        grid = new bool[Size, Size, Size];
    }

    public bool IsEmpty(Vector3Int pos)
    {
        if (!InBounds(pos)) return false;
        return !grid[pos.x, pos.y, pos.z];
    }

    public bool InBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < Size &&
               pos.y >= 0 && pos.y < Size &&
               pos.z >= 0 && pos.z < Size;
    }

    public bool GetCell(int x, int y, int z) => grid[x, y, z];

    public void SetCell(Vector3Int pos, bool value)
    {
        if (InBounds(pos)) grid[pos.x, pos.y, pos.z] = value;
    }

    public bool CanPlace(IEnumerable<Vector3Int> cells)
    {
        foreach (var c in cells)
            if (!InBounds(c) || grid[c.x, c.y, c.z]) return false;
        return true;
    }

    // スポーン専用: 範囲外セルは無視し、範囲内セルが既存ブロックと重なる場合のみ false
    public bool CanSpawn(IEnumerable<Vector3Int> cells)
    {
        foreach (var c in cells)
            if (InBounds(c) && grid[c.x, c.y, c.z]) return false;
        return true;
    }

    public void Place(IEnumerable<Vector3Int> cells)
    {
        foreach (var c in cells)
            if (InBounds(c)) grid[c.x, c.y, c.z] = true;
    }

    // 指定重力方向で揃った面のインデックスを返す（0〜4）
    // 優先順位は FaceEliminator 側で制御
    public List<int> GetFilledFaces(GravityDirection gravity)
    {
        var result = new List<int>();
        for (int i = 0; i < Size; i++)
        {
            if (IsFaceFull(i, gravity)) result.Add(i);
        }
        return result;
    }

    bool IsFaceFull(int index, GravityDirection gravity)
    {
        for (int a = 0; a < Size; a++)
        for (int b = 0; b < Size; b++)
        {
            var pos = SliceToGrid(index, a, b, gravity);
            if (!grid[pos.x, pos.y, pos.z]) return false;
        }
        return true;
    }

    // 指定面をクリアし、その奥のブロックを 1 マスシフトする
    public void ClearFaceAndShift(int faceIndex, GravityDirection gravity)
    {
        // 面をクリア
        for (int a = 0; a < Size; a++)
        for (int b = 0; b < Size; b++)
        {
            var pos = SliceToGrid(faceIndex, a, b, gravity);
            grid[pos.x, pos.y, pos.z] = false;
        }

        // 消えた面より「奥」(重力の反対方向) をシフト
        // 重力方向 = 落下方向。面 0 が最も重力先端。
        // faceIndex+1 〜 4 を faceIndex 方向にシフト
        for (int i = faceIndex; i < Size - 1; i++)
        for (int a = 0; a < Size; a++)
        for (int b = 0; b < Size; b++)
        {
            var dst = SliceToGrid(i,     a, b, gravity);
            var src = SliceToGrid(i + 1, a, b, gravity);
            grid[dst.x, dst.y, dst.z] = grid[src.x, src.y, src.z];
        }
        // 最奥面をクリア
        for (int a = 0; a < Size; a++)
        for (int b = 0; b < Size; b++)
        {
            var pos = SliceToGrid(Size - 1, a, b, gravity);
            grid[pos.x, pos.y, pos.z] = false;
        }
    }

    // 面インデックス + 2次元座標 → グリッド座標
    public static Vector3Int SliceToGrid(int depth, int a, int b, GravityDirection gravity)
    {
        return gravity switch
        {
            GravityDirection.Down    => new Vector3Int(a,        depth,     b),
            GravityDirection.Up      => new Vector3Int(a,        Size-1-depth, b),
            GravityDirection.Left    => new Vector3Int(depth,    a,         b),
            GravityDirection.Right   => new Vector3Int(Size-1-depth, a,    b),
            GravityDirection.Forward => new Vector3Int(a,        b,         depth),
            GravityDirection.Back    => new Vector3Int(a,        b,         Size-1-depth),
            _ => Vector3Int.zero
        };
    }
}
