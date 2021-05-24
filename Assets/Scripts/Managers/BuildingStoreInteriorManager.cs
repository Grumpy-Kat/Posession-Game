using System.Collections.Generic;
using UnityEngine;

public class BuildingStoreInteriorManager : BuildingTypeInteriorManager {
    public override List<Person> GenerateInterior(int seed, Vector2 pos) {
        base.GenerateInterior(seed, pos);
        return people;
    }
}

