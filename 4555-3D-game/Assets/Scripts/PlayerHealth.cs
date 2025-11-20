using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip playerHurtSound;
    [SerializeField] private AudioClip playerHealSound;
    [SerializeField] private float soundVolume = 1f;

    [Header("Invulnerability")]
    [SerializeField] private float invulnDuration = 1.0f;   // seconds
    [SerializeField] private float flashInterval = 0.1f;     // optional visual feedback

    private bool isInvulnerable = false;

    private int maxQuarterHearts;
    private int currentQuarterHearts;
    private PlayerUIManager uiManager;
    private PlayerInput input;
    private SpriteRenderer spriteRenderer;

    private Renderer[] renderers;
    private Color[] originalColors;


    public bool IsDead { get; private set; } = false;

    void Awake()
    {
        uiManager = PlayerUIManager.Instance;
        input = GetComponent<PlayerInput>();

        // Grab ALL renderers (SpriteRenderer, MeshRenderer, SkinnedMeshRenderer)
        renderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;
    }


    public void TakeDamage(int quarterHearts)
    {
        if (IsDead) return;
        if (isInvulnerable) return; // NEW: i-frames block damage

        currentQuarterHearts = Mathf.Max(currentQuarterHearts - quarterHearts, 0);
        uiManager.UpdateHealthUI(this, input.playerIndex);

        // hurt sound
        if (playerHurtSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(playerHurtSound, soundVolume);

        // Start i-frame window
        StartCoroutine(InvulnerabilityCoroutine());

        if (currentQuarterHearts <= 0)
            Die();
    }

    public void Heal(int quarterHearts)
    {
        if (IsDead) return;

        currentQuarterHearts = Mathf.Min(currentQuarterHearts + quarterHearts, maxQuarterHearts);
        uiManager.UpdateHealthUI(this, input.playerIndex);

        if (playerHealSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(playerHealSound, soundVolume);
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;
        bool toggle = false;

        while (elapsed < invulnDuration)
        {
            elapsed += flashInterval;
            toggle = !toggle;

            // Flash between white and red
            for (int i = 0; i < renderers.Length; i++)
            {
                if (toggle)
                    renderers[i].material.color = Color.red;
                else
                    renderers[i].material.color = originalColors[i];
            }

            yield return new WaitForSeconds(flashInterval);
        }

        // Restore original color
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.color = originalColors[i];

        isInvulnerable = false;
    }


    public int getMaxHealth() => maxQuarterHearts;
    public int getCurrentHealth() => currentQuarterHearts;

    public void setMaxHealth(int quarterHearts)
    {
        maxQuarterHearts = quarterHearts;
        currentQuarterHearts = maxQuarterHearts;
    }

    private void Die()
    {
        IsDead = true;

        if (input != null)
            input.enabled = false;

        GameManager.Instance.OnPlayerDied();
        Debug.Log($"Player {input.playerIndex} died");

        Destroy(gameObject);
    }

    public void setPlayerUIModule(PlayerUIManager uiManager, int playerIndex) { }
}
