using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour {
    public static InputManager Instance;

    private PlayerManager playerManager;

    Action<Vector3> cbLeftMouseBtnClicked;
    Action<Vector3> cbRightMouseBtnClicked;
    Action<Vector2> cbHorizontalVerticalKeyClicked;
    Action<int> cbUpDownArrowKeyClicked;
    Action<int> cbEKeyClicked;
    Action<int> cbQKeyClicked;
    Action<int> cbTabKeyClicked;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        UpdateMouse();
        UpdateKeyboard();
    }

    private void UpdateMouse() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && cbLeftMouseBtnClicked != null) {
            cbLeftMouseBtnClicked(mousePos);
        }
        if (Input.GetMouseButtonDown(2) && cbRightMouseBtnClicked != null) {
            cbRightMouseBtnClicked(mousePos);
        }
    }

    private void UpdateKeyboard() {
        if (cbHorizontalVerticalKeyClicked != null) {
            cbHorizontalVerticalKeyClicked(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && cbUpDownArrowKeyClicked != null) {
            cbUpDownArrowKeyClicked(-1);
        } else if (Input.GetKeyDown(KeyCode.DownArrow) && cbUpDownArrowKeyClicked != null) {
            cbUpDownArrowKeyClicked(1);
        }

        if (cbEKeyClicked != null) {
            if (Input.GetKeyDown(KeyCode.E)) {
                cbEKeyClicked(0);
            } else if (Input.GetKeyUp(KeyCode.E)) {
                cbEKeyClicked(1);
            }
        }

        if (cbQKeyClicked != null) {
            if (Input.GetKeyDown(KeyCode.Q)) {
                cbQKeyClicked(0);
            } else if (Input.GetKeyUp(KeyCode.Q)) {
                cbQKeyClicked(1);
            }
        }

        if (cbTabKeyClicked != null) {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                cbTabKeyClicked(0);
            } else if (Input.GetKeyUp(KeyCode.Tab)) {
                cbTabKeyClicked(1);
            }
        }
    }

    public bool MouseHits(Vector3 otherPos) {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return (Mathf.FloorToInt(mousePos.x) == Mathf.FloorToInt(otherPos.x) && Mathf.FloorToInt(mousePos.y) == Mathf.FloorToInt(otherPos.y));
    }

    public bool MouseHits(Vector3 otherPos, Vector3 mousePos) {
        return (Mathf.FloorToInt(mousePos.x) == Mathf.FloorToInt(otherPos.x) && Mathf.FloorToInt(mousePos.y) == Mathf.FloorToInt(otherPos.y));
    }

    public void RegisterLeftMouseBtnClicked(Action<Vector3> cb) {
        cbLeftMouseBtnClicked += cb;
    }

    public void UnregisterLeftMouseBtnClicked(Action<Vector3> cb) {
        cbLeftMouseBtnClicked -= cb;
    }

    public void RegisterRightMouseBtnClicked(Action<Vector3> cb) {
        cbRightMouseBtnClicked += cb;
    }

    public void UnregisterRightMouseBtnClicked(Action<Vector3> cb) {
        cbRightMouseBtnClicked -= cb;
    }

    public void RegisterHorizontalVerticalKeyClicked(Action<Vector2> cb) {
        cbHorizontalVerticalKeyClicked += cb;
    }

    public void UnregisterHorizontalVerticalKeyClicked(Action<Vector2> cb) {
        cbHorizontalVerticalKeyClicked -= cb;
    }

    public void RegisterUpDownArrowKeyClicked(Action<int> cb) {
        cbUpDownArrowKeyClicked += cb;
    }

    public void UnregisterUpDownArrowKeyClicked(Action<int> cb) {
        cbUpDownArrowKeyClicked -= cb;
    }

    public void RegisterEKeyClicked(Action<int> cb) {
        cbEKeyClicked += cb;
    }

    public void UnregisterEKeyClicked(Action<int> cb) {
        cbEKeyClicked -= cb;
    }

    public void RegisterQKeyClicked(Action<int> cb) {
        cbQKeyClicked += cb;
    }

    public void UnregisterQKeyClicked(Action<int> cb) {
        cbQKeyClicked -= cb;
    }

    public void RegisterTabKeyClicked(Action<int> cb) {
        cbTabKeyClicked += cb;
    }

    public void UnregisterTabKeyClicked(Action<int> cb) {
        cbTabKeyClicked -= cb;
    }
}

