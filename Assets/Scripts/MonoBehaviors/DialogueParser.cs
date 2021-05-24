using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DialogueParser : MonoBehaviour {
    public class DialogueLine {
        public string name;
        public string line;
        public string[] choices;

        public DialogueLine(string name, string line) {
            this.name = name;
            this.line = line;
            choices = new string[0];
        }
    }

    private List<DialogueLine> lines;

    public void LoadDialogue(string filename, string fileLoc = "Assets/Dialogue/", string fileExtension = ".txt") {
        string line = "";
        lines = new List<DialogueLine>();
        StreamReader reader = new StreamReader(fileLoc + filename + fileExtension);
        using (reader) {
            do {
                line = reader.ReadLine();
                AddLine(lines.Count, line);
            } while (line != null);
            reader.Close();
        }
    }

    public void AddLine(int index, string line) {
        if (line != null) {
            if (line != "") {
                string[] lineData = line.Split(';');
                if (lineData[0] == "Player") {
                    DialogueLine lineEntry = new DialogueLine(lineData[0], "");
                    lineEntry.choices = new string[lineData.Length - 1];
                    for (int i = 1; i < lineData.Length; i++) {
                        lineEntry.choices[i - 1] = lineData[i];
                    }
                    lines.Insert(index, lineEntry);
                } else {
                    DialogueLine lineEntry = new DialogueLine(lineData[0], lineData[1]);
                    lines.Insert(index, lineEntry);
                }
            } else {
                lines.Insert(index, new DialogueLine("", ""));
            }
        }
    }

    public void ChangeLine(int index, string line) {
        if (index > -1 && index < lines.Count) {
            lines[index].line = line;
        }
    }

    public string GetName(int lineNum) {
        if (lineNum < lines.Count) {
            return lines[lineNum].name;
        }
        return "";
    }

    public string GetLine(int lineNum) {
        if (lineNum < lines.Count) {
            return lines[lineNum].line;
        }
        return "";
    }

    public string[] GetChoices(int lineNum) {
        if (lineNum < lines.Count) {
            return lines[lineNum].choices;
        }
        return new string[0];
    }
}
