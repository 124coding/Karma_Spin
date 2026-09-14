using UnityEngine;
using UnityEditor;

public class SlotSizeAdjuster : EditorWindow
{
    private SlotManager slotManager;
    private Vector2 symbolSize = new Vector2(100f, 100f); // 심볼 1개의 크기
    private float spacing = 0f; // 심볼 간의 여백

    [MenuItem("Tools/ 슬롯 크기 일괄 조절기")]
    public static void ShowWindow()
    {
        GetWindow<SlotSizeAdjuster>("슬롯 크기 조절기");
    }

    private void OnGUI()
    {
        GUILayout.Label("슬롯머신 UI 크기 일괄 세팅", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        slotManager = (SlotManager)EditorGUILayout.ObjectField("Slot Manager", slotManager, typeof(SlotManager), true);

        EditorGUILayout.Space();
        symbolSize = EditorGUILayout.Vector2Field("심볼 크기 (Width x Height)", symbolSize);
        spacing = EditorGUILayout.FloatField("심볼 위아래 간격", spacing);

        EditorGUILayout.Space();

        if (GUILayout.Button("크기 및 위치 일괄 적용!", GUILayout.Height(40)))
        {
            ApplySizeChanges();
        }
    }

    private void ApplySizeChanges()
    {
        if(slotManager == null)
        {
            Debug.LogError("SlotManager를 할당해주세요!");
            return;
        }

        // Ctrl + z로 취소 가능
        Undo.RegisterFullObjectHierarchyUndo(slotManager.gameObject, "Resize Slot UI");

        float stepDistance = symbolSize.y + spacing;

        for (int i = 0; i < slotManager.reels.Length; ++i)
        {
            ReelController reel = slotManager.reels[i];
            if (reel == null) continue;

            reel.symbolHeight = symbolSize.y + spacing;

            // Reel 마스크(배경) 크기 조절
            RectTransform reelRect = reel.GetComponent<RectTransform>();
            if (reelRect != null)
            {
                reelRect.sizeDelta = new Vector2(symbolSize.x, (symbolSize.y * 4) + (spacing * 3));
                float xPos = (i - 1) * (symbolSize.x + spacing);
                reelRect.anchoredPosition = new Vector2(xPos, 0f); // Y축 정중앙 정렬
            }

            if (reel.reelContent != null)
            {
                // Content 위치 정중앙 초기화
                reel.reelContent.anchoredPosition = Vector2.zero;
                reel.reelContent.sizeDelta = new Vector2(symbolSize.x, stepDistance * reel.symbolImages.Length);

                // 심볼 개수의 절반(예: 7개면 3)을 중앙 인덱스로 설정
                int middleIndex = reel.symbolImages.Length / 2;

                for (int j = 0; j < reel.symbolImages.Length; j++)
                {
                    RectTransform symbolRect = reel.symbolImages[j].rectTransform;
                    symbolRect.sizeDelta = symbolSize;

                    // 가운데 심볼이 Y=0에 위치하고, 인덱스가 작을수록 위(+), 클수록 아래(-) 배치
                    float yPos = (middleIndex - j) * stepDistance;
                    symbolRect.anchoredPosition = new Vector2(0, yPos);
                }
            }

            // 변경된 프리팹/씬 데이터를 저장하도록 유니티에 알림
            EditorUtility.SetDirty(slotManager);
            Debug.Log("<color=green>슬롯머신 크기 및 위치 세팅이 완료되었습니다!</color>");
        }
    }
}
