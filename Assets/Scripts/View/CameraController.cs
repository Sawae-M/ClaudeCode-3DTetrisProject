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
        new Vector3(0,  0, -1), // 0 Top    : 画面上 = 手前方向
        new Vector3(0,  0,  1), // 1 Bottom : 画面上 = 奥方向
        Vector3.up,             // 2 Right
        Vector3.up,             // 3 Left
        Vector3.up,             // 4 Front
        Vector3.up,             // 5 Back
    };

    // ─────────────────────────────────────────
    //  隣接テーブル [現在視点][W=0,S=1,A=2,D=3]
    //  D の軌道: Front(4)→Right(2)→Back(5)→Left(3)→Front(4)
    //  A の軌道: その逆
    //  W: どの横面からでも Top(0) へ
    //  S: どの横面からでも Bottom(1) へ
    // ─────────────────────────────────────────
    static readonly int[,] Adjacency = {
    //       W  S  A  D
    /* 0 */ { 4, 5, 3, 2 }, // Top    : W→Front S→Back  A→Left  D→Right
    /* 1 */ { 4, 5, 3, 2 }, // Bottom : 同上（ミラーなし）
    /* 2 */ { 0, 1, 4, 5 }, // Right  : W→Top  S→Bot   A→Front D→Back
    /* 3 */ { 0, 1, 5, 4 }, // Left   : W→Top  S→Bot   A→Back  D→Front
    /* 4 */ { 0, 1, 3, 2 }, // Front  : W→Top  S→Bot   A→Left  D→Right
    /* 5 */ { 0, 1, 2, 3 }, // Back   : W→Top  S→Bot   A→Right D→Left
    };

    int  currentView = 4;
    bool isMoving    = false;

    void Start() => ApplyInstant(currentView);

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
        Quaternion toRot   = Quaternion.LookRotation(
                                -toDir,          // キューブ中心を向く
                                ViewUp[nextView]);

        float t = 0f;
        while (t < arcDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / arcDuration));

            // 球面上を Slerp で弧を描いて移動
            transform.position = Center + Vector3.Slerp(fromDir, toDir, s) * distance;
            transform.rotation = Quaternion.Slerp(fromRot, toRot, s);
            yield return null;
        }

        transform.position = Center + toDir * distance;
        transform.rotation = toRot;
        currentView = nextView;
        isMoving    = false;
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
