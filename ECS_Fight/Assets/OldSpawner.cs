using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldSpawner : MonoBehaviour
{
    public GameObject prefab;
    [SerializeField] private int xSize = 10;
    [SerializeField] private int ySize = 10;
    [Range(0.1f, 2f)]
    [SerializeField] private float spacing = 1f;

    private List<GameObject> instances = new List<GameObject>();

    private float amplitude = 10f;
    private float xOffset = 0.25f;
    private float yOffset = 0.25f;
    private float moveSpeed = 1.0f;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        InstantiateEntityGrid(xSize, ySize, spacing);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        for (var i = 0; i < instances.Count; i++)
        {
            Vector3 transPos = instances[i].transform.position;
            float zPos = amplitude * Mathf.Sin((float)Time.realtimeSinceStartup * moveSpeed
           + transPos.x * xOffset + transPos.y * yOffset);

            instances[i].transform.position = new Vector3(transPos.x, transPos.y, zPos);
        }
    }

    private void InstantiateEntityGrid(int dimX, int dimY, float spacing = 1f)
    {
        for (int i = 0; i < dimX; i++)
        {
            for (int j = 0; j < dimY; j++)
            {
                InstantiateEntity(new Vector3(i * spacing, j * spacing, 0f));
            }
        }
    }
    private void InstantiateEntity(Vector3 pos)
    {
        GameObject inst = GameObject.Instantiate(prefab);
        inst.transform.position = pos;
        inst.transform.localScale = Vector3.one;
        instances.Add(inst);
    }
}
