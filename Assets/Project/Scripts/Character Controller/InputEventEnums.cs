/*
 * Author: Joseph Malibiran
 * Last Updated: October 11, 2020
 */

using System.Collections;
using System.Collections.Generic;

namespace FPSCharController {
    public enum InputEvent {
        None,
        Forward, Leftward, Backward, Rightward,
        Shoot, AimSights, Reload, SwitchWeaponPrev, SwitchWeaponNext,
        Sprint, Crouch, Prone, Jump,
        Pause, Menu, Settings, Profile, Scoreboard, Latency
    };
}
