using System;
using UnityEngine;

public enum BuildingInteriorType { Residential, Store, Office, NonEnterable, Special }

[Serializable]
public class BuildingInterior {
	public string name;
	public BuildingInteriorType type;

	[Space(20)]
	public string[] sprites;
}

