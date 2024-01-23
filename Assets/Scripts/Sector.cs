namespace Portals
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Sector : MonoBehaviour
    {
        public List<Plane> Planes = new List<Plane>();

        public List<Vector3> vertices = new List<Vector3>();

        public List<Vector3> verticestp = new List<Vector3>();

        public List<int> triangles = new List<int>();

        public List<GameObject> OutPortals = new List<GameObject>();

        // Start is called before the first frame update
        void Awake()
        {
            SetMesh();
        }

        public void SetMesh()
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

            mesh.GetTriangles(triangles, 0);

            mesh.GetVertices(vertices);

            for (int i = 0; i < vertices.Count; i++)
            {
                verticestp.Add(transform.TransformPoint(vertices[i]));
            }
        }
    }
}
