using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static int currentLevel;
    void Start()
    {
        currentLevel = 0;
    }

    private void Update()
    {
        Debug.Log("Current Level: " + currentLevel);
    }
}
