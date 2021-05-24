using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PeopleManager : MonoBehaviour {
    public static PeopleManager Instance { get; private set; }

    private PlayerManager playerManager;

    [SerializeField] private bool isTesting = false;

    [Space(20)]
    [SerializeField] private int minPeople = 300;
    [SerializeField] private int maxPeople = 700;

    [Space(20)]
    [SerializeField] private GameObject worldObj;
    [SerializeField] private Transform personParent;

    [Space(20)]
    [SerializeField] private Color[] skinTones;
    [SerializeField] private Texture2D bodySprite;
    [SerializeField] private Texture2D shadowSprite;

    [Space(20)]
    [SerializeField] private Texture2D[] femaleHairStyles;
    [SerializeField] private Texture2D[] maleHairStyles;

    [Space(20)]
    [SerializeField] private PersonTypeInfo[] personTypes;
    public Dictionary<PersonType, PersonTypeInfo> personTypesMap;

    [Space(20)]
    [SerializeField] private Material defaultMat;
    [SerializeField] private Material possessedMat;

    [Space(20)]
    [SerializeField] private GameObject possessedEffect;

    [Space(20)]
    [SerializeField] private int minInventoryItems = 2;
    [SerializeField] private int maxInventoryItems = 6;

    [Space(20)]
    [SerializeField] private int minPersonSpeed = 20;
    public int maxPersonSpeed = 100;
    [SerializeField] private float speedOffset = 0.5f;

    [Space(20)]
    [SerializeField] private int minPersonStrength = 20;
    public int maxPersonStrength = 100;

    [Space(20)]
    [SerializeField] private int minPersonResilience = 20;
    public int maxPersonResilience = 100;
    [SerializeField] private float resilienceOffset = 45;

    [Space(20)]
    [SerializeField] private AudioClip[] femaleHurtSounds;
    [SerializeField] private AudioClip[] maleHurtSounds;

    public List<Person> people { get; private set; }
    public List<Person> currPeople { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        playerManager = GameObject.FindObjectOfType<PlayerManager>();

        GeneratePersonTypesMap();
        InitPeopleClass();

        int numPeople = Random.Range(minPeople, maxPeople);
        people = new List<Person>();

        Vector2 pos = CityManager.Instance.FindClosestPos(new Vector2(CityManager.Instance.Width() / 2, CityManager.Instance.Height() / 2));
        SpawnPerson(pos, PersonType.Regular, "0", null);

        for (int i = 1; i < numPeople; i++) {
            SpawnPerson(CityManager.Instance.GetRandWeightedPos(), PersonType.Regular, i.ToString(), CameraManager.Instance.col);
        }

        currPeople = people;

        people[0].SetPossessed(true);
        playerManager.SetPossessed(people[0], false);
    }

    private void Update() {
        if (personParent.gameObject.activeInHierarchy) {
            for (int i = 0; i < currPeople.Count; i++) {
                Person person = currPeople[i];
                if (person.objChild.activeInHierarchy) {
                    person.Update(Time.deltaTime);
                }
            }
        }
    }

    private void FixedUpdate() {
        if (personParent.gameObject.activeInHierarchy && !isTesting) {
            float speed = BuildingInteriorManager.Instance.PersonSpeedAI();
            for (int i = 0; i < currPeople.Count; i++) {
                Person person = currPeople[i];
                if (person.objChild.activeInHierarchy && !person.isPossessed) {
                    person.FixedUpdate(Time.deltaTime, GetSpeed(person, speed));
                }
            }
        }
    }

    private void GeneratePersonTypesMap() {
        personTypesMap = new Dictionary<PersonType, PersonTypeInfo>();
        for (int i = 0; i < personTypes.Length; i++) {
            personTypesMap.Add(personTypes[i].type, personTypes[i]);
        }
    }

    private void InitPeopleClass() {
        Person.defaultMat = defaultMat;
        Person.possessedMat = possessedMat;
        Person.possessedEffect = possessedEffect;
    }

    public Person SpawnPerson(Vector2 pos, PersonType type = PersonType.Regular, string name = "", BoxCollider2D col = null, PersonStateType defaultState = PersonStateType.Other, PatrolRoute route = null) {
        GameObject obj = GameObject.Instantiate(worldObj, new Vector3(pos.x, pos.y, (pos.y / 10) - 0.01f), Quaternion.identity, personParent);
        obj.name = "Person" + name;
        Transform objChild = obj.transform.GetChild(0);

        PersonTypeInfo info = personTypesMap[type];
        int gender = Random.Range(0, 2);
        Texture2D[][] anims = RenderTexture(info, gender);
        CityManager.Instance.BuildMesh(objChild.gameObject, 1, 1);
        MeshRenderer renderer = objChild.GetComponent<MeshRenderer>();
        renderer.material.mainTexture = anims[0][0];
        renderer.material.shader = Shader.Find("Transparent/Diffuse");

        float scale = BuildingInteriorManager.Instance.PersonScale();
        obj.transform.localScale = new Vector3(scale, scale, scale);

        if (route != null) {
            PatrolRoute newRoute = obj.AddComponent<PatrolRoute>();
            newRoute.patrolStops = route.patrolStops;
        }

        if (col != null) {
            if ((pos.x < col.bounds.min.x) || (pos.y < col.bounds.min.y) || (pos.x > col.bounds.max.x) || (pos.y > col.bounds.max.y)) {
                objChild.gameObject.SetActive(false);
            } else {
                objChild.gameObject.SetActive(true);
            }
        }

        PersonState state = PersonState.GetState(defaultState == PersonStateType.Other ? info.defaultState : defaultState);

        List<Item> inventory = new List<Item>();
        inventory.AddRange(ItemManager.Instance.GenerateItems(ItemManager.Instance.GetEmptyItem(), objChild));
        ItemManager.Instance.FindItemObjInWorld(inventory[0].id).SetActive(true);
        inventory.AddRange(ItemManager.Instance.GenerateItems(Random.Range(minInventoryItems, maxInventoryItems + 1), objChild, new List<ItemType>(new ItemType[] { ItemType.Artifact })));
        if (info.requiredItems != null && info.requiredItems.Count > 0) {
            List<Item> requiredInventory = ItemManager.Instance.GenerateItems(info.requiredItems, objChild);
            ItemManager.Instance.FindItemObjInWorld(requiredInventory[Random.Range(0, requiredInventory.Count)].id).SetActive(true);
            inventory.AddRange(requiredInventory);
        }

        int personSpeed = Random.Range(minPersonSpeed, maxPersonSpeed - minPersonSpeed);
        int personStrength = Random.Range(minPersonStrength, maxPersonStrength);
        int personResilience = Random.Range(minPersonResilience, maxPersonResilience);

        AudioClip[] hurtSounds = (gender == 0 ? femaleHurtSounds : maleHurtSounds);

        people.Add(new Person(obj, anims, PersonGlobalState.Instance, state, inventory, personSpeed, personStrength, personResilience, playerManager.maxHealth, hurtSounds, info.attacks));
        return people[people.Count - 1];
    }

    private Texture2D[][] RenderTexture(PersonTypeInfo info, int gender) {
        Color[] shadowPixels = shadowSprite.GetPixels();
        Color bodyColor = skinTones[Random.Range(0, skinTones.Length)];
        Color[] bodyPixels = bodySprite.GetPixels();
        Color topColor = Color.white;
        Color[] topPixels = new Color[0];
        Color bottomColor = Color.white;
        Color[] bottomPixels = new Color[0];
        Color shoesColor = Color.white;
        Color[] shoesPixels = new Color[0];
        Color hairColor = Color.white;
        Color[] hairPixels = new Color[0];

        if (gender == 0) {
            if (info.femaleTopStyles.Count() > 0) {
                topPixels = info.femaleTopStyles[Random.Range(0, info.femaleTopStyles.Length)].GetPixels();
            }
            if (info.femaleBottomStyles.Count() > 0) {
                bottomPixels = info.femaleBottomStyles[Random.Range(0, info.femaleBottomStyles.Length)].GetPixels();
            }
            if (info.femaleShoeStyles.Count() > 0) {
                shoesPixels = info.femaleShoeStyles[Random.Range(0, info.femaleShoeStyles.Length)].GetPixels();
            }
            hairPixels = femaleHairStyles[Random.Range(0, femaleHairStyles.Length)].GetPixels();
        } else {
            if (info.maleTopStyles.Count() > 0) {
                topPixels = info.maleTopStyles[Random.Range(0, info.maleTopStyles.Length)].GetPixels();
            }
            if (info.maleBottomStyles.Count() > 0) {
                bottomPixels = info.maleBottomStyles[Random.Range(0, info.maleBottomStyles.Length)].GetPixels();
            }
            if (info.maleShoeStyles.Count() > 0) {
                shoesPixels = info.maleShoeStyles[Random.Range(0, info.maleShoeStyles.Length)].GetPixels();
            }
            hairPixels = maleHairStyles[Random.Range(0, maleHairStyles.Length)].GetPixels();
        }

        if (info.colorRandomly) {
            topColor = Random.ColorHSV();
            bottomColor = Random.ColorHSV();
            shoesColor = Random.ColorHSV();
        }
        hairColor = Random.ColorHSV();

        Texture2D[][] anims = new Texture2D[2][];
        // Idle
        anims[0] = RenderAnimationTexture(1, 96, bodySprite.width, bodySprite.height, shadowPixels, bodyColor, bodyPixels, topColor, topPixels, bottomColor, bottomPixels, shoesColor, shoesPixels, hairColor, hairPixels);
        // Walking
        anims[1] = RenderAnimationTexture(6, 96, bodySprite.width, bodySprite.height, shadowPixels, bodyColor, bodyPixels, topColor, topPixels, bottomColor, bottomPixels, shoesColor, shoesPixels, hairColor, hairPixels);
        return anims;
    }

    private Texture2D[] RenderAnimationTexture(int animLength, int animStartPos, int bodyWidth, int bodyHeight, Color[] shadowPixels, Color bodyColor, Color[] bodyPixels, Color topColor, Color[] topPixels, Color bottomColor, Color[] bottomPixels, Color shoesColor, Color[] shoesPixels, Color hairColor, Color[] hairPixels) {
        Texture2D[] anim = new Texture2D[animLength];

        for (int i = 0; i < animLength; i++) {
            anim[i] = new Texture2D(32, 34);
            anim[i].filterMode = FilterMode.Point;
            Color[] texPixels = anim[i].GetPixels();
            for (int x = 0; x < anim[i].width; x++) {
                for (int y = anim[i].height - 1; y >= 0; y--) {
                    int texPixel = x + (int)anim[i].width * y;
                    if (y < 2) {
                        texPixels[texPixel] = shadowPixels[x + (int)anim[i].width * y];
                        continue;
                    }
                    int pixel = (x + (anim[i].width * i)) + bodyWidth * ((y - 2) + animStartPos);
                    if (hairPixels.Length > 0 && hairPixels[pixel].a > 0) {
                        texPixels[texPixel] = hairPixels[pixel] * hairColor;
                    } else if (shoesPixels.Length > 0 && shoesPixels[pixel].a > 0) {
                        texPixels[texPixel] = shoesPixels[pixel] * shoesColor;
                    } else if (topPixels.Length > 0 && topPixels[pixel].a > 0) {
                        texPixels[texPixel] = topPixels[pixel] * topColor;
                    } else if (bottomPixels.Length > 0 && bottomPixels[pixel].a > 0) {
                        texPixels[texPixel] = bottomPixels[pixel] * bottomColor;
                    } else {
                        if (y >= anim[i].height - 2 || (bodyPixels.Length > 0 && bodyPixels[pixel].a > 0)) {
                            texPixels[texPixel] = bodyPixels[pixel] * bodyColor;
                        } else {
                            texPixels[texPixel] = shadowPixels[x + (int)anim[i].width * y];
                        }
                    }
                }
            }
            anim[i].SetPixels(texPixels);
            anim[i].Apply();
        }

        return anim;
    }

    public void DeletePerson(Person person) {
        people.Remove(person);
    }

    public Person FindPerson(GameObject obj) {
        for (int i = 0; i < people.Count; i++) {
            if (people[i].obj == obj || people[i].objChild == obj) {
                return people[i];
            }
        }
        return null;
    }

    public void SetCurrPeople(List<Person> newPeople) {
        for (int i = 0; i < currPeople.Count; i++) {
            currPeople[i].objChild.SetActive(false);
        }
        currPeople = newPeople;
        for (int i = 0; i < currPeople.Count; i++) {
            Person person = currPeople[i];
            BoxCollider2D col = CameraManager.Instance.col;
            if (!person.isPossessed && ((person.pos.x < col.bounds.min.x) || (person.pos.y < col.bounds.min.y) || (person.pos.x > col.bounds.max.x) || (person.pos.y > col.bounds.max.y))) {
                person.objChild.gameObject.SetActive(false);
            } else {
                person.objChild.gameObject.SetActive(true);
            }
        }
    }

    public void AddCurrPeople(List<Person> newPeople) {
        int prevAmt = currPeople.Count;
        currPeople.AddRange(newPeople);
        for (int i = prevAmt; i < currPeople.Count; i++) {
            Person person = currPeople[i];
            BoxCollider2D col = CameraManager.Instance.col;
            if (!person.isPossessed && ((person.pos.x < col.bounds.min.x) || (person.pos.y < col.bounds.min.y) || (person.pos.x > col.bounds.max.x) || (person.pos.y > col.bounds.max.y))) {
                person.objChild.gameObject.SetActive(false);
            } else {
                person.objChild.gameObject.SetActive(true);
            }
        }
    }

    public float GetSpeed(Person person, float baseSpeed) {
        return (baseSpeed * ((float)person.speed / maxPersonSpeed)) + speedOffset;
    }

    public float GetResilience(Person person, float baseResilience) {
        return (baseResilience * (1f - ((float)person.resilience / maxPersonResilience))) + resilienceOffset;
    }

    public void SendMsg(Message msg) {
        if (personParent.gameObject.activeInHierarchy && !isTesting) {
            for (int i = 0; i < currPeople.Count; i++) {
                Person person = currPeople[i];
                if (person.objChild.activeInHierarchy && !person.isPossessed) {
                    person.globalState.ReceiveMsg(person, msg);
                    person.currState.ReceiveMsg(person, msg);
                }
            }
        }
    }
}

