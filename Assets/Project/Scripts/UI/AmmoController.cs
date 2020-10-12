using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoController : MonoBehaviour
{
    public GameObject[] BulletIcons;
    //public GameObject SoundController;
    private int tempCount, MaxAmmo, CurrentAmmo, ReloadDelay, ReloadCount;
    private bool Reloading;
    // Start is called before the first frame update
    void Start()
    {
        MaxAmmo = 7;
        CurrentAmmo = 7;
        ReloadDelay = 1600;
    }

    // Update is called once per frame
    void Update()
    {
        
        
        
        //temp test
        
        if(!Reloading)
        {
            tempCount++;
            if(tempCount > 180)
            {
                if(CurrentAmmo == -1)
                {
                    StartReload();
                    return;
                }
                    Fired();
                    tempCount = 0;
            }

        }
        
        if(Reloading)
        {
            ReloadCount++;
            if(ReloadCount > ReloadDelay)
            {
                for(int i = 0; i < BulletIcons.Length; i++)
                {
                    BulletIcons[i].SetActive(true);
                }
                Reloading = false;
                ReloadCount = 0;
                CurrentAmmo = 7;
                tempCount = 0;
            }
        }
        
    }

    public void Fired()
    {
        BulletIcons[CurrentAmmo].SetActive(false);
        CurrentAmmo--;
        //SoundController.SendMessage("PlayShootSound");
    }

    public void StartReload()
    {
        Reloading = true;
    }
}
