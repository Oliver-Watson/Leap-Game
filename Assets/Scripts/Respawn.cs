using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Respawn : MonoBehaviour
{
    private int activeScene;
    private void OnTriggerEnter(Collider other)
    {
        activeScene = LevelManager.currentLevel;

        StartCoroutine(ReloadLevel());
    }

    IEnumerator ReloadLevel()
    {
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(activeScene);
    }
}
