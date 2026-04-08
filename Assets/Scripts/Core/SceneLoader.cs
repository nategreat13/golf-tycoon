using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GolfGame.Core
{
    public static class SceneLoader
    {
        private static string currentScene;

        public static void LoadScene(string sceneName)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartCoroutine(LoadSceneAsync(sceneName));
            }
        }

        private static IEnumerator LoadSceneAsync(string sceneName)
        {
            // Disable the Boot camera so it doesn't cover loaded scenes
            DisableBootCamera();

            // Unload current scene if one is loaded
            if (!string.IsNullOrEmpty(currentScene))
            {
                var unload = SceneManager.UnloadSceneAsync(currentScene);
                if (unload != null)
                    yield return unload;
            }

            // Load new scene additively
            var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (load != null)
            {
                yield return load;
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            }

            currentScene = sceneName;
        }

        private static void DisableBootCamera()
        {
            var bootCam = GameObject.Find("BootCamera");
            if (bootCam != null)
                bootCam.SetActive(false);
        }
    }
}
