using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SlotVisualController : MonoBehaviour
{
    [Header("기믹 시각화 연출")]
    public Image[] cellOverlays = new Image[9];

    public Color colorFreeze = new Color(0f, 0.8f, 1f, 0.5f);
    public Color colorBurn = new Color(1f, 0.2f, 0f, 0.5f);
    public Color colorCorrupted = new Color(0.6f, 0f, 0.8f, 0.5f);
    public Color colorEarthLock = new Color(0.3f, 0.3f, 0.3f, 0.7f);

    public void UpdateVisuals(SlotGimmickState state, int activeReelIndex, List<Vector2Int> activeCells)
    {
        // 초기화
        for(int i = 0; i < cellOverlays.Length; i++)
        {
            if (cellOverlays[i] != null) cellOverlays[i].gameObject.SetActive(false);
        }

        if (state == SlotGimmickState.Normal) return;

        Color targetColor = Color.clear;

        switch (state)
        {
            case SlotGimmickState.Water_Frozen: targetColor = colorFreeze; break;
            case SlotGimmickState.Fire_Burned: targetColor = colorBurn; break;
            case SlotGimmickState.Wood_Corrupted: targetColor = colorCorrupted; break;
            case SlotGimmickState.Earth_Locked: targetColor = colorEarthLock; break;
        }

        if(state == SlotGimmickState.Earth_Locked && activeReelIndex != -1)
        {
            for(int y = 0; y < 3; y++)
            {
                int index = activeReelIndex * 3 + y;
                if(index < cellOverlays.Length && cellOverlays[index] != null)
                {
                    cellOverlays[index].color = targetColor;
                    cellOverlays[index].gameObject.SetActive(true);
                }
            }
        }
        else
        {
            foreach(Vector2Int pos in activeCells)
            {
                int index = pos.x * 3 + pos.y;
                if (index >= 0 && index < cellOverlays.Length && cellOverlays[index] != null)
                {
                    cellOverlays[index].color = targetColor;
                    cellOverlays[index].gameObject.SetActive(true);
                }
            }
        }
    }
}
