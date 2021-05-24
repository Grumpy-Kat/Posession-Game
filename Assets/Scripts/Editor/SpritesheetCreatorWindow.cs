using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class SpritesheetCreatorWindow : EditorWindow {
    private Vector2 spriteSize = new Vector2(32, 96);
    private Vector2 sheetSize = new Vector2(10, 10);

    private string sheetPath = "Imgs/Buildings/spritesheet.png";
    private List<Texture2D> sprites = new List<Texture2D>();

    [MenuItem("Window/Spritesheet Creator")]
    public static void ShowWindow() {
        GetWindow<SpritesheetCreatorWindow>("Spritesheet Creator");
    }

    private void OnGUI() {
        spriteSize = EditorGUILayout.Vector2Field("Sprite Size", spriteSize);
        sheetSize = EditorGUILayout.Vector2Field("Spritesheet Size", sheetSize);
        sheetPath = EditorGUILayout.TextField("Sprite Sheet Path", sheetPath);

        MultipleObjectField();

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Spritesheet")) {
            Texture2D[] temp = new Texture2D[sprites.Count];
            for (int i = 0; i < sprites.Count; i++) {
                temp[int.Parse(sprites[i].name)] = sprites[i];
            }
            sprites = new List<Texture2D>(temp);

            Texture2D sheet = new Texture2D((int)(spriteSize.x * sheetSize.x), (int)(spriteSize.y * sheetSize.y));
            sheet.filterMode = FilterMode.Point;
            for (int x = 0; x < (int)sheetSize.x; x++) {
                for (int y = 0; y < (int)sheetSize.y; y++) {
                    AddSprite(new Vector2(x, y), sheet);
                }
            }
            sheet.Apply();

            File.WriteAllBytes(Application.dataPath + "/" + sheetPath, sheet.EncodeToPNG());
        }
    }

    private void MultipleObjectField() {
        Event e = Event.current;
        GUILayout.Label("Sprites");
        if (GUILayout.Button("Clear Sprites")) {
            sprites.Clear();
        }
        Rect dropArea = GUILayoutUtility.GetRect(0f, 200f, GUILayout.ExpandWidth(true));
        switch (e.type) {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(e.mousePosition)) {
                    return;
                }
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (e.type == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag();
                    foreach (Object obj in DragAndDrop.objectReferences) {
                        if (obj.GetType() == typeof(Texture2D) && !sprites.Contains((Texture2D)obj)) {
                            sprites.Add((Texture2D)obj);
                        }
                    }
                }
                break;
        }
        string text = "";
        foreach (Texture2D sprite in sprites) {
            text += sprite.name + "\n";
        }
        GUI.Box(dropArea, text);
        GUILayout.Label("(" + sprites.Count + " sprites)");
    }

    private void AddSprite(Vector2 pos, Texture2D sheet) {
        // Combines multiple sprites together to make a spritesheet
        int sprite = (int)(pos.x + sheetSize.x * pos.y);

        if (sprite >= sprites.Count) {
            return;
        }

        Color32[] pixels = sprites[sprite].GetPixels32();
        pos.x *= spriteSize.x;
        pos.y *= spriteSize.y;
        for (int x = 0; x < (int)spriteSize.x; x++) {
            for (int y = 0; y < (int)spriteSize.y; y++) {
                if (y < sprites[sprite].height) {
                    sheet.SetPixel(x + (int)pos.x, GetY(y + (int)pos.y + (int)(spriteSize.y - sprites[sprite].height), sheet), pixels[GetI(x, y, sprites[sprite])]);
                } else {
                    sheet.SetPixel(x + (int)pos.x, GetY((y - (int)sprites[sprite].height) + (int)pos.y, sheet), Color.clear);
                }
            }
        }
    }

    private int GetI(int x, int y, Texture2D img) {
        return x + img.width * GetY(y, img);
    }

    private int GetY(int y, Texture2D img) {
        return (img.height - y - 1);
    }
}
