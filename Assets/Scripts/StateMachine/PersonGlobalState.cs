using UnityEngine;

public class PersonGlobalState : PersonState {
    private static PersonGlobalState instance;
    public static PersonGlobalState Instance {
        get {
            if (instance == null) {
                instance = new PersonGlobalState();
            }
            return instance;
        }
    }

    public override void EnterState(Person person) {
        if (person.obj.GetComponent<PatrolRoute>() != null) {
            person.SetState(PersonPatrollingState.Instance);
        }
    }

    public override void ExecuteState(Person person) { }

    public override void ExitState(Person person) { }

    public override void ReceiveMsg(Person person, Message msg) {
        switch (msg.type) {
            case MessageType.Attack:
                MessageAttack attack = (MessageAttack)msg;
                if (Vector3.Distance(person.pos, attack.src) > attack.range) {
                    return;
                }
                // TODO: actually calculate if person is visible, instead of this fake way (raycasts?)
                if (attack.requiresSightline && (((int)person.pos.x) != ((int)attack.src.x) && ((int)person.pos.y) != ((int)attack.src.y))) {
                    return;
                }
                if (person.attacks) {
                    person.SetState(PersonAttackingState.Instance);
                } else {
                    person.SetState(PersonRunningState.Instance);
                }
                break;
        }
    }
}
