/*
 * Author: Joseph Malibiran
 * Last Updated: October 8, 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSNetworkCode {

    //FlagNetworkMsg is a class that only has a flag enum property. The term 'flag' in this context identifies the type of a network message.
    public class FlagNetworkMsg {
        public flag flag;

        public FlagNetworkMsg() {
            flag = flag.DEFAULT;
        }

        public FlagNetworkMsg(flag getFlag) {
            flag = getFlag;
        }
    }
}
