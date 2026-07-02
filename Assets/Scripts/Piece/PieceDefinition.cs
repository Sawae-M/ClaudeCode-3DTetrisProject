using UnityEngine;

[CreateAssetMenu(menuName = "TetrisCube/PieceDefinition")]
public class PieceDefinition : ScriptableObject
{
    [Tooltip("ピース名 (I, Cross, S, L, Z, Zigzag, ZigzagR)")]
    public string pieceName;

    [Tooltip("ローカル座標オフセット群（重力 Down 基準）")]
    public Vector3Int[] cells;

    [Tooltip("ブロック色")]
    public Color color = Color.white;
}
