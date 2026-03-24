using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Reactor : MonoBehaviour
{
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;
    public float flashDuration = 0.12f;

    private Renderer rend;
    private Coroutine flashRoutine;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void PlayHitFeedback()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (rend != null && rend.material != null)
        {
            rend.material.color = hitColor;
        }

        yield return new WaitForSecondsRealtime(flashDuration);

        if (rend != null && rend.material != null)
        {
            rend.material.color = normalColor;
        }
    }
}