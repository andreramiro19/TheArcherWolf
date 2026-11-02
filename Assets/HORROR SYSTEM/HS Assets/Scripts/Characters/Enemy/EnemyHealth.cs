using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int health = 100;

    private bool isEnemyDead = false;
    private EnemyAnimations enemyAnimations;
    private EnemyController enemyController;

    private void Start()
    {
        enemyAnimations = GetComponent<EnemyAnimations>();
        enemyController = GetComponent<EnemyController>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isEnemyDead) return;

        health -= damageAmount;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            ReactToDamage();
        }
    }

    private void Die()
    {
        isEnemyDead = true;
        enemyAnimations.Die();
        enemyController.StopEnemy();
        enemyController.DisableEnemySound();

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = false;
        }

        Destroy(gameObject, 15f);
    }

    private void ReactToDamage()
    {
        enemyAnimations.ReactToDamage();
    }

    public bool IsEnemyDead()
    {
        return isEnemyDead;
    }
}