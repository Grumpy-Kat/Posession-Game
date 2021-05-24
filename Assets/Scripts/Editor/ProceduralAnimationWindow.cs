using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ProceduralAnimationWindow : EditorWindow {
    private Vector2 frameSize = new Vector2(32, 32);
    private Vector2 refFrame = new Vector2(0, 0);
    private Vector2 startFrame = new Vector2(32, 0);
    private Vector2 endFrame = new Vector2(160, 0);

    private Texture2D refImg;
    private string imgPath = "Imgs/People/Regular";
    private List<Texture2D> imgs = new List<Texture2D>();

    [MenuItem("Window/Procedural Animation")]
    public static void ShowWindow() {
        GetWindow<ProceduralAnimationWindow>("Procedural Animation");
    }

    private void OnGUI() {
        frameSize = EditorGUILayout.Vector2Field("Frame Size", frameSize);
        refFrame = EditorGUILayout.Vector2Field("Reference Frame", refFrame);
        startFrame = EditorGUILayout.Vector2Field("Start Frame", startFrame);
        endFrame = EditorGUILayout.Vector2Field("End Frame", endFrame);
        imgPath = EditorGUILayout.TextField("Image Path", imgPath);
        refImg = (Texture2D)EditorGUILayout.ObjectField("Reference Image", (Object)refImg, typeof(Texture2D), false);
        MultipleObjectField();

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Animations")) {
            foreach (Texture2D img in imgs) {
                GenerateAnimations(img);
            }
        }
    }

    private void MultipleObjectField() {
        Event e = Event.current;
        GUILayout.Label("Images");
        if (GUILayout.Button("Clear Images")) {
            imgs.Clear();
        }
        Rect dropArea = GUILayoutUtility.GetRect(0f, 55f, GUILayout.ExpandWidth(true));
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
                        if (obj.GetType() == typeof(Texture2D) && !imgs.Contains((Texture2D)obj)) {
                            imgs.Add((Texture2D)obj);
                        }
                    }
                }
                break;
        }
        string text = "";
        foreach (Texture2D img in imgs) {
            text += img.name + "\n";
        }
        GUI.Box(dropArea, text);
    }

    private void GenerateAnimations(Texture2D img) {
        // Uses same colors in reference image and sprite image to map sprites to create procedural animations
        // Then saves by overriding original sprite
        // Mostly used for animating clothes based on the person animation reference image
        Dictionary<Color32, Color32> colors = new Dictionary<Color32, Color32>();
        Color32[] refPixels = refImg.GetPixels32();
        Color32[] imgPixels = img.GetPixels32();

        for (int x = (int)refFrame.x; x < (int)(refFrame.x + frameSize.x); x++) {
            for (int y = (int)refFrame.y; y < (int)(refFrame.y + frameSize.y); y++) {
                int i = GetI(x, y, img);
                if (imgPixels[i].a != 0) {
                    if (colors.ContainsKey(refPixels[i])) {
                        Debug.Log("Multiple color: " + x + " " + y);
                    } else {
                        colors.Add(refPixels[i], imgPixels[i]);
                    }
                }
            }
        }

        Vector2 numFrames = new Vector2(((endFrame.x - startFrame.x) / frameSize.x) + 1, ((endFrame.y - startFrame.y) / frameSize.y) + 1);
        for (int x = 0; x < (int)numFrames.x; x++) {
            for (int y = 0; y < (int)numFrames.y; y++) {
                GenerateAnimation((int)(startFrame.x + (frameSize.x * x)), (int)(startFrame.y + (frameSize.y * y)), colors, refPixels, img);
            }
        }
        img.Apply();
        File.WriteAllBytes(Application.dataPath + "/" + imgPath + "/" + img.name + ".png", img.EncodeToPNG());
    }

    private void GenerateAnimation(int startX, int startY, Dictionary<Color32, Color32> colors, Color32[] refPixels, Texture2D img) {
        for (int x = startX; x < startX + (int)frameSize.x; x++) {
            for (int y = startY; y < startY + (int)frameSize.y; y++) {
                if (colors.ContainsKey(refPixels[GetI(x, y, img)])) {
                    img.SetPixel(x, GetY(y, img), colors[refPixels[GetI(x, y, img)]]);
                } else {
                    img.SetPixel(x, GetY(y, img), Color.clear);
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
