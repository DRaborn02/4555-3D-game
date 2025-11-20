using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[CreateAssetMenu]
public class PlayerDeviceData : ScriptableObject
{
    public List<InputDevice> devices = new();
}
