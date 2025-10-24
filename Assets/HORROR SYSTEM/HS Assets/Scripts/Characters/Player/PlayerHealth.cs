using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Health UI")]
    public TMP_Text healthText;
    public Image healthImage;

    [Header("Audio Settings")]
    public string treatmentSound;

    [Header("Health UI Color Settings")]
    public Color healthColorMax = Color.green;
    public Color healthColorMiddle = Color.yellow;
    public Color healthColorMin = Color.red;

    private bool isPlayerDead = false;
    private PlayerAnimations playerAnimations;

    private void Start()
    {
        playerAnimations = GetComponent<PlayerAnimations>();

        currentHealth = maxHealth;
        UpdateHealthUI(currentHealth, maxHealth);
    }

    public void Heal(int value)
    {
        currentHealth += value;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        UpdateHealthText(currentHealth, maxHealth);
    }

    public void SetMaxHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI(currentHealth, maxHealth);
        AudioManager.instance.Play(treatmentSound);
    }

    public void TakeDamage(int damage)
    {
        if (isPlayerDead) return;

        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        if (currentHealth <= 0)
        {
            Die();
        }

        UpdateHealthUI(currentHealth, maxHealth);
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    private void Die()
    {
        isPlayerDead = true;
        playerAnimations.SetDeath();
        playerAnimations.ResetRigWeights();
    }

    public bool IsPlayerDead()
    {
        return isPlayerDead;
    }

    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        UpdateHealthText(currentHealth, maxHealth);
        UpdateHealthImageColor(currentHealth, maxHealth);
    }

    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        int healthPercentage = (int)Mathf.Round(((float)currentHealth / maxHealth) * 100);
        healthText.text = healthPercentage + "%";
    }

    private void UpdateHealthImageColor(int currentHealth, int maxHealth)
    {
        int healthPercentage = (int)Mathf.Round(((float)currentHealth / maxHealth) * 100);
        UpdateHealthTextColor(healthPercentage);
    }

    private void UpdateHealthTextColor(int healthPercentage)
    {
        if (healthPercentage >= 50)
        {
            healthText.color = Color.Lerp(healthColorMax, healthColorMiddle, (50 - healthPercentage) / 50f);
            healthImage.color = Color.Lerp(healthColorMax, healthColorMiddle, (50 - healthPercentage) / 50f);
        }
        else if (healthPercentage >= 25)
        {
            healthText.color = Color.Lerp(healthColorMiddle, healthColorMin, (25 - healthPercentage) / 25f);
            healthImage.color = Color.Lerp(healthColorMiddle, healthColorMin, (25 - healthPercentage) / 25f);
        }
        else
        {
            healthText.color = healthColorMin;
            healthImage.color = healthColorMin;
        }
    }
}