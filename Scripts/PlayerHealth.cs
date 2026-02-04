using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Events")]
    public UnityEvent onDamaged;
    public UnityEvent onDeath;
    public UnityEvent<int> onHealthChanged;

    [Header("Visual Feedback (Optional)")]
    public bool flashScreenOnDamage = true;
    public Color damageFlashColor = new Color(1f, 0f, 0f, 0.3f);
    public float flashDuration = 0.2f;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Player took {damage} damage! Health: {currentHealth}/{maxHealth}");
        onDamaged?.Invoke();
        onHealthChanged?.Invoke(currentHealth);

        if (flashScreenOnDamage)
        {
            StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        onHealthChanged?.Invoke(currentHealth);
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player has died!");

        onDeath?.Invoke();


    }

    System.Collections.IEnumerator DamageFlash()
    {

        yield return new WaitForSeconds(flashDuration);
    }

    [ContextMenu("Test Damage (25)")]
    void TestDamage()
    {
        TakeDamage(25);
    }

    [ContextMenu("Test Heal (25)")]
    void TestHeal()
    {
        Heal(25);
    }
}
