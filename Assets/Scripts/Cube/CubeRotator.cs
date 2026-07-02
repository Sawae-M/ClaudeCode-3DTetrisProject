using System.Collections;
using UnityEngine;

public class CubeRotator : MonoBehaviour
{
    [Header("References")]
    public Transform cubeRoot;       // ブロックを全部入れる親オブジェクト
    public GravityManager gravityManager;
    public PieceController pieceController;

    [Header("Settings")]
    public float rotateDuration = 0.3f;

    bool isRotating;

    // キー入力受付 (GameManager から呼ぶか直接 Update で読む)
    void Update()
    {
        if (isRotating || GameManager.Instance?.State != GameState.Playing) return;

        // Q/E : Z軸回転
        if (Input.GetKeyDown(KeyCode.Q)) StartCoroutine(Rotate(Vector3.forward,  90f));
        if (Input.GetKeyDown(KeyCode.E)) StartCoroutine(Rotate(Vector3.forward, -90f));
        // Z/X : X軸回転
        if (Input.GetKeyDown(KeyCode.Z)) StartCoroutine(Rotate(Vector3.right,    90f));
        if (Input.GetKeyDown(KeyCode.X)) StartCoroutine(Rotate(Vector3.right,   -90f));
        // C/V : Y軸回転
        if (Input.GetKeyDown(KeyCode.C)) StartCoroutine(Rotate(Vector3.up,       90f));
        if (Input.GetKeyDown(KeyCode.V)) StartCoroutine(Rotate(Vector3.up,       -90f));
    }

    // 5×5×5 グリッドの中心（セル座標 2,2,2 の中点）
    static readonly Vector3 CubeCenter = new Vector3(2f, 2f, 2f);

    IEnumerator Rotate(Vector3 axis, float angle)
    {
        isRotating = true;

        pieceController?.AttachToCubeRoot(cubeRoot);

        // ワールド座標でのキューブ中心点
        Vector3 worldCenter = cubeRoot.TransformPoint(CubeCenter);

        // 開始・終了クォータニオンを記録して Slerp でアニメ
        Quaternion startRot = cubeRoot.rotation;
        Vector3    startPos = cubeRoot.position;
        Quaternion endRot   = Quaternion.AngleAxis(angle, axis) * startRot;
        // 中心点を維持した終了位置を計算
        Vector3 endPos = worldCenter + Quaternion.AngleAxis(angle, axis) * (startPos - worldCenter);

        float t = 0f;
        while (t < rotateDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.Clamp01(t / rotateDuration);
            cubeRoot.rotation = Quaternion.Slerp(startRot, endRot, s);
            cubeRoot.position = Vector3.Lerp(startPos, endPos, s);
            yield return null;
        }

        // 誤差をゼロにして完全に揃える
        cubeRoot.rotation = endRot;
        cubeRoot.position = endPos;

        gravityManager.RotateGravity(axis, angle);
        pieceController?.DetachFromCubeRoot();

        isRotating = false;
    }

    public bool IsRotating => isRotating;
}
