using System.Collections;
using UnityEngine;

public class CubeRotator : MonoBehaviour
{
    [Header("References")]
    public Transform cubeRoot;
    public GravityManager gravityManager;
    public PieceController pieceController;

    [Header("Settings")]
    public float rotateDuration = 0.3f;

    bool isRotating;

    static readonly Vector3 CubeCenter = new Vector3(2f, 2f, 2f);

    void Update()
    {
        if (isRotating || GameManager.Instance?.State != GameState.Playing) return;

        if (Input.GetKeyDown(KeyCode.Q)) StartCoroutine(Rotate(Vector3.forward,  90f));
        if (Input.GetKeyDown(KeyCode.E)) StartCoroutine(Rotate(Vector3.forward, -90f));
        if (Input.GetKeyDown(KeyCode.Z)) StartCoroutine(Rotate(Vector3.right,    90f));
        if (Input.GetKeyDown(KeyCode.X)) StartCoroutine(Rotate(Vector3.right,   -90f));
        if (Input.GetKeyDown(KeyCode.C)) StartCoroutine(Rotate(Vector3.up,       90f));
        if (Input.GetKeyDown(KeyCode.V)) StartCoroutine(Rotate(Vector3.up,      -90f));
    }

    IEnumerator Rotate(Vector3 axis, float angle)
    {
        isRotating = true;

        // 回転前にピースをキューブルートの子にして一緒に回す
        pieceController?.AttachToCubeRoot(cubeRoot);

        Vector3    worldCenter = cubeRoot.TransformPoint(CubeCenter);
        Quaternion startRot    = cubeRoot.rotation;
        Vector3    startPos    = cubeRoot.position;
        Quaternion endRot      = Quaternion.AngleAxis(angle, axis) * startRot;
        Vector3    endPos      = worldCenter + Quaternion.AngleAxis(angle, axis) * (startPos - worldCenter);

        float t = 0f;
        while (t < rotateDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.Clamp01(t / rotateDuration);
            cubeRoot.rotation = Quaternion.Slerp(startRot, endRot, s);
            cubeRoot.position = Vector3.Lerp(startPos, endPos, s);
            yield return null;
        }

        cubeRoot.rotation = endRot;
        cubeRoot.position = endPos;

        // 重力方向を更新してからピースをデタッチ（DetachFromCubeRoot内でRedrawが走る）
        gravityManager.RotateGravity(axis, angle);
        pieceController?.DetachFromCubeRoot();

        isRotating = false;
    }

    public bool IsRotating => isRotating;
}
