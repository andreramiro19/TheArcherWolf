using UnityEngine;
using UnityEngine.Events;
using Invector.vCharacterController;
using Invector.vCharacterController.AI;

/// <summary>
/// Attach to a container GameObject holding a group of enemies (kept inactive
/// in the scene until Activate() is called). Activates the group and fires
/// onGroupDefeated once every enemy inside it has died.
/// </summary>
public class EnemyGroupMonitor : MonoBehaviour
{
    [SerializeField] private UnityEvent onGroupDefeated;

    private int aliveCount;
    private bool activated;

    /// <summary>
    /// Activates the group. If forceTarget is given, every enemy immediately
    /// targets it directly (bypassing their normal proximity/FOV detection) —
    /// needed for a scripted ambush where enemies should charge in the moment
    /// they appear, regardless of how far they start from the player.
    /// </summary>
    public void Activate(Transform forceTarget = null)
    {
        if (activated)
        {
            return;
        }

        activated = true;
        gameObject.SetActive(true);

        vCharacter[] characters = GetComponentsInChildren<vCharacter>(true);
        aliveCount = characters.Length;

        if (aliveCount == 0)
        {
            onGroupDefeated?.Invoke();
            return;
        }

        foreach (vCharacter character in characters)
        {
            character.gameObject.SetActive(true);
            character.onDead.AddListener(_ => OnMemberDefeated());

            if (forceTarget)
            {
                var ai = character.GetComponent<vSimpleMeleeAI_Controller>();
                if (ai)
                {
                    ai.SetCurrentTarget(forceTarget);
                }
            }
        }
    }

    private void OnMemberDefeated()
    {
        aliveCount--;

        if (aliveCount <= 0)
        {
            onGroupDefeated?.Invoke();
        }
    }
}
