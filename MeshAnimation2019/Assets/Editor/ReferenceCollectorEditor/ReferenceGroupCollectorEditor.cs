using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(ReferenceGroupCollector))]
[CanEditMultipleObjects]
public class ReferenceGroupCollectorEditor : Editor
{

    string searchGroupKey;
    private string searchKey;
    private Object prefab;

    private ReferenceGroupCollector referenceGroupCollector;
    private string groupKey = "";
    bool onlySprite = true;
    bool lockName = true;

    private void DelNullReference()
    {
        var dataProperty = serializedObject.FindProperty("dataDir");
        for (int i = dataProperty.arraySize - 1; i >= 0; i--)
        {
            var list = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Value");
            if (list.arraySize == 0)
            {
                dataProperty.DeleteArrayElementAtIndex(i);
                continue;
            }
            for (int j = list.arraySize - 1; j >= 0; j--)
            {
                var gameObjectProperty = list.GetArrayElementAtIndex(j).FindPropertyRelative("gameObject");
                if (gameObjectProperty.objectReferenceValue == null)
                {
                    list.DeleteArrayElementAtIndex(j);
                }
            }
        }
    }

    private void Awake()
    {
        referenceGroupCollector = (ReferenceGroupCollector)target;
        if (referenceGroupCollector.dataDir.Count == 0 && !referenceGroupCollector.dataDir.ContainsKey(ReferenceGroupCollector.RootGroup))
            referenceGroupCollector.dataDir.Add(new ReferenceGroupData(ReferenceGroupCollector.RootGroup));
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        Undo.RecordObject(referenceGroupCollector, "Changed Settings");
        EditorGUILayout.BeginHorizontal();
        groupKey = EditorGUILayout.TextField(groupKey);
        if (GUILayout.Button("创建组"))
        {
            CreatGroup(groupKey);
            groupKey = "";
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        var key = EditorGUILayout.TextField(searchKey, GUI.skin.GetStyle("SearchTextField"));
        if (GUILayout.Button("", GUI.skin.GetStyle("SearchCancelButton")))
        {
            key = "";
        }
        if (key != searchKey)
        {
            Search(key);
        }
        EditorGUILayout.ObjectField(prefab, typeof(Object), false);
        if (GUILayout.Button("删除"))
        {
            referenceGroupCollector.Remove(searchKey);
            prefab = null;
        }
        EditorGUILayout.EndHorizontal();
        lockName = EditorGUILayout.ToggleLeft("锁定组名", lockName);
        onlySprite = EditorGUILayout.ToggleLeft("图片只接受Sprite", onlySprite);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        List<string> delectGroup = new List<string>();
        for (int index = 0; index < referenceGroupCollector.dataDir.Count; ++index)
        {
            var item = referenceGroupCollector.dataDir[index];
            if (item.Key == searchGroupKey)
            {
                EditorGUILayout.BeginVertical("LightmapEditorSelectedHighlight");
            }
            else
            {
                EditorGUILayout.BeginVertical();
            }
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            if (lockName)
            {
                item.Foldout = EditorGUILayout.Foldout(item.Foldout, item.Key, true);
            }
            else
            {
                item.Foldout = EditorGUILayout.Foldout(item.Foldout, "");
                var newKey = EditorGUILayout.TextField(item.Key);
                if (!newKey.Equals(item.Key))
                {
                    bool isRename = true;
                    if (string.Empty.Equals(newKey))
                    {
                        Debug.LogWarning("分组key不能为空");
                        isRename = false;
                    }
                    foreach (var dir in referenceGroupCollector.dataDir)
                    {
                        if (dir.Key == newKey && dir.Value != item.Value)
                        {
                            Debug.LogWarning("已包含分组");
                            isRename = false;
                            break;
                        }
                    }
                    if (isRename)
                    {
                        item.Key = newKey;
                    }
                }
            }
            int lastOrEnd = 0;
            if (index == 0) lastOrEnd = 1;
            if (index == referenceGroupCollector.dataDir.Count - 1) lastOrEnd = -1;
            if (referenceGroupCollector.dataDir.Count != 1)
            {
                if (lastOrEnd != 1 && GUILayout.Button("▲", GUI.skin.GetStyle("ButtonLeft"), GUILayout.MaxWidth(30)))
                {
                    referenceGroupCollector.dataDir[index] = referenceGroupCollector.dataDir[index - 1];
                    referenceGroupCollector.dataDir[index - 1] = item;
                }
                if (lastOrEnd != -1 && GUILayout.Button("▼", GUI.skin.GetStyle(lastOrEnd == 1 ? "ButtonLeft" : "ButtonMid"), GUILayout.MaxWidth(30)))
                {
                    referenceGroupCollector.dataDir[index] = referenceGroupCollector.dataDir[index + 1];
                    referenceGroupCollector.dataDir[index + 1] = item;
                }
            }
            if (GUILayout.Button("X", GUI.skin.GetStyle("ButtonRight"), GUILayout.MaxWidth(30)))
            {
                delectGroup.Add(item.Key);
            }
            EditorGUILayout.EndHorizontal();
            if (item.Foldout)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("添加引用"))
                {
                    referenceGroupCollector.Add(Guid.NewGuid().GetHashCode().ToString(), null, item.Key);
                }
                if (GUILayout.Button("全部删除"))
                {
                    item.Value.Clear();
                }
                if (GUILayout.Button("删除空引用"))
                {
                    for (int i = 0; i < item.Value.Count; ++i)
                    {
                        if (!item.Value[i].gameObject)
                        {
                            item.Value.RemoveAt(i--);
                        }
                    }
                }
                if (GUILayout.Button("排序"))
                {
                    item.Value.Sort(new ReferenceCollectorDataComparer());
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                ShowGroup(item.Key, item.Value);
                var are = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
                GUI.contentColor = Color.white;
                GUI.Box(are, "\nDrag Are");
                EditorGUILayout.Space();
                var eventType = Event.current.type;
                switch (eventType)
                {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        if (are.Contains(Event.current.mousePosition))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            if (eventType == EventType.DragPerform)
                            {
                                DragAndDrop.AcceptDrag();
                                for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                                {
                                    var o = DragAndDrop.objectReferences[i];
                                    bool result = true;
                                    if (onlySprite && o is Texture2D)
                                    {
                                        var tex = o as Texture2D;
                                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DragAndDrop.paths[i]);
                                        if (sprite != null)
                                        {
                                            result = referenceGroupCollector.Add(o.name, sprite, item.Key);
                                        }
                                        else
                                        {
                                            Debug.LogError("目前只接受Sprite,请将图集转成Sprite类型！");
                                        }
                                    }
                                    else
                                    {
                                        result = referenceGroupCollector.Add(o.name, o, item.Key);
                                    }
                                    if (!result)
                                    {
                                        SameKeyError(o.name);
                                        Search(o.name);
                                    }

                                }
                            }
                            Event.current.Use();
                        }
                        break;
                    default:
                        break;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        foreach (var item in delectGroup)
        {
            referenceGroupCollector.dataDir.RemoveGroup(item);
        }

        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfRequiredOrScript();
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(referenceGroupCollector);
        }
    }

