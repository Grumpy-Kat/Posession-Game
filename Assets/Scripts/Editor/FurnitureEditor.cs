using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Furniture))]
public class FurnitureEditor : PropertyDrawer {
    public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(pos, label, property);
        EditorGUI.LabelField(pos, label);
        EditorGUI.indentLevel++;

        EditorGUI.PropertyField(new Rect(pos.x, pos.y + 20, pos.width, 17), property.FindPropertyRelative("name"));
        EditorGUI.PropertyField(new Rect(pos.x, pos.y + 40, pos.width, 17), property.FindPropertyRelative("id"));
        EditorGUI.PropertyField(new Rect(pos.x, pos.y + 60, pos.width, 17), property.FindPropertyRelative("spawnChance"));
        EditorGUI.PropertyField(new Rect(pos.x, pos.y + 80, pos.width, 17), property.FindPropertyRelative("minNum"));
        EditorGUI.PropertyField(new Rect(pos.x, pos.y + 100, pos.width, 17), property.FindPropertyRelative("maxNum"));

        EditorGUI.LabelField(new Rect(pos.x, pos.y + 120, pos.width, 17), "Sprites");
        EditorGUI.indentLevel++;
        Rect spritesPos = new Rect(pos.x, pos.y + 140, pos.width, 17);
        SerializedProperty sprites = property.FindPropertyRelative("sprites");
        if (sprites.arraySize != 4) {
            sprites.arraySize = 4;
        }
        for (int i = 0; i < 4; i++) {
            sprites.GetArrayElementAtIndex(i).FindPropertyRelative("dir").intValue = i;
            EditorGUI.PropertyField(new Rect(spritesPos.x, spritesPos.y, 200, spritesPos.height), sprites.GetArrayElementAtIndex(i).FindPropertyRelative("sprite"), GUIContent.none);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(new Rect(spritesPos.x + 140, spritesPos.y, 100, spritesPos.height), sprites.GetArrayElementAtIndex(i).FindPropertyRelative("dir"), GUIContent.none);
            EditorGUI.EndDisabledGroup();
            spritesPos.y += 20;
        }
        EditorGUI.indentLevel--;

        Rect requirementsPos = new Rect(pos.x, pos.y + 240, 115, 17);
        SerializedProperty requirements = property.FindPropertyRelative("requirements");
        requirements.arraySize = 3;
        for (int i = 0; i < 3; i++) {
            SerializedProperty row = requirements.GetArrayElementAtIndex(i).FindPropertyRelative("data");
            row.arraySize = 3;
            for (int j = 0; j < 3; j++) {
                if (i == 1 && j == 1) {
                    row.GetArrayElementAtIndex(j).intValue = -3;
                } else {
                    EditorGUI.PropertyField(requirementsPos, row.GetArrayElementAtIndex(j), GUIContent.none);
                }
                requirementsPos.x += 60;
            }
            requirementsPos.x = pos.x;
            requirementsPos.y += 25;
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return 329;
    }
}

