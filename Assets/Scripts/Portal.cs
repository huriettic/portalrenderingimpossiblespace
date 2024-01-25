namespace Portals
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Portal : MonoBehaviour
    {
        public Plane PortalPlane;

        public GameObject TargetSector;

        public List<Plane> PortalPlanes = new List<Plane>();

        public List<Vector3> corners = new List<Vector3>();

        public List<Vector3> cornertp = new List<Vector3>();

        public List<int> triangles = new List<int>();

        // Start is called before the first frame update
        void Awake()
        {
            SetMesh();
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
