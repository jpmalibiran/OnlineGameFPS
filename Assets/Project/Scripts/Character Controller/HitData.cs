/*
 * Author: Joseph Malibiran
 * Last Updated: October 11, 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSCharController {
    public class HitData {
        public bool is_hit;
        public int hit_id;
        public int hit_origin_id;
        public int damage;
        public float distance;

        public HitData() {
            is_hit = false;
            hit_id = 0;
            hit_origin_id = 0;
            damage = 0;
            distance = 0;
        }

        public HitData(bool getIsHit, int getID, int getOrigin, int getDamage, float getDistance) {
            is_hit = false;
            hit_id = 0;
            hit_origin_id = 0;
            damage = 0;
            distance = 0;
        }
    }
}
