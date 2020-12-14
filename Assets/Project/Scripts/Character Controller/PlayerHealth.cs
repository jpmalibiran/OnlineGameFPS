using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public GameObject MatchController;

    public int HealthPointsMax, HealthPoints;
    // Start is called before the first frame update
    void Start()
    {
        HealthPointsMax = 100;
        HealthPoints = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (HealthPoints <= 0)
        {
            Destroy(gameObject);
            //MatchController.SendMessage("EnemyKilled");
        }
    }

    public void GotHit(int Damage)
    {
        HealthPoints -= Damage;
    }
}
