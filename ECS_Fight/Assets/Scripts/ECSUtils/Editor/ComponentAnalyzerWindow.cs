using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Unity.Entities;

public class ComponentAnalyzerWindow : EditorWindow
{
    private TreeViewState _mTreeViewState;

    private ComponentAnalyzerTreeView _mComponentAnalyzerTreeView;
    
    [SerializeField] private bool showOnlyProblematicComponents;
    [SerializeField] private string excludeString;

    void OnGUI ()
    {
        showOnlyProblematicComponents = EditorGUILayout.Toggle("Show problems only:", showOnlyProblematicComponents);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Exclude:", GUILayout.Width(40));
        excludeString = GUILayout.TextField(excludeString);
        GUILayout.EndHorizontal();
        _mComponentAnalyzerTreeView.OnGUI(new Rect(0, 40, position.width, position.height - 40));
        _mComponentAnalyzerTreeView.ShowOnlyProblematic(showOnlyProblematicComponents);
        _mComponentAnalyzerTreeView.Exclude(excludeString);
    }

    void OnEnable ()
    {
        // Check whether there is already a serialized view state (state 
        // that survived assembly reloading)
        if (_mTreeViewState == null)
            _mTreeViewState = new TreeViewState ();

        _mComponentAnalyzerTreeView = new ComponentAnalyzerTreeView(_mTreeViewState);
    }

    // Add menu named "My Window" to the Window menu
    [MenuItem ("DOTS/Analyze Components Size")]
    static void ShowWindow ()
    {
        // Get existing open window or if none, make a new one:
        var window = GetWindow<ComponentAnalyzerWindow> ();
        window.titleContent = new GUIContent ("Components Size");
        window.Show ();
    }
}

class ComponentAnalyzerTreeView : TreeView
{
    bool _showOnlyProblematicComponents;
    string _excludeString = "";
    
    public ComponentAnalyzerTreeView(TreeViewState treeViewState)
        : base(treeViewState)
    {
        Reload();
    }

    public void ShowOnlyProblematic(bool value)
    {
        if (_showOnlyProblematicComponents != value)
        {
            _showOnlyProblematicComponents = value;
            Reload();
        }
    }

    public void Exclude(string excludeString)
    {
        if (_excludeString == excludeString) return;
        _excludeString = excludeString;
        Reload();
    }
        
    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem      { id = 0, depth = -1, displayName = "Root" };
        
        var id = 1;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies)
        {
            if (IsExcluded(assembly.GetName().Name))
            {
                continue;
            }
            var assemblyItem = new TreeViewItem   { id = id, displayName = assembly.GetName().Name };
            var problems = 0;
            foreach (var type in assembly.DefinedTypes)
            {
                if (IsExcluded(type.Name))
                {
                    continue;
                }
                if (type.IsAbstract)
                {
                    continue;
                }
                if (typeof(IComponentData).IsAssignableFrom(type))
                {
                    var size = TypeSize.GetTypeSize(type);
                    id++;
                    
                    var warnings = new List<String>();
                    var possibleSize = TypeSize.GetStructSize(type, warnings);
                    if (warnings.Count > 0)
                    {
                        Debug.LogWarning($"{string.Join(",\n",warnings.ToArray())}");
                    }
                    var prefix = size <= possibleSize ? "✔︎" : "✘️";
                    if (possibleSize < size)
                    {
                        problems++;
                    }

                    var show = !_showOnlyProblematicComponents || size > possibleSize;

                    if (show)
                    {
                        var fields = new List<FieldInfo>();
                        TypeSize.CollectFields(type, fields);
                        
                        var text = $"{prefix} {type.Name} holds {fields.Count} values";
                        if (fields.Count > 0)
                        {
                            text += $" in {size} bytes";
                        }
                        if (size > possibleSize)
                        {
                            text += $", where {possibleSize} bytes is possible";
                        }
                        var componentItem = new TreeViewItem   { id = id, displayName = text };
                        assemblyItem.AddChild(componentItem);    
                    }
                    
                }
            }

            if (problems > 0)
            {
                assemblyItem.displayName = $"{assemblyItem.displayName} [{problems}]";
            }
            if (assemblyItem.hasChildren)
            {
                root.AddChild(assemblyItem);
            }
            id++;
        }

        if (root.hasChildren == false)
        {
            root.AddChild(new TreeViewItem(1, 1, "No components were found"));
        }
        
        SetupDepthsFromParentsAndChildren(root);
        return root;
    }

    protected override void SelectionChanged(IList<int> selectedIds)
    {
        base.SelectionChanged(selectedIds);
        var rows = FindRows(selectedIds);
        
    }

    private bool IsExcluded(string value)
    {
        if (_excludeString == null || _excludeString.Trim().Length == 0)
        {
            return false;
        }
        foreach (var exclude in _excludeString.Split(','))
        {
            var trimmedExclude = exclude.Trim();
            if (trimmedExclude.Length == 0)
            {
                continue;
            }
            if (value.StartsWith(trimmedExclude))
            {
                return true;
            }
        }

        return false;
    }
}

public static class TypeSize
{
    private static readonly ConcurrentDictionary<Type, int> Cache = new ConcurrentDictionary<Type, int>();
    public static int GetTypeSize(Type type)
    {
        return Cache.GetOrAdd(type, _ => UnsafeUtility.SizeOf(type));
    }

    public static int GetStructSize(Type type, List<string> warnings)
    {
        var fields = new List<FieldInfo>();
        CollectFields(type, fields);
        var biggestSize = 1;
        var sum = 0;
        foreach (var field in fields)
        {
            var pType = field.FieldType;
            if (pType.GetCustomAttribute<StructLayoutAttribute>() != null)
            {
                Debug.LogWarning($"{pType.FullName}");
            }
            var fSize = GetTypeSize(pType);
            if (pType.IsEnum && fSize > 1)
            {
                warnings.Add($"Consider defining `enum {pType.Name}: byte` in order to reduce size from {fSize} bytes to 1");
                fSize = 1;
            }

            sum += fSize;
            biggestSize = fSize > biggestSize ? fSize : biggestSize;
        }

        if ((sum % biggestSize) == 0)
        {
            return sum == 0 ? 1 : sum;
        }
        return sum + (biggestSize - (sum % biggestSize));
    }

    public static void CollectFields(Type type, List<FieldInfo> list)
    {
        var fields = type.GetFields(BindingFlags.Public | 
                                    BindingFlags.NonPublic | 
                                    BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.FieldType == type)
            {
                continue;
            }
            if (field.IsStatic)
            {
                continue;
            }
            
            if (field.FieldType.IsExplicitLayout)
            {
                list.Add(field);
                continue;
            }
            if (field.FieldType.IsPrimitive || field.FieldType.IsEnum)
            {
                list.Add(field);
            }
            else if(field.FieldType.IsValueType)
            {
                CollectFields(field.FieldType, list);
            }
        }
    }
}