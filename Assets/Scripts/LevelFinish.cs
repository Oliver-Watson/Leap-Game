using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinish : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(ToNextLevel());
        LevelManager.currentLevel++;
        Debug.Log("Next level trigger");
        Debug.Log("Current Level: " + LevelManager.currentLevel);
    }

    IEnumerator ToNextLevel()
    {
        
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(1);

    }
}
