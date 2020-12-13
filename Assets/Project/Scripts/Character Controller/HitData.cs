/*
 * Author: Joseph Malibiran
 * Last Updated: October 11, 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSCharController {
    public class HitData {
        
        public string hit_name;
        public string hit_origin_name;
        public Vector3 hit_location;
        public float distance;
        public int damage;
        public bool is_hit;

        public HitData() {
            is_hit = false;
            hit_name = "";
            hit_origin_name = "";
            hit_location = Vector3.zero;
            damage = 0;
            distance = 0;
        }

        public HitData(bool getIsHit, string getID, string getOrigin, Vector3 getLocation, int getDamage, float getDistance) {
            is_hit = false;
            hit_name = getID;
            hit_origin_name = getOrigin;
            hit_location = getLocation;
            damage = 0;
            distance = 0;
        }
    }
}
