using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private bool isTesting = false;

    public Person possessed { get; private set; }
    [HideInInspector] public Vector2 pos;

    [Space(20)]
    [SerializeField] private Transform buildingCollision;

    [Space(20)]
    [SerializeField] private float accelerationSpeed = 5f;
    [SerializeField] private float speedMultiplier = 5f;
    private Vector2 currSpeed;
    [SerializeField] private GameObject possessionEffect;
    [SerializeField] private AudioClip possessionSoundEffect;
    [SerializeField] private AudioClip possessionEndSoundEffect;
    [SerializeField] private float camOffset = 0.7f;

    [Space(20)]
    [SerializeField] private float maxTimeInBody = 120;
    private float currTimeInBody = 0;

    [Space(20)]
    [SerializeField] private Transform inventoryParent;
    [SerializeField] private Text inventoryName;
    [SerializeField] private Text inventoryDescription;
    [SerializeField] private GameObject inventoryPrefab;
    [SerializeField] private Vector3 inventoryPos;
    [SerializeField] private Vector3 inventoryOffset;
    private Image[] inventoryObjs;
    private int[] inventoryItemIds;
    private int selectedInventoryIndex = 0;
    [SerializeField] private Color selectedInventoryColor;
    [SerializeField] private Color deselectedInventoryColor;
    private Animator personStatsAnim;
    private Animator inventoryAnim;

    [Space(20)]
    [SerializeField] private Slider staminaBar;
    [SerializeField] private int maxStamina = 100;
    public int stamina { get; protected set; }
    [SerializeField] private int staminaLoss = 40;
    [SerializeField] private int staminaRegenRate = 1;
    private float currStaminaTime = 0;

    [Space(20)]
    [SerializeField] private Slider suspicionBar;
    [SerializeField] private int maxSuspicion = 100;
    public int suspicion { get; protected set; }
    public int suspicionThreshold = 95;

    [Space(20)]
    [SerializeField] private Slider personSpeedBar;
    [SerializeField] private Slider personStrengthBar;
    [SerializeField] private Slider personResilienceBar;

    [Space(20)]
    [SerializeField] private GameObject hitPanel;
    [SerializeField] private Slider healthBar;
    public int maxHealth = 100;
    [SerializeField] private float healthRegenRate = 3;
    private float currHealthTime = 0;

    [Space(20)]
    [SerializeField] private GameObject blindnessPanel;
    private float shakeAmt = 0f;

    [Space(20)]
    public Color enemyHitColor;
    public Color possessedHitColor;

    [Space(20)]
    [SerializeField] private Color suspicionLossColor;
    [SerializeField] private Color suspicionRegenColor;
    [SerializeField] private Color staminaLossColor;
    [SerializeField] private Color staminaRegenColor;
    [SerializeField] private Color healthLossColor;
    [SerializeField] private Color healthRegenColor;

    [Space(20)]
    [SerializeField] private float ambientSoundVolume = 0.55f;
    private Dictionary<int, AudioClip> ambientSounds;
    private Dictionary<int, float> ambientSoundRatios;

    private void Start() {
        Instance = this;

        InputManager.Instance.RegisterLeftMouseBtnClicked(OnLeftMouseBtnClicked);
        InputManager.Instance.RegisterHorizontalVerticalKeyClicked(OnHorizontalVerticalKeyClicked);
        InputManager.Instance.RegisterUpDownArrowKeyClicked(OnUpDownArrowKeyClicked);
        InputManager.Instance.RegisterTabKeyClicked(OnTabKeyClicked);
        InputManager.Instance.RegisterEKeyClicked(OnEKeyClicked);

        personStatsAnim = inventoryParent.parent.parent.GetChild(0).GetComponent<Animator>();
        personStatsAnim.gameObject.SetActive(true);
        inventoryAnim = inventoryParent.parent.GetComponent<Animator>();
        inventoryAnim.gameObject.SetActive(true);

        stamina = maxStamina;
        staminaBar.maxValue = maxStamina;
        staminaBar.value = stamina;

        suspicion = maxSuspicion;
        suspicionBar.maxValue = maxSuspicion;
        suspicionBar.value = suspicion;

        personSpeedBar.maxValue = PeopleManager.Instance.maxPersonSpeed;
        personStrengthBar.maxValue = PeopleManager.Instance.maxPersonStrength;
        personResilienceBar.maxValue = PeopleManager.Instance.maxPersonResilience;

        healthBar.maxValue = maxHealth;

        ambientSounds = new Dictionary<int, AudioClip>();
        ambientSoundRatios = new Dictionary<int, float>();
    }

    private void FixedUpdate() {
        if (possessed != null) {
            if (pos.x != -1 && pos.y != -1) {
                possessed.Move(pos);
                SetSoundRatio();
            }
            SetCameraPosition(possessed.objTransform.position);
            buildingCollision.localPosition = Vector3.zero;
        }
    }

    private void Update() {
        if (possessed == null) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            possessed.AddHealth(-maxHealth);
        }

        if (possessed.health < maxHealth) {
            currHealthTime += Time.deltaTime;
            if (currHealthTime >= healthRegenRate) {
                possessed.AddHealth(1);
                currHealthTime = 0;
            }
        }

        if (stamina < maxStamina) {
            currStaminaTime += Time.deltaTime;
            if (currStaminaTime >= staminaRegenRate) {
                AddStamina(1);
                currStaminaTime = 0;
            }
        }
        currTimeInBody += Time.deltaTime;
        if (!isTesting) {
            float resilience = PeopleManager.Instance.GetResilience(possessed, maxTimeInBody);
            if (currTimeInBody > resilience * 0.83f) {
                float currShakeAmt = Mathf.Abs(1 - ((resilience - currTimeInBody) / resilience)) * 0.25f;
                CameraManager.Instance.ConstantShake(currShakeAmt - shakeAmt);
                shakeAmt = currShakeAmt;
            }
            if (currTimeInBody > resilience * 0.93f && Random.value < 0.05f) {
                CameraManager.Instance.Flash(blindnessPanel, Mathf.Abs(1 - ((resilience - currTimeInBody) / resilience)) * 0.7f);
            }
            if (currTimeInBody > resilience) {
                possessed.AddHealth(-possessed.health);
            }
        }
    }

    private void SetSoundRatio() {
        int posX = Mathf.RoundToInt(pos.x);
        int posY = Mathf.RoundToInt(pos.y);
        Dictionary<int, int> numTypes = new Dictionary<int, int>();
        int total = 0;
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                int zone = CityManager.Instance.BuildingAt(posX + x, posY + y);
                if (zone > CityManager.ROAD) {
                    if (numTypes.ContainsKey(zone)) {
                        numTypes[zone]++;
                    } else {
                        numTypes[zone] = 1;
                    }
                    total++;
                }
            }
        }

        foreach (KeyValuePair<int, int> type in numTypes) {
            if (ambientSounds.ContainsKey(type.Key)) {
                AudioManager.Instance.SetVolume(ambientSounds[type.Key], ((float)type.Value / total) * ambientSoundVolume);
                ambientSoundRatios[type.Key] = ((float)type.Value / total) * ambientSoundVolume;
            } else {
                AudioClip[] sounds = CityManager.Instance.AmbientSoundsAt(type.Key);
                AudioClip sound = sounds[Random.Range(0, sounds.Length)];
                AudioManager.Instance.PlaySound(sound, sound.length, 0, ((float)type.Value / total) * ambientSoundVolume, OnAmbientSoundFinishedPlaying);
                ambientSounds.Add(type.Key, sound);
                ambientSoundRatios.Add(type.Key, ((float)type.Value / total) * ambientSoundVolume);
            }
        }
    }

    private void OnLeftMouseBtnClicked(Vector3 mousePos) {
        List<Person> people = PeopleManager.Instance.people;
        for (int i = 0; i < people.Count; i++) {
            if (people[i].objChild.activeInHierarchy && InputManager.Instance.MouseHits(people[i].pos, mousePos)) {
                SetPossessed(people[i]);
                return;
            }
        }
    }

    private void OnHorizontalVerticalKeyClicked(Vector2 keyPos) {
        if (possessed == null) {
            return;
        }
        if (!possessed.objChild.activeSelf) {
            possessed.objChild.SetActive(true);
        }
        float speed = PeopleManager.Instance.GetSpeed(possessed, BuildingInteriorManager.Instance.PersonSpeedPossessed()) * speedMultiplier;
        currSpeed.x = Mathf.Lerp(currSpeed.x, keyPos.x, Time.deltaTime * accelerationSpeed);
        currSpeed.y = Mathf.Lerp(currSpeed.y, keyPos.y, Time.deltaTime * accelerationSpeed);
        pos = possessed.pos + new Vector3(currSpeed.x * speed * Time.deltaTime, currSpeed.y * speed * Time.deltaTime, 0);
    }

    private void OnUpDownArrowKeyClicked(int key) {
        if (!inventoryParent.gameObject.activeSelf) {
            return;
        }

        selectedInventoryIndex += key;

        if (selectedInventoryIndex < 0) {
            selectedInventoryIndex = inventoryObjs.Length - 1;
        } else if (selectedInventoryIndex > inventoryObjs.Length - 1) {
            selectedInventoryIndex = 0;
        }

        SelectObject(selectedInventoryIndex);
    }

    private void OnTabKeyClicked(int isClicked) {
        if (isClicked != 0) {
            return;
        }

        personStatsAnim.SetBool("IsOn", !personStatsAnim.GetBool("IsOn"));
        inventoryAnim.SetBool("IsOn", !inventoryAnim.GetBool("IsOn"));
    }

    private void OnEKeyClicked(int isClicked) {
        if (isClicked == 0) {
            pos = new Vector2(-1, -1);
        }
    }

    private void OnAmbientSoundFinishedPlaying(AudioClip finishedSound) {
        int key = -1;
        foreach (KeyValuePair<int, AudioClip> ambientSound in ambientSounds) {
            if (finishedSound.Equals(ambientSound.Value)) {
                key = ambientSound.Key;
                break;
            }
        }

        if (key != -1) {
            AudioClip[] sounds = CityManager.Instance.AmbientSoundsAt(key);
            AudioClip sound = sounds[Random.Range(0, sounds.Length)];
            AudioManager.Instance.PlaySound(sound, sound.length, 0, ambientSoundRatios[key], OnAmbientSoundFinishedPlaying);
            ambientSounds[key] = sound;
        }
    }

    public void SetPossessed(Person person, bool showEffect = true) {
        if (person == possessed || stamina < staminaLoss) {
            return;
        }
        
        if (possessed != null) {
            possessed.SetPossessed(false);
        }

        this.possessed = person;
        possessed.SetPossessed(true);
        
        pos = possessed.pos;
        buildingCollision.parent = possessed.objTransform;
        
        if (showEffect) {
            if (possessionEffect != null) {
                GameObject.Instantiate(possessionEffect, pos, Quaternion.identity);
            }
            CameraManager.Instance.Shake(0.5f);
            AddStamina(-staminaLoss);
            AudioManager.Instance.PlaySound(possessionSoundEffect, possessionSoundEffect.length);
            AudioManager.Instance.PlaySound(possessionEndSoundEffect, possessionEndSoundEffect.length, possessionSoundEffect.length / 2);
        }
        
        currTimeInBody = 0;
        CameraManager.Instance.StopConstantShake(shakeAmt);
        
        personSpeedBar.value = person.speed;
        personStrengthBar.value = person.strength;
        personResilienceBar.value = person.resilience;
        healthBar.value = person.health;
        
        UpdateInventory();
        selectedInventoryIndex = -1;
        
        PeopleManager.Instance.SendMsg(new MessagePossession(possessed.objTransform.position));
    }

    private void SetCameraPosition(Vector3 pos) {
        switch (possessed.dir) {
            case 0:
                pos.x += camOffset;
                break;
            case 1:
                pos.y += camOffset;
                break;
            case 2:
                pos.x -= camOffset;
                break;
            case 3:
                pos.y -= camOffset;
                break;
        }
        CameraManager.Instance.Move(pos);
    }

    private void UpdateInventory() {
        ClearInventory();
       
        List<Item> inventory = possessed.inventory;
        inventoryObjs = new Image[inventory.Count];
        inventoryItemIds = new int[inventory.Count];
        
        for (int i = 0; i < inventory.Count; i++) {
            GameObject obj = GameObject.Instantiate(inventoryPrefab, inventoryParent);
            ((RectTransform)(obj.transform)).anchoredPosition = inventoryPos + (inventoryOffset * i);
            obj.name = inventory[i].name;
            obj.transform.GetChild(0).GetComponent<Image>().sprite = inventory[i].icon;
            inventoryObjs[i] = obj.GetComponent<Image>();
            inventoryItemIds[i] = inventory[i].id;
            if (ItemManager.Instance.FindItemObjInWorld(inventory[i].id).activeSelf) {
                selectedInventoryIndex = i;
            }
        }
        
        SelectObject(selectedInventoryIndex);
    }

    private void ClearInventory() {
        if (inventoryParent.childCount > 0) {
            for (int i = inventoryParent.childCount - 1; i >= 0; i--) {
                Destroy(inventoryParent.GetChild(i).gameObject);
            }
        }
    }

    private void SelectObject(int index) {
        for (int i = 0; i < inventoryObjs.Length; i++) {
            if (i == selectedInventoryIndex) {
                inventoryObjs[i].color = selectedInventoryColor;
                ItemManager.Instance.FindItemObjInWorld(inventoryItemIds[i]).SetActive(true);
            } else {
                inventoryObjs[i].color = deselectedInventoryColor;
                ItemManager.Instance.FindItemObjInWorld(inventoryItemIds[i]).SetActive(false);
            }
        }
        
        Item item = ItemManager.Instance.FindItemInWorld(inventoryItemIds[selectedInventoryIndex]);
        
        inventoryName.text = item.name;
        inventoryDescription.text = item.description;
    }

    public void AddHealth(int value) {
        StopAllCoroutines();
        StartCoroutine(AddHealthActual(value));
    }

    private IEnumerator AddHealthActual(int value) {
        healthBar.value = possessed.health;
        currHealthTime = 0;
        Flash((value < 0 ? healthLossColor : healthRegenColor), healthBar);
        if (value < 0) {
            possessed.Flash(possessedHitColor, PeopleManager.Instance, 0.3f);
            hitPanel.SetActive(true);
        }
        if (possessed.health <= 0) {
            PlayerPrefs.SetInt("IsDead", 1);
            SceneManager.LoadScene("MainMenu");
            SetPossessed(null, false);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        hitPanel.SetActive(false);
    }

    public void AddStamina(int value) {
        stamina += value;
        staminaBar.value = stamina;
        currStaminaTime = 0;
        Flash((value < 0 ? staminaLossColor : staminaRegenColor), staminaBar);
    }

    public void AddSuspicion(int value) {
        suspicion += value;
        suspicionBar.value = suspicion;
        Flash((value < 0 ? suspicionLossColor : suspicionRegenColor), suspicionBar);
    }

    public void Flash(Color color, Slider bar, float flashTime = 0.5f) {
        StartCoroutine(FlashActual(color, bar, flashTime));
    }

    private IEnumerator FlashActual(Color color, Slider bar, float flashTime) {
        Image barImg = bar.fillRect.GetComponent<Image>();
        Color orgColor = barImg.color;
        barImg.color = color;

        yield return new WaitForSecondsRealtime(flashTime);

        barImg.color = orgColor;
    }
}

