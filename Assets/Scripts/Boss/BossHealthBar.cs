using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI 연결")]
    public Image currentBar;
    public Image nextBar;
    public TextMeshProUGUI phaseText;

    [Header("색상 설정")]
    public Color[] phaseColors;
    public Color shieldColor = new Color(0.7f, 0.7f, 0.7f);

    // BossManager에서 데미지를 입을 때마다 해당 함수 호출
    public void UpdateHealthUI(float currentHP, float maxHP, int currentPhase, bool isShielded = false)
    {
        // 현재 체력바 깍기
        currentBar.fillAmount = currentHP / maxHP;
        
        // 남은 줄 수 텍스트 갱신
        phaseText.text = $"x{currentPhase}";

        // 색상 업데이트
        if(currentPhase > 0)
        {
            if (isShielded)
            {
                // 쉴드가 켜져 있다면 금속/회색으로 덮어씌우고 텍스트 추가
                currentBar.color = shieldColor;
                nextBar.color = shieldColor;
                phaseText.text = $"x{currentPhase} <size=70%><color=#CCCCCC>(봉인됨)</color></size>";
            }
            else
            {
                if(currentPhase == 1)
                {
                    // 평소에는 정상적인 페이즈 색상 적용
                    currentBar.color = GetColor(currentPhase);
                    nextBar.color = new Color(0f, 0f, 0f);
                }
                else
                {
                    currentBar.color = GetColor(currentPhase);
                    nextBar.color = GetColor(currentPhase - 1);
                }

                phaseText.text = $"x{currentPhase}";
            }
        }
        else
        {
            // 피냐타 모드(0줄) 진입 시의 처리
            currentBar.color = Color.gray; // 샌드백 색상
            nextBar.color = Color.white; // 배경색
            phaseText.text = "Dead";
        }
    }

    private Color GetColor(int phase)
    {
        if (phaseColors.Length == 0) return Color.white;

        if (phase <= 0) return new Color(0, 0, 0, 0);

        int colorIndex = (phase - 1) % phaseColors.Length;
        return phaseColors[colorIndex];
    }
}
