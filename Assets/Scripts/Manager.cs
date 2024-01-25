namespace Portals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class Manager : MonoBehaviour
    {
        public Camera Cam;

        public Collider Player;

        public Vector4[] PlanePos;

        public MaterialPropertyBlock BlockOne;

        public GameObject CurrentSector;

        public List<Plane> Planes = new List<Plane>(6);

        public List<GameObject> Sectors = new List<GameObject>();

        public List<GameObject> VisitedSector = new List<GameObject>();

        public List<Vector3> cornerout = new List<Vector3>();

        public List<float> m_Dists = new List<float>();

        public List<GameObject> AllSector = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            SetPlanes();

            PlanePos = new Vector4[20];

            BlockOne = new MaterialPropertyBlock();
        }

        // Update is called once per frame
        void Update()
        {
            Player.GetComponent<Move>().Controller();

            Sectors.Clear();
  
            CheckSector(CurrentSector);

            Planes.Clear();

            Cam.ReadFrustumPlanes(Planes);

            Planes.RemoveAt(5);

            Planes.RemoveAt(4);

            VisitedSector.Clear();

            GetSector(Planes, CurrentSector);
        }

        public void SetPlanes()
        {
            for (int i = 0; i < AllSector.Count; i++)
            {
                for (int e = 0; e < AllSector[i].GetComponent<Sector>().triangles.Count; e += 3)
                {
                    Vector3 p1 = AllSector[i].GetComponent<Sector>().verticestp[AllSector[i].GetComponent<Sector>().triangles[e + 0]];
                    Vector3 p2 = AllSector[i].GetComponent<Sector>().verticestp[AllSector[i].GetComponent<Sector>().triangles[e + 1]];
                    Vector3 p3 = AllSector[i].GetComponent<Sector>().verticestp[AllSector[i].GetComponent<Sector>().triangles[e + 2]];

                    AllSector[i].GetComponent<Sector>().Planes.Add(new Plane(p1, p2, p3));
                }
                for (int e = 0; e < AllSector[i].GetComponent<Sector>().OutPortals.Count; e++)
                {
                    for (int r = 0; r < AllSector[i].GetComponent<Sector>().OutPortals[e].GetComponent<Portal>().triangles.Count; r += 3)
                    {
                        Vector3 p1 = AllSector[i].GetComponent<Sector>().OutPortals[e].GetComponent<Portal>().cornertp[AllSector[i].GetComponent<Sector>().OutPortals[e].GetComponent<Portal>().triangles[r + 0]];
                        Vector3 p2 = AllSector[i].GetComponent<Sector>().OutPortals[e].GetComponent<Portal>().cornertp[AllSector[i].GetComponent<Sector>().OutPortals[e].GetComponent<Portal>().triangles[r + 1]];
                        Vector3 p3 = AllSector[i].GetComponent<Sector>().OutPortals[e].GetComponent<Portal>().cornertp[AllSector[i].GetComponent<Sector>().OutPortals[e].GetComponent<Portal>().triangles[r + 2]];

                        AllSector[i].GetComponent<Sector>().Planes.Add(new Plane(p1, p2, p3));
                    }
                }
            }
        }

        public bool CheckRadius(GameObject PSector)
        {
            Vector3 CamPoint = Cam.transform.position;

            bool PointIn = true;

            for (int e = 0; e < PSector.GetComponent<Sector>().Planes.Count; e++)
            {
                if (PSector.GetComponent<Sector>().Planes[e].GetDistanceToPoint(CamPoint) < -0.6f)
                {
                    PointIn = false;
                    break;
                }
            }
            return PointIn;
        }

        public void CreateClippingPlanes(List<Vector3> aVertices, List<Plane> aList, Vector3 aViewPos)
        {
            int count = aVertices.Count;
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var p1 = aVertices[i];
                var p2 = aVertices[j];
                var n = Vector3.Cross(p1 - p2, aViewPos - p2);
                var l = n.magnitude;
                if (l < 0.01f)
                    continue;
                aList.Add(new Plane(n / l, aViewPos));
            }
        }

        public List<Vector3> ClippingPlane(List<Vector3> invertices, Plane aPlane, float aEpsilon = 0.001f)
        {
                m_Dists.Clear();
                List<Vector3> outvertices = new List<Vector3>();
                int count = invertices.Count;
                if (m_Dists.Capacity < count)
                    m_Dists.Capacity = count;
                for (int i = 0; i < count; i++)
                {
                    Vector3 p = invertices[i];
                    m_Dists.Add(aPlane.GetDistanceToPoint(p));
                }
                for (int i = 0; i < count; i++)
                {
                    int j = (i + 1) % count;
                    float d1 = m_Dists[i];
                    float d2 = m_Dists[j];
                    Vector3 p1 = invertices[i];
                    Vector3 p2 = invertices[j];
                    bool split = d1 > aEpsilon;
                    if (split)
                    {
                        outvertices.Add(p1);
                    }
                    else if (d1 > -aEpsilon)
                    {
                        // point on clipping plane so just keep it
                        outvertices.Add(p1);
                        continue;
                    }
                    // both points are on the same side of the plane
                    if ((d2 > -aEpsilon && split) || (d2 < aEpsilon && !split))
                    {
                        continue;
                    }
                    float d = d1 / (d1 - d2);
                    outvertices.Add(p1 + (p2 - p1) * d);
                }
            return outvertices;
        }

        public List<Vector3> ClippingPlanes(List<Vector3> invertices, List<Plane> aPlanes)
        {
            for (int i = 0; i < aPlanes.Count; i++)
            {
                invertices = ClippingPlane(invertices, aPlanes[i]);
            }
            return invertices;
        }

        public void GetSectors(GameObject ASector)
        {
            Sectors.Add(ASector);

            for (int i = 0; i < ASector.GetComponent<Sector>().OutPortals.Count; ++i)
            {
                GameObject p = ASector.GetComponent<Sector>().OutPortals[i];

                bool t = CheckRadius(p.GetComponent<Portal>().TargetSector);

                if (Sectors.Contains(p.GetComponent<Portal>().TargetSector))
                {
                    continue;
                }

                if (t == true)
                {
                    GetSectors(p.GetComponent<Portal>().TargetSector);
                }
            }
        }

        public void CheckSector(GameObject Current)
        {
            GetSectors(Current);

            Vector3 CamPoint = Cam.transform.position;

            for (int i = 0; i < Sectors.Count; i++)
            {
                bool PointIn = true;

                for (int e = 0; e < Sectors[i].GetComponent<Sector>().Planes.Count; e++)
                {
                    if (Sectors[i].GetComponent<Sector>().Planes[e].GetDistanceToPoint(CamPoint) < 0)
                    {
                        PointIn = false;
                        break;
                    }
                }

                if (PointIn == true)
                {
                    CurrentSector = Sectors[i];
                }
            }

            IEnumerable<GameObject> except = AllSector.Except(Sectors);

            foreach (GameObject sector in except)
            {
                Physics.IgnoreCollision(Player, sector.GetComponent<MeshCollider>(), true);
            }

            foreach (GameObject sector in Sectors)
            {
                Physics.IgnoreCollision(Player, sector.GetComponent<MeshCollider>(), false);
            }
        }

        public void GetSector(List<Plane> APlanes, GameObject BSector)
        {
            Vector3 CamPoint = Cam.transform.position;

            BlockOne.SetInt("_Int", APlanes.Count);

            Array.Clear(PlanePos, 0, APlanes.Count);

            for (int i = 0; i < APlanes.Count; i++)
            {
                PlanePos[i] = new Vector4(APlanes[i].normal.x, APlanes[i].normal.y, APlanes[i].normal.z, APlanes[i].distance);
            }
    
            BlockOne.SetVectorArray("_Plane", PlanePos);

            Matrix4x4 matrix = Matrix4x4.TRS(BSector.transform.position, BSector.transform.rotation, BSector.transform.lossyScale);

            Graphics.DrawMesh(BSector.GetComponent<MeshFilter>().mesh, matrix, BSector.GetComponent<Renderer>().sharedMaterial, 0, Camera.main, 0, BlockOne, false, false);

            VisitedSector.Add(BSector);

            for (int i = 0; i < BSector.GetComponent<Sector>().OutPortals.Count; ++i)
            {
                GameObject p = BSector.GetComponent<Sector>().OutPortals[i];

                float d = p.GetComponent<Portal>().portalPlane.GetDistanceToPoint(CamPoint);

                List<Plane> PortalPlanes = new List<Plane>();

                if (d < -0.1f)
                {
                    continue;
                }

                if (VisitedSector.Contains(p.GetComponent<Portal>().TargetSector) && d <= 0)
                {
                    continue;
                }

                if (Sectors.Contains(p.GetComponent<Portal>().TargetSector))
                {
                    for (int n = 0; n < APlanes.Count; n++)
                    {
                        PortalPlanes.Add(APlanes[n]);
                    }
                        
                    GetSector(PortalPlanes, p.GetComponent<Portal>().TargetSector);

                    continue;
                }

                if (d != 0)
                {
                    cornerout = ClippingPlanes(p.GetComponent<Portal>().cornertp, APlanes);

                    if (cornerout.Count > 2)
                    {
                        CreateClippingPlanes(cornerout, PortalPlanes, CamPoint);

                        GetSector(PortalPlanes, p.GetComponent<Portal>().TargetSector);
                    }
                }
            }
        }
    }
}
