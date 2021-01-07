using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif


[Serializable]
public class ReferenceCollectorData
{
    public string key;
    public Object gameObject;
}

public class ReferenceCollectorDataComparer : IComparer<ReferenceCollectorData>
{
    public int Compare(ReferenceCollectorData x, ReferenceCollectorData y)
    {
        return string.Compare(x.key, y.key, StringComparison.Ordinal);
    }
}


[Serializable]
public class ReferenceGroupData
{
    public string Key;
    [SerializeField]
    public List<ReferenceCollectorData> Value;
    public bool Foldout;

    public ReferenceGroupData(string key)
    {
        Key = key;
        Value = new List<ReferenceCollectorData>();
        Foldout = true;
    }
}

public static class ExpReferenceGroupFunc
{
    public static bool ContainsKey(this List<ReferenceGroupData> data, string key)
    {
        foreach (var item in data)
        {
            if (item.Key == key)
                return true;
        }
        return false;
    }
    public static ReferenceGroupData RemoveGroup(this List<ReferenceGroupData> data,string key)
    {
        ReferenceGroupData re = null;
        int index = -1;
        for(int i = 0;i< data.Count;++i)
        {
            if(data[i].Key == key)
            {
                index = i;
                break;
            }
        }
        if (index >= 0)
        {
            re = data[index];
            data.RemoveAt(index);
        }
        return re;
    }

    public static bool AddGroupItem(this List<ReferenceGroupData> data, string key, ReferenceCollectorData value)
    {
        foreach (var item in data)
        {
            if (item.Key == key)
            {
                item.Value.Add(value);
                return true;
            }
        }
        return false;
    }

    public static bool RemoveAtGroupItem(this List<ReferenceGroupData> data, string key, int value)
    {
        foreach (var item in data)
        {
            if (item.Key == key)
            {
                item.Value.RemoveAt(value);
                return true;
            }
        }
        return false;
    }

    public static bool RemoveGroupItem(this List<ReferenceGroupData> data, string key, ReferenceCollectorData value)
    {
        foreach (var item in data)
        {
            if (item.Key == key)
            {
                item.Value.Remove(value);
                return true;
            }
        }
        return false;
    }

    public static int GoupCount(this List<ReferenceGroupData> data, string key)
    {
        foreach (var item in data)
        {
            if (item.Key == key)
            {
                return item.Value.Count;
            }
        }
        return 0;
    }
}


public class ReferenceGroupCollector : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    public List<ReferenceGroupData> dataDir = new List<ReferenceGroupData>();

	public readonly Dictionary<string, Object> dict = new Dictionary<string, Object>();
    public readonly Dictionary<string, Dictionary<string, Object>> groupDir = new Dictionary<string, Dictionary<string, Object>>();

    public const string RootGroup = "Root";
#if UNITY_EDITOR
    public bool Add(string key,Object obj,string group = RootGroup)
    {
        var result = obj == null?null:Remove(obj);
        if (result != null)
        {
            Debug.LogWarning("已存在预设！");
        }
        else
        {
            if (CheckKey(key))
            {
                return false;
            }
            result = new ReferenceCollectorData() { key = key, gameObject = obj };
        }
        if (!dataDir.ContainsKey(group))
            dataDir.Add(new ReferenceGroupData(group));
        dataDir.AddGroupItem(group,result);
        return true;
    }

    /// <summary>
    /// 删除空引用
    /// </summary>
    public void RemoveEmptyRefer()
    {
        foreach(var group in dataDir)
        {
            for (int i = 0; i < group.Value.Count; ++i)
            {
                if (!group.Value[i].gameObject)
                {
                    group.Value.RemoveAt(i--);
                }
            }
        }
    }

    public void Remove(int index,string group)
    {
        if (!dataDir.ContainsKey(group))
            return;
        if (index >= dataDir.GoupCount(group))
            return;
        dataDir.RemoveAtGroupItem(group,index);
    }

    public void Remove(ReferenceCollectorData data,string group)
    {
        if (!dataDir.ContainsKey(group))
            return;
        dataDir.RemoveGroupItem(group,data);
    }

    public ReferenceCollectorData Remove(string key)
    {
        foreach (var item in dataDir)
        {
            for (int i = 0; i < item.Value.Count; ++i)
            {
                if (item.Value[i].key == key)
                {
                    var result = item.Value[i];
                    item.Value.RemoveAt(i--);
                    return result;
                }
            }
        }
        return null;
    }

    public bool CheckKey(string key)
    {
        foreach (var item in dataDir)
        {
            for (int i = 0; i < item.Value.Count; ++i)
            {
                if (item.Value[i].key == key)
                {
                    if (item.Value[i].gameObject)
                    {
                        return true;
                    }
                    else
                    {
                        Remove(key);
                        return false;
                    }
                }
            }
        }
        return false;
    }

    public ReferenceCollectorData Remove(Object obj)
    {
        foreach (var item in dataDir)
        {
            for (int i = 0; i < item.Value.Count; ++i)
            {
                if (item.Value[i].gameObject == obj)
                {
                    var result = item.Value[i];
                    item.Value.RemoveAt(i--);
                    return result;
                }
            }
        }
        return null;
    }

    public Object GetObject(string key,out string group)
    {
        foreach (var item in dataDir)
        {
            group = item.Key;
            for (int i = 0; i < item.Value.Count; ++i)
            {
                if (item.Value[i].key == key)
                {
                    return item.Value[i].gameObject;
                }
            }
        }
        group = "";
        return null;
    }

    public void Clear()
	{
		SerializedObject serializedObject = new SerializedObject(this);
		var dataProperty = serializedObject.FindProperty("dataDir");
		dataProperty.ClearArray();
		EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

	public void Sort()
	{
		SerializedObject serializedObject = new SerializedObject(this);
        foreach(var item in dataDir)
        {
            item.Value.Sort(new ReferenceCollectorDataComparer());
        }
		EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}
#endif

	public T Get<T>(string key) where T : class
	{
		Object dictGo;
		if (!dict.TryGetValue(key, out dictGo))
		{
			return null;
		}
		return dictGo as T;
	}

    public List<T> GetObjsByGroup<T>(string group)where T:class
    {
        List<T> results = new List<T>();
        Dictionary<string, Object> objs;
        if(!groupDir.TryGetValue(group,out objs))
        {
            return results;
        }
        foreach(var item in objs)
        {
            if (item.Value is T)
            {
                var temp = item.Value as T;
                if (temp != null)
                    results.Add(temp);
            }
        }
        return results;
    }

    public Dictionary<string,T> GetObjsDirByGroup<T>(string group) where T : class
    {
        var results = new Dictionary<string, T>();
        Dictionary<string, Object> objs;
        if (!groupDir.TryGetValue(group, out objs))
        {
            return results;
        }
        foreach (var item in objs)
        {
            if (item.Value is T)
            {
                var temp = item.Value as T;
                if (temp != null)
                    results.Add(item.Key,temp);
            }
        }
        return results;
    }

    public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
        groupDir.Clear();
		dict.Clear();
        foreach (var data in dataDir)
        {
            if (!groupDir.ContainsKey(data.Key))
                groupDir.Add(data.Key,new Dictionary<string, Object>());
            foreach (ReferenceCollectorData referenceCollectorData in data.Value)
            {
                if (!dict.ContainsKey(referenceCollectorData.key))
                {
                    dict.Add(referenceCollectorData.key, referenceCollectorData.gameObject);
                    groupDir[data.Key].Add(referenceCollectorData.key, referenceCollectorData.gameObject);
                }
            }
        }
	}
}