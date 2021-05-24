using UnityEngine;

public class PersonAttackingState : PersonState {
    private static PersonAttackingState instance;
    public static PersonAttackingState Instance {
        get {
            if (instance == null) {
                instance = new PersonAttackingState();
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

    public override void ReceiveMsg(Person person, Message msg) {
        if (msg.type == MessageType.Possession) {
            if (PlayerManager.Instance.suspicion > PlayerManager.Instance.suspicionThreshold) {
                person.SetState(person.lastState);
            }
        }
    }

    private void SetDest(Person person) {
        Person possessed = PlayerManager.Instance.possessed;
        Vector3 diff = person.pos - possessed.pos;
        if (diff.x > diff.y) {
            if (diff.x > 0) {
                person.SetDest(CityManager.Instance.FindClosestPos((Vector2)possessed.pos + new Vector2(1, 0)));
            } else {
                person.SetDest(CityManager.Instance.FindClosestPos((Vector2)possessed.pos + new Vector2(-1, 0)));
            }
        } else {
            if (diff.y > 0) {
                person.SetDest(CityManager.Instance.FindClosestPos((Vector2)possessed.pos + new Vector2(0, 1)));
            } else {
                person.SetDest(CityManager.Instance.FindClosestPos((Vector2)possessed.pos + new Vector2(0, -1)));
            }
        }
    }
}
