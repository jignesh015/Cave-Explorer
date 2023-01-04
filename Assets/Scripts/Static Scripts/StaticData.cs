using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public static class StaticData
    {
        #region PHOTON EVENT CODES
        //SYMBOL WHEEL PUZZLE EVENTS
        public static byte AssignPuzzlePieceSymbolsEventCode = 1;
        public static byte CheckIfSWPuzzleSolvedEventCode = 2;
        public static byte SWPuzzleSolvedEventCode = 3;

        //DOOR HOLE PUZZLE EVENTS
        public static byte AssignHolePieceSymbolEventCode = 4;
        public static byte CheckIfDHPuzzleSolvedEventCode = 5;
        public static byte DHPuzzleSolvedEventCode = 6;
        #endregion
    }
}
