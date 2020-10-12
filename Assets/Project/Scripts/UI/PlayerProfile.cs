using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
