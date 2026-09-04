using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI 연결")]
    public Image currentBar;
    public Image nextBar;
    public TextMeshProUGUI phaseText;

    [Header("색상 설정")]
    public Color[] phaseColors;
    public Color shieldColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("애니메이션 설정")]
    public float animationDuration = 0.25f; // 체력바가 깎이는 시간 (0.25초)
    private Coroutine healthAnimCoroutine;  // 현재 실행 중인 코루틴 추적

    private int visualPhase = -1;

    // BossManager에서 데미지를 입을 때마다 해당 함수 호출
    public void UpdateHealthUI(float currentHP, float maxHP, int currentPhase, bool isShielded = false)
    {
        if (currentPhase <= 0)
        {
            if (healthAnimCoroutine != null) StopCoroutine(healthAnimCoroutine); // 실행 중인 애니메이션 즉시 정지

            currentBar.color = Color.gray;
            nextBar.color = Color.black;
            currentBar.fillAmount = 1f;
            phaseText.text = "피냐타 모드!";

            return;
        }

        // 현재 체력바 깍기
        float targetFillAmount = currentHP / maxHP;

        if (visualPhase != -1 && currentPhase < visualPhase)
        {
            currentBar.fillAmount = 1f;
        }

        visualPhase = currentPhase;

        // 남은 줄 수 텍스트 갱신
        phaseText.text = $"x{currentPhase}";

        // 색상 업데이트
        if (isShielded)
        {
            currentBar.color = shieldColor;
            phaseText.text = $"x{currentPhase} <size=70%><color=#CCCCCC>(봉인됨)</color></size>";
        }
        else
        {
            currentBar.color = GetColor(currentPhase);
            phaseText.text = $"x{currentPhase}";
        }
        nextBar.color = GetColor(currentPhase - 1);

        // 체력바 깎기 애니메이션 (Lerp)
        if (healthAnimCoroutine != null)
        {
            StopCoroutine(healthAnimCoroutine);
        }

        healthAnimCoroutine = StartCoroutine(AnimateHealthBar(targetFillAmount));
    }

    private IEnumerator AnimateHealthBar(float targetFill)
    {
        float startFill = currentBar.fillAmount;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            currentBar.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / animationDuration);
            yield return null;
        }

        currentBar.fillAmount = targetFill;
    }

    private Color GetColor(int phase)
    {
        if (phaseColors.Length == 0) return Color.white;

        if (phase <= 0) return new Color(0, 0, 0, 0);

        int colorIndex = (phase - 1) % phaseColors.Length;
        return phaseColors[colorIndex];
    }
}
