using UnityEngine;

public class VirtualShieldProximityFade : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Renderer[] shieldRenderers;

    [Tooltip("Distance at which the shield starts becoming visible.")]
    [SerializeField] private float visibleStartDistance = 8f;

    [Tooltip("Distance at which the shield reaches its full (originally authored) visibility — 0 means the player has to be touching it.")]
    [SerializeField] private float fullyVisibleDistance = 0.5f;

    // The shield materials are legacy Particle shaders (Additive / Alpha
    // Blended), which expose their main color as "_TintColor", not the
    // standard "_Color" that Renderer.material.color maps to.
    private const string TintColorProperty = "_TintColor";

    private Color[] baseColors;

    private void Awake()
    {
        baseColors = new Color[shieldRenderers.Length];

        for (int i = 0; i < shieldRenderers.Length; i++)
        {
            baseColors[i] = shieldRenderers[i].material.GetColor(TintColorProperty);
        }

        if (!player)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found)
            {
                player = found.transform;
            }
        }
    }

    private void Update()
    {
        if (!player)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // 0 at/beyond visibleStartDistance, 1 at/inside fullyVisibleDistance.
        float t = Mathf.InverseLerp(visibleStartDistance, fullyVisibleDistance, distance);
        t = Mathf.Clamp01(t);

        for (int i = 0; i < shieldRenderers.Length; i++)
        {
            Color c = baseColors[i];
            c.a = baseColors[i].a * t;
            shieldRenderers[i].material.SetColor(TintColorProperty, c);
        }
    }
}
