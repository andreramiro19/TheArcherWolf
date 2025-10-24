using UnityEngine;
using System.Collections.Generic;

public class FootstepHandler : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioSource footstepAudioSource;
    public LayerMask groundLayer;
    public float raycastDistance = 1f;

    [Header("Footstep Group")]
    public List<FootstepGroup> footstepGroups = new List<FootstepGroup>();

    private Dictionary<Texture2D, FootstepGroup> footstepGroupsByTexture;

    private void Start()
    {
        InitializeFootstepGroups();
    }

    private void InitializeFootstepGroups()
    {
        footstepGroupsByTexture = new Dictionary<Texture2D, FootstepGroup>();
        foreach (var group in footstepGroups)
        {
            foreach (var texture in group.surfaceTextures)
            {
                if (!footstepGroupsByTexture.ContainsKey(texture))
                {
                    footstepGroupsByTexture.Add(texture, group);
                }
            }
        }
    }

    public void PlayFootstep()
    {
        Texture2D surfaceTexture = GetSurfaceTexture();

        if (surfaceTexture != null && footstepGroupsByTexture.ContainsKey(surfaceTexture))
        {
            var group = footstepGroupsByTexture[surfaceTexture];

            int randomIndex = Random.Range(0, group.audioClips.Length);
            footstepAudioSource.clip = group.audioClips[randomIndex];
            footstepAudioSource.volume = group.footstepVolume;
            footstepAudioSource.Play();
        }
    }

    private Texture2D GetSurfaceTexture()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                return renderer.material.mainTexture as Texture2D;
            }
        }
        return null;
    }

    [System.Serializable]
    public class FootstepGroup
    {
        public List<Texture2D> surfaceTextures = new List<Texture2D>();
        public AudioClip[] audioClips;
        [Range(0, 1)] public float footstepVolume = 1.0f;
    }
}
