// Credit to alexguirre: https://github.com/alexguirre
// Source from: https://github.com/alexguirre/RAGENativeUI/tree/master/Source/Internals/UsingTls.cs
// License: https://github.com/alexguirre/RAGENativeUI/blob/master/LICENSE.md

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RuntimeTxd.Memory.Internal;

public static unsafe class WinFunctions
{
    public delegate int NtQueryInformationThreadDelegate(IntPtr threadHandle, uint threadInformationClass,
        THREAD_BASIC_INFORMATION* outThreadInformation, ulong threadInformationLength, ulong* returnLength);

    [Flags]
    public enum ThreadAccess
    {
        QUERY_INFORMATION = 0x0040
    }

    static WinFunctions()
    {
        var ntdllHandle = GetModuleHandle("ntdll.dll");
        NtQueryInformationThread =
            Marshal.GetDelegateForFunctionPointer<NtQueryInformationThreadDelegate>(GetProcAddress(ntdllHandle, "NtQueryInformationThread"));
    }

    public static NtQueryInformationThreadDelegate NtQueryInformationThread { get; }

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    public static extern IntPtr GetModuleHandle(string moduleName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    public static extern IntPtr GetProcAddress(IntPtr moduleHandle, string procName);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenThread(ThreadAccess desiredAccess, bool inheritHandle, int threadId);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll")]
    public static extern int GetCurrentThreadId();

    public static int GetProcessMainThreadId()
    {
        var lowestStartTime = long.MaxValue;
        ProcessThread lowestStartTimeThread = null;
        foreach (ProcessThread thread in Process.GetCurrentProcess().Threads)
        {
            var startTime = thread.StartTime.Ticks;
            if (startTime < lowestStartTime)
            {
                lowestStartTime = startTime;
                lowestStartTimeThread = thread;
            }
        }

        return lowestStartTimeThread == null ? -1 : lowestStartTimeThread.Id;
    }

    public static IntPtr GetTlsPointer(int threadId)
    {
        var threadHandle = IntPtr.Zero;
        try
        {
            threadHandle = OpenThread(ThreadAccess.QUERY_INFORMATION, false, threadId);

            var threadInfo = new THREAD_BASIC_INFORMATION();

            var status = NtQueryInformationThread(threadHandle, 0, &threadInfo,
                (ulong)sizeof(THREAD_BASIC_INFORMATION), null);
            if (status != 0) return IntPtr.Zero;

            var teb = (TEB*)threadInfo.TebBaseAddress;
            return teb->ThreadLocalStoragePointer;
        }
        finally
        {
            if (threadHandle != IntPtr.Zero)
                CloseHandle(threadHandle);
        }
    }

    public static long GetTlsValue(IntPtr tlsPtr, int valueOffset)
    {
        return *(long*)(*(byte**)tlsPtr + valueOffset);
    }

    public static void SetTlsValue(IntPtr tlsPtr, long value, int valueOffset)
    {
        *(long*)(*(byte**)tlsPtr + valueOffset) = value;
    }

    public static void CopyTlsValue(IntPtr sourceTlsPtr, IntPtr targetTlsPtr, int valueOffset)
    {
        *(long*)(*(byte**)targetTlsPtr + valueOffset) = *(long*)(*(byte**)sourceTlsPtr + valueOffset);
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x30)]
    public struct THREAD_BASIC_INFORMATION
    {
        [FieldOffset(0x0000)] public readonly int ExitStatus;
        [FieldOffset(0x0008)] public readonly IntPtr TebBaseAddress;
    }

    // http://msdn.moonsols.com/win7rtm_x64/TEB.html
    [StructLayout(LayoutKind.Explicit, Size = 0x1818)]
    public struct TEB
    {
        [FieldOffset(0x0058)] public readonly IntPtr ThreadLocalStoragePointer;
    }
}