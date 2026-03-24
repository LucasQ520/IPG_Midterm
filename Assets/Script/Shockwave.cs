using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ShockwaveFanVisual : MonoBehaviour
{
    public float duration = 0.25f;
    public float yOffset = 0.03f;

    private Renderer rend;
    private Material runtimeMaterial;

    private void Awake()
    {
        rend = GetComponent<Renderer>();

        if (rend != null)
        {
            runtimeMaterial = rend.material;
        }
    }

    public void Play(Vector3 forward, float radius, float angle, float fixedY)
    {
        transform.position = new Vector3(transform.position.x, fixedY + yOffset, transform.position.z);

        if (forward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
        }

        StartCoroutine(PlayRoutine(radius, angle, fixedY));
    }

    private IEnumerator PlayRoutine(float radius, float angle, float fixedY)
    {
        float t = 0f;

        float widthScale = Mathf.Max(0.5f, angle / 45f);
        Vector3 startScale = new Vector3(0.2f * widthScale, 1f, 0.2f);
        Vector3 endScale = new Vector3(widthScale, 1f, radius);

        transform.localScale = startScale;

        if (runtimeMaterial != null)
        {
            Color c = runtimeMaterial.color;
            c.a = 0.85f;
            runtimeMaterial.color = c;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            transform.localScale = Vector3.Lerp(startScale, endScale, p);

            Vector3 pos = transform.position;
            pos.y = fixedY + yOffset;
            transform.position = pos;

            if (runtimeMaterial != null)
            {
                Color c = runtimeMaterial.color;
                c.a = Mathf.Lerp(0.85f, 0f, p);
                runtimeMaterial.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}