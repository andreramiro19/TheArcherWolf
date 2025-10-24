using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    protected GameObject player;

    public virtual void Interact(GameObject player = null)
    {
        this.player = player;
    }

    protected void Destroy(float destroyTimer)
    {
        Destroy(this.gameObject, destroyTimer);
    }

    public GameObject GetPlayer()
    {
        return this.player;
    }
}