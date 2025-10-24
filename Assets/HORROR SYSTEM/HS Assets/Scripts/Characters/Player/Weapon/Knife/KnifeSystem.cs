using UnityEngine;
using System.Collections.Generic;

public class KnifeSystem : MonoBehaviour
{
    [Header("Knife Settings")]
    public int knifeDamage = 5;
    public Collider knifeCollider;

    [Header("Decal Settings")]
    public List<LayerDecalKnife> layerDecals;
    public float decalLifetime = 5f;

    private void Start()
    {
        knifeCollider.enabled = false;
    }

    public void EnableCollider()
    {
        knifeCollider.enabled = true;
    }

    public void DisableCollider()
    {
        knifeCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(knifeDamage);
            }
        }

        int collisionLayer = other.gameObject.layer;

        foreach (var layerDecalPair in layerDecals)
        {
            if ((layerDecalPair.layerMask & (1 << collisionLayer)) != 0)
            {
                Vector3 contactPoint = other.ClosestPoint(transform.position);
                Vector3 contactNormal = (contactPoint - transform.position).normalized;
                Transform decalInstance = Instantiate(layerDecalPair.decalPrefab, contactPoint, Quaternion.LookRotation(contactNormal));
                Destroy(decalInstance.gameObject, decalLifetime);
                break;
            }
        }
    }
}

[System.Serializable]
public class LayerDecalKnife
{
    public LayerMask layerMask;
    public Transform decalPrefab;
}