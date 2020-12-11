/*
 * Author: Joseph Malibiran
 * Last Updated: December 8, 2020
 */

using System.Collections;
using System.Collections.Generic;
using FPSGameplayCode;

//TODO OBSOLETE DEPRECATED
namespace FPSGameplayCode {
    public static class GameStateManager {
        private static State gameState = State.DEFAULT;

        public static State GetState() {
            return gameState;
        }

        public static void SetState(State insertState) {
            gameState = insertState;
        }
    }
}
