using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject PausePanel;

    private bool paused = false;


    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (!paused)
                Pause();
            else
                UnPause();
        }
    }

    public void Quit() {
        AppHelper.Quit();
    }

    public void Pause() {
        Time.timeScale = 0f;
        PausePanel.SetActive(true);
        paused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UnPause() {
        Time.timeScale = 1f;
        PausePanel.SetActive(false);
        paused = false;


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
