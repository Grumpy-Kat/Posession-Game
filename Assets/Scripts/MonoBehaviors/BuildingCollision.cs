using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class BuildingCollision : MonoBehaviour {
    private DialogueManager dialogueManager;
    private BuildingInteriorManager buildingInteriorManager;

    private BoxCollider2D col;

    [SerializeField] [Range(0f, 1f)] private float range = 0.15f;
    [SerializeField] private LayerMask buildingMask;
    [SerializeField] private Text tooltipText;
    private GameObject tooltip;

    private void Start() {
        dialogueManager = GameObject.FindObjectOfType<DialogueManager>();
        dialogueManager.personPos = new Vector3(-1, -1, 0);
        dialogueManager.isMainAction = false;

        buildingInteriorManager = GameObject.FindObjectOfType<BuildingInteriorManager>();
        buildingInteriorManager.buildingPos = new Vector3(-1, -1, 0);
        buildingInteriorManager.isMainAction = false;

        col = GetComponent<BoxCollider2D>();

        tooltip = tooltipText.rectTransform.parent.gameObject;
    }

    private void Update() {
        Vector3 pos = PlayerManager.Instance.possessed.objTransform.position;
        float posX = pos.x % 1;
        float posY = pos.y % 1;
        // below = -0.25f
        // above = 0.75f
        // to the left = -0.4f
        // to the right = 0.6f
        //
        // Ex: (2, 2)
        //	  below = 1.75f
        //	  above = 2.75f
        //	  to the left = 1.4f
        //	  to the right = 2.6f
        if (col.IsTouchingLayers(buildingMask.value)) {
            // if(((1 - posX) <= range || posX >= (1 - range)) && ((1 - posY) <= range || posY >= (1 - range))) {
            buildingInteriorManager.buildingPos = new Vector2((posX >= 0.5f) ? Mathf.FloorToInt(pos.x) : Mathf.CeilToInt(pos.x), (posY >= 0.7f) ? Mathf.FloorToInt(pos.y) : Mathf.CeilToInt(pos.y));
            Debug.Log(pos + " " + buildingInteriorManager.buildingPos + " " + posX + " " + posY);
            buildingInteriorManager.isMainAction = true;
            dialogueManager.isMainAction = false;
            SetTooltip(buildingInteriorManager.GetTooltip());
        }
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Person" && !PeopleManager.Instance.FindPerson(col.gameObject).isPossessed) {
            if (col.transform.GetChild(0).tag == "Person") {
                dialogueManager.personPos = col.transform.position;
            } else {
                dialogueManager.personPos = col.transform.parent.position;
            }
            dialogueManager.isMainAction = true;
            buildingInteriorManager.isMainAction = false;
            SetTooltip(dialogueManager.GetTooltip());
        }
    }

    private void OnTriggerExit2D(Collider2D col) {
        if (col.tag == "Person") {
            dialogueManager.personPos = new Vector3(-1, -1, 0);
            dialogueManager.isMainAction = false;
            SetTooltip("");
        }
        /* if(col.tag == "Building") {
			buildingInteriorManager.buildingPos = new Vector2(-1, -1);
			buildingInteriorManager.isMainAction = false;
			SetTooltip("");
		} */
    }

    private void SetTooltip(string text) {
        tooltipText.text = text;
        if (text.Equals("")) {
            tooltip.SetActive(false);
        } else {
            tooltip.SetActive(true);
        }
    }
}
