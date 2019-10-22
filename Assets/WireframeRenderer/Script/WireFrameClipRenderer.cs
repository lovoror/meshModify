using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace B83.WireframeMesh
{
    public struct Edge
    {
        public Vector3 P0;
        public Vector3 P1;
        public Vector3 pos { get { return P0; } }
        public Vector3 dir { get { return P1 - P0; } }
        public Edge(Vector3 aP0, Vector3 aP1)
        {
            P0 = P1 = aP0;
            if (aP0.x > aP1.x) P0 = aP1;
            else if (aP0.x < aP1.x) P1 = aP1;
            else if (aP0.y > aP1.y) P0 = aP1;
            else if (aP0.y < aP1.y) P1 = aP1;
            else if (aP0.z > aP1.z) P0 = aP1;
            else if (aP0.z < aP1.z) P1 = aP1;
        }
        public override int GetHashCode()
        {
            return P0.GetHashCode() ^ P1.GetHashCode()<<5;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Edge))
                return false;
            Edge e2 = (Edge)obj;
            return P0 == e2.P0 && P1 == e2.P1;
        }
        public Edge Clip(Plane aPlane, out Vector3 aMidPoint)
        {
            var d = P1-P0;
            Ray ray;
            if (Vector3.Dot(aPlane.normal, d) > 0)
                ray = new Ray(P1, d = -d);
            else
                ray = new Ray(P0, d);
            float dist;
            if (aPlane.Raycast(ray, out dist))
                return new Edge(ray.origin+d, aMidPoint = ray.GetPoint(dist));
            aMidPoint = Vector3.zero;
            return this;
        }
        public bool IsOnEdge(Vector3 aPos)
        {
            aPos -= pos;
            float t = Vector3.Dot(aPos, dir) / dir.sqrMagnitude;
            return t >= 0f && t <= 1f;
        }
    }
    public class Triangle
    {
        public Edge E0;
        public Edge E1;
        public Edge E2;
        public Vector3 P0;
        public Vector3 P1;
        public Vector3 P2;
        public Triangle(Vector3 aP0, Vector3 aP1, Vector3 aP2)
        {
            P0 = aP0;
            P1 = aP1;
            P2 = aP2;
            E0 = new Edge(aP0, aP1);
            E1 = new Edge(aP1, aP2);
            E2 = new Edge(aP2, aP0);
        }
        public void Clip(Plane aPlane, HashSet<Edge> aEdges, bool aOnlyClipLine)
        {
            byte side = 0;
            if (aPlane.GetSide(P0))
                side |= 0x1;
            if (aPlane.GetSide(P1))
                side |= 0x2;
            if (aPlane.GetSide(P2))
                side |= 0x4;
            if (side >= 7)
                return;
            if (side == 0)
            {
                if (!aOnlyClipLine)
                {
                    aEdges.Add(E0);
                    aEdges.Add(E1);
                    aEdges.Add(E2);
                }
                return;
            }
            Vector3 mp0;
            Vector3 mp1;
            if(aOnlyClipLine)
            {
                if (side == 2 || side == 5)
                {
                    E0.Clip(aPlane, out mp0);
                    E1.Clip(aPlane, out mp1);
                }
                else if (side == 3 || side == 4)
                {
                    E1.Clip(aPlane, out mp0);
                    E2.Clip(aPlane, out mp1);
                }
                else //if (side == 1 || side == 6)
                {
                    E0.Clip(aPlane, out mp0);
                    E2.Clip(aPlane, out mp1);
                }
            }
            else if (side ==1)
            {
                aEdges.Add(E0.Clip(aPlane, out mp0));
                aEdges.Add(E1);
                aEdges.Add(E2.Clip(aPlane, out mp1));
            }
            else if (side == 2)
            {
                aEdges.Add(E0.Clip(aPlane, out mp0));
                aEdges.Add(E1.Clip(aPlane, out mp1));
                aEdges.Add(E2);
            }
            else if (side == 4)
            {
                aEdges.Add(E0);
                aEdges.Add(E1.Clip(aPlane, out mp0));
                aEdges.Add(E2.Clip(aPlane, out mp1));
            }
            else if (side == 3)
            {
                aEdges.Add(E1.Clip(aPlane, out mp0));
                aEdges.Add(E2.Clip(aPlane, out mp1));
            }
            else if (side == 5)
            {
                aEdges.Add(E0.Clip(aPlane, out mp0));
                aEdges.Add(E1.Clip(aPlane, out mp1));
            }
            else //if (side == 6)
            {
                aEdges.Add(E0.Clip(aPlane, out mp0));
                aEdges.Add(E2.Clip(aPlane, out mp1));
            }
            aEdges.Add(new Edge(mp0,mp1));
        }
    }

    public class WireframeClipMesh
    {
        List<Triangle> triangles = new List<Triangle>();
        HashSet<Edge> edges = new HashSet<Edge>();
        Mesh mesh = new Mesh();
        List<Vector3> lineVerts = new List<Vector3>();
        int[] lineIndices = new int[0];
        Dictionary<Vector3, int> vertIndex = new Dictionary<Vector3, int>();

        Transform trans;
        public Mesh Mesh { get { return mesh; } }

        public WireframeClipMesh(Transform aSourceObject, Mesh aSourceMesh)
        {
            trans = aSourceObject;
            if (aSourceMesh == null)
                aSourceMesh = trans.GetComponent<MeshFilter>().sharedMesh;
            var verts = aSourceMesh.vertices;
            var tris = aSourceMesh.triangles;
            for (int i = 0; i < tris.Length; i += 3)
                triangles.Add(new Triangle(verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]]));

        }
        public Mesh ClipWorld(Plane aWorldPlane, bool aOnlyClipLine)
        {
            Vector3 n = aWorldPlane.normal;
            Vector3 p = -n * aWorldPlane.distance;
            n = trans.InverseTransformDirection(n);
            p = trans.InverseTransformPoint(p);
            Plane localPlane = new Plane(n, p);

            return ClipLocal(localPlane, aOnlyClipLine);
        }
        public Mesh ClipLocal(Plane aLocalPlane, bool aOnlyClipLine)
        {
            Vector3 n = aLocalPlane.normal;
            Vector3 p = -n * aLocalPlane.distance;
            n = trans.InverseTransformDirection(n);
            p = trans.InverseTransformPoint(p);
            Plane clip = new Plane(n, p);
            edges.Clear();
            for (int i = 0; i < triangles.Count; i++)
            {
                triangles[i].Clip(clip, edges, aOnlyClipLine);
            }

            lineVerts.Clear();
            vertIndex.Clear();
            if (lineIndices.Length != edges.Count*2)
            {
                lineIndices = new int[edges.Count * 2];
            }
            int k = 0;
            foreach (Edge e in edges)
            {
                int i0;
                if (!vertIndex.TryGetValue(e.P0, out i0))
                {
                    i0 = lineVerts.Count;
                    lineVerts.Add(e.P0);
                    vertIndex.Add(e.P0, i0);
                }
                int i1;
                if (!vertIndex.TryGetValue(e.P1, out i1))
                {
                    i1 = lineVerts.Count;
                    lineVerts.Add(e.P1);
                    vertIndex.Add(e.P1, i1);
                }
                lineIndices[k++] = i0;
                lineIndices[k++] = i1;

            }
            mesh.Clear();
            mesh.SetVertices(lineVerts);
            mesh.SetIndices(lineIndices, MeshTopology.Lines, 0, true);
            return mesh;
        }
    }


    public class WireFrameClipRenderer : MonoBehaviour
    {
        public Transform planeTrans;
        public Plane plane;
        WireframeClipMesh cMesh;
        private void Start()
        {
            cMesh = new WireframeClipMesh(transform, null); 
            var go = new GameObject("wireframe");
            go.transform.parent = transform;
            go.transform.localPosition = go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.AddComponent<MeshFilter>().sharedMesh = cMesh.Mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = GetComponent<MeshRenderer>().sharedMaterials;
        }
        public void Update()
        {
            plane = new Plane(planeTrans.forward, planeTrans.position);
            cMesh.ClipWorld(plane, true);
        }
    }
}