/*
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
    public class AccountNetMsg:FlagNetMsg {
        public string version;
        public string display_name;
        public uint account_ID;

        public AccountNetMsg(Flag getFlag) {
            flag = getFlag;
        }

    }

}
