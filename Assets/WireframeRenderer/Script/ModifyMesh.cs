using UnityEngine;
using System.Collections;

public class ModifyMesh : MonoBehaviour{
    
    void DeleteTri(Transform tm,int index)
    {
        bool deleteScript = false;
        if(tm.gameObject.GetComponent<MeshCollider>() != null)
        {
            Destroy(tm.gameObject.GetComponent<MeshCollider>());
            deleteScript = true;
        }

        Destroy(tm.gameObject.GetComponent<MeshCollider>());
        if(tm.GetComponent<MeshFilter>() == null)
            return;
        Mesh mesh = tm.GetComponent<MeshFilter>().mesh;

        int[] oldTriangles = mesh.triangles;
        int[] newTriangles = new int[mesh.triangles.Length - 3];

        int i = 0;
        int j = 0;
        while (j < mesh.triangles.Length){
            if(j != index * 3){
                newTriangles[i++] = oldTriangles[j++];
                newTriangles[i++] = oldTriangles[j++];
                newTriangles[i++] = oldTriangles[j++];
            }else{
                j+=3;
            }
        }
        tm.GetComponent<MeshFilter>().mesh.triangles = newTriangles;
        if(deleteScript)
            tm.gameObject.AddComponent<MeshCollider>();
    }

    void DeleteSquare(Transform tm,int index1, int index2)
    {
        bool deleteScript = false;
        if(tm.gameObject.GetComponent<MeshCollider>() != null)
        {
            Destroy(tm.gameObject.GetComponent<MeshCollider>());
            deleteScript = true;
        }
        if(tm.GetComponent<MeshFilter>() == null)
            return;
        Mesh mesh = tm.GetComponent<MeshFilter>().mesh;

        int[] oldTriangles = mesh.triangles;
        int[] newTriangles = new int[mesh.triangles.Length - 6];

        int i = 0;
        int j = 0;
        while (i < newTriangles.Length && j < mesh.triangles.Length){
            if(j != index1 * 3 &&  j != index2 *3){
                newTriangles[i++] = oldTriangles[j++];
                newTriangles[i++] = oldTriangles[j++];
                newTriangles[i++] = oldTriangles[j++];
            }else{
                j+=3;
            }
        }
        tm.GetComponent<MeshFilter>().mesh.triangles = newTriangles;
         if(deleteScript)
            tm.gameObject.AddComponent<MeshCollider>();
    }
    void StartDeleteQuad(Transform tm,int HitIndex)
    {
        if(tm.GetComponent<MeshFilter>() == null)
            return;
        Mesh mesh = tm.GetComponent<MeshFilter>().mesh;

        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector3 p0 = vertices[triangles[HitIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[HitIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[HitIndex * 3 + 2]];

        float edge1 = Vector3.Distance(p0, p1);
        float edge2 = Vector3.Distance(p0, p2);
        float edge3 = Vector3.Distance(p1, p2);

        Vector3 sharedPoint1 = Vector3.zero;
        Vector3 sharedPoint2 = Vector3.zero;

        if(edge1> edge2 && edge1 > edge3)
        {
            sharedPoint1 = p0;
            sharedPoint2 = p1;
        }else if(edge2> edge1 && edge2 > edge3)
        {
            sharedPoint1 = p0;
            sharedPoint2 = p2;
        }else
        {
            sharedPoint1 = p1;
            sharedPoint2 = p2;
        }

        int index1 = FindVertexIndex(mesh, sharedPoint1);
        int index2 = FindVertexIndex(mesh, sharedPoint2);
        
        int nextTriangle = FindTriangleIndex(sharedPoint1, sharedPoint2, mesh, HitIndex);
        DeleteSquare(tm, HitIndex, nextTriangle);
        
    }
    int FindVertexIndex(Mesh mesh, Vector3 vertice)
    {
        Vector3[] vertices = mesh.vertices;
        for(int i = 0; i < vertices.Length;++i)
        {
            if(vertices[i]== vertice)
            {
                return i;
            }
        }
        return -1;
    }

    int FindTriangleIndex(Vector3 vertice1, Vector3 vertice2, Mesh mesh, int notTriIndex)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        int i = 0;
        int j = 0;

        while(j < triangles.Length)
        {
            if(i == notTriIndex){
                i++;
                j+=3;
                continue;
            }
                
            if(vertices[triangles[j]] == vertice1 && ( vertices[triangles[j+1]]==vertice2 || vertices[triangles[j+2]]==vertice2 ))
            {
                return i;
            }
            else if(vertices[triangles[j]] == vertice2 && ( vertices[triangles[j+1]]==vertice1 || vertices[triangles[j+2]]==vertice1 ))
            {
                return i;
            } 
            else if(vertices[triangles[j+1]] == vertice2 && ( vertices[triangles[j]]==vertice1 || vertices[triangles[j+2]]==vertice1 ))
            {
                return i;
            }
            else if(vertices[triangles[j+1]] == vertice1 && ( vertices[triangles[j]]==vertice2 || vertices[triangles[j+2]]==vertice2 ))
            {
                return i;
            }else if(vertices[triangles[j+2]] == vertice2 && ( vertices[triangles[j]]==vertice1 || vertices[triangles[j+1]]==vertice1 ))
            {
                return i;
            }
            else if(vertices[triangles[j+2]] == vertice1 && ( vertices[triangles[j]]==vertice2 || vertices[triangles[j+1]]==vertice2 ))
            {
                return i;
            }
            i++;
            j+=3;
        }
        return -1;
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, 1000))
            {
                //DeleteTri(hit.transform, hit.triangleIndex);
                if(hit.transform.gameObject.GetComponent<MeshCollider>() != null)
                    StartDeleteQuad(hit.transform, hit.triangleIndex);
                else
                    DeleteSquare(hit.transform, 0, 1);
            }
        }
    }
}