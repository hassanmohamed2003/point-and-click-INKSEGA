using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameState
{
    public static bool IsEndless = false;
    public static int CurrentLevelID;
    public static bool NoFail = false;
    public static bool LockedCheckpoint = false;
    public static bool DisableRope = false;
    public static bool UnlockAllLevels = false;

    public static bool AnyCheatsActive()
    {
        return NoFail || LockedCheckpoint || DisableRope || UnlockAllLevels;
    }
}