    void CreatGroup(string key)
    {
        if (string.Empty.Equals(key))
        {
            Debug.LogWarning("分组key不能为空");
            return;
        }
        if (referenceGroupCollector.dataDir.ContainsKey(key))
        {
            Debug.LogWarning("已包含分组");
            return;
        }
        referenceGroupCollector.dataDir.Add(new ReferenceGroupData(key));
    }

    void ShowGroup(string group, List<ReferenceCollectorData> data)
    {
        var delList = new List<int>();
        for (int i = 0; i < data.Count; ++i)
        {
            if (data[i].key == searchKey)
            {
                EditorGUILayout.BeginHorizontal("SelectionRect");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
            }
            var newKey = EditorGUILayout.TextField(data[i].key, GUILayout.Width(150));
            if (!newKey.Equals(data[i].key))
            {
                bool isRename = true;
                foreach (var dir in referenceGroupCollector.dataDir)
                {
                    foreach (var detail in dir.Value)
                    {
                        if (newKey.Equals(detail.key) && detail.gameObject != data[i].gameObject)
                        {
                            SameKeyError(newKey);
                            isRename = false;
                        }
                    }
                }
                if (isRename)
                {
                    data[i].key = newKey;
                }
            }
            var obj = data[i].gameObject;
            if (obj is Sprite)
            {
                data[i].gameObject = EditorGUILayout.ObjectField(data[i].gameObject, typeof(Sprite), true);
            }
            else
            {
                data[i].gameObject = EditorGUILayout.ObjectField(data[i].gameObject, typeof(Object), true);
            }
            if (GUILayout.Button("X"))
            {
                delList.Add(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        foreach (var i in delList)
        {
            var item = data[i];
            data.RemoveAt(i);
            referenceGroupCollector.Remove(item, group);
        }
    }

    Object Search(string key = "")
    {
        searchKey = key;
        prefab = referenceGroupCollector.GetObject(searchKey, out searchGroupKey);
        return prefab;
    }

    void SameKeyError(string key)
    {
        Debug.Log($"已存在Key:{key}！");
    }
}