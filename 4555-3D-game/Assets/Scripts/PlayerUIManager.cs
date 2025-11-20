using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Sprite quarterHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite threeQuartersHeart;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private int heartSize = 30;
    [SerializeField] private int heartSpacing = 45;

    public static PlayerUIManager Instance;

    private Image[] hearts;

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
        int HP = playerHealth.getMaxHealth();
        Transform healthUI = uiRoot.transform.Find("MainPanel").Find("HeartsPanel");

        // Create array to hold heart images of the correct size
        int totalHearts = Mathf.CeilToInt(HP / 4f);
        hearts = new Image[totalHearts];

        int currentHearts = 0;

        if (playerIndex == 1)
        {
            // Offset for second player
            heartSpacing = -heartSpacing;
        }

        for (int i = HP; i > 0;)
        {

            var heartImage = new GameObject("Heart" + currentHearts).AddComponent<Image>();
            hearts[currentHearts] = heartImage;
            heartImage.transform.SetParent(healthUI);
            heartImage.rectTransform.sizeDelta = new Vector2(heartSize, heartSize);
            heartImage.rectTransform.localPosition = new Vector3(heartSpacing * currentHearts, 0, 0);

            // add whole hearts for every 4 HP
            if (i / 4 > 0)
            {
                heartImage.sprite = fullHeart;
                i -= 4;
            }
            else if (i % 4 == 3)
            {
                heartImage.sprite = threeQuartersHeart;
                i -= 3;
            }
            else if (i % 4 == 2)
            {
                heartImage.sprite = halfHeart;
                i -= 2;
            }
            else if (i % 4 == 1)
            {
                heartImage.sprite = quarterHeart;
                i -= 1;
            }

            currentHearts++;

        }
    }

    public void UpdateHealthUI(PlayerHealth playerHealth, int playerIndex)
    {
        GameObject uiRoot = (playerIndex == 0) ? p1UI :
                            (playerIndex == 1) ? p2UI : null;

        if (uiRoot == null) return;

        // --- Get the heart images for THIS UI root ---
        Transform heartsPanel = uiRoot.transform.Find("MainPanel/HeartsPanel");
        if (heartsPanel == null)
        {
            Debug.LogError("HeartsPanel not found in " + uiRoot.name);
            return;
        }

        // Gather all heart images inside this panel
        Image[] heartsLocal = heartsPanel.GetComponentsInChildren<Image>(includeInactive: true);

        // Safety: ensure naming matches Heart0, Heart1, etc.
        // Sort by name so Heart0 -> index 0, Heart1 -> index 1, etc.
        System.Array.Sort(heartsLocal, (a, b) => a.name.CompareTo(b.name));

        int maxHP = playerHealth.getMaxHealth();
        int currentHP = playerHealth.getCurrentHealth();
        int maxHeartIndex = (maxHP / 4) + (maxHP % 4 != 0 ? 1 : 0) - 1;

        int hpToFill = currentHP;

        for (int i = 0; i <= maxHeartIndex; i++)
        {
            if (hpToFill >= 4)
            {
                heartsLocal[i].sprite = fullHeart;
                hpToFill -= 4;
            }
            else if (hpToFill == 3)
            {
                heartsLocal[i].sprite = threeQuartersHeart;
                hpToFill -= 3;
            }
            else if (hpToFill == 2)
            {
                heartsLocal[i].sprite = halfHeart;
                hpToFill -= 2;
            }
            else if (hpToFill == 1)
            {
                heartsLocal[i].sprite = quarterHeart;
                hpToFill -= 1;
            }
            else
            {
                heartsLocal[i].sprite = emptyHeart;
            }
        }

        // Flip hearts only for Player 2
        bool mirror = (playerIndex == 1);

        for (int i = 0; i <= maxHeartIndex; i++)
        {
            RectTransform rt = heartsLocal[i].rectTransform;

            rt.localScale = mirror
                ? new Vector3(-1, 1, 1)
                : new Vector3(1, 1, 1);
        }
    }

}
