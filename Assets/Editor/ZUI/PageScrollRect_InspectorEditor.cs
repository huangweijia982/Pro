using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
[CustomEditor(typeof(PageScrollRect))]
public class PageScrollRect_InspectorEditor : ScrollRectEditor
{
    private PageScrollRect ps;
    private SerializedObject obj;
    private SerializedProperty pageNum;
    private SerializedProperty loadItemMode;
    private SerializedProperty slidMode;
    private SerializedProperty movementType;
    protected override void OnEnable()
    {
        base.OnEnable();
        ps = (PageScrollRect)target;
        obj = new SerializedObject(target);
        pageNum = obj.FindProperty("pageNum");
        loadItemMode = obj.FindProperty("loadItemMode");
        slidMode = obj.FindProperty("slidMode");
        movementType = obj.FindProperty("_movementType");
    }
    public override void OnInspectorGUI()
    {
        ps.prePageBtn = (Button)EditorGUILayout.ObjectField("prePageBtn",ps.prePageBtn,typeof(Button));
        ps.nextPageBtn = (Button)EditorGUILayout.ObjectField("nextPageBtn", ps.nextPageBtn, typeof(Button));
        ps.pageTxt = (Text)EditorGUILayout.ObjectField("pageTxt", ps.pageTxt, typeof(Text));
        ps.contentGG = (GridLayoutGroup)EditorGUILayout.ObjectField("contentGG", ps.contentGG, typeof(GridLayoutGroup));
        ps.content = (RectTransform)EditorGUILayout.ObjectField("content", ps.content, typeof(RectTransform));
        EditorGUILayout.PropertyField(slidMode);
       
        ps.pageSize = EditorGUILayout.FloatField("pageSize", ps.pageSize);
        ps.slidSpeed= EditorGUILayout.FloatField("slidSpeed",ps.slidSpeed);
        ps.dragSensitive= EditorGUILayout.Slider(new GUIContent("dragSensitive"), ps.dragSensitive, 0, 1);
        EditorGUILayout.PropertyField(loadItemMode);
        if(loadItemMode.enumValueIndex==0) EditorGUILayout.PropertyField(pageNum);
        obj.ApplyModifiedProperties();
        EditorGUILayout.PropertyField(movementType);
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
   
}
