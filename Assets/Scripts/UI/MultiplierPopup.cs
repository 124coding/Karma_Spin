using System.Collections;
using TMPro;
using UnityEngine;

public class MultiplierPopup : MonoBehaviour
{
    public TextMeshProUGUI popupText;
    public float duration = 1f;

    public void Setup(string text, Color color)
    {
        popupText.text = text;
        popupText.color = color;

        StartCoroutine(AnimateAndDestroy());
    }

    private IEnumerator AnimateAndDestroy()
    {
        float halfDuration = duration / 2f;
        float elapsed = 0f;

        while(elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1.5f, elapsed / halfDuration);
            transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        elapsed = 0f;

        Color startColor = popupText.color;

        while(elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;

            // 크기 원상복구
            float scale = Mathf.Lerp(1.5f, 1f, elapsed / halfDuration);
            transform.localScale = new Vector3(scale, scale, scale);

            // 투명도(Alpha) 서서히 감소
            float alpha = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
            popupText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null;
        }

        Destroy(gameObject); // 연출 끝나면 삭제
    }
}
