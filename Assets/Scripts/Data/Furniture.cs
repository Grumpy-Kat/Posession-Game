using System;
using UnityEngine;

[Serializable]
public class Furniture {
    [Serializable]
    public class RotatedSprite {
        public Sprite sprite;
        public int dir;
    }

    [Serializable]
    public class Row {
        public int[] data;
    }

    public const int WALL = -1;
    public const int NOWALL = -2;
    public const int EMPTY = -3;
    public const int NONE = -4;
    public const int OBJECT = -5;

    public string name;
    public float spawnChance;
    [SerializeField] private int minNum;
    [SerializeField] private int maxNum;
    public RotatedSprite[] sprites;
    // Must be unique for every different furniture type
    public int id;

    [Space(20)]
    // -5 = must be object, -4 = no requirement, -3 = must be empty, -2 = no wall, -1 = wall, >=0 = furniture type id
    public Row[] requirements;

    public int MinNum() {
        return (minNum < 0 ? 0 : minNum);
    }

    public int MaxNum() {
        return (maxNum < 0 ? (int.MaxValue - 1) : maxNum);
    }

    public bool RequiresWall() {
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                if (requirements[i].data[j] == WALL) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool RequiresObject(int objId) {
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                if (requirements[i].data[j] == objId || requirements[i].data[j] == OBJECT) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool RequiresOtherObject() {
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                if (requirements[i].data[j] > WALL || requirements[i].data[j] == OBJECT) {
                    return true;
                }
            }
        }
        return false;
    }

    public int Get(int x, int y) {
        return requirements[x].data[y];
    }

    public int[,] GetRequirements() {
        int[,] requirementsArray = new int[3, 3];
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                requirementsArray[i, j] = Get(i, j);
            }
        }
        return requirementsArray;
    }

    public static int[,] RotateRequirements(int[,] orgRequirements) {
        int[,] requirements = new int[3, 3];
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                requirements[i, j] = orgRequirements[3 - j - 1, i];
            }
        }
        return requirements;
    }
}
