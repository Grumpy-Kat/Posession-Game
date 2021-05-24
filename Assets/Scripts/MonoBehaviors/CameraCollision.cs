using UnityEngine;

public class CameraCollision : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Person") {
            if (PeopleManager.Instance.currPeople.Contains(PeopleManager.Instance.FindPerson(other.gameObject))) {
                if (other.transform.GetChild(0).tag == "Person") {
                    other.transform.GetChild(0).gameObject.SetActive(true);
                } else {
                    other.gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Person") {
            if (other.transform.GetChild(0).tag == "Person") {
                other.transform.GetChild(0).gameObject.SetActive(false);
            } else {
                other.gameObject.SetActive(false);
            }
        }
    }
}
