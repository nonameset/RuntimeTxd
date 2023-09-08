using System;
using System.Runtime.InteropServices;
using GTA;
using RuntimeTxd.Memory.Internal;

namespace RuntimeTxd.Memory.GTA
{
    public static unsafe class GameFunction
    {
        #region Fields

        private static bool _anyAssertFailed = false;
        
        #endregion

        #region Delegates
        
        public delegate void RegisterIndividualFileDelegate(int* fileId, string filePath, bool unused, string registerAs, bool errorIfFailed);

        #endregion

        #region Properties
        
        public static RegisterIndividualFileDelegate RegisterIndividualFile { get; private set; }
        
        #endregion

        #region Methods

        public static bool Init()
        {
            IntPtr address = Game.FindPattern("48 89 5C 24 ? 48 89 6C 24 ? 48 89 7C 24 ? 41 54 41 56 41 57 48 83 EC 50 48 8B EA");

            if (AssertAddress(address))
            {
                RegisterIndividualFile = Marshal.GetDelegateForFunctionPointer<RegisterIndividualFileDelegate>(address);
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

        public static int RegisterPackFile(string filePath, string registerAs)
        {
            using UsingTls tls = UsingTls.Scope();
            int fileId;
            RegisterIndividualFile(&fileId, filePath, false, registerAs, false);
            return fileId;
        }

        #endregion
    }
}