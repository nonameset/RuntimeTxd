using System;
using GTA;

namespace RuntimeTxd.Memory.GTA;

internal static unsafe class GameOffsets
{
    #region Fields

    private static bool _anyAssertFailed = false;

    #endregion

    #region Properties

    public static int TLS_AllocatorOffset { get; private set; } = -1;

    #endregion

    #region Methods

    public static bool Init()
    {
        IntPtr address = Game.FindPattern("B9 ?? ?? ?? ?? 48 8B 0C 01 45 33 C9 49 8B D2");
        if (AssertAddress(address))
        {
            TLS_AllocatorOffset = *(int*)(address + 1);
        }

        return !_anyAssertFailed;
    }

    private static bool AssertAddress(IntPtr address)
    {
        if (address == IntPtr.Zero)
        {
            _anyAssertFailed = true;
            return false;
        }

        return true;
    }

    #endregion
}