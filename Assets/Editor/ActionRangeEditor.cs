using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ActionRange))]
public class ActionRangeDrawer : PropertyDrawer
{
    SerializedProperty X, Y;
    string name;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        
        name = property.displayName;
        property.Next(true);
        X = property.Copy();
        property.Next(true);
        Y = property.Copy();


        Rect contentPosition = EditorGUI.PrefixLabel(position, new GUIContent(name));

        //Check if there is enough space to put the name on the same line (to save space)
        if (position.height > 16f)
        {
            position.height = 16f;
            EditorGUI.indentLevel += 1;
            contentPosition = EditorGUI.IndentedRect(position);
            contentPosition.y += 18f;
        }

        float half = contentPosition.width / 2;
        GUI.skin.label.padding = new RectOffset(3, 3, 6, 6);

        //show the X and Y from the point
        EditorGUIUtility.labelWidth = 30f;
        contentPosition.width *= 0.48f;
        EditorGUI.indentLevel = 0;

        // Begin/end property & change check make each field
        // behave correctly when multi-object editing.
        EditorGUI.BeginProperty(contentPosition, label, X);
        {
            EditorGUI.BeginChangeCheck();
            var newVal = EditorGUI.FloatField(contentPosition, new GUIContent("Min"), X.floatValue);
            if (EditorGUI.EndChangeCheck())
                X.floatValue = newVal;
        }
        EditorGUI.EndProperty();

        contentPosition.x += half + 0.04f*half;

        EditorGUI.BeginProperty(contentPosition, label, Y);
        {
            EditorGUI.BeginChangeCheck();
            var newVal = EditorGUI.FloatField(contentPosition, new GUIContent("Max"), Y.floatValue);
            if (EditorGUI.EndChangeCheck())
                Y.floatValue = newVal;
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return Screen.width < 333 ? (16f + 18f) : 16f;
    }
}