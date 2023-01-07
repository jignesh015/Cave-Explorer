using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public static class StaticData
    {
        #region PHOTON EVENT CODES
        //START GAME EVENT
        public static byte StartGameEventCode = 1;
        //SYMBOL WHEEL PUZZLE EVENTS
        public static byte AssignPuzzlePieceSymbolsEventCode = 2;
        public static byte CheckIfSWPuzzleSolvedEventCode = 3;
        public static byte SWPuzzleSolvedEventCode = 4;

        //DOOR HOLE PUZZLE EVENTS
        public static byte AssignHolePieceSymbolEventCode = 5;
        public static byte CheckIfDHPuzzleSolvedEventCode = 6;
        public static byte DHPuzzleSolvedEventCode = 7;
        #endregion
    }
}
