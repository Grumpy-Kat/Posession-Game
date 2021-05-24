using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Zone {
    public string name;

    [Space(20)]
    public bool isEnterable;
    public bool isWalkable;
    public bool isConnectable;
    public bool randomizeColor;

    [Space(20)]
    public Texture2D background;
    public List<Sprite> buildings;
    public Texture2D spritesheet;
    public Vector2 spritesheetSize = new Vector2(10, 10);
    public Vector2 spriteSize = new Vector2(32, 96);
    public int spriteCount;

    [Space(20)]
    public BuildingInterior[] interiorTypes;

    [Space(20)]
    public SpecialBuildingTypeInfo[] specialBuildingTypes;

    [Space(20)]
    public AudioClip[] ambientSounds;
}

