using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class HandleLabel : MonoBehaviour
{
    public ModifySkinnedMesh meshEdit;
    public SkinnedMeshRenderer meshRenderer;
    public Mesh mesh;
    private Vector3[] verArray;
    public Vector3[] VertexArray { get { return verArray; } }
    private int[] indiceArray;
    public int[] IndiceArray { get { return indiceArray; } }
    // Use this for initialization
    public void Init()
    {
        mesh = (Mesh)Instantiate(meshRenderer.sharedMesh);
        meshRenderer.sharedMesh = mesh;
        verArray = new Vector3[mesh.vertexCount];
        indiceArray = mesh.GetIndices(0);
        ConvertMesh(mesh);
    }
    public List<int> CanGoIndex(int curIndex)
    {
        List<int> returnIdx = new List<int>();
       
        if (indiceArray == null)
            return returnIdx;
        int j = 0;
        //모든 사각형의 배열(List<List<int(4)>>)에서
        //해당 인덱스가 들어가 있는지 확인
        //해당인덱스의 연결된 인덱스 반환

        while (j < indiceArray.Length)
        {

        }
        return returnIdx;
    }
    public int[] AllIndice()
    {
      return indiceArray;
    }
    Mesh ConvertMesh(Mesh source)
    {
        // Input
        var inVertices = source.vertices;
        var inNormals = source.normals;
        var inTangents = source.tangents;
        var inBoneWeights = source.boneWeights;
        var inUV1 = source.uv;
        var inIndices = source.GetIndices(0);
        var inTriangles = source.triangles;
        // Output
        var outVertices = new List<Vector3>();
        var outNormals = new List<Vector3>();
        var outTangents = new List<Vector4>();
        var outBoneWeights = new List<BoneWeight>();
        var outUV1 = new List<Vector2>();
        var outUV2 = new List<Vector2>(); // centroid
        var outUV3 = new List<Vector4>(); // left, right

        // Enumerate all the triangles and belonging vertices.
        for (var i = 0; i < inIndices.Length; i += 3)
        {
            // Simply copy the original vertex attributes.
            // (position, normal, tangent, weight, UV1)

            var i1 = inIndices[i + 0];
            var i2 = inIndices[i + 1];
            var i3 = inIndices[i + 2];

            var v1 = inVertices[i1];
            var v2 = inVertices[i2];
            var v3 = inVertices[i3];

            var n1 = inNormals[i1];
            var n2 = inNormals[i2];
            var n3 = inNormals[i3];

            var t1 = inTangents[i1];
            var t2 = inTangents[i2];
            var t3 = inTangents[i3];

            outVertices.Add(v1);
            outVertices.Add(v2);
            outVertices.Add(v3);

            outNormals.Add(n1);
            outNormals.Add(n2);
            outNormals.Add(n3);

            outTangents.Add(t1);
            outTangents.Add(t2);
            outTangents.Add(t3);

            outBoneWeights.Add(inBoneWeights[i1]);
            outBoneWeights.Add(inBoneWeights[i2]);
            outBoneWeights.Add(inBoneWeights[i3]);

            outUV1.Add(inUV1[i1]);
            outUV1.Add(inUV1[i2]);
            outUV1.Add(inUV1[i3]);

            // Calculate the binormal vectors.
            var bn1 = Vector3.Cross(n1, t1) * t1.w;
            var bn2 = Vector3.Cross(n2, t2) * t2.w;
            var bn3 = Vector3.Cross(n3, t3) * t3.w;

            // Calculate the centroid of the triangle.
            var c = (v1 + v2 + v3) / 3;

            // Differences to the centroid.
            var difCV1 = c - v1;
            var difCV2 = c - v2;
            var difCV3 = c - v3;

            // Differences to the other vertices.
            var difV2V1 = v2 - v1;
            var difV3V1 = v3 - v1;

            var difV3V2 = v3 - v2;
            var difV1V2 = v1 - v2;

            var difV1V3 = v1 - v3;
            var difV2V3 = v2 - v3;

            // UV2 = [(C - V) * Tangent, (C - V) * Binormal]
            outUV2.Add(new Vector2(Vector3.Dot(difCV1, t1), Vector3.Dot(difCV1, bn1)));
            outUV2.Add(new Vector2(Vector3.Dot(difCV2, t2), Vector3.Dot(difCV2, bn2)));
            outUV2.Add(new Vector2(Vector3.Dot(difCV3, t3), Vector3.Dot(difCV3, bn3)));

            // UV3 = [(V_left  - V) * tangent, (V_left  - V) * binormal,
            //        (V_right - V) * tangent, (V_right - V) * binormal]
            outUV3.Add(new Vector4(Vector3.Dot(difV2V1, t1), Vector3.Dot(difV2V1, bn1),
                                   Vector3.Dot(difV3V1, t1), Vector3.Dot(difV3V1, bn1)));
            outUV3.Add(new Vector4(Vector3.Dot(difV3V2, t2), Vector3.Dot(difV3V2, bn2),
                                   Vector3.Dot(difV1V2, t2), Vector3.Dot(difV1V2, bn2)));
            outUV3.Add(new Vector4(Vector3.Dot(difV1V3, t3), Vector3.Dot(difV1V3, bn3),
                                   Vector3.Dot(difV2V3, t3), Vector3.Dot(difV2V3, bn3)));
        }

        GetWorldVerticePos();

        // Enumerate vertex indices.
        var indices = Enumerable.Range(0, outVertices.Count).ToArray();

        // Make a clone of the source mesh to avoid
        // the SMR internal caching problem - https://goo.gl/mORHCR
        var mesh = Object.Instantiate<Mesh>(source);
        mesh.name = mesh.name.Substring(0, mesh.name.Length - 7);

        // Clear the unused attributes.
        mesh.colors = null;
        mesh.uv4 = null;

        // Overwrite the vertices.
        mesh.subMeshCount = 0;
        mesh.SetVertices(outVertices);
        mesh.SetNormals(outNormals);
        mesh.SetTangents(outTangents);
        mesh.SetUVs(0, outUV1);
        mesh.SetUVs(1, outUV2);
        mesh.SetUVs(2, outUV3);
        mesh.bindposes = source.bindposes;
        mesh.boneWeights = outBoneWeights.ToArray();

        // Add point primitives.
        mesh.subMeshCount = 1;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        // Finishing up.
        mesh.UploadMeshData(true);

        return mesh;
    }
    void GetWorldVerticePos()
    {
        Matrix4x4[] boneMatrices = new Matrix4x4[meshRenderer.bones.Length];
        for (int i = 0; i < boneMatrices.Length; i++)
            boneMatrices[i] = meshRenderer.bones[i].localToWorldMatrix * mesh.bindposes[i];

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            BoneWeight weight = mesh.boneWeights[i];

            Matrix4x4 bm0 = boneMatrices[weight.boneIndex0];
            Matrix4x4 bm1 = boneMatrices[weight.boneIndex1];
            Matrix4x4 bm2 = boneMatrices[weight.boneIndex2];
            Matrix4x4 bm3 = boneMatrices[weight.boneIndex3];

            Matrix4x4 vertexMatrix = new Matrix4x4();

            for (int n = 0; n < 16; n++)
            {
                vertexMatrix[n] =
                    bm0[n] * weight.weight0 +
                    bm1[n] * weight.weight1 +
                    bm2[n] * weight.weight2 +
                    bm3[n] * weight.weight3;
            }

            verArray[i] = vertexMatrix.MultiplyPoint3x4(mesh.vertices[i]);
            //normals[i] = vertexMatrix.MultiplyVector(mesh.normals[i]);
        }
    }
}