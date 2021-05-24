using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { MeleeWeapon, RangedWeapon, Artifact, Misc }

[Serializable]
public class ItemTypeInfo {
	public string name;
	public ItemType type;

	[Space(20)]
	public GameObject prefab;
	public Vector3 offset;
}

