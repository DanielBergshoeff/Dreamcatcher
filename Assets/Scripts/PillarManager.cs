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

    private List<LineRenderer> myLineRenderers;

    private void Awake() {
        Instance = this;
        AllPillars = new List<Pillar>();
        BindingPillars = new List<PillarBind>();
        myLineRenderers = new List<LineRenderer>();

        
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
            BindingPillars = new List<PillarBind>();
        }
    }

    public void AddMesh(Vector3 pos1, Vector3 pos2, Vector3 pos3) {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = TriangleMat;
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

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
            0, 2, 1,
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[3]
        {
            -Vector3.up,
            -Vector3.up,
            -Vector3.up,
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
