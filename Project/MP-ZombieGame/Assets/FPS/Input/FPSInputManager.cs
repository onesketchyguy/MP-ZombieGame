using UnityEngine;

namespace FPS
{
    public static class FPSInputManager
    {
        private static FPSControls inputActions;
        public static bool enabled { get; private set; } = false;

        public static void Init()
        {
            if (inputActions == null) inputActions = new FPSControls();
            else inputActions.Dispose();

            inputActions = new FPSControls();
        }

        public static void Dispose()
        {
            if (inputActions != null)
            {
                inputActions.Disable();
                inputActions.Dispose();
            }
        }

        public static void Enable()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("Tried to enable input without initializing input manager first!");
                Init();
            }

            inputActions.Enable();
            enabled = true;
        }

        public static void Disable()
        {
            if (inputActions != null) inputActions.Disable();
            enabled = false;

            Debug.LogError("Disabled input!");
        }

        public static FPSControls.PlayerActions GetPlayerInput()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("Tried to get input without initializing input manager first!");
                Init();
            }

            return inputActions.Player;
        }
    }
}
