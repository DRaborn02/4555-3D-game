using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;

public class DeviceAssignmentManager : MonoBehaviour
{
    [Header("Setup")]
    public int targetPlayerCount = 1;

    [Header("Assigned Devices")]
    public List<InputDevice> assignedDevices = new List<InputDevice>();

    public bool keyboardMouseAssigned = false;

    public event System.Action OnDeviceAssigned;
    public event System.Action OnDeviceUnassigned;

    void Update()
    {
        // Handle other devices (gamepads, joysticks, etc.)
        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard || device is Mouse) continue; // Skip KB/M
            if (assignedDevices.Contains(device)) continue;

            if (DeviceWasUsedThisFrame(device))
            {
                AssignDevice(device);
                break;
            }
        }

        // Handle Keyboard+Mouse as one device
        if (!keyboardMouseAssigned && KeyboardMouseWasUsedThisFrame())
        {
            AssignKeyboardMouse();
        }

        
    }

    private void AssignKeyboardMouse()
    {
        if (assignedDevices.Count >= targetPlayerCount)
            return;

        if (Keyboard.current != null)
        {
            assignedDevices.Add(Keyboard.current); // Use Keyboard as representative
            keyboardMouseAssigned = true;
            Debug.Log("Assigned device: KeyboardMouse");
            OnDeviceAssigned?.Invoke();
        }
    }

    private bool KeyboardMouseWasUsedThisFrame()
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (kb != null)
        {
            foreach (var key in kb.allKeys)
            {
                if (key.wasPressedThisFrame)
                    return true;
            }
        }

        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame ||
                mouse.rightButton.wasPressedThisFrame ||
                mouse.middleButton.wasPressedThisFrame ||
                mouse.scroll.ReadValue() != Vector2.zero ||
                mouse.delta.ReadValue() != Vector2.zero)
                return true;
        }

        return false;
    }

    private bool DeviceWasUsedThisFrame(InputDevice device)
    {
        foreach (var control in device.allControls)
        {
            if (control is ButtonControl button && button.wasPressedThisFrame) return true;
            if (control is StickControl stick && stick.ReadValue().sqrMagnitude > 0.2f) return true;
            if (control is DpadControl dpad &&
                (dpad.up.wasPressedThisFrame || dpad.down.wasPressedThisFrame ||
                 dpad.left.wasPressedThisFrame || dpad.right.wasPressedThisFrame))
                return true;
        }
        return false;
    }

    private void AssignDevice(InputDevice device)
    {
        if (assignedDevices.Count >= targetPlayerCount) return;

        assignedDevices.Add(device);
        Debug.Log("Assigned device: " + device.displayName);
        OnDeviceAssigned?.Invoke();
    }

    public void UnassignDevice(int index)
    {
        if (index < 0 || index >= assignedDevices.Count) return;

        InputDevice device = assignedDevices[index];
        if (device is Keyboard) keyboardMouseAssigned = false;

        assignedDevices.RemoveAt(index);
        OnDeviceUnassigned?.Invoke();
    }
}
