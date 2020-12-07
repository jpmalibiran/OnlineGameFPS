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
        QUEUE_MATCHMAKING,
        LEAVE_MATCHMAKING,
        CREATE_ACCOUNT,
        LOGIN_ACCOUNT,
        FETCH_ACCOUNT,
        FAILED_ACCOUNT_CREATION,
        FAILED_LOGIN,
        FAILED_FETCH,
        FAILED_MMQUEUE,
        MATCH_START,
        MATCH_UPDATE
    };
}