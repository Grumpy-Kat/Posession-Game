using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingTypeInteriorManager : MonoBehaviour, IBuildingInteriorManager {
    public const int AIR = 0;
    public const int HALLWAY = 1;

    public static Transform roomParent;

    [SerializeField] protected int width = 25;
    [SerializeField] protected int height = 25;

    [Space(20)]
    [SerializeField] protected GameObject floorPrefab;
    [SerializeField] protected GameObject furniturePrefab;

    [Space(20)]
    [SerializeField] private List<Sprite> floorSprites;
    [SerializeField] private Sprite doorSprite;

    [Space(20)]
    [SerializeField] protected List<Room> requiredRooms;
    [SerializeField] protected List<Room> extraRooms;

    protected System.Random rand;

    protected int[,] map;
    protected int[,] furnitureMap;

    // Area of each room
    protected List<Rect> roomAreas;
    // Mum of each roomType
    protected Dictionary<int, int> roomTypes;
    // Positions of door in each room, corresponds to index of roomAreas
    protected List<Dictionary<Vector3, string>> doors;

    protected List<Person> people;

    public Vector2 SpawnPos(Vector2 pos) { return new Vector2((int)width / 2, 1); }

    public virtual List<Person> GenerateInterior(int seed, Vector2 pos) {
        map = new int[width, height];

        furnitureMap = new int[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                furnitureMap[x, y] = -1;
            }
        }

        rand = new System.Random(seed);

        roomAreas = new List<Rect>();
        roomTypes = new Dictionary<int, int>();
        doors = new List<Dictionary<Vector3, string>>();

        GeneratePeople();

        return people;
    }

    public void DisplayInterior() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                GameObject obj = GameObject.Instantiate(floorPrefab, roomParent);
                obj.transform.localPosition = new Vector3(x, y, 0);
                obj.name = "Floor_" + x + "_" + y + "_" + map[x, y];
                obj.GetComponent<SpriteRenderer>().sprite = GetSprite(x, y, floorSprites);
                if (furnitureMap[x, y] != -1) {
                    GameObject furnitureObj = GameObject.Instantiate(furniturePrefab, roomParent);
                    furnitureObj.transform.localPosition = new Vector3(x, y, 0);
                    furnitureObj.name = "Furniture" + x + "_" + y + "_" + map[x, y];
                    SpriteRenderer renderer = furnitureObj.GetComponent<SpriteRenderer>();
                    renderer.sprite = GetFurnitureSprite(x, y);
                    renderer.sortingOrder = ((width - y) * 10);
                }
            }
        }
    }

    protected void GeneratePeople() {
        people = new List<Person>();
        people.Add(PlayerManager.Instance.possessed);
        // TODO: finish this
    }

    protected void DisplayDoor(float x, float y, float rot) {
        GameObject obj = GameObject.Instantiate(furniturePrefab, new Vector3(x, y, 0), Quaternion.Euler(new Vector3(0, 0, rot)), roomParent);
        obj.name = "Door_" + x + "_" + y;
        obj.GetComponent<SpriteRenderer>().sprite = doorSprite;
    }

    protected Sprite GetSprite(int x, int y, List<Sprite> sprites) {
        string spriteName = "_";

        if (y - 1 >= 0) {
            if (map[x, y - 1] == map[x, y]) {
                spriteName += "N";
            }
        }

        if (x + 1 < width) {
            if (map[x + 1, y] == map[x, y]) {
                spriteName += "E";
            }
        }

        if (y + 1 < height) {
            if (map[x, y + 1] == map[x, y]) {
                spriteName += "S";
            }
        }

        if (x - 1 >= 0) {
            if (map[x - 1, y] == map[x, y]) {
                spriteName += "W";
            }
        }

        for (int i = 0; i < sprites.Count; i++) {
            if (sprites[i].name.Equals(spriteName)) {
                return sprites[i];
            }
        }

        Debug.Log("Sprite" + spriteName + " not found.");
        return null;
    }

    protected Sprite GetFurnitureSprite(int x, int y) {
        Furniture furniture = GetRoom(map[x, y]).GetFurnitureIdMap()[furnitureMap[x, y]];
        int dir = GetFurnitureDir(furniture, x, y);
        for (int i = 0; i < furniture.sprites.Length; i++) {
            if (furniture.sprites[i].dir == dir) {
                return furniture.sprites[i].sprite;
            }
        }
        return null;
    }

    protected void FillRoom(Rect rect, int type) {
        for (int x = (int)rect.xMin; x < (int)rect.xMax; x++) {
            for (int y = (int)rect.yMin; y < (int)rect.yMax; y++) {
                map[x, y] = type;
            }
        }
    }

    protected int IsRoomPlaceable(Rect rect, Room room) {
        int area = (int)(rect.width * rect.height);

        if (area > room.MaxArea() || rect.width > room.MaxSide() || rect.height > room.MaxSide()) {
            return 1;
        }

        if (area < room.MinArea() || rect.width < room.MinSide() || rect.height < room.MinSide()) {
            return -1;
        }

        return 0;
    }

    protected int GetFurnitureDir(Furniture furniture, int orgX, int orgY) {
        int[,] requirements = furniture.GetRequirements();
        if (IsFurniturePlaceable(requirements, orgX, orgY, false)) {
            return 1;
        }

        requirements = Furniture.RotateRequirements(requirements);
        if (IsFurniturePlaceable(requirements, orgX, orgY, false)) {
            return 0;
        }

        requirements = Furniture.RotateRequirements(requirements);
        if (IsFurniturePlaceable(requirements, orgX, orgY, false)) {
            return 3;
        }

        requirements = Furniture.RotateRequirements(requirements);
        if (IsFurniturePlaceable(requirements, orgX, orgY, false)) {
            return 2;
        }

        return -1;
    }

    protected bool IsFurniturePlaceable(Furniture furniture, int orgX, int orgY) {
        // dir = 0
        int[,] requirements = furniture.GetRequirements();
        if (IsFurniturePlaceable(requirements, orgX, orgY)) {
            return true;
        }

        // dir = 1
        requirements = Furniture.RotateRequirements(requirements);
        if (IsFurniturePlaceable(requirements, orgX, orgY)) {
            return true;
        }

        // dir = 2
        requirements = Furniture.RotateRequirements(requirements);
        if (IsFurniturePlaceable(requirements, orgX, orgY)) {
            return true;
        }

        // dir = 3
        requirements = Furniture.RotateRequirements(requirements);
        if (IsFurniturePlaceable(requirements, orgX, orgY)) {
            return true;
        }

        return false;
    }

    protected bool IsFurniturePlaceable(int[,] requirements, int orgX, int orgY, bool isPlacing = true) {
        if (orgX < 0 || orgX >= width || orgY < 0 || orgY >= height) {
            return false;
        }
        if (furnitureMap[orgX, orgY] != -1 && isPlacing) {
            return false;
        }
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (orgX + x < 0 || orgX + x >= width || orgY + y < 0 || orgY + y >= height) {
                    continue;
                }
                int requirement = requirements[x + 1, y + 1];
                bool isWall = IsWall(orgX + x, orgY + y, orgX, orgY);
                if (requirement == Furniture.NONE) {
                    continue;
                }
                if (requirement == Furniture.WALL && !isWall) {
                    return false;
                }
                if (requirement == Furniture.NOWALL && isWall) {
                    return false;
                }
                if ((requirement == Furniture.EMPTY && (isWall || furnitureMap[orgX + x, orgY + y] != -1)) && (!(x == 0 && y == 0 && !isPlacing))) {
                    return false;
                }
                if (requirement == Furniture.OBJECT && furnitureMap[orgX + x, orgY + y] == -1) {
                    return false;
                }
                if (requirement >= 0 && furnitureMap[orgX + x, orgY + y] != requirement) {
                    return false;
                }
            }
        }
        return true;
    }

    protected Room GetRoom(int roomType) {
        return ((roomType < 0) ? extraRooms[-roomType] : requiredRooms[roomType]);
    }

    public int[,] Map() { return map; }
    public int Width() { return width; }
    public int Height() { return height; }

    public bool IsWall(int x, int y) {
        return (IsWall(x, y, x + 1, y) || IsWall(x, y, x - 1, y) || IsWall(x, y, x, y + 1) || IsWall(x, y, x, y - 1));
    }

    public bool IsWall(int x, int y, int dirX, int dirY) {
        return (x < 0 || x >= width || y < 0 || y >= height) || (dirX < 0 || dirX >= width || dirY < 0 || dirY >= height) || (map[x, y] != map[dirX, dirY]);
    }

    public Vector2[] Neighbors(int x, int y) {
        Vector2[] neighbors = new Vector2[4];

        neighbors[0] = new Vector2(-1, -1);
        if (y - 1 >= 0) {
            neighbors[0] = new Vector2(x, y - 1);
        }

        neighbors[1] = new Vector2(-1, -1);
        if (x + 1 < width) {
            neighbors[1] = new Vector2(x + 1, y);
        }

        neighbors[2] = new Vector2(-1, -1);
        if (y + 1 < height) {
            neighbors[2] = new Vector2(x, y + 1);
        }

        neighbors[3] = new Vector2(-1, -1);
        if (x - 1 >= 0) {
            neighbors[3] = new Vector2(x - 1, y);
        }

        return neighbors;
    }
}

