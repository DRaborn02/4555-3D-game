using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHalfHearts = 6; // 6 = 3 full hearts
    [SerializeField] private int currentHalfHearts;
    [SerializeField] private PlayerController playerController; // drag in the PlayerController


    private Image[] heartImages;

    void Awake()
    {
        currentHalfHearts = maxHalfHearts;
    }

    public void BindUI(Image[] hearts)
    {
        heartImages = hearts;
        currentHalfHearts = maxHalfHearts;
        RefreshHearts();
    }

    public void TakeDamage(int halfHearts)
    {
        currentHalfHearts = Mathf.Max(currentHalfHearts - halfHearts, 0);
        RefreshHearts();

        // If at or below 1 half-heart, disable player control
        if (currentHalfHearts <= 1 && playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("Player is knocked out!");
        }
    }

    public void Heal(int halfHearts)
    {
        currentHalfHearts = Mathf.Min(currentHalfHearts + halfHearts, maxHalfHearts);
        RefreshHearts();
    }

    private void RefreshHearts()
    {
        if (heartImages == null) return;

        // Disable from the back (Heart5 → Heart0)
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].enabled = (i < currentHalfHearts);
        }

        Debug.Log($"Current health = {currentHalfHearts}/{maxHalfHearts}");
    }
}
