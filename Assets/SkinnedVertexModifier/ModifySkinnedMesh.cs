using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;


public class ModifySkinnedMesh : MonoBehaviour
{
    public SkinnedMeshRenderer meshRenderer;
    
    public Mesh mesh;
    Vector3 hitPos;

    List<List<int>> QuadList = new List<List<int>> ();

    Vector3[] verArray;
    Vector3 verPos1;
    Vector3 verPos2;
    Vector3 verPos3;

    void Start()
    {
        mesh = (Mesh)Instantiate(meshRenderer.sharedMesh);
        meshRenderer.sharedMesh = mesh;
        verArray = new Vector3[mesh.vertexCount];
        ConvertMesh(mesh);
        GetWorldVerticePos();
        FindAllQuad();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
   
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000))
            {
                hitPos = hit.point;
                int index = HitTestMesh(mesh, hitPos);
                if(index > -1)
                {
                    StartDeleteQuadPoint(index);
                    //ConvertMesh(mesh);
                }
                verArray = new Vector3[mesh.vertexCount];

            }
            Debug.DrawRay(ray.origin, ray.direction *10, Color.green);
        }
    }

    int HitTestMesh(Mesh source, Vector3 hitPos)
    {
        // Input
        var inVertices = source.vertices;
       
        var inNormals = source.normals;
        var inTangents = source.tangents;
        var inBoneWeights = source.boneWeights;
        var inUV1 = source.uv;
        var inIndices = source.GetIndices(0);

        // Output
        var outVertices = new List<Vector3>();

        GetWorldVerticePos();

        // Enumerate all the triangles and belonging vertices.
        for (var i = 0; i < inIndices.Length; i += 3)
        {
            var i1 = inIndices[i + 0];
            var i2 = inIndices[i + 1];
            var i3 = inIndices[i + 2];

            var v1 = verArray[i1];
            var v2 = verArray[i2];
            var v3 = verArray[i3];

            if (!SameSide(hitPos, v1, v2, v3))
                continue;
            bool hit = TestTriangle((v1),(v2),(v3), hitPos);

            if (hit)
            {
                Debug.Log(string.Format("{0}/{1}/{2}",i1,i2,i3));
                verPos1 = v1;
                verPos2 = v2;
                verPos3 = v3;
                return i;
            }
               
          
        }
        return -1;
    }
    bool drawOnce = false;
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0);

        //Gizmos.DrawCube(hitPos, new Vector3(0.1f, 0.1f, 0.1f));
         
        if(verArray == null)
            return;
        for(int i = 0; i< verArray.Length; ++i)
        {
           Gizmos.DrawCube(verArray[i], new Vector3(0.1f, 0.1f, 0.1f));
        }

            // for(int i = 0 ; i< QuadList.Count;++i){
            //     foreach(var item in QuadList[i]){
                    
                    
                  
            //          var vPos = verArray[item];
            //          Gizmos.DrawCube(vPos, new Vector3(0.01f, 0.01f, 0.01f)); 
                       
            //     }
            // }
         
        
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
    bool SameSide(Vector3 p1, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 cp1 = Vector3.Cross(b - a, p1 - a);
        Vector3 cp2 = Vector3.Cross(p1 - a, c - a);
        if (Vector3.Dot(cp1, cp2) >= 0)
            return true;
        else
            return false;
    }

   
    bool TestTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {

        // Compute vectors        
        Vector3 v0 = C - A;
        Vector3 v1 = B - A;
        Vector3 v2 = P - A;

        // Compute dot products
        float dot00 = Vector3.Dot(v0, v0);
        float dot01 = Vector3.Dot(v0, v1);
        float dot02 = Vector3.Dot(v0, v2);
        float dot11 = Vector3.Dot(v1, v1);
        float dot12 = Vector3.Dot(v1, v2);

        // Compute barycentric coordinates
        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if point is in triangle
        return (u >= 0) && (v >= 0) && (u + v < 1);


    }
    void DeleteQuad(int index1, int index2)
    {
        int[] oldTriangles = mesh.triangles;
        int[] newTriangles = new int[mesh.triangles.Length - 6]; //6:deleted 2 triangle

        int i = 0;
        int j = 0;
        while (i < newTriangles.Length && j < mesh.triangles.Length)
        {
            if (j != index1 * 3 && j != index2 * 3)
            {
                newTriangles[i++] = oldTriangles[j++];
                newTriangles[i++] = oldTriangles[j++];
                newTriangles[i++] = oldTriangles[j++];
            }
            else
            {
                j += 3;
            }
        }
       mesh.triangles = newTriangles;
    }

    void StartDeleteQuadPoint(int HitIndex)
    {
        
        //Vector3[] vertices = mesh.vertices;
        var inIndices = mesh.GetIndices(0);

        Vector3 p0 = verArray[inIndices[HitIndex]];
        Vector3 p1 = verArray[inIndices[HitIndex + 1]];
        Vector3 p2 = verArray[inIndices[HitIndex + 2]];

        float edge1 = Vector3.Distance(p0, p1);
        float edge2 = Vector3.Distance(p0, p2);
        float edge3 = Vector3.Distance(p1, p2);

        Vector3 sharedPoint1 = Vector3.zero;
        Vector3 sharedPoint2 = Vector3.zero;

        if (edge1 > edge2 && edge1 > edge3)
        {
            sharedPoint1 = p0;
            sharedPoint2 = p1;
        }
        else if (edge2 > edge1 && edge2 > edge3)
        {
            sharedPoint1 = p0;
            sharedPoint2 = p2;
        }
        else
        {
            sharedPoint1 = p1;
            sharedPoint2 = p2;
        }


        int triIdx = FindTriangleIndex(p0, p1, p2, mesh, verArray);
        int nextTriangle = FindTriangleIndex2(sharedPoint1, sharedPoint2, mesh, verArray, triIdx);
        
       
         DeleteQuad(triIdx, nextTriangle);
    }

    int FindTriangleIndex(Vector3 vertice1, Vector3 vertice2, Vector3 vertice3, Mesh mesh, Vector3[] verArray)
    {
        int[] indice = mesh.GetIndices(0);
        Vector3[] vertices = verArray;
        int i = 0;
        int j = 0;

        while (j < indice.Length)
        {
           
            if (vertices[indice[j]] == vertice1 && vertices[indice[j + 1]] == vertice2 && vertices[indice[j + 2]] == vertice3)
            {
                return i;
            }
            if (vertices[indice[j]] == vertice2 && vertices[indice[j + 1]] == vertice1 && vertices[indice[j + 2]] == vertice3)
            {
                return i;
            }
            if (vertices[indice[j]] == vertice3 && vertices[indice[j + 1]] == vertice2 && vertices[indice[j + 2]] == vertice1)
            {
                return i;
            }
            if (vertices[indice[j]] == vertice1 && vertices[indice[j + 1]] == vertice3 && vertices[indice[j + 2]] == vertice2)
            {
                return i;
            }
            if (vertices[indice[j]] == vertice2 && vertices[indice[j + 1]] == vertice3 && vertices[indice[j + 2]] == vertice1)
            {
                return i;
            }
            if (vertices[indice[j]] == vertice3 && vertices[indice[j + 1]] == vertice1 && vertices[indice[j + 2]] == vertice2)
            {
                return i;
            }

            i++;
            j += 3;
           
        }
        return -1;
    }
    int FindTriangleIndex2(Vector3 vertice1, Vector3 vertice2, Mesh mesh, Vector3[] verArray, int notTriIndex)
    {
        int[] indice = mesh.GetIndices(0);
        Vector3[] vertices = verArray;
        int i = 0;
        int j = 0;

        while (j < indice.Length)
        {
            if (i == notTriIndex)
            {
                i++;
                j += 3;
                continue;
            }
            if (vertices[indice[j]] == vertice1 && (vertices[indice[j + 1]] == vertice2 || vertices[indice[j + 2]] == vertice2))
            {
                return i;
            }
            else if (vertices[indice[j]] == vertice2 && (vertices[indice[j + 1]] == vertice1 || vertices[indice[j + 2]] == vertice1))
            {
                return i;
            }
            else if (vertices[indice[j + 1]] == vertice2 && (vertices[indice[j]] == vertice1 || vertices[indice[j + 2]] == vertice1))
            {
                return i;
            }
            else if (vertices[indice[j + 1]] == vertice1 && (vertices[indice[j]] == vertice2 || vertices[indice[j + 2]] == vertice2))
            {
                return i;
            }
            else if (vertices[indice[j + 2]] == vertice2 && (vertices[indice[j]] == vertice1 || vertices[indice[j + 1]] == vertice1))
            {
                return i;
            }
            else if (vertices[indice[j + 2]] == vertice1 && (vertices[indice[j]] == vertice2 || vertices[indice[j + 1]] == vertice2))
            {
                return i;
            }
            i++;
            j += 3;
        }
        return -1;
    }

    
    void FindAllQuad()
    {
        var inIndices = mesh.GetIndices(0);
        for (var i = 0; i < inIndices.Length; i += 3)
        {
            var i1 = inIndices[i];
            var i2 = inIndices[i + 1];
            var i3 = inIndices[i + 2];

            var v1 = verArray[i1];
            var v2 = verArray[i2];
            var v3 = verArray[i3];

            var vecA = v2 - v1;
            var vecB = v3 - v1;

            var area = Vector3.Cross(vecA,vecB).magnitude * 0.5f;

            bool findQuad = false;
            
            for(var j = 0; j < inIndices.Length; j+=3){

                var i4 = inIndices[j];
                var i5 = inIndices[j + 1];
                var i6 = inIndices[j + 2];
                
                if(((i5 == i2 && i6 == i3) || (i5 == i3 && i6 == i2)) && i4 != i1)
                {
                    var vecC = v2 - verArray[i4];
                    var vecD = v3 - verArray[i4];
                    var area2 = Vector3.Cross(vecC,vecD).magnitude;
                    if(area == area2){
                        findQuad = true;
                        var innerList = new List<int>();
                        innerList.Add(i1);
                        innerList.Add(i2); 
                        innerList.Add(i3); 
                        innerList.Add(i4);   
                        QuadList.Add(innerList);
                        break;
                    }
                }
              
            }

            // if(!findQuad){
            //     var innerList = new List<int>();
            //     innerList.Add(i1);
            //     innerList.Add(i2); 
            //     innerList.Add(i3); 
            //     QuadList.Add(innerList);
            // }
           
        }
        Debug.Log(QuadList.Count);
        for(int i = 0 ; i< QuadList.Count;++i){
            foreach(var item in QuadList[i]){
                Debug.Log(string.Format("{0}/{1}",i,item));
            }
        }
    }

}
