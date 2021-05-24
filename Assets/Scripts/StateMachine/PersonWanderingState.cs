using System.Linq;
using UnityEngine;

public class PersonWanderingState : PersonState {
    private static PersonWanderingState instance;
    public static PersonWanderingState Instance {
        get {
            if (instance == null) {
                instance = new PersonWanderingState();
            }
            return instance;
        }
    }

    public override void EnterState(Person person) {
        SetDest(person);
    }

    public override void ExecuteState(Person person) {
        if (person.dest == person.curr) {
            SetDest(person);
        }
    }

    public override void ExitState(Person person) { }

    public override void ReceiveMsg(Person person, Message msg) { }

    private void SetDest(Person person) {
        person.SetDest(CityManager.Instance.pathfindingGraph.nodes.Keys.ToList()[Random.Range(0, CityManager.Instance.pathfindingGraph.nodes.Keys.Count)]);
    }
}
