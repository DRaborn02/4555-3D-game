using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance;

    [Header("References")]
    public GameObject p1UI;
    public GameObject p2UI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Ensure both start off hidden
        p1UI.SetActive(false);
        p2UI.SetActive(false);
    }

    public void AssignUI(Inventory inventory, PlayerHealth playerHealth, int playerIndex)
    {
        GameObject uiRoot = null;

        if (playerIndex == 0) // P1
        {
            p1UI.SetActive(true);
            uiRoot = p1UI;
        }
        else if (playerIndex == 1) // P2
        {
            p2UI.SetActive(true);
            uiRoot = p2UI;
        }

        if (uiRoot == null) return;

        // Hook up inventory UI
        inventory.BindUI(uiRoot);

        // Hook up health UI
        var healthUI = uiRoot.transform.Find("HeartsPanel").GetComponentsInChildren<Image>();
        playerHealth.BindUI(healthUI);
    }

}
