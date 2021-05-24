using UnityEngine;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour {
    private DialogueManager manager;
    private DialogueParser parser;

    [HideInInspector] public string nextLine;
    [HideInInspector] public string choice;

    public void SetText(string text) {
        manager = GameObject.FindObjectOfType<DialogueManager>();
        parser = GameObject.FindObjectOfType<DialogueParser>();
        this.GetComponentInChildren<Text>().text = text;
    }

    public void ParseChoice() {
        int index = -1;
        if (choice.Contains(":")) {
            string[] commands = choice.Split(':');
            for (int i = 0; i < commands.Length; i++) {
                string command = commands[i].Split(',')[0];
                string mod = commands[i].Split(',')[1];
                if (command == "line") {
                    index = int.Parse(mod);
                }
                EnterCommand(command, mod);
            }
            if (index == -1) {
                index = manager.lineNum + 2;
            }
        } else {
            string command = choice.Split(',')[0];
            string mod = choice.Split(',')[1];
            if (command != "line") {
                index = manager.lineNum + 2;
            } else {
                index = int.Parse(mod);
            }
            EnterCommand(command, mod);
        }
        parser.AddLine(index, "PlayerLine;" + nextLine);
        manager.playerTalking = false;
        manager.lineNum++;
        manager.ShowDialogue();
        manager.lineNum++;
    }

    private void EnterCommand(string command, string mod) {
        Debug.Log(command);
        if (command == "line") {
            manager.lineNum = int.Parse(mod) - 1;
            manager.ShowDialogue();
        } else if (command == "exit") {
            manager.CloseDialogue();
        }
    }
}
