using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInteriorManager : MonoBehaviour {
    public static BuildingInteriorManager Instance { get; private set; }

    private BuildingResidentialInteriorManager residentialManager;
    private BuildingStoreInteriorManager storeManager;
    private BuildingOfficeInteriorManager officeManager;
    private SpecialBuildingManager specialBuildingManager;

    [SerializeField] private GameObject cityParent;
    [SerializeField] private GameObject roomParent;

    [Space(20)]
    [SerializeField] private float outsidePersonScale = 0.4f;
    [SerializeField] private float insidePersonScale = 1f;

    [Space(20)]
    [SerializeField] private float outsidePersonSpeedAI = 1.3f;
    [SerializeField] private float insidePersonSpeedAI = 2f;

    [Space(20)]
    [SerializeField] private float outsidePersonSpeedPossessed = 2.2f;
    [SerializeField] private float insidePersonSpeedPossessed = 2.7f;

    public bool isInside { get; protected set; }
    private List<Person> outsidePeople;
    public PathInteriorGraph pathfindingGraph;

    private int generalSeed;

    [HideInInspector] public bool isMainAction;
    [HideInInspector] public Vector2 buildingPos;

    private Vector2 spawnPos;
    private IBuildingInteriorManager interior;

    private int floor = 0;

    private void Awake() {
        Instance = this;

        generalSeed = Mathf.Abs((int)DateTime.Now.Ticks / 10000000);
        isMainAction = false;
        isInside = false;
    }

    private void Start() {
        residentialManager = GameObject.FindObjectOfType<BuildingResidentialInteriorManager>();
        storeManager = GameObject.FindObjectOfType<BuildingStoreInteriorManager>();
        officeManager = GameObject.FindObjectOfType<BuildingOfficeInteriorManager>();
        specialBuildingManager = GameObject.FindObjectOfType<SpecialBuildingManager>();

        BuildingTypeInteriorManager.roomParent = roomParent.transform;

        InputManager.Instance.RegisterEKeyClicked(OnEKeyClicked);
    }

    private void OnEKeyClicked(int isClicked) {
        if (isClicked != 0) {
            return;
        }

        // Switches all attributes from outside to inside or vice versa
        if (isInside) {
            PlayerManager.Instance.possessed.Move((Vector2)roomParent.transform.position, false, false);

            cityParent.SetActive(true);

            roomParent.SetActive(false);
            foreach (Transform child in roomParent.transform) {
                Destroy(child.gameObject);
            }

            isInside = false;

            // Keeps track of outside and inside people and switches them when exiting or entering
            PeopleManager.Instance.SetCurrPeople(outsidePeople);
            for (int i = 0; i < outsidePeople.Count; i++) {
                outsidePeople[i].objTransform.localScale = new Vector3(outsidePersonScale, outsidePersonScale, outsidePersonScale);
            }
        } else {
            if (buildingPos.x != -1 && buildingPos.y != -1 && isMainAction && !CityManager.Instance.IsWalkable((int)buildingPos.x, (int)buildingPos.y)) {
                BuildingInteriorType type = CityManager.Instance.InteriorAt((int)buildingPos.x, (int)buildingPos.y);
                
                if (type == BuildingInteriorType.NonEnterable) {
                    return;
                }

                spawnPos = buildingPos;
                // Debug.Log((int)spawnPos.x + "" + (int)spawnPos.y + "" + floor + "" + generalSeed);
                roomParent.transform.position = PlayerManager.Instance.possessed.pos;
                outsidePeople = PeopleManager.Instance.currPeople;
                isInside = true;

                List<Person> insidePeople = new List<Person>();

                // Generates interior based on type by deferring to other manager
                switch (type) {
                    case BuildingInteriorType.Residential:
                        insidePeople = residentialManager.GenerateInterior(int.Parse(spawnPos.x + "" + spawnPos.y + "" + floor + "" + generalSeed), spawnPos);
                        cityParent.SetActive(false);
                        roomParent.SetActive(true);
                        residentialManager.DisplayInterior();
                        pathfindingGraph = new PathInteriorGraph(residentialManager);
                        interior = residentialManager;
                        PlayerManager.Instance.possessed.Move((Vector2)PlayerManager.Instance.possessed.pos + residentialManager.SpawnPos(spawnPos), false, false);
                        break;
                    case BuildingInteriorType.Store:
                        insidePeople = storeManager.GenerateInterior(int.Parse(spawnPos.x + "" + spawnPos.y + "" + floor + "" + generalSeed), spawnPos);
                        cityParent.SetActive(false);
                        roomParent.SetActive(true);
                        storeManager.DisplayInterior();
                        pathfindingGraph = new PathInteriorGraph(storeManager);
                        interior = storeManager;
                        PlayerManager.Instance.possessed.Move((Vector2)PlayerManager.Instance.possessed.pos + storeManager.SpawnPos(spawnPos), false, false);
                        break;
                    case BuildingInteriorType.Office:
                        insidePeople = officeManager.GenerateInterior(int.Parse(spawnPos.x + "" + spawnPos.y + "" + floor + "" + generalSeed), spawnPos);
                        cityParent.SetActive(false);
                        roomParent.SetActive(true);
                        officeManager.DisplayInterior();
                        pathfindingGraph = new PathInteriorGraph(officeManager);
                        interior = officeManager;
                        PlayerManager.Instance.possessed.Move((Vector2)PlayerManager.Instance.possessed.pos + officeManager.SpawnPos(spawnPos), false, false);
                        break;
                    case BuildingInteriorType.Special:
                        insidePeople = specialBuildingManager.GenerateInterior(int.Parse(spawnPos.x + "" + spawnPos.y + "" + floor + "" + generalSeed), spawnPos);
                        cityParent.SetActive(false);
                        roomParent.SetActive(true);
                        specialBuildingManager.DisplayInterior();
                        pathfindingGraph = new PathInteriorGraph(specialBuildingManager);
                        interior = specialBuildingManager;
                        PlayerManager.Instance.possessed.Move((Vector2)PlayerManager.Instance.possessed.pos + specialBuildingManager.SpawnPos(spawnPos), false, false);
                        break;
                }

                // Keeps track of outside and inside people and switches them when exiting or entering
                PeopleManager.Instance.SetCurrPeople(insidePeople);
                for (int i = 0; i < insidePeople.Count; i++) {
                    insidePeople[i].objTransform.localScale = new Vector3(insidePersonScale, insidePersonScale, insidePersonScale);
                }
            }
        }
    }

    public PathGraph PathfindingGraph(bool getActual = false) {
        if (getActual) {
            return pathfindingGraph;
        }
        if (!isInside) {
            return CityManager.Instance.pathfindingGraph;
        }
        return pathfindingGraph;
    }

    public string GetTooltip() {
        // Debug.Log(CityManager.Instance.InteriorAt((int) buildingPos.x, (int) buildingPos.y) + " " + CityManager.Instance.IsWalkable((int) buildingPos.x, (int) buildingPos.y));
        if (buildingPos.x != -1 && buildingPos.y != -1 && isMainAction && !CityManager.Instance.IsWalkable((int)buildingPos.x, (int)buildingPos.y) && CityManager.Instance.InteriorAt((int)buildingPos.x, (int)buildingPos.y) != BuildingInteriorType.NonEnterable) {
            return "Press E to enter.";
        }
        return "";
    }

    public Vector2 InsidePos(Vector2 pos) {
        return (pos - spawnPos);
    }

    public Vector2 OutsidePos(Vector2 pos) {
        return (pos + spawnPos);
    }

    public int Width() {
        return interior.Width();
    }

    public int Height() {
        return interior.Height();
    }

    public float PersonScale() {
        return (isInside ? insidePersonScale : outsidePersonScale);
    }

    public float PersonSpeedAI() {
        return (isInside ? insidePersonSpeedAI : outsidePersonSpeedAI);
    }

    public float PersonSpeedPossessed() {
        return (isInside ? insidePersonSpeedPossessed : outsidePersonSpeedPossessed);
    }
}

