/*
 * Author: Joseph Malibiran
 * Last Updated: October 8, 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSNetworkCode {

    //The enum 'flag' in this context identifies the type of a network message.
    public enum Flag {
        DEFAULT,
        CONNECT,
        DISCONNECT,
        PING,
        PONG,
        MESSAGE,
        PLAYERDATA,
        VERSION,
        INVALID_VERSION,
        QUEUE_MATCHMAKING
    };
}