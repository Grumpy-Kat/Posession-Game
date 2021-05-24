using System.Collections.Generic;
using UnityEngine;

public class PersonSpawnPoint : MonoBehaviour {
    [SerializeField] private PersonType type;
    [SerializeField] private PersonStateType defaultState = PersonStateType.Other;
    [SerializeField] private int dir = 2;

    [SerializeField] private int spawnMin = 0;
    [SerializeField] private int spawnMax = 2;
    [SerializeField] private float spawnRate = 45;
    private float currTime = 0;

    private void Start() {
        // Object is placed and parameters to modified to determine what people to spawn at the location of the object
        SpawnPeople();
    }

    private void Update() {
        if (spawnRate > 0) {
            currTime -= Time.deltaTime;
            if (currTime <= 0) {
                SpawnPeople();
            }
        }
    }

    private void SpawnPeople() {
        int spawnAmt = Random.Range(spawnMin, spawnMax + 1);
        List<Person> people = new List<Person>();
        PatrolRoute route = GetComponent<PatrolRoute>();
        for (int i = 0; i < spawnAmt; i++) {
            people.Add(PeopleManager.Instance.SpawnPerson(transform.position, type, gameObject.name, CameraManager.Instance.col, defaultState, route));
            people[i].SetDir((dir == -1 ? Random.Range(0, 4) : dir));
        }
        PeopleManager.Instance.AddCurrPeople(people);
        currTime = spawnRate;
    }
}

