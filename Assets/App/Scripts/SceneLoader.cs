using UnityEngine;
using UnityEngine.SceneManagement;

namespace App.Scripts
{
    public class SceneLoader : MonoBehaviour
    {
        private void Awake()
        {
            SceneManager.LoadScene(1, LoadSceneMode.Additive);
        }
    }
}