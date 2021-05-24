using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour {
    [SerializeField] private float speed = 5;
    private int damageAmt;
    private int range;

    private Vector3 startPos;
    private Person shooter;

    private void Start() {
        GetComponent<Rigidbody2D>().velocity = transform.right * speed;
        startPos = transform.position;
    }

    private void Update() {
        if (Vector3.Distance(startPos, transform.position) > range) {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D col) {
        Person person = PeopleManager.Instance.FindPerson(col.gameObject);
        if (person == shooter) {
            return;
        }
        if (person != null) {
            person.AddHealth(-damageAmt, PeopleManager.Instance);
            person.Move(person.pos + (transform.right * 0.1f), false);
        }
        Explode();
    }

    private void Explode() {
        Destroy(gameObject);
    }

    public void SetInfo(int damageAmt, int range, Person shooter) {
        this.damageAmt = damageAmt;
        this.range = range;
        this.shooter = shooter;
    }
}
