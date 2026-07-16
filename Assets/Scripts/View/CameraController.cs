using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    public float distance    = 12f;
    public float arcDuration = 0.4f;

    static readonly Vector3 Center = new Vector3(2f, 2f, 2f);

    // 6視点: 0=Top 1=Bottom 2=Right 3=Left 4=Front 5=Back
    static readonly Vector3[] ViewDir = {
        Vector3.up,      // 0 Top
        Vector3.down,    // 1 Bottom
        Vector3.right,   // 2 Right
        Vector3.left,    // 3 Left
        Vector3.back,    // 4 Front (カメラが -Z 側)
        Vector3.forward, // 5 Back  (カメラが +Z 側)
    };

    static readonly Vector3[] ViewUp = {
        new Vector3(0,  0, -1), // 0 Top
        new Vector3(0,  0,  1), // 1 Bottom
        Vector3.up,             // 2 Right
        Vector3.up,             // 3 Left
        Vector3.up,             // 4 Front
        Vector3.up,             // 5 Back
    };

    // 隣接テーブル [現在視点][W=0, S=1, A=2, D=3]
    static readonly int[,] Adjacency = {
    //       W  S  A  D
    /* 0 */ { 4, 5, 3, 2 }, // Top
    /* 1 */ { 4, 5, 3, 2 }, // Bottom
    /* 2 */ { 0, 1, 4, 5 }, // Right
    /* 3 */ { 0, 1, 5, 4 }, // Left
    /* 4 */ { 0, 1, 3, 2 }, // Front
    /* 5 */ { 0, 1, 2, 3 }, // Back
    };

    int  currentView = 4; // デフォルト Front
    bool isMoving    = false;

    void Start()
    {
        ApplyInstant(currentView);
        // 初期視点に合わせて重力を設定
        GravityManager.Instance?.SetFromView(currentView);
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.W)) TryMove(0);
        if (Input.GetKeyDown(KeyCode.S)) TryMove(1);
        if (Input.GetKeyDown(KeyCode.A)) TryMove(2);
        if (Input.GetKeyDown(KeyCode.D)) TryMove(3);
    }

    void TryMove(int dir)
    {
        int next = Adjacency[currentView, dir];
        StartCoroutine(ArcTo(next));
    }

    IEnumerator ArcTo(int nextView)
    {
        isMoving = true;

        Vector3    fromDir = ViewDir[currentView];
        Vector3    toDir   = ViewDir[nextView];
        Quaternion fromRot = transform.rotation;
        Quaternion toRot   = Quaternion.LookRotation(-toDir, ViewUp[nextView]);

        float t = 0f;
        while (t < arcDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / arcDuration));
            transform.position = Center + Vector3.Slerp(fromDir, toDir, s) * distance;
            transform.rotation = Quaternion.Slerp(fromRot, toRot, s);
            yield return null;
        }

        transform.position = Center + toDir * distance;
        transform.rotation = toRot;
        currentView = nextView;

        // 視点変更 → 重力方向を自動更新
        GravityManager.Instance?.SetFromView(currentView);

        isMoving = false;
    }

    void ApplyInstant(int idx)
    {
        currentView        = idx;
        transform.position = Center + ViewDir[idx] * distance;
        transform.rotation = Quaternion.LookRotation(-ViewDir[idx], ViewUp[idx]);
    }

    public int  CurrentView => currentView;
    public bool IsMoving    => isMoving;
}
