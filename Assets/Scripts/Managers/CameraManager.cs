using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    public static CameraManager Instance { get; private set; }

    [SerializeField] private Transform cam;
    public BoxCollider2D col { get; private set; }
    [SerializeField] private float activeRange = 2;
    [SerializeField] private float cameraSpeed = 0.1f;

    private float shakeAmt = 0f;
    private float constantShakeAmt = 0f;
    [SerializeField] private float maxPos = 1;

    private Vector3 camTargetPos;

    private void Awake() {
        Instance = this;

        Camera camActual = cam.GetComponent<Camera>();
        col = cam.gameObject.AddComponent<BoxCollider2D>();
        col.size = (Vector2)(camActual.ScreenToWorldPoint(new Vector3(camActual.pixelWidth * activeRange, camActual.pixelHeight * activeRange, camActual.nearClipPlane)) - cam.transform.position);
        col.isTrigger = true;

        camTargetPos = cam.position;
    }

    private void Update() {
        float totalShakeAmt = shakeAmt + constantShakeAmt;
        if (totalShakeAmt > 0) {
            Vector3 pos = Vector3.zero;
            pos.x += totalShakeAmt * totalShakeAmt * maxPos * Random.Range(-1f, 1f);
            pos.y += totalShakeAmt * totalShakeAmt * maxPos * Random.Range(-1f, 1f);
            cam.parent.position = pos;
            shakeAmt -= (shakeAmt > 0 ? Time.deltaTime : 0);
        } else {
            cam.parent.position = Vector3.zero;
        }
        cam.position = Vector3.Lerp(cam.position, camTargetPos, cameraSpeed);
    }

    public void Move(Vector3 pos) {
        camTargetPos = new Vector3(pos.x, pos.y, cam.position.z);
    }

    public void Shake(float amt) {
        shakeAmt += amt;
    }

    public void ConstantShake(float amt) {
        constantShakeAmt += amt;
    }

    public void StopConstantShake(float amt) {
        constantShakeAmt -= amt;
    }

    public void Flash(GameObject obj, float flashTime = 0.05f) {
        StartCoroutine(FlashActual(obj, flashTime));
    }

    private IEnumerator FlashActual(GameObject obj, float flashTime) {
        obj.SetActive(!obj.activeSelf);

        yield return new WaitForSecondsRealtime(flashTime);

        obj.SetActive(!obj.activeSelf);
    }
}
