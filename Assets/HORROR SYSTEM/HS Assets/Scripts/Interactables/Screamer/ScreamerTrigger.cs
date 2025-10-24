using UnityEngine;

public class ScreamerTrigger : MonoBehaviour
{
    public GameObject screamerObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            screamerObject.GetComponent<InteractiveObject>().Interact();
            gameObject.GetComponent<Collider>().enabled = false;
        }
    }
}
