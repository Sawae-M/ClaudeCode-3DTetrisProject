using System.Collections.Generic;
using UnityEngine;

// 面消去の優先順位と連続消去を管理
public class FaceEliminator : MonoBehaviour
{
    // 仕様：上→下→左→右→前→後 の優先順位
    static readonly GravityDirection[] Priority = {
        GravityDirection.Up,
        GravityDirection.Down,
        GravityDirection.Left,
        GravityDirection.Right,
        GravityDirection.Forward,
        GravityDirection.Back,
    };

    public int EliminateAll(Board board, GravityDirection currentGravity)
    {
        int totalCleared = 0;

        // 全重力方向を優先順位順に確認して消去
        foreach (var dir in Priority)
        {
            var faces = board.GetFilledFaces(dir);
            if (faces.Count == 0) continue;

            // インデックスが小さい（重力先端寄り）順に消去
            faces.Sort();
            foreach (int idx in faces)
            {
                board.ClearFaceAndShift(idx, dir);
                totalCleared++;
            }
        }

        return totalCleared;
    }
}
