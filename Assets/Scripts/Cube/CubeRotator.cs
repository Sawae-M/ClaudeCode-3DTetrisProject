using UnityEngine;

// キューブの物理回転は廃止。
// 重力方向はカメラ視点（CameraController）が管理する。
// このクラスはシーンとの互換性のために残しているが何もしない。
public class CubeRotator : MonoBehaviour
{
    [Header("References (未使用)")]
    public Transform cubeRoot;
    public GravityManager gravityManager;
    public PieceController pieceController;

    public bool IsRotating => false;

    public void AttachToCubeRoot(Transform root) { }
    public void DetachFromCubeRoot() { }
}
