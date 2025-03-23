using UnityEngine;
using UnityEngine.SceneManagement;

namespace App.Scripts
{
    public class SceneLoader : MonoBehaviour
    {
        private void Awake()
        {
            if (SceneManager.sceneCount == 1) SceneManager.LoadScene(1, LoadSceneMode.Additive);
        }
    }
}