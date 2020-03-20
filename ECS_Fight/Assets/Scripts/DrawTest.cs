using System.Collections.Generic;
using UnityEngine;

public class DrawTest : MonoBehaviour
{
    public Mesh mesh;
    public Material material;

    List<Matrix4x4> matrices = new List<Matrix4x4>();

    void Start()
    {
        if (!material.enableInstancing || !SystemInfo.supportsInstancing)
        {
            enabled = false;
            return;
        }

        for (int i = 0; i < 10; ++i)
        {
            Matrix4x4 mt = Matrix4x4.TRS(transform.position + (i * 1.1f) * Vector3.up, transform.rotation, Vector3.one);
            matrices.Add(mt);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices);
    }
}