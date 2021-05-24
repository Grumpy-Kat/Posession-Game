using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Room {
    public string name;

    [Space(20)]
    [SerializeField] private int minNum;
    [SerializeField] private int maxNum;
    [SerializeField] private int minArea;
    [SerializeField] private int maxArea;
    [SerializeField] private int minSide;
    [SerializeField] private int maxSide;
    [SerializeField] private float spawnChance;

    [Space(20)]
    public Furniture[] furniture;
    private Dictionary<int, Furniture> furnitureIdMap;

    public int MinNum() {
        return (minNum < 0 ? 0 : minNum);
    }

    public int MaxNum() {
        return (maxNum < 0 ? int.MaxValue : maxNum);
    }

    public int MinArea() {
        return (minArea < 0 ? 1 : minArea);
    }

    public int MaxArea() {
        return (maxArea < 0 ? int.MaxValue : maxArea);
    }

    public int MinSide() {
        return (minSide < 0 ? 1 : minSide);
    }

    public int MaxSide() {
        return (maxSide < 0 ? int.MaxValue : maxSide);
    }

    public float SpawnChance() {
        if (spawnChance == -1) {
            Debug.Log("Trying to spawn " + name);
        }
        return spawnChance;
    }

    public Dictionary<int, Furniture> GetFurnitureIdMap() {
        if (furnitureIdMap != null) {
            return furnitureIdMap;
        }
        furnitureIdMap = new Dictionary<int, Furniture>();
        for (int i = 0; i < furniture.Length; i++) {
            furnitureIdMap.Add(furniture[i].id, furniture[i]);
        }
        return furnitureIdMap;
    }
}