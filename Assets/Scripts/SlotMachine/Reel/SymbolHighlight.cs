using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SymbolHighlight : MonoBehaviour
{
    public Image symbolImage;
    public float duration = 0.5f;

    public void Setup(Sprite sprite)
    {
        symbolImage.sprite = sprite;
        symbolImage.color = new Color(1f, 1f, 1f, 1f);

        StartCoroutine(AnimatedAndDestroy());
    }

    private IEnumerator AnimatedAndDestroy()
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = new Vector3(1.5f, 1.5f, 1.5f);
        Color startColor = symbolImage.color;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            float alpha = Mathf.Lerp(1f, 0f, t);
            symbolImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }
}
