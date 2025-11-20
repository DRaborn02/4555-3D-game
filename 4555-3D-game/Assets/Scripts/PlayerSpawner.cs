using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    public PlayerDeviceData deviceData;
    public PlayerInputManager inputManager;

    void Start()
    {
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        foreach (var device in deviceData.devices)
        {
            var player = inputManager.JoinPlayer(
                playerIndex: -1,
                controlScheme: FindSchemeFor(device),
                pairWithDevice: device
            );
        }
    }

    string FindSchemeFor(InputDevice device)
    {
        if (device is Keyboard) return "Keyboard";
        if (device is Gamepad) return "Gamepad";
        return "Gamepad"; // fallback
    }
}
