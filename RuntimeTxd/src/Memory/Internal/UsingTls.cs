// Credit to alexguirre: https://github.com/alexguirre
// Source from: https://github.com/alexguirre/RAGENativeUI/tree/master/Source/Internals/UsingTls.cs
// License: https://github.com/alexguirre/RAGENativeUI/blob/master/LICENSE.md

using System;
using RuntimeTxd.Memory.GTA;

namespace RuntimeTxd.Memory.Internal;

internal struct UsingTls : IDisposable
{
    public void Dispose()
    {
        thisThreadRefCount--;
        if (thisThreadRefCount == 0)
            WinFunctions.SetTlsValue(thisThreadTls, thisThreadSavedValue, GameOffsets.TLS_AllocatorOffset);
    }

    private static void EnsureTlsPointers()
    {
        if (mainThreadTls == IntPtr.Zero)
            mainThreadTls = WinFunctions.GetTlsPointer(WinFunctions.GetProcessMainThreadId());

        if (thisThreadTls == IntPtr.Zero) thisThreadTls = WinFunctions.GetTlsPointer(WinFunctions.GetCurrentThreadId());
    }

    public static UsingTls Scope()
    {
        if (thisThreadRefCount == 0)
        {
            EnsureTlsPointers();

            thisThreadSavedValue = WinFunctions.GetTlsValue(thisThreadTls, GameOffsets.TLS_AllocatorOffset);
            WinFunctions.CopyTlsValue(mainThreadTls, thisThreadTls, GameOffsets.TLS_AllocatorOffset);
        }

        thisThreadRefCount++;
        return default;
    }

    public static unsafe long Get(int offset)
    {
        EnsureTlsPointers();

        return *(long*)(*(byte**)thisThreadTls + offset);
    }

    public static unsafe void Set(int offset, long value)
    {
        EnsureTlsPointers();

        *(long*)(*(byte**)thisThreadTls + offset) = value;
    }

    public static unsafe long GetFromMain(int offset)
    {
        EnsureTlsPointers();

        return *(long*)(*(byte**)mainThreadTls + offset);
    }

    private static IntPtr mainThreadTls;
    [ThreadStatic] private static int thisThreadRefCount;
    [ThreadStatic] private static IntPtr thisThreadTls;
    [ThreadStatic] private static long thisThreadSavedValue;
}