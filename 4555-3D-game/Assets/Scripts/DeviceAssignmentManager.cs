using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;
public class DeviceAssignmentManager : MonoBehaviour 
{ 
    public int targetPlayerCount = 1; 
    public List<InputDevice> assignedDevices = new List<InputDevice>(); 

    void Update() 
    { 
        foreach (var device in InputSystem.devices) 
        { 
            if (assignedDevices.Contains(device)) continue; 
            if (DeviceWasUsedThisFrame(device)) 
            { 
                AssignDevice(device); break; 
            } 
        } 
    } 

    bool DeviceWasUsedThisFrame(InputDevice device) 
    { 
        foreach (var control in device.allControls) 
        { 
            if (control is ButtonControl button && button.wasPressedThisFrame) 
                return true; 
            if (control is KeyControl key && key.wasPressedThisFrame) 
                return true; 
            if (control is DpadControl dpad && (dpad.up.wasPressedThisFrame || dpad.down.wasPressedThisFrame || dpad.left.wasPressedThisFrame || dpad.right.wasPressedThisFrame)) 
                return true; 
            if (control is StickControl stick) 
            { 
                Vector2 v = stick.ReadValue(); 
                if (v.sqrMagnitude > 0.2f) 
                    return true; 
            } 
            if (control is Mouse mouse && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)) 
                return true; 
        } 
        return false; 
    } 
    
    void AssignDevice(InputDevice device) 
    { 
        if (assignedDevices.Count >= targetPlayerCount) 
            return; 
        assignedDevices.Add(device); Debug.Log("Assigned device: " + device.displayName); 
        if (OnDeviceAssigned != null) 
            OnDeviceAssigned.Invoke(); 
    } 
    
    public event System.Action OnDeviceAssigned; 
    public event System.Action OnDeviceUnassigned; 
    public void UnassignDevice(int index) 
    { 
        if (index >= 0 && index < assignedDevices.Count) 
            assignedDevices.RemoveAt(index); 
    } 
}
