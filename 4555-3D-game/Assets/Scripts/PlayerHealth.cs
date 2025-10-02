using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    private int maxQuarterHearts;
    private int currentQuarterHearts;
    private PlayerUIManager uiManager;
    private PlayerInput input;

    void Awake()
    {
        uiManager = PlayerUIManager.Instance;
        input = GetComponent<PlayerInput>();
    }

    public void TakeDamage(int quarterHearts)
    {
        currentQuarterHearts = Mathf.Max(currentQuarterHearts - quarterHearts, 0);
        uiManager.UpdateHealthUI(this, input.playerIndex);
        // print("Player " + input.playerIndex + " took damage, current health: " + currentQuarterHearts);
    }

    public void Heal(int quarterHearts)
    {
        currentQuarterHearts = Mathf.Min(currentQuarterHearts + quarterHearts, maxQuarterHearts);
        uiManager.UpdateHealthUI(this, input.playerIndex);
    }

    public int getMaxHealth()
    {
        return maxQuarterHearts;
    }

    public int getCurrentHealth()
    {
        return currentQuarterHearts;
    }

    public void setMaxHealth(int quarterHearts)
    {
        maxQuarterHearts = quarterHearts;
        currentQuarterHearts = maxQuarterHearts;
    }

    public void setPlayerUIModule(PlayerUIManager uiManager, int playerIndex)
    {
        
    }

}
