// Editor専用: シーンの GameObject を自動セットアップするメニュー
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class SceneSetup
{
    [MenuItem("TetrisCube/Setup Scene")]
    static void Setup()
    {
        // --- CubeRoot ---
        var cubeRoot = new GameObject("CubeRoot");
        cubeRoot.transform.position = Vector3.zero;

        // --- Managers ---
        var managers = new GameObject("Managers");
        var gm            = managers.AddComponent<GameManager>();
        var gravityMgr    = managers.AddComponent<GravityManager>();
        var faceElim      = managers.AddComponent<FaceEliminator>();

        // --- Board ---
        var boardGO = new GameObject("Board");
        var board         = boardGO.AddComponent<Board>();
        var boardRenderer = boardGO.AddComponent<BoardRenderer>();
        boardRenderer.cubeRoot = cubeRoot.transform;

        // --- Piece ---
        var pieceGO   = new GameObject("PieceController");
        var pieceCtr  = pieceGO.AddComponent<PieceController>();
        var spawner   = pieceGO.AddComponent<PieceSpawner>();
        var ghost     = pieceGO.AddComponent<GhostPiece>();

        // --- Cube Rotator ---
        var rotatorGO = new GameObject("CubeRotator");
        var rotator   = rotatorGO.AddComponent<CubeRotator>();
        rotator.cubeRoot      = cubeRoot.transform;
        rotator.gravityManager = gravityMgr;
        rotator.pieceController = pieceCtr;

        // --- Camera ---
        var camGO = new GameObject("MainCamera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        var camCtrl = camGO.AddComponent<CameraController>();

        // --- Obstacle ---
        var obstGO  = new GameObject("ObstacleSpawner");
        var obstacle = obstGO.AddComponent<ObstacleSpawner>();
        obstacle.board = board;
        obstacle.boardRenderer = boardRenderer;
        obstacle.gravityManager = gravityMgr;

        // --- UI ---
        var uiGO = new GameObject("UIManager");
        var ui   = uiGO.AddComponent<UIManager>();

        // GameManager の参照を設定
        gm.board           = board;
        gm.boardRenderer   = boardRenderer;
        gm.pieceSpawner    = spawner;
        gm.gravityManager  = gravityMgr;
        gm.cubeRotator     = rotator;
        gm.cameraController = camCtrl;
        gm.obstacleSpawner = obstacle;
        gm.uiManager       = ui;

        // PieceController の参照
        pieceCtr.board           = board;
        pieceCtr.boardRenderer   = boardRenderer;
        pieceCtr.gravityManager  = gravityMgr;
        pieceCtr.pieceSpawner    = spawner;
        pieceCtr.faceEliminator  = faceElim;
        pieceCtr.ghostPiece      = ghost;
        pieceCtr.cameraController = camCtrl;

        spawner.pieceController = pieceCtr;
        spawner.board           = board;
        spawner.gravityManager  = gravityMgr;

        ghost.board           = board;
        ghost.gravityManager  = gravityMgr;

        boardRenderer.board = board;

        // ライト
        var lightGO = new GameObject("DirectionalLight");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        Debug.Log("Scene setup complete! Assign Prefabs and Piece assets in Inspector.");
    }
}
#endif
