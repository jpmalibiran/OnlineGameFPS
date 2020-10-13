using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//(Oct 13, 2020) Note: This functionality is removed from the Player Prefab and moved to LocalClientInput.cs. Otherwise, the input is triggered for each Player Prefab in the scene.
public class PlayerProfile : MonoBehaviour
{
    public GameObject Profile;

    void Start()
    {
        Profile.SetActive(false);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Profile.SetActive(true);
        }
        if (Input.GetKeyUp(KeyCode.Y))
        {
            Profile.SetActive(false);
        }
    }
}
