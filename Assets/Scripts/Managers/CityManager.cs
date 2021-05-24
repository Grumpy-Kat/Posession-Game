using System.Collections.Generic;
using UnityEngine;

public class CityManager : MonoBehaviour {
    public static CityManager Instance { get; private set; }

    private SpecialBuildingManager specialBuildingManager;

    public const int AIR = 0;
    public const int ROAD = 1;

    public const int EFFECTS = 32767;

    [SerializeField] private int worldWidth = 100;
    [SerializeField] private int worldHeight = 100;

    [SerializeField] private int tileWidth = 32;
    [SerializeField] private int tileHeight = 32;
    [SerializeField] private int maxBuildingHeight = 3;

    [Space(20)]
    [SerializeField] private GameObject worldObj;
    [SerializeField] private Texture2D groundTex;
    [SerializeField] private Transform cityParent;

    [Space(20)]
    [SerializeField] private List<Color> colorPalette;
    [SerializeField] private int minSaturation = 50;
    [SerializeField] private int saturationInc = 5;
    [SerializeField] private int maxSaturation = 170;
    [SerializeField] private int minValue = 50;
    [SerializeField] private int valueInc = 5;
    [SerializeField] private int maxValue = 200;
    [SerializeField] private int numVariations = 3;
    private List<Color> colorPaletteVariations;

    [Space(20)]
    [SerializeField] private Zone[] zones;

    [Space(20)]
    [SerializeField] private int minRoadGap = 2;
    [SerializeField] private int maxRoadGap = 6;
    [SerializeField] private int minRoadLength = 30;

    private int[,] map;
    private string[,] mapInteriors;

    public PathCityGraph pathfindingGraph;
    private List<Vector2> weightedMap;

    private void Awake() {
        Instance = this;

        specialBuildingManager = GameObject.FindObjectOfType<SpecialBuildingManager>();
        specialBuildingManager.GenerateBuildingsMaps();

        BuildGroundMesh();
        map = new int[worldWidth, worldHeight];
        mapInteriors = new string[worldWidth, worldHeight];
        GenerateCity();
        DisplayCity();

        pathfindingGraph = new PathCityGraph();

        GenerateWeightedMap();
    }

    private void GenerateCity() {
        GenerateBuildings();
        GenerateVerticalRoads();
        GenerateHorizontalRoads();
    }

