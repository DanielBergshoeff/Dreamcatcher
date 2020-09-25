using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance;

    public List<Creature> AllCreatures;

    private void Awake() {
        Instance = this;
        AllCreatures = new List<Creature>();
    }

    public void CheckForButterfly(Vector3 p1, Vector3 p2, Vector3 p3) {
        foreach(Creature c in AllCreatures) {
            if(PointInTriangle(c.transform.position, p1, p2, p3)) {
                c.SwitchToButterfly();
            }
        }
    }

    float sign(Vector3 p1, Vector3 p2, Vector3 p3) {
        return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
    }

    bool PointInTriangle(Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3) {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = sign(pt, v1, v2);
        d2 = sign(pt, v2, v3);
        d3 = sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }
}
