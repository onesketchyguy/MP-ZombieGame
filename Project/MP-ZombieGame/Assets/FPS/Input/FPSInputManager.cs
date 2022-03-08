using UnityEngine;

namespace FPS
{
    public static class FPSInputManager
    {
        private static FPSControls inputActions;

        public static void Enable()
        {
            if (inputActions == null) inputActions = new FPSControls();
            inputActions.Enable();
        }

        public static void Disable()
        {
            inputActions.Disable();
        }

        public static FPSControls.PlayerActions GetPlayerInput()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("Tried to get input without enabling input manager first!");
                Enable();
            }

            return inputActions.Player;
        }
    }
}
