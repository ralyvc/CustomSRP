using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBall : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");
    static int metallicId = Shader.PropertyToID("_Metallic");
    static int smoothnessId = Shader.PropertyToID("_Smoothness");

    [SerializeField] Mesh mesh = default;

    [SerializeField] Material material = default;

    Matrix4x4[] matrices = new Matrix4x4[1023];
    Vector4[] baseColors = new Vector4[1023];
    float[] metallic = new float[1023];
    float[] smoothness = new float[1023];

    MaterialPropertyBlock block;

    private void Awake()
    {
        for (var i = 0; i < matrices.Length; i++)
        {
            matrices[i] = Matrix4x4.TRS(
                UnityEngine.Random.insideUnitSphere * 10f,
                Quaternion.Euler(UnityEngine.Random.value * 360f, UnityEngine.Random.value * 360f,
                    UnityEngine.Random.value * 360f),
                Vector3.one * UnityEngine.Random.Range(0.5f, 1.5f)
            );
            baseColors[i] = new Vector4(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value,
                1f);
            metallic[i] = UnityEngine.Random.Range(0.0f, 1f);
            smoothness[i] = UnityEngine.Random.Range(0.05f, 0.95f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
            block.SetVectorArray(baseColorId, baseColors);
            block.SetFloatArray(metallicId, metallic);
            block.SetFloatArray(smoothnessId, smoothness);
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, 1023, block);

    }
}