using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {
    private DialogueParser parser;

    [HideInInspector] public string dialogue;
    [HideInInspector] public string characterName;
    [HideInInspector] public int lineNum;
    [HideInInspector] public string[] choices;
    [HideInInspector] public bool playerTalking;

    private List<Button> btns;

    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private Text nameText;
    [SerializeField] private Text dialogueText;

    [Space(20)]
    [SerializeField] private GameObject choicePrefab;

    [HideInInspector] public bool isMainAction;
    [HideInInspector] public Vector3 personPos;

    private void Start() {
        btns = new List<Button>();

        dialogue = "";
        characterName = "";
        playerTalking = false;
        parser = GameObject.FindObjectOfType<DialogueParser>();

        InputManager.Instance.RegisterEKeyClicked(OnEKeyClicked);
        isMainAction = false;
    }

    private void OnEKeyClicked(int isClicked) {
        if (isClicked != 0) {
            return;
        }

        if (personPos.x == -1 || personPos.y == -1 || !isMainAction) {
            CloseDialogue();
            return;
        }

        if (!dialogueBox.activeInHierarchy) {
            parser.LoadDialogue("Dialogue0");
            OpenDialogue();
        }

        if (!playerTalking) {
            ShowDialogue();
            lineNum++;
        }
    }

    public void OpenDialogue() {
        dialogueBox.SetActive(true);
        lineNum = 0;
    }

    public void ShowDialogue() {
        ParseLine();
        UpdateUI();
    }

    public void CloseDialogue() {
        dialogueBox.SetActive(false);
    }

    private void ParseLine() {
        if (parser.GetName(lineNum) == "PlayerLine") {
            playerTalking = false;
            characterName = "Player";
            dialogue = parser.GetLine(lineNum);
        } else if (parser.GetName(lineNum) != "Player") {
            playerTalking = false;
            characterName = parser.GetName(lineNum);
            dialogue = parser.GetLine(lineNum);
        } else {
            playerTalking = true;
            characterName = "";
            dialogue = "";
            choices = parser.GetChoices(lineNum);
            GenerateButtons();
        }
    }

    private void GenerateButtons() {
        for (int i = 0; i < choices.Length; i++) {
            GameObject obj = GameObject.Instantiate(choicePrefab, dialogueBox.transform.GetChild(2));
            ChoiceButton choice = obj.GetComponent<ChoiceButton>();
            string[] choiceOptions = choices[i].Split(':');
            Debug.Log(choices[i] + " |  " + choiceOptions.Length);
            choice.SetText(choiceOptions[0]);
            choice.nextLine = choiceOptions[1];

            string choiceText = "";
            for (int j = 2; j < choiceOptions.Length; j++) {
                choiceText += choiceOptions[j];
                if (j != choiceOptions.Length - 1) {
                    choiceText += ":";
                }
            }
            choice.choice = choiceText;

            btns.Add(obj.GetComponent<Button>());
        }
    }

    private void UpdateUI() {
        if (!playerTalking) {
            ClearButtons();
        }
        dialogueText.text = dialogue;
        nameText.text = characterName;
    }

    private void ClearButtons() {
        for (int i = btns.Count - 1; i >= 0; i--) {
            Destroy(btns[i].gameObject);
            btns.Remove(btns[i]);
        }
    }

    public string GetTooltip() {
        if (personPos.x == -1 || personPos.y == -1 || !isMainAction) {
            return "";
        }
        return "Press E to talk.";
    }
}
