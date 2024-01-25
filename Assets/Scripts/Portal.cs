namespace Portals
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Portal : MonoBehaviour
    {
        public Plane portalPlane;

        public GameObject TargetSector;

        public List<Vector3> corners = new List<Vector3>();

        public List<Vector3> cornertp = new List<Vector3>();

        public List<int> triangles = new List<int>();

        // Start is called before the first frame update
        void Awake()
        {
            SetMesh();

            portalPlane = new Plane(cornertp[0], cornertp[1], cornertp[2]);
        }

        public void SetMesh()
        {
            Mesh portalMesh = GetComponent<MeshFilter>().sharedMesh;

            portalMesh.GetTriangles(triangles, 0);

            portalMesh.GetVertices(corners);

            for (int i = 0; i < corners.Count; i++)
            {
                cornertp.Add(transform.TransformPoint(corners[i]));
            }
        }
    }
}
