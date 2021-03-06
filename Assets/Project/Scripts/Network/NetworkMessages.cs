﻿/*
 * Author: Joseph Malibiran
 * Last Updated: October 21, 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace FPSNetworkCode {

    //FlagNetworkMsg is a class that only has a flag enum property. The term 'flag' in this context identifies the type of a network message.
    [Serializable]
    public class FlagNetMsg {
        public Flag flag;

        public FlagNetMsg() {
            flag = Flag.DEFAULT;
        }

        public FlagNetMsg(Flag getFlag) {
            flag = getFlag;
        }
    }

    [Serializable]
    public class StringNetMsg:FlagNetMsg {
        public string message;

        public StringNetMsg(Flag getFlag) {
            flag = getFlag;
        }
    }

    [Serializable]
    public class ConnectNetMsg:FlagNetMsg {
        public string version;
        public string username;

        public ConnectNetMsg(string getVersion, string getUsername) {
            flag = Flag.CONNECT;
            version = getVersion;
            username = getUsername;
        }

    }

    [Serializable]
    public class AccountNetMsg:FlagNetMsg {
        public string version;
        public string display_name;
        public uint account_ID;

        public AccountNetMsg(Flag getFlag) {
            flag = getFlag;
        }

    }

    [Serializable]
    public class LoginMsg {
        public string version;
        public string username;
        public string password;
        public Flag flag;

        public LoginMsg(Flag getFlag){
            flag = getFlag;
        }

        public LoginMsg(Flag getFlag, string getVersion, string getUsername, string getPassword){
            flag = getFlag;
            version = getVersion;
            username = getUsername;
            password = getPassword;
        }
    }

    [Serializable] 
    public class ProfileDataRequest {
        public Flag flag;
        public string username;

        public ProfileDataRequest(string targetUsername){
            flag = Flag.FETCH_ACCOUNT;
            username = targetUsername;
        }
    }

    [Serializable] 
    public class ProfileData {
        public Flag flag;
        public string username;
        public int mmr;
        public int totalGames;
        public int gamesWon;
        public int gamesLost;
        public int kills;
        public int deaths;
        public int progress;
        //Note: use the kills and deaths value to determine k:d ratio

        public ProfileData() {
            flag = Flag.FETCH_ACCOUNT;
            username = "default";
            mmr = 1500;
            progress = 0;
        }

        public ProfileData(string insertUsername, int insertMMR, int insertProgress) {
            flag = Flag.FETCH_ACCOUNT;
            username = insertUsername;
            mmr = insertMMR;
            progress = insertProgress;
        }
    }

    //From Server to client only
    [Serializable] 
    public class DroppedPlayer {
        public Flag flag;
        public string username;
    }

    //Not a network message
    [Serializable]
    public class PlayerData {
        public Transform objReference;
        public Transform objChaserReference;
        public string username;
        public Vector3 position;
        public float yaw;
        public float pitch;
        public float latency;
        public int health;
        public bool isFiring;
        //public int bullets;
        //TODO enum gun
    }

    //From client
    [Serializable]
    public class PlayerMoveData {
        public Flag flag;
        public string username;
        public Coordinates position;
        public Orientation orientation;
        public int health;

        public PlayerMoveData(string getUsername, float getPosX, float getPosY, float getPosZ, float getYaw, float getPitch, int getHealth) {
            flag = Flag.MATCH_UPDATE;
            username = getUsername;
            position = new Coordinates(getPosX, getPosY, getPosZ);
            orientation = new Orientation(getYaw, getPitch);
            health = getHealth;
        }
    }

    //From server
    [Serializable]
    public class MoveUpdateData {
        public Flag flag;
        public PlayerMoveData[] players;

        public MoveUpdateData() {
            flag = Flag.MATCH_UPDATE;
        }
    }

    
    [Serializable]
    public class HitScanData {
        public Flag flag;
        public string usernameOrigin;
        public string usernameTarget;
        public Coordinates hitPosition;
        public int damage;
        public bool isHit; //might not be needed since we have class MissShotsData

        public HitScanData(string getOrigin, string getTarget, Coordinates getHitPos, int getDamage, bool getIsHit) {
            flag = Flag.HITSCAN_UPDATE;
            usernameOrigin = getOrigin;
            usernameTarget = getTarget;
            hitPosition = getHitPos;
            damage = getDamage;
            isHit = getIsHit;
        }
    }

    [Serializable]
    public class MissShotsData {
        public Flag flag;
        public string usernameOrigin;
        public Coordinates hitPosition;

        public MissShotsData(string getOrigin, Coordinates getHitPos){
            flag = Flag.MISSSHOT_UPDATE;
            usernameOrigin = getOrigin;
            hitPosition = getHitPos;
        }
    }

    [Serializable]
    public class DirectHitData {
        public Flag flag;
        public string usernameOrigin;
        public string usernameTarget;
    }

    [Serializable]
    public class Coordinates {
        public float x;
        public float y;
        public float z;

        public Coordinates(float getX, float getY, float getZ) {
            x = getX;
            y = getY;
            z = getZ;
        }
    }

    [Serializable]
    public class Orientation {
        public float yaw;
        public float pitch;

        public Orientation(float getYaw, float getPitch) {
            yaw = getYaw;
            pitch = getPitch;
        }
    }

    //From client
    [Serializable]
    public class DeathMessage{
        public Flag flag;
        public string usernameKiller;
        public string usernameVictim;
    }

    //From server
    [Serializable]
    public class RespawnMessage{
        public Flag flag;
        public Coordinates spawn;
    }
}
