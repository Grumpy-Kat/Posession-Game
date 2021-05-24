using System.Linq;
using UnityEngine;

public class PersonSearchingState : PersonState {
    private static PersonSearchingState instance;
    public static PersonSearchingState Instance {
        get {
            if (instance == null) {
                instance = new PersonSearchingState();
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
        // TODO: actually search for possessed
        person.SetDest(CityManager.Instance.pathfindingGraph.nodes.Keys.ToList()[Random.Range(0, CityManager.Instance.pathfindingGraph.nodes.Keys.Count)]);
    }
}
