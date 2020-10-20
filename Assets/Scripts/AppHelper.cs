using UnityEditor;

public static class AppHelper
{
#if UNITY_WEBPLAYER
     public static string webplayerQuitURL = "https://fairfire.itch.io/threads-of-the-past";
#endif
    public static void Quit() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
         Application.OpenURL(webplayerQuitURL);
#else
         Application.Quit();
#endif
    }
}