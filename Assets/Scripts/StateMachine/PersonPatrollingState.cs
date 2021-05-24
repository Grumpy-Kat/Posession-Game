using UnityEngine;

public class PersonPatrollingState : PersonState {
    private static PersonPatrollingState instance;
    public static PersonPatrollingState Instance {
        get {
            if (instance == null) {
                instance = new PersonPatrollingState();
            }
            return instance;
        }
    }

    public override void EnterState(Person person) {
        SetDest(person, person.obj.GetComponent<PatrolRoute>());
    }

    public override void ExecuteState(Person person) {
        if (person.dest == person.curr) {
            PatrolRoute route = person.obj.GetComponent<PatrolRoute>();
            route.SetCurrIndex(route.currIndex + 1);
            SetDest(person, route);
        }
    }

    public override void ExitState(Person person) { }

    public override void ReceiveMsg(Person person, Message msg) { }

    private void SetDest(Person person, PatrolRoute route) {
        person.SetDest(new Vector2(Mathf.RoundToInt(route.patrolStops[route.currIndex].stopPos.position.x), Mathf.RoundToInt(route.patrolStops[route.currIndex].stopPos.position.y)));
    }
}
