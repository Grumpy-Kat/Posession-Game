using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecialBuildingManager : MonoBehaviour, IBuildingInteriorManager {
    [SerializeField] private SpecialBuilding[] buildings;
    // Places sprite in world that maps to a special prefab which does not use procedural generation to create interior
    // Allows for custom interactions and missions, such as police station
    public Dictionary<SpecialBuildingType, SpecialBuilding> buildingTypesMap { get; protected set; }
    public Dictionary<Sprite, SpecialBuildingType> buildingsSpritesMap { get; protected set; }

    private List<Vector2> locs = new List<Vector2>();
    private List<SpecialBuildingType> types = new List<SpecialBuildingType>();

    private int[,] interiorMap;
    private int interiorWidth;
    private int interiorHeight;

    [Space(20)]
    [SerializeField] private int spawnMinPoliceOfficer = 0;
    [SerializeField] private int spawnMaxPoliceOfficer = 4;
    [SerializeField] private float spawnRatePoliceOfficer = 45;
    private float currTimePoliceOfficer = 0;

    [Space(20)]
    [SerializeField] private int spawnMinSoldier = 0;
    [SerializeField] private int spawnMaxSoldier = 1;
    [SerializeField] private float spawnRateSoldier = 45;
    private float currTimeSoldier = 0;

    private void Update() {
        if (!BuildingInteriorManager.Instance.isInside) {
            currTimePoliceOfficer += Time.deltaTime;
            if (currTimePoliceOfficer > spawnRatePoliceOfficer) {
                int spawnAmt = Random.Range(spawnMinPoliceOfficer, spawnMaxPoliceOfficer + 1);
                Spawn(PersonType.Soldier, SpecialBuildingType.PoliceStation, spawnAmt);
                currTimePoliceOfficer = 0;
            }

            currTimeSoldier += Time.deltaTime;
            if (currTimeSoldier > spawnRateSoldier) {
                int spawnAmt = Random.Range(spawnMinSoldier, spawnMaxSoldier + 1);
                Spawn(PersonType.Soldier, SpecialBuildingType.PoliceStation, spawnAmt);
                currTimeSoldier = 0;
            }
        }
    }

    private void Spawn(PersonType personType, SpecialBuildingType buildingType, int spawnAmt) {
        for (int i = 0; i < locs.Count; i++) {
            if (types[i] == buildingType) {
                for (int j = 0; j < spawnAmt; j++) {
                    Vector2 pos = CityManager.Instance.FindClosestPos(locs[i]);
                    PeopleManager.Instance.SpawnPerson(pos, personType, "Special" + i + j, CameraManager.Instance.col);
                }
            }
        }
    }

    public void GenerateBuildingsMaps() {
        GenerateBuildingTypesMap();
        GenerateBuildingSpritesMap();
    }

    private void GenerateBuildingTypesMap() {
        buildingTypesMap = new Dictionary<SpecialBuildingType, SpecialBuilding>();
        for (int i = 0; i < buildings.Length; i++) {
            buildingTypesMap.Add(buildings[i].type, buildings[i]);
        }
    }

    private void GenerateBuildingSpritesMap() {
        buildingsSpritesMap = new Dictionary<Sprite, SpecialBuildingType>();
        SpecialBuilding building;
        for (int i = 0; i < buildings.Length; i++) {
            building = buildings[i];
            for (int j = 0; j < building.NumSpecialZones(); j++) {
                Sprite[] zoneBuildings = building.GetZone(j + CityManager.ROAD + 1);
                for (int k = 0; k < zoneBuildings.Length; k++) {
                    if (!buildingsSpritesMap.ContainsKey(zoneBuildings[k])) {
                        buildingsSpritesMap.Add(zoneBuildings[k], building.type);
                    }
                }
            }
        }
    }

    public void AddBuilding(int x, int y, SpecialBuildingType type) {
        locs.Add(new Vector2(x, y));
        types.Add(type);
    }

    public bool IsSpecialSprite(string name) {
        for (int i = 0; i < buildingsSpritesMap.Keys.Count; i++) {
            if (buildingsSpritesMap.Keys.ToArray()[i].name.Equals(name)) {
                return true;
            }
        }
        return false;
    }

    public Vector2 SpawnPos(Vector2 pos) { return new Vector2((int)buildingTypesMap[types[locs.IndexOf(pos)]].width / 2, 1); }

    public List<Person> GenerateInterior(int seed, Vector2 pos) {
        SpecialBuilding building = buildingTypesMap[types[locs.IndexOf(pos)]];
        interiorWidth = building.width;
        interiorHeight = building.height;
        System.Random rand = new System.Random(seed);
        GameObject obj = GameObject.Instantiate(building.interiors[rand.Next(0, building.interiors.Length)], BuildingTypeInteriorManager.roomParent);
        obj.transform.localPosition = Vector3.zero;
        GenerateInteriorMap(obj.transform.GetChild(0));
        SetSortingOrders(obj.transform.GetChild(1));
        SetSortingOrders(obj.transform.GetChild(2));
        return GeneratePeople(obj.transform);
    }

    private void GenerateInteriorMap(Transform parent) {
        interiorMap = new int[interiorWidth, interiorHeight];
        for (int i = 0; i < parent.childCount; i++) {
            Transform child = parent.GetChild(i);
            interiorMap[(int)child.localPosition.x, (int)child.localPosition.y] = int.Parse(child.name);
        }
    }

    private void SetSortingOrders(Transform parent) {
        SpriteRenderer[] renderers = parent.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].sortingOrder = (int)((interiorWidth - renderers[i].transform.localPosition.y) * 10) + renderers[i].sortingOrder;
        }
    }

    protected List<Person> GeneratePeople(Transform parent) {
        List<Person> people = new List<Person>();
        people.Add(PlayerManager.Instance.possessed);
        return people;
    }

    public void DisplayInterior() { }

    public int[,] Map() { return interiorMap; }
    public int Width() { return interiorWidth; }
    public int Height() { return interiorHeight; }

    public bool IsWall(int x, int y) {
        return (IsWall(x, y, x + 1, y) || IsWall(x, y, x - 1, y) || IsWall(x, y, x, y + 1) || IsWall(x, y, x, y - 1));
    }

    public bool IsWall(int x, int y, int dirX, int dirY) {
        return (x < 0 || x >= interiorWidth || y < 0 || y >= interiorHeight) || (dirX < 0 || dirX >= interiorWidth || dirY < 0 || dirY >= interiorHeight) || (interiorMap[x, y] != interiorMap[dirX, dirY]);
    }

    public Vector2[] Neighbors(int x, int y) {
        Vector2[] neighbors = new Vector2[4];

        neighbors[0] = new Vector2(-1, -1);
        if (y - 1 >= 0) {
            neighbors[0] = new Vector2(x, y - 1);
        }

        neighbors[1] = new Vector2(-1, -1);
        if (x + 1 < interiorWidth) {
            neighbors[1] = new Vector2(x + 1, y);
        }

        neighbors[2] = new Vector2(-1, -1);
        if (y + 1 < interiorHeight) {
            neighbors[2] = new Vector2(x, y + 1);
        }

        neighbors[3] = new Vector2(-1, -1);
        if (x - 1 >= 0) {
            neighbors[3] = new Vector2(x - 1, y);
        }

        return neighbors;
    }
}