    private void GenerateBuildings() {
        Vector2 offset = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));
        for (int x = 0; x < worldWidth; x++) {
            for (int y = 0; y < worldHeight; y++) {
                map[x, y] = Mathf.FloorToInt(Mathf.PerlinNoise(offset.x + ((float)x / worldWidth), offset.y + ((float)y / worldHeight)) * (zones.Length - (ROAD + 1))) + ROAD + 1;
            }
        }
    }

    private void GenerateVerticalRoads() {
        int x = 0;
        while (x < worldWidth) {
            int yOrg = (int)Random.Range(0, worldHeight - minRoadLength);
            int yMax = yOrg + Random.Range(minRoadLength, worldWidth - yOrg);
            for (int y = yOrg; y < yMax; y++) {
                map[x, y] = ROAD;
            }
            x += Random.Range(minRoadGap, maxRoadGap);
        }
    }

    private void GenerateHorizontalRoads() {
        int y = 0;
        while (y < worldHeight) {
            int xOrg = (int)Random.Range(0, worldHeight - minRoadLength);
            int xMax = xOrg + Random.Range(minRoadLength, worldWidth - xOrg);
            for (int x = xOrg; x < xMax; x++) {
                map[x, y] = ROAD;
            }
            y += Random.Range(minRoadGap, maxRoadGap);
        }
    }

    private void DisplayCity() {
        GenerateColors();
        
        for (int y = worldHeight - 1; y >= 0; y--) {
            bool[] walkable = new bool[worldWidth];
            Texture2D tex = new Texture2D(tileWidth * worldWidth, tileHeight * maxBuildingHeight);
            tex.filterMode = FilterMode.Point;
            for (int x = 0; x < worldWidth; x++) {
                if (map[x, y] < 0) {
                    Debug.Log(x + "_" + y + " Type" + map[x, y] + " unknown.");
                    map[x, y] = 0;
                }

                if (map[x, y] >= zones.Length) {
                    Debug.Log(x + "_" + y + " Type" + map[x, y] + " unknown.");
                    map[x, y] = zones.Length - 1;
                }

                if (zones[map[x, y]].isWalkable) {
                    walkable[x] = true;
                } else {
                    walkable[x] = false;
                }
                
                Color[] sprite;
                Vector2 spriteSize;
                if (zones[map[x, y]].spriteCount < Random.Range(0, zones[map[x, y]].spriteCount + zones[map[x, y]].specialBuildingTypes.Length + 1)) {
                    SpecialBuildingTypeInfo type;
                    sprite = GetSpecialBuildingSprite(x, y, out type);
                    spriteSize = new Vector2(type.sprite.width, type.sprite.height);
                    mapInteriors[x, y] = type.sprite.name;
                    if (Random.value >= specialBuildingManager.buildingTypesMap[type.type].spawnChance) {
                        int index;
                        sprite = GetSprite(x, y, out index);
                        spriteSize = zones[map[x, y]].spriteSize;
                        mapInteriors[x, y] = index.ToString();
                    }
                } else {
                    int index;
                    sprite = GetSprite(x, y, out index);
                    spriteSize = zones[map[x, y]].spriteSize;
                    mapInteriors[x, y] = index.ToString();
                }
                RenderTexture(tex, sprite, spriteSize, (zones[map[x, y]].background == null ? null : zones[map[x, y]].background), x * tileWidth, zones[map[x, y]].randomizeColor);
            }
            tex.Apply();
            GameObject obj = GameObject.Instantiate(worldObj, cityParent);
            obj.transform.localPosition = new Vector3(0, y, ((float)y + 0.9f) / 10);
            obj.name = "Tile" + y;
            obj.tag = "Building";
            obj.layer = 9;
            BuildCityMesh(obj, tex, walkable);
        }
        StaticBatchingUtility.Combine(cityParent.gameObject);
        // TODO: empty colorPaletteVariations?
    }

    private void GenerateColors() {
        // Choose color for buildings based on approved colors and variations based on settings
        colorPaletteVariations = new List<Color>(colorPalette);
        for (int i = 0; i < colorPalette.Count; i++) {
            for (int j = i + 1; j < colorPalette.Count; j++) {
                for (int k = 0; k < numVariations; k++) {
                    colorPaletteVariations.Add(Color.Lerp(colorPalette[i], colorPalette[j], Random.Range(0f, 1f)));
                }
            }
        }
        colorPalette = new List<Color>(colorPaletteVariations);

        for (int i = 0; i < colorPalette.Count; i++) {
            float h, s, v;
            Color.RGBToHSV(colorPalette[i], out h, out s, out v);
            for (int j = minSaturation; j <= maxSaturation; j += saturationInc) {
                colorPaletteVariations.Add(Color.HSVToRGB(h, s * (j / 255f), v));
            }

            for (int j = minValue; j <= maxValue; j += valueInc) {
                colorPaletteVariations.Add(Color.HSVToRGB(h, s, v * (j / 255f)));
            }

            colorPaletteVariations.Add(Color.HSVToRGB((h + 0.61803398875f) % 1f, s, v));
        }
    }

    private void RenderTexture(Texture2D tex, Color[] sprite, Vector2 spriteSize, Texture2D background, int startX, bool applyColor) {
        Color[] backgroundPixels = (background != null ? background.GetPixels() : new Color[0]);
        Color color = colorPaletteVariations[Random.Range(0, colorPaletteVariations.Count)];

        for (int x = 0; x < tileWidth; x++) {
            for (int y = (tileHeight * maxBuildingHeight) - 1; y >= 0; y--) {
                if (y < (int)spriteSize.y) {
                    Color spritePixel = sprite[x + (int)spriteSize.x * y];
                    if (background != null && spritePixel.a == 0 && y < tileHeight) {
                        tex.SetPixel(x + startX, y, backgroundPixels[x + background.width * y]);
                    } else {
                        if (spritePixel.a != 0 && applyColor) {
                            if (spritePixel.r == spritePixel.g && spritePixel.g == spritePixel.b) {
                                spritePixel *= color;
                            } else {
                                spritePixel -= color;
                            }
                            spritePixel.a = 1;
                        }
                        tex.SetPixel(x + startX, y, spritePixel);
                    }
                } else {
                    tex.SetPixel(x + startX, y, Color.clear);
                }
            }
        }
    }


    private void BuildGroundMesh() {
        GameObject obj = GameObject.Instantiate(worldObj, new Vector3(-0.25f, -0.25f, worldHeight + 10), Quaternion.identity);
        obj.name = "Ground";
        BuildMesh(obj, worldWidth, worldHeight);
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.material.mainTexture = groundTex;
        renderer.material.mainTextureScale = new Vector2(worldWidth / 2, worldHeight / 2);
        StaticBatchingUtility.Combine(obj);
    }

    private void BuildCityMesh(GameObject obj, Texture2D tex, bool[] walkable) {
        BuildMesh(obj, worldWidth, maxBuildingHeight);
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.material.mainTexture = tex;
        renderer.material.shader = Shader.Find("Transparent/Diffuse");

        int startPos = -1;
        for (int i = 0; i <= walkable.Length; i++) {
            if (i == walkable.Length || walkable[i]) {
                if (startPos != -1) {
                    BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2((i - startPos) - 0.0625f, 0.9375f);
                    collider.offset = new Vector2(((float)(i - startPos) / 2) + startPos, 0.5f);
                    startPos = -1;
                }
            } else {
                if (startPos == -1) {
                    startPos = i;
                }
            }
        }
    }

    public void BuildMesh(GameObject obj, int width, int height) {
        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(width, 0, 0);
        vertices[2] = new Vector3(0, height, 0);
        vertices[3] = new Vector3(width, height, 0);

        normals[0] = Vector3.up;
        normals[1] = Vector3.up;
        normals[2] = Vector3.up;
        normals[3] = Vector3.up;

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;
        obj.GetComponent<MeshFilter>().mesh = mesh;
    }

    private Color[] GetSpecialBuildingSprite(int x, int y, out SpecialBuildingTypeInfo type) {
        type = zones[map[x, y]].specialBuildingTypes[Random.Range(0, zones[map[x, y]].specialBuildingTypes.Length)];
        return type.sprite.GetPixels();
    }

    private Color[] GetSprite(int x, int y, out int index) {
        if (!zones[map[x, y]].isConnectable) {
            // TODO: pre-split sprites up like a normal person
            int maxY = Mathf.CeilToInt((float)zones[map[x, y]].spriteCount / zones[map[x, y]].spritesheetSize.y);
            int spriteY = Random.Range(0, maxY);
            int spriteX = Random.Range(0, (int)(spriteY == maxY - 1 ? zones[map[x, y]].spriteCount % zones[map[x, y]].spritesheetSize.y : zones[map[x, y]].spritesheetSize.x));
            index = spriteX + (int)zones[map[x, y]].spritesheetSize.x * ((int)zones[map[x, y]].spritesheetSize.y - spriteY - 1);
            return zones[map[x, y]].spritesheet.GetPixels(spriteX * (int)zones[map[x, y]].spriteSize.x, ((int)zones[map[x, y]].spritesheetSize.y - spriteY - 1) * (int)zones[map[x, y]].spriteSize.y, (int)zones[map[x, y]].spriteSize.x, (int)zones[map[x, y]].spriteSize.y);
        }
        string spriteName = "_";

        if (y - 1 >= 0) {
            if (map[x, y - 1] == map[x, y]) {
                spriteName += "N";
            }
        }

        if (x + 1 < worldWidth) {
            if (map[x + 1, y] == map[x, y]) {
                spriteName += "E";
            }
        }

        if (y + 1 < worldHeight) {
            if (map[x, y + 1] == map[x, y]) {
                spriteName += "S";
            }
        }

        if (x - 1 >= 0) {
            if (map[x - 1, y] == map[x, y]) {
                spriteName += "W";
            }
        }

        List<Sprite> sprites = zones[map[x, y]].buildings;
        for (int i = 0; i < sprites.Count; i++) {
            if (sprites[i].name.Equals(spriteName)) {
                index = i;
                return sprites[i].texture.GetPixels();
            }
        }


        Debug.Log("Sprite" + spriteName + " not found.");
        index = -1;
        return new Color[0];
    }

    private void GenerateWeightedMap() {
        weightedMap = new List<Vector2>();
        for (int x = 0; x < worldWidth; x++) {
            for (int y = 0; y < worldHeight; y++) {
                if (IsWalkable(x, y)) {
                    Dictionary<int, int> numTypes = new Dictionary<int, int>();
                    int mostCommon = 0;
                    for (int i = -1; i <= 1; i++) {
                        for (int j = -1; j <= 1; j++) {
                            int zone = BuildingAt(x + i, y + j);
                            if (zone > CityManager.ROAD) {
                                if (numTypes.ContainsKey(zone)) {
                                    numTypes[zone]++;
                                } else {
                                    numTypes[zone] = 1;
                                }
                                if (!numTypes.ContainsKey(mostCommon) || numTypes[zone] > numTypes[mostCommon]) {
                                    mostCommon = zone;
                                }
                            }
                        }
                    }
                    for (int i = ROAD; i < mostCommon; i++) {
                        weightedMap.Add(new Vector2(x, y));
                    }
                }
            }
        }
    }

    public int Width() { return worldWidth; }
    public int Height() { return worldHeight; }
    public int NumZones() { return zones.Length; }

    public int BuildingAt(int x, int y) {
        if ((x < worldWidth) && (x >= 0) && (y >= 0) && (y < worldHeight)) {
            return map[x, y];
        }

        return 0;
    }

    public BuildingInteriorType InteriorAt(int x, int y) {
        if ((x < worldWidth) && (x >= 0) && (y >= 0) && (y < worldHeight)) {
            if (zones[map[x, y]].isWalkable) {
                return BuildingInteriorType.NonEnterable;
            }

            BuildingInterior[] interiorTypes = zones[map[x, y]].interiorTypes;

            for (int i = 0; i < interiorTypes.Length; i++) {
                for (int j = 0; j < interiorTypes[i].sprites.Length; j++) {
                    if (mapInteriors[x, y].Equals(interiorTypes[i].sprites[j])) {
                        return interiorTypes[i].type;
                    }
                }
            }

            if (specialBuildingManager.IsSpecialSprite(mapInteriors[x, y])) {
                return BuildingInteriorType.Special;
            }

            return (zones[map[x, y]].isEnterable ? BuildingInteriorType.Residential : BuildingInteriorType.NonEnterable);
        }

        return BuildingInteriorType.Residential;
    }

    public AudioClip[] AmbientSoundsAt(int zone) { return zones[zone].ambientSounds; }

    public bool IsEnterable(int x, int y) { return zones[map[x, y]].isEnterable; }
    public bool IsWalkable(int x, int y) { return zones[map[x, y]].isWalkable; }
    public bool IsConnectable(int x, int y) { return zones[map[x, y]].isConnectable; }

    public Vector2[] BuildingNeighbors(int x, int y, bool sameTypeOnly = true) {
        Vector2[] neighbors = new Vector2[4];

        neighbors[0] = new Vector2(-1, -1);
        if (y - 1 >= 0) {
            if (!sameTypeOnly || map[x, y - 1] == map[x, y]) {
                neighbors[0] = new Vector2(x, y - 1);
            }
        }

        neighbors[1] = new Vector2(-1, -1);
        if (x + 1 < worldWidth) {
            if (!sameTypeOnly || map[x + 1, y] == map[x, y]) {
                neighbors[1] = new Vector2(x + 1, y);
            }
        }

        neighbors[2] = new Vector2(-1, -1);
        if (y + 1 < worldHeight) {
            if (!sameTypeOnly || map[x, y + 1] == map[x, y]) {
                neighbors[2] = new Vector2(x, y + 1);
            }
        }

        neighbors[3] = new Vector2(-1, -1);
        if (x - 1 >= 0) {
            if (!sameTypeOnly || map[x - 1, y] == map[x, y]) {
                neighbors[3] = new Vector2(x - 1, y);
            }
        }

        return neighbors;
    }

    public float Distance(Vector2 start, Vector2 end) {
        return (Mathf.Abs(start.x - end.x)) + (Mathf.Abs(start.y - end.y));
    }

    public Vector2 FindClosestPos(Vector2 ideal) {
        List<Vector2> possiblePos = new List<Vector2>();

        int x = (int)ideal.x;
        int y = (int)ideal.y;
        while (y >= 0 && !zones[map[x, y]].isWalkable) {
            y--;
        }
        if (y >= 0) {
            possiblePos.Add(new Vector2(x, y));
        }

        x = (int)ideal.x;
        y = (int)ideal.y;
        while (x < worldWidth && !zones[map[x, y]].isWalkable) {
            x++;
        }
        if (x < worldWidth) {
            possiblePos.Add(new Vector2(x, y));
        }

        x = (int)ideal.x;
        y = (int)ideal.y;
        while (y < worldHeight && !zones[map[x, y]].isWalkable) {
            y++;
        }
        if (y < worldHeight) {
            possiblePos.Add(new Vector2(x, y));
        }

        x = (int)ideal.x;
        y = (int)ideal.y;
        while (x >= 0 && !zones[map[x, y]].isWalkable) {
            x--;
        }
        if (x >= 0) {
            possiblePos.Add(new Vector2(x, y));
        }

        if (possiblePos.Count == 0) {
            return FindClosestPos(new Vector2(CityManager.Instance.Width() / 2, CityManager.Instance.Height() / 2));
        }

        Vector2 closest = possiblePos[0];
        for (int i = 1; i < possiblePos.Count; i++) {
            if (Distance(closest, ideal) > Distance(possiblePos[i], ideal)) {
                closest = possiblePos[i];
            }
        }
        return closest;
    }

    public PathGraph PathfindingGraph(bool getActual = false) {
        if (getActual) {
            return pathfindingGraph;
        }
        if (BuildingInteriorManager.Instance.isInside) {
            return BuildingInteriorManager.Instance.pathfindingGraph;
        }
        return pathfindingGraph;
    }

    public Vector2 GetRandWeightedPos() {
        return weightedMap[Random.Range(0, weightedMap.Count)];
    }
}
