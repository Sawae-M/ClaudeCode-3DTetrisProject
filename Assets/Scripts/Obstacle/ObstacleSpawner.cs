using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public Board board;
    public BoardRenderer boardRenderer;
    public GravityManager gravityManager;

    [Header("Settings")]
    public bool  spawnEnabled  = false; // 邪魔ブロック機能のON/OFF（デフォルト無効）
    public float spawnInterval = 20f;   // 何秒ごとに邪魔ブロックを出すか
    public int   blocksPerWave = 3;     // 1波あたりのブロック数
    public GameObject warningBlockPrefab;

    float timer;

    void Update()
    {
        if (!spawnEnabled) return;
        if (GameManager.Instance?.State != GameState.Playing) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            StartCoroutine(SpawnWave());
        }
    }

    IEnumerator SpawnWave()
    {
        var positions = PickRandomPositions(blocksPerWave);

        // 警告表示（半透明）
        var warnings = new List<GameObject>();
        foreach (var pos in positions)
        {
            var go = Instantiate(warningBlockPrefab);
            go.transform.position = new Vector3(pos.x, pos.y, pos.z);
            warnings.Add(go);
        }

        // 1ターン待機（1秒固定）
        yield return new WaitForSeconds(1.0f);

        foreach (var go in warnings)
            Destroy(go);

        // 空きスペースに確定配置
        foreach (var pos in positions)
        {
            if (board.IsEmpty(pos))
            {
                board.SetCell(pos, true);
                boardRenderer.SetCell(pos, true);
            }
        }
    }

    List<Vector3Int> PickRandomPositions(int count)
    {
        var result = new List<Vector3Int>();
        int tries = 0;
        while (result.Count < count && tries < 100)
        {
            tries++;
            var pos = new Vector3Int(
                Random.Range(0, Board.Size),
                Random.Range(0, Board.Size),
                Random.Range(0, Board.Size));
            if (board.IsEmpty(pos) && !result.Contains(pos))
                result.Add(pos);
        }
        return result;
    }
}
