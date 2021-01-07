using UnityEngine;
using UnityEditor;
using System;

public class SSShaderGUI : ShaderGUI
{
    private static Material copyMat;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        Material targetMat = materialEditor.target as Material;

        if (GUILayout.Button("Copy"))
        {
            copyMat = targetMat;
        }

        if (copyMat != null 
            //&& targetMat.shader == copyMat.shader
            )
        {
            if (GUILayout.Button(string.Format("Paste({0})", copyMat.name)))
            {
                targetMat.CopyPropertiesFromMaterial(copyMat);
                EditorUtility.SetDirty(targetMat);
                copyMat = null;
            }
        }
    }
}