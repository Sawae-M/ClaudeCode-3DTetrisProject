// Editor専用: ピースScriptableObjectを一括生成するメニュー
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class PieceDataCreator
{
    [MenuItem("TetrisCube/Create Piece Assets")]
    static void CreateAll()
    {
        string folder = "Assets/ScriptableObjects/Pieces";
        AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Pieces");

        Create(folder, "I_Piece",    Color.cyan, new[]
        {
            new Vector3Int(0,0,0), new Vector3Int(1,0,0),
            new Vector3Int(2,0,0), new Vector3Int(3,0,0),
        });

        Create(folder, "Cross_Piece", Color.yellow, new[]
        {
            new Vector3Int( 0,0, 0), new Vector3Int( 1,0, 0),
            new Vector3Int(-1,0, 0), new Vector3Int( 0,0, 1),
            new Vector3Int( 0,0,-1),
        });

        Create(folder, "S_Piece", Color.green, new[]
        {
            new Vector3Int(0,0,0), new Vector3Int(1,0,0),
            new Vector3Int(1,0,1), new Vector3Int(2,0,1),
        });

        Create(folder, "L_Piece", new Color(1f,0.5f,0f), new[]
        {
            new Vector3Int(0,0,0), new Vector3Int(0,0,1),
            new Vector3Int(0,0,2), new Vector3Int(1,0,2),
        });

        Create(folder, "Z_Piece", Color.red, new[]
        {
            // S の反転
            new Vector3Int(0,0,1), new Vector3Int(1,0,1),
            new Vector3Int(1,0,0), new Vector3Int(2,0,0),
        });

        Create(folder, "Zigzag_Piece", Color.magenta, new[]
        {
            // XY方向に段差
            new Vector3Int(0,0,0), new Vector3Int(1,0,0),
            new Vector3Int(1,1,0), new Vector3Int(2,1,0),
        });

        Create(folder, "ZigzagR_Piece", new Color(0.5f,0f,1f), new[]
        {
            // Zigzag の反転
            new Vector3Int(0,1,0), new Vector3Int(1,1,0),
            new Vector3Int(1,0,0), new Vector3Int(2,0,0),
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Piece assets created in " + folder);
    }

    static void Create(string folder, string name, Color color, Vector3Int[] cells)
    {
        var asset = ScriptableObject.CreateInstance<PieceDefinition>();
        asset.pieceName = name;
        asset.color     = color;
        asset.cells     = cells;
        AssetDatabase.CreateAsset(asset, $"{folder}/{name}.asset");
    }
}
#endif
