using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class DeviceAssignmentUI : MonoBehaviour
{
    [Header("References")]
    public DeviceAssignmentManager deviceManager;
    public PlayerDeviceData playerDeviceData;

    [Header("Panels")]
    public GameObject modeSelectPanel;
    public GameObject deviceAssignPanel;

    [Header("Player 1 UI")]
    public GameObject player1Panel;
    public TextMeshProUGUI player1DeviceText;
    public Button player1DisconnectButton;
    public GameObject player1ReadyIndicator;

    [Header("Player 2 UI")]
    public GameObject player2Panel;
    public TextMeshProUGUI player2DeviceText;
    public GameObject player2ReadyIndicator;

    [Header("Buttons")]
    public Button startGameButton;
    public Button backButton;

    [Header("Scene to Load")]
    public string sceneName;

    void Start()
    {
        // Button behaviors
        startGameButton.onClick.AddListener(StartGame);
        backButton.onClick.AddListener(BackToModeSelect);

        player1DisconnectButton.onClick.AddListener(() =>
        {
            deviceManager.UnassignDevice(0);
            RefreshUI();
        });

        // When DeviceAssignmentManager detects a new device
        deviceManager.OnDeviceAssigned += RefreshUI;

        RefreshUI();
    }

    void Update()
    {
        // Keep UI reactive in real time
        RefreshUI();
    }

    public void SelectPlayers(int count)
    {
        deviceManager.targetPlayerCount = count;

        modeSelectPanel.SetActive(false);
        deviceAssignPanel.SetActive(true);

        RefreshUI();
    }

    public void BackToModeSelect()
    {
        modeSelectPanel.SetActive(true);
        deviceAssignPanel.SetActive(false);

        deviceManager.assignedDevices.Clear();
        deviceManager.keyboardMouseAssigned = false; // reset the flag
        RefreshUI();
    }


    public void RefreshUI()
    {
        // Enable correct player panels
        player1Panel.SetActive(deviceManager.targetPlayerCount >= 1);
        player2Panel.SetActive(deviceManager.targetPlayerCount >= 2);

        // --- PLAYER 1 ---
        if (deviceManager.assignedDevices.Count >= 1)
        {
            var dev = deviceManager.assignedDevices[0];

            if (dev != null)
            {
                player1DeviceText.text = dev.displayName;
                player1DisconnectButton.interactable = true;
                player1ReadyIndicator.SetActive(true);
            }
            else
            {
                player1DeviceText.text = "Waiting…";
                player1DisconnectButton.interactable = false;
                player1ReadyIndicator.SetActive(false);
            }
        }
        else
        {
            player1DeviceText.text = "Waiting…";
            player1DisconnectButton.interactable = false;
            player1ReadyIndicator.SetActive(false);
        }

        // --- PLAYER 2 ---
        if (deviceManager.targetPlayerCount >= 2)
        {
            if (deviceManager.assignedDevices.Count >= 2)
            {
                var dev = deviceManager.assignedDevices[1];

                if (dev != null)
                {
                    player2DeviceText.text = dev.displayName;
                    player2ReadyIndicator.SetActive(true);
                }
                else
                {
                    player2DeviceText.text = "Waiting…";
                    player2ReadyIndicator.SetActive(false);
                }
            }
            else
            {
                player2DeviceText.text = "Waiting…";
                player2ReadyIndicator.SetActive(false);
            }
        }

        // --- START BUTTON ---
        // Only enabled when ALL players have non-null devices
        bool allAssigned = true;

        if (deviceManager.assignedDevices.Count == deviceManager.targetPlayerCount)
        {
            foreach (var dev in deviceManager.assignedDevices)
            {
                if (dev == null)
                {
                    allAssigned = false;
                    break;
                }
            }
        }
        else allAssigned = false;

        startGameButton.interactable = allAssigned;
    }


    public void StartGame()
    {
        playerDeviceData.devices.Clear();
        playerDeviceData.devices.AddRange(deviceManager.assignedDevices);

        Debug.Log("Devices locked in. Starting game…");

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void DisconnectPlayer(int index)
    {
        deviceManager.UnassignDevice(index);
        RefreshUI();
    }

}
