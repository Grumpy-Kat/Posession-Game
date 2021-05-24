using System;
using UnityEngine;

public enum SpecialBuildingType { PoliceStation, MilitaryBase, Other }

[Serializable]
public class SpecialBuildingTypeInfo {
	public string name;
	public SpecialBuildingType type;
	public Texture2D sprite;
}

