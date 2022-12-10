using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveExplorer
{
    public static class StaticData
    {
        #region PHOTON EVENT CODES
        public static byte AssignPuzzlePieceSymbolsEventCode = 1;
        public static byte CheckIfPuzzleSolvedEventCode = 2;
        public static byte PuzzleSolvedEventCode = 3;
        #endregion
    }
}
