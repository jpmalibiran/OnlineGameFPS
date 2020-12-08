using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileMgr : MonoBehaviour
{
    public string _Username;
    public int _totalGames, _wins, _losses, _kd, _kills, _deaths, _kdleft, _kdright, _level, _matches;
    public float _mmr, _exp;

    // Start is called before the first frame update
    void Start()
    {
        _Username = "Username Not Set";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetProfile(string UserName, float mmr, int totalGames, int wins,
                           int losses, int kd, int kdLeft, int kdRight, int kills, int deaths,
                           float exp, int level)
    {
        _Username = UserName;
        _mmr = mmr;
        _totalGames = totalGames;
        _wins = wins;
        _losses = losses;
        _kd = kd;
        _kills = kills;
        _deaths = deaths;
        _kdleft = kdLeft;
        _kdright = kdRight;
        _level = level;
        _exp = exp;
    }
}
