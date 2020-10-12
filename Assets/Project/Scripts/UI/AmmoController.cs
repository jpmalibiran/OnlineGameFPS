using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoController : MonoBehaviour
{
    public GameObject[] BulletIcons;
    //public GameObject SoundController; //Note: The audio source component is now at the Player prefab
    private int tempCount, MaxAmmo, CurrentAmmo, ReloadDelay, ReloadCount;
    private bool Reloading;
    // Start is called before the first frame update
    void Start()
    {
        MaxAmmo = 8;
        CurrentAmmo = 8;
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
                    //Fired(); //Note: This is now executed in FirstPersonController.cs when the player shoots
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
                CurrentAmmo = 8;
                tempCount = 0;
            }
        }
        
    }

    public void Fired()
    {
        BulletIcons[CurrentAmmo-1].SetActive(false);
        CurrentAmmo--;
        //SoundController.SendMessage("PlayShootSound"); //Note: The audio source component is now at the Player prefab
    }

    public void StartReload()
    {
        Reloading = true;
    }

    //Used to check if there are any ammo left. A Player shouldn't be able to fire without ammo.
    public int GetAmmoAmount() {
        return CurrentAmmo;
    }
}
