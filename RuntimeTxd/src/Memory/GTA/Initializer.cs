namespace RuntimeTxd.Memory.GTA
{
    public static class Initializer
    {
        #region Fields

        private static bool _success;
        
        #endregion

        #region Properties

        public static bool Initialized { get; private set; } = false;

        public static bool Successful
        {
            get
            {
                if (!Initialized)
                    Initialize();
                return _success;
            }
        }
        
        #endregion

        #region Methods

        public static void Initialize()
        {
            if (Initialized) return;

            Initialized = true;

            _success = GameFunction.Init() && GameOffsets.Init();
        }

        #endregion
    }
}