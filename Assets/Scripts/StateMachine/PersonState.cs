using UnityEngine;

public enum PersonStateType { Wandering, Attacking, Searching, Patrolling, Standing, Running, Other };

public abstract class PersonState {
    public abstract void EnterState(Person person);
    public abstract void ExecuteState(Person person);
    public abstract void ExitState(Person person);
    public abstract void ReceiveMsg(Person person, Message msg);

    public static PersonState GetState(PersonStateType type) {
        switch (type) {
            case PersonStateType.Wandering:
                return PersonWanderingState.Instance;
            case PersonStateType.Attacking:
                return PersonAttackingState.Instance;
            case PersonStateType.Searching:
                return PersonSearchingState.Instance;
            case PersonStateType.Patrolling:
                return PersonPatrollingState.Instance;
            case PersonStateType.Standing:
                return PersonStandingState.Instance;
            case PersonStateType.Running:
                return PersonStandingState.Instance;
        }
        return PersonGlobalState.Instance;
    }
}
