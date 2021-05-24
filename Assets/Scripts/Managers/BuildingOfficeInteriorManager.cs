using System.Collections.Generic;
using UnityEngine;

public class BuildingOfficeInteriorManager : BuildingTypeInteriorManager {
	public override List<Person> GenerateInterior(int seed, Vector2 pos) {
		base.GenerateInterior(seed, pos);
		return people;
	}
}

