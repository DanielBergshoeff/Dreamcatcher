using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarManager : MonoBehaviour
{
    public static PillarManager Instance;

    public List<Pillar> AllPillars;

    public List<PillarBind> BindingPillars;

    public GameObject LineRendererPrefab;
    public Material TriangleMat;
    public GameObject PortalPrefab;

    public GameObject PortalCam;
    public GameObject MyPortal;
    public GameObject OtherPortal;
    public GameObject OtherWorld;

    public bool InstantPortal = false;

    private List<LineRenderer> myLineRenderers;
    private List<LineRenderer> portalRenderers;
    private bool portalCreated = false;

    private void Awake() {
        Instance = this;
        AllPillars = new List<Pillar>();
        BindingPillars = new List<PillarBind>();
        myLineRenderers = new List<LineRenderer>();
        portalRenderers = new List<LineRenderer>();
        /*
        PortalCam.SetActive(portalCreated);
        OtherWorld.SetActive(portalCreated);*/
    }

    private void Update() {
        if (InstantPortal && !portalCreated)
            CreatePortal();

        if (portalCreated) {
            Vector3 playerOffsetFromPortal = Camera.main.transform.position - MyPortal.transform.position;
            PortalCam.transform.position = OtherPortal.transform.position + playerOffsetFromPortal;

            float angularDif = Quaternion.Angle(MyPortal.transform.rotation, OtherPortal.transform.rotation);
            Quaternion portalRotationalDif = Quaternion.AngleAxis(angularDif, Vector3.up);

            Vector3 newCameraDir = portalRotationalDif * Camera.main.transform.forward;
            PortalCam.transform.rotation = Quaternion.LookRotation(newCameraDir, Vector3.up);
        }
    }

    public void SwapPortals() {
        GameObject temp = MyPortal;
        MyPortal = OtherPortal;
        OtherPortal = temp;
    }

    public void CreatePortal() {
        GameObject go = Instantiate(PortalPrefab);

        Vector3 avgPosition = Vector3.zero;
        foreach(Pillar p in AllPillars) {
            avgPosition += p.transform.position;
            LineRenderer lr = Instantiate(LineRendererPrefab).GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, p.transform.position + Vector3.up * 10f);
            portalRenderers.Add(lr);
        }

        avgPosition = avgPosition / AllPillars.Count;
        avgPosition += Vector3.up * 10f;
        go.transform.position = avgPosition;

        MyPortal = go;

        foreach(LineRenderer lr in portalRenderers) {
            Vector3 direction = avgPosition - lr.GetPosition(0);
            Vector3 heading = direction.normalized;
            Vector3 newPos = lr.GetPosition(0) + heading * (direction.magnitude - 5f);
            lr.SetPosition(1, newPos);
        }

        PortalCam.SetActive(true);
        OtherWorld.SetActive(true);
        portalCreated = true;
    }

    public void AddPillarToBind(Pillar pillar, Vector3 bindPos) {
        for (int i = 0; i < BindingPillars.Count; i++) {
            if (BindingPillars[i].MyPillar == pillar && (BindingPillars.Count <= 2 || i != 0)) {
                return;
            }
            else if (BindingPillars[i].MyPillar == pillar && BindingPillars.Count == 3)
                break;
            else if (BindingPillars[i].MyPillar != pillar && BindingPillars.Count > 2) {
                return;
            }
        }

        BindingPillars.Add(new PillarBind(pillar, bindPos));
        if (BindingPillars.Count <= 1) {
            myLineRenderers.Add(Instantiate(LineRendererPrefab).GetComponent<LineRenderer>());
            return;
        }

        List<Vector3> positions = new List<Vector3>();
        foreach (PillarBind pb in BindingPillars) {
            positions.Add(pb.BindPosition);
        }

        if(BindingPillars.Count == 4) {
            positions[3] = positions[0];
        }

        myLineRenderers[myLineRenderers.Count - 1].positionCount = positions.Count;
        myLineRenderers[myLineRenderers.Count - 1].SetPositions(positions.ToArray());

        if(BindingPillars.Count == 4) {
            AddMesh(BindingPillars[0].BindPosition, BindingPillars[1].BindPosition, BindingPillars[2].BindPosition);
            CreatureManager.Instance.CheckForButterfly(BindingPillars[0].BindPosition, BindingPillars[1].BindPosition, BindingPillars[2].BindPosition);
            BindingPillars = new List<PillarBind>();
        }
    }

    public void AddMesh(Vector3 pos1, Vector3 pos2, Vector3 pos3) {
        GameObject row = new GameObject();
        Instantiate(row);

        MeshRenderer meshRenderer = row.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = TriangleMat;
        MeshFilter meshFilter = row.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[3]
        {
            pos1,
            pos2,
            pos3
        };
        mesh.vertices = vertices;

        int[] tris = new int[3]
        {
            // lower left triangle
            0, 2, 1
        };
        mesh.triangles = tris;

        var dir = Vector3.Cross(pos2 - pos1, pos3 - pos1);
        var norm = Vector3.Normalize(dir);

        Vector3[] normals = new Vector3[3]
        {
            -norm,
            -norm,
            -norm,
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[3]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
        };
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }
}

public class PillarBind
{
    public Pillar MyPillar;
    public Vector3 BindPosition;

    public PillarBind(Pillar pillar, Vector3 bindPosition) {
        MyPillar = pillar;
        BindPosition = bindPosition;
    }
}
