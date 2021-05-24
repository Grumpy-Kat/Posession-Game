using System;
using UnityEngine;

[Serializable]
public class SpecialBuilding {
    public string name;
    public SpecialBuildingType type;
    public float spawnChance;
    public int width = 25;
    public int height = 25;

    [Space(20)]
    public Sprite[] ruralBuildings;
    public Sprite[] suburbanBuildings;
    public Sprite[] exurbanBuildings;
    public Sprite[] urbanBuildings;

    [Space(20)]
    public GameObject[] interiors;

    public int NumSpecialZones() { return CityManager.Instance.NumZones() - (CityManager.ROAD + 1); }

    public Sprite[] GetZone(int index) {
        switch (index) {
            case 2:
                return ruralBuildings;
            case 3:
                return suburbanBuildings;
            case 4:
                return exurbanBuildings;
            case 5:
                return urbanBuildings;
        }
        Debug.Log(index + " is invalid zone!");
        return new Sprite[0];
    }
}

