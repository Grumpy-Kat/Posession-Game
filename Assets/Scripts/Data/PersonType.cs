using System;
using System.Collections.Generic;
using UnityEngine;

public enum PersonType { Regular, Soldier, PoliceOfficer, SecretAgent, Other }

[Serializable]
public class PersonTypeInfo {
	public string name;
	public PersonType type;
	public bool colorRandomly = true;
	public bool attacks = false;
	public PersonStateType defaultState = PersonStateType.Wandering;

	[Space(20)]
	public List<string> requiredItems;

	[Space(20)]
	public Texture2D[] femaleTopStyles;
	public Texture2D[] maleTopStyles;

	[Space(20)]
	public Texture2D[] femaleBottomStyles;
	public Texture2D[] maleBottomStyles;

	[Space(20)]
	public Texture2D[] femaleShoeStyles;
	public Texture2D[] maleShoeStyles;
}

