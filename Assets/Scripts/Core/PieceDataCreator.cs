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

        Create(folder, "I_Piece", Color.cyan, new[]
        {
            new Vector3Int(0,0,0), new Vector3Int(1,0,0),
            new Vector3Int(2,0,0), new Vector3Int(3,0,0),
        });

        Create(folder, "O_Piece", Color.yellow, new[]
        {
            new Vector3Int(0,0,0), new Vector3Int(1,0,0),
            new Vector3Int(0,0,1), new Vector3Int(1,0,1),
        });

        Create(folder, "L_Piece", new Color(1f,0.5f,0f), new[]
        {
            new Vector3Int(0,0,0), new Vector3Int(0,1,0),
            new Vector3Int(0,2,0), new Vector3Int(1,0,0),
        });

        Create(folder, "T_Piece", new Color(0.6f,0f,1f), new[]
        {
            new Vector3Int(0,0,0), new Vector3Int(1,0,0),
            new Vector3Int(2,0,0), new Vector3Int(1,0,1),
        });

        Create(folder, "S_Piece", Color.green, new[]
        {
            new Vector3Int(0,0,0), new Vector3Int(1,0,0),
            new Vector3Int(1,0,1), new Vector3Int(2,0,1),
        });

        Create(folder, "Tripod_Piece", Color.red, new[]
        {
            new Vector3Int(0,0,0), new Vector3Int(1,0,0),
            new Vector3Int(0,1,0), new Vector3Int(0,0,1),
        });

        Create(folder, "Twist_Piece", Color.magenta, new[]
        {
            new Vector3Int(0,0,0), new Vector3Int(1,0,0),
            new Vector3Int(1,1,0), new Vector3Int(1,1,1),
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
