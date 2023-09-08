using System;
using GTA;
using GTA.Native;
using GTA.UI;
using RuntimeTxd.Memory.GTA;

namespace RuntimeTxd
{
    public class Main : Script
    {
        #region Constructor

        public Main()
        {
            Tick += OnTick;

            // Initialize our signatures.
            Initializer.Initialize();

            // If fileId equals to -1, it failed to register.
            int fileId = RegisterArchive("scripts\\archives\\archive.ytd", "archive.ytd");
            Notification.Show("Registered fileId: " + fileId);
        }

        #endregion

        #region Methods

        private void OnTick(object sender, EventArgs e)
        {
            // Render the texture from the .ytd file we registered in game.
            Function.Call(Hash.DRAW_SPRITE, "archive", "fukasaku", 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, 255, 255, 255, 255, false);
            
            bool isLoaded = Function.Call<bool>(Hash.HAS_STREAMED_TEXTURE_DICT_LOADED, "archive");
            
            if (!isLoaded)
                Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, "archive", true);
            
            Screen.ShowHelpTextThisFrame("Registered: " + isLoaded);
        }

        /// <summary>
        /// Wrapper function to register a .ytd file and request it.
        /// </summary>
        private int RegisterArchive(string filePath, string registerAs)
        {
            int fileId = GameFunction.RegisterPackFile(filePath, registerAs);
            registerAs = registerAs.Replace(".ytd", string.Empty);
            Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, registerAs, true);
            return fileId;
        }

        #endregion
    }
}