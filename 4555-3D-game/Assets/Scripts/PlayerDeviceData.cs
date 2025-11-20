using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerDeviceData", menuName = "Game/PlayerDeviceData")]
public class PlayerDeviceData : ScriptableObject
{
    [Tooltip("Store assigned devices per player. Keyboard+Mouse is represented by the Keyboard device.")]
    public List<InputDevice> devices = new List<InputDevice>();
}
