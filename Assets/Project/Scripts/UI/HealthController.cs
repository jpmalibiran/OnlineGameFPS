using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    private RectTransform mRT;
    public float weaponDamage;
    // Start is called before the first frame update
    void Start()
    {
        if (weaponDamage == 0) weaponDamage = 65;
        mRT = this.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        //temp test
       // mRT.sizeDelta = new Vector2(mRT.rect.width - 0.01f, mRT.rect.height);
    }

    public void HitMe() //Can add a parameter later if need be. 
    {
        //for now all damage can be the same
        mRT.sizeDelta = new Vector2(mRT.rect.width - 80, mRT.rect.height);
    }

    public void Restore() {
        mRT.sizeDelta = new Vector2(400, mRT.rect.height);
    }
}
