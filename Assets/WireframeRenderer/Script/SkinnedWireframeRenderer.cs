using System.Collections.Generic;
using System;
using UnityEngine;


public class SkinnedWireframeRenderer : MonoBehaviour
{

    private const float Distance = 1.0001f;
    private static readonly Color Color = Color.black;
    private List<Vector3> _renderingQueue;
    private Dictionary<int, int[]> moveablePoint = new Dictionary<int, int[]>();
    // ReSharper disable once MemberCanBePrivate.Global
    public Material WireMaterial;

    public Mesh mesh;
    public SkinnedMeshRenderer meshRenderer;
    Vector3[] verArray;
    Matrix4x4[] boneMatrices;
    void Start()
    {
        mesh = (Mesh)Instantiate(meshRenderer.sharedMesh);
        meshRenderer.sharedMesh = mesh;
        verArray = new Vector3[mesh.vertexCount];

        boneMatrices = new Matrix4x4[meshRenderer.bones.Length];

        GetWorldVerticePos();

    }
 
    void GetWorldVerticePos()
    {
        Matrix4x4 vertexMatrix = new Matrix4x4();
        //Matrix4x4[] boneMatrices = new Matrix4x4[meshRenderer.bones.Length];
        for (int i = 0; i < boneMatrices.Length; i++)
            boneMatrices[i] = meshRenderer.bones[i].localToWorldMatrix * mesh.bindposes[i];

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            BoneWeight weight = mesh.boneWeights[i];

            Matrix4x4 bm0 = boneMatrices[weight.boneIndex0];
            Matrix4x4 bm1 = boneMatrices[weight.boneIndex1];
            Matrix4x4 bm2 = boneMatrices[weight.boneIndex2];
            Matrix4x4 bm3 = boneMatrices[weight.boneIndex3];

            for (int n = 0; n < 16; n++)
            {
                vertexMatrix[n] =
                    bm0[n] * weight.weight0 +
                    bm1[n] * weight.weight1 +
                    bm2[n] * weight.weight2 +
                    bm3[n] * weight.weight3;
            }

            verArray[i] = vertexMatrix.MultiplyPoint3x4(mesh.vertices[i]);
        }
    }

    // ReSharper disable once UnusedMember.Global
    private void Update()
    {
        Render();
    }

    // ReSharper disable once UnusedMember.Global
    public void Render()
    {

        GetWorldVerticePos();

       
        var inIndices = mesh.GetIndices(0);

        for (var i = 0; i < inIndices.Length; i += 3)
        {
            var i1 = inIndices[i + 0];
            var i2 = inIndices[i + 1];
            var i3 = inIndices[i + 2];

            var v1 = verArray[i1];
            var v2 = verArray[i2];
            var v3 = verArray[i3];

            float EdgeA = (v1 - v2).magnitude;
            float EdgeB = (v2 - v3).magnitude;
            float EdgeC = (v3 - v1).magnitude;

            var vertex1 = v1;
            var vertex2 = v2;
            var vertex3 = v3;

            if (EdgeA > EdgeB && EdgeA > EdgeC)
            {
                //can move 2<->3, 3<->1
                Debug.DrawLine(vertex2, vertex3, Color.red, 0, false);
                Debug.DrawLine(vertex3, vertex1, Color.red, 0, false);
            }
            else if (EdgeB > EdgeC && EdgeB > EdgeA)
            {
                Debug.DrawLine(vertex1, vertex3, Color.red, 0, false);
                Debug.DrawLine(vertex2, vertex1, Color.red, 0, false);       
            }
            else
            {
                Debug.DrawLine(vertex1, vertex2, Color.red, 0, false);
                Debug.DrawLine(vertex2, vertex3, Color.red, 0, false);
            }


        }
       
    }


}
