using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReelController : MonoBehaviour
{
    public List<SymbolData> reelStrip = new List<SymbolData>();
    public bool isSpinning = false;
    public float duration = 1.5f;

    [Header("시각적 롤링 설정")]
    public RectTransform reelContent;      // 심볼 이미지들을 담고 있는 부모 컨테이너
    public Image[] symbolImages;           // 화면에 보이는 4~5개의 UI Image 컴포넌트
    public float symbolHeight = 150f;      // 심볼 1개의 UI 높이 (픽셀)
    public float maxSpinSpeed = 2000f;     // 회전 최고 속도

    private int currentStripIndex = 0;     // 현재 100칸 릴 중 어디를 읽고 있는지

    public void InitializeReel(List<SymbolData> generatedStrip)
    {
        reelStrip = generatedStrip;
        UpdateVisuals(); // 초기 이미지 세팅
    }

    public IEnumerator Spin(Action<SymbolData[]> onComplete)
    {
        isSpinning = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 시각적 롤링 연출
            elapsed += Time.fixedDeltaTime;

            // 2차 함수를 이용한 자연스러운 감속
            float t = elapsed / duration;
            float currentSpeed = Mathf.Lerp(maxSpinSpeed, 100f, t * t);

            // 컨테이너를 아래로 이동
            reelContent.anchoredPosition += Vector2.down * currentSpeed * Time.fixedDeltaTime;

            // 심볼 1개 높이만큼 아래로 내려가면 컨베이어 벨트 한 칸 이동
            while(reelContent.anchoredPosition.y <= -symbolHeight)
            {
                reelContent.anchoredPosition += Vector2.up * symbolHeight;
                currentStripIndex = (currentStripIndex - 1 + reelStrip.Count) % reelStrip.Count;
                UpdateVisuals();
            }

            yield return new WaitForFixedUpdate();
        }

        float snapDuration = 0.15f;
        float snapElapsed = 0f;

        Vector2 startPos = reelContent.anchoredPosition;

        while(snapElapsed < snapDuration)
        {
            snapElapsed += Time.fixedDeltaTime;
            float snapT = snapElapsed / snapDuration;

            reelContent.anchoredPosition = Vector2.Lerp(startPos, Vector2.zero, snapT);
            yield return new WaitForFixedUpdate();
        }

        reelContent.anchoredPosition = Vector2.zero;
        isSpinning = false;

        int visibleOffset = 2;

        int firstVisibleIndex = (currentStripIndex + visibleOffset) % reelStrip.Count;

        SymbolData[] results = new SymbolData[3];
        results[0] = reelStrip[firstVisibleIndex];
        results[1] = reelStrip[(firstVisibleIndex + 1) % reelStrip.Count];
        results[2] = reelStrip[(firstVisibleIndex + 2) % reelStrip.Count];

        onComplete?.Invoke(results);
    }

    private void UpdateVisuals()
    {
        for(int i = 0; i < symbolImages.Length; i++)
        {
            int dataIndex = (currentStripIndex + i) % reelStrip.Count;
            symbolImages[i].sprite = reelStrip[dataIndex].symbolSprite;
        }
    }
}
