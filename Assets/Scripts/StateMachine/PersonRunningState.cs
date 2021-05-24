using System.Linq;
using UnityEngine;

public class PersonRunningState : PersonState {
    private static PersonRunningState instance;
    public static PersonRunningState Instance {
        get {
            if (instance == null) {
                instance = new PersonRunningState();
            }
            return instance;
        }
    }

    public override void EnterState(Person person) {
        SetDest(person);
    }

    public override void ExecuteState(Person person) {
        if (person.dest == person.curr) {
            person.SetState(person.lastState);
        }
    }

    public override void ExitState(Person person) { }

    public override void ReceiveMsg(Person person, Message msg) { }

    private void SetDest(Person person) {
        Person possessed = PlayerManager.Instance.possessed;
        Vector3 diff = person.pos - possessed.pos;
        if (diff.x > diff.y) {
            if (diff.x > 0) {
                person.SetDest(CityManager.Instance.FindClosestPos((Vector2)possessed.pos + new Vector2(10, 0)));
            } else {
                person.SetDest(CityManager.Instance.FindClosestPos((Vector2)possessed.pos + new Vector2(-10, 0)));
            }
        } else {
            if (diff.y > 0) {
                person.SetDest(CityManager.Instance.FindClosestPos((Vector2)possessed.pos + new Vector2(0, 10)));
            } else {
                person.SetDest(CityManager.Instance.FindClosestPos((Vector2)possessed.pos + new Vector2(0, -10)));
            }
        }
    }
}
