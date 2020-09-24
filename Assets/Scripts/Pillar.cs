using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillar : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PillarManager.Instance.AllPillars.Add(this);
    }
}
