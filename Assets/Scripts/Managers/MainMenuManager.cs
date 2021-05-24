using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    [SerializeField] private GameObject backgroundPrefab;
    [SerializeField] private Transform backgroundParent;
    [SerializeField] private float scrollSpeed = 3000;
    [SerializeField] private Sprite[] backgrounds;
    [SerializeField] private float[] backgroundSpeeds;
    private Transform[] backgroundObjs;

    private Transform cam;
    private Vector2 screenBounds;

    [Space(20)]
    [SerializeField] private GameObject mainMenuObj;
    [SerializeField] private GameObject loadingObj;
    [SerializeField] private GameObject settingsObj;
    [SerializeField] private GameObject deathObj;

    private void Start() {
        // Controls parallax scrolling main menu
        if (PlayerPrefs.GetInt("IsDead", 0) == 1) {
            PlayerPrefs.SetInt("IsDead", 0);
            mainMenuObj.SetActive(false);
            deathObj.SetActive(true);
        } else {
            mainMenuObj.SetActive(true);
            deathObj.SetActive(false);
        }

        cam = Camera.main.transform;
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.position.z));
        backgroundObjs = new Transform[backgrounds.Length];
        Vector2 camSize = new Vector2(Camera.main.aspect * Camera.main.orthographicSize * 2, Camera.main.orthographicSize * 2);
        for (int i = 0; i < backgrounds.Length; i++) {
            GameObject obj = GameObject.Instantiate(backgroundPrefab, backgroundParent);
            obj.name = "Background_" + i;
            SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in renderers) {
                renderer.sprite = backgrounds[i];
                renderer.sortingOrder = i;
            }
            obj.transform.localScale = new Vector3(obj.transform.localScale.x * (camSize.x / backgrounds[i].bounds.size.x), obj.transform.localScale.y * (camSize.y / backgrounds[i].bounds.size.y), 1);
            backgroundObjs[i] = obj.transform;
        }
    }

    private void Update() {
        cam.position += new Vector3(scrollSpeed * Time.deltaTime, 0, 0);
    }

    private void LateUpdate() {
        for (int i = 0; i < backgroundObjs.Length; i++) {
            Transform firstChild = backgroundObjs[i].GetChild(0);
            Transform lastChild = backgroundObjs[i].GetChild(2);
            float objWidth = lastChild.GetComponent<SpriteRenderer>().bounds.extents.x;
            if (cam.position.x + screenBounds.x > lastChild.position.x + objWidth) {
                firstChild.SetAsLastSibling();
                firstChild.position = new Vector3(lastChild.position.x + (objWidth * 2), 0, 0);
            } else if (cam.position.x - screenBounds.x < firstChild.position.x - objWidth) {
                lastChild.SetAsFirstSibling();
                lastChild.position = new Vector3(firstChild.position.x - (objWidth * 2), 0, 0);
            }
            float speed = 1 - Mathf.Clamp01(Mathf.Abs(cam.position.z / backgroundSpeeds[i]));
            backgroundObjs[i].Translate(Vector3.right * speed * Time.deltaTime);
        }
    }

    public void NewGame() {
        mainMenuObj.SetActive(false);
        loadingObj.SetActive(true);
        settingsObj.SetActive(false);
        deathObj.SetActive(false);
        StartCoroutine(LoadGameActual());
    }

    public void LoadGame() {
        mainMenuObj.SetActive(false);
        loadingObj.SetActive(true);
        settingsObj.SetActive(false);
        deathObj.SetActive(false);
        StartCoroutine(LoadGameActual());
    }

    public void Settings() {
        mainMenuObj.SetActive(false);
        loadingObj.SetActive(false);
        settingsObj.SetActive(true);
        deathObj.SetActive(false);
    }

    public void PlayAgain() {
        mainMenuObj.SetActive(true);
        loadingObj.SetActive(false);
        settingsObj.SetActive(false);
        deathObj.SetActive(false);
    }

    private IEnumerator LoadGameActual() {
        AsyncOperation op = SceneManager.LoadSceneAsync("Game");
        Slider slider = loadingObj.GetComponentInChildren<Slider>();
        while (!op.isDone) {
            slider.value = Mathf.Lerp(slider.value, Mathf.Clamp01(op.progress), Time.deltaTime * 10);
            yield return null;
        }
    }
}
