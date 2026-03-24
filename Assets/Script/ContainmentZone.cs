using System.Collections;
using UnityEngine;

public class ContainmentZone : MonoBehaviour
{
    public float containDelay = 0.2f;

    private void OnTriggerEnter(Collider other)
    {
        Bubble bubble = other.GetComponent<Bubble>();
        if (bubble != null)
        {
            StartCoroutine(ContainAfterDelay(bubble));
        }
    }

    private IEnumerator ContainAfterDelay(Bubble bubble)
    {
        if (bubble == null) yield break;

        yield return new WaitForSeconds(containDelay);

        if (bubble != null)
        {
            bubble.Contain();
        }
    }
}