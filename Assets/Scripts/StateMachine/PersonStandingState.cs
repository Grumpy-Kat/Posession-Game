using System.Linq;

public class PersonStandingState : PersonState {
    private static PersonStandingState instance;
    public static PersonStandingState Instance {
        get {
            if (instance == null) {
                instance = new PersonStandingState();
            }
            return instance;
        }
    }

    public override void EnterState(Person person) {
        person.SetDest(person.pos);
    }

    public override void ExecuteState(Person person) { }

    public override void ExitState(Person person) { }

    public override void ReceiveMsg(Person person, Message msg) { }
}
