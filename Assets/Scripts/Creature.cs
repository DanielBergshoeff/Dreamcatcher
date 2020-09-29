using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public GameObject Shadow;
    public GameObject Butterfly;

    private bool switchNow = false;
    private bool isButterfly = false;

    // Start is called before the first frame update
    void Start()
    {
        CreatureManager.Instance.AllCreatures.Add(this);
        Butterfly.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isButterfly)
            return;

        if (switchNow){
            if(Shadow.transform.localScale.x - Time.deltaTime < 0f) {
                Shadow.SetActive(false);
                isButterfly = true;
            }
            else {
                Shadow.transform.localScale = Shadow.transform.localScale - Vector3.one * Time.deltaTime;
            }
        }
    }

    public void SwitchToButterfly() {
        if (isButterfly)
            return;

        Butterfly.SetActive(true);
        switchNow = true;
    }
}
