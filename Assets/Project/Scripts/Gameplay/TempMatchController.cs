using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempMatchController : MonoBehaviour
{
    public GameObject EnemiesRemaining, FadePanel, MusicPrefab;

    public int Enemies = 4;
    private bool isEnding;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isEnding && Enemies == 0)
        {
            FadeToMain();
            isEnding = true;
        }
    }

    public void EnemyKilled()
    {
        Enemies--;
    }

    public void FadeToMain()
    {
        FadePanel.SetActive(true);

    }

    public void ExitMatch()
    {
        SceneManager.LoadScene(5);
        Cursor.lockState = CursorLockMode.None;
        Instantiate(MusicPrefab);
    }
}
