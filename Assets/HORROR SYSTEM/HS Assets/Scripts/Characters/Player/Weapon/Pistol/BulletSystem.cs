using UnityEngine;
using System.Collections.Generic;

public class BulletSystem : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float bulletSpeed = 20f;
    public int bulletDamage = 10;

    [Header("Decal Settings")]
    public List<LayerDecalPistol> layerDecals;
    public float decalLifetime = 5f;

    private Rigidbody bulletRigidbody;

    private void Awake()
    {
        InitializeBullet();
    }

    private void Start()
    {
        LaunchBullet();
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void InitializeBullet()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    private void LaunchBullet()
    {
        bulletRigidbody.linearVelocity = transform.forward * bulletSpeed;
    }

    private void HandleCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(bulletDamage);
            }
        }

        int collisionLayer = collision.gameObject.layer;

        foreach (var layerDecalPair in layerDecals)
        {
            if ((layerDecalPair.layerMask & (1 << collisionLayer)) != 0)
            {
                Vector3 collisionNormal = collision.contacts[0].normal;
                Transform decalInstance = Instantiate(layerDecalPair.decalPrefab, transform.position, Quaternion.LookRotation(collisionNormal));
                Destroy(decalInstance.gameObject, decalLifetime);
                break;
            }
        }

        Destroy(gameObject);
    }
}

[System.Serializable]
public class LayerDecalPistol
{
    public LayerMask layerMask;
    public Transform decalPrefab;
}