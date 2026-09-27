using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PvpShopManager : MonoBehaviour
{
    private PvpBattleManager currentBattleManager;
    private PlayerManager localPlayer;
    private SlotManager localSlotManager;

    [Header("전체 아이템 풀")]
    public List<ItemData> allActiveItems; // 생성해둔 모든 소모품 데이터
    public List<Relic> allRelicItems;

    [Header("상점 UI 연결")]
    public GameObject shopPanel;
    public PvpUIManager uiManager;

    [Header("UI 진열대")]
    public ShopSlotUI[] activeSlots;
    public ShopSlotUI[] relicSlots;

    [Header("플레이어 연결")]
    public PlayerManager player1;

    [Header("상점 상태")]
    public GameObject readyDimPanel;
    private int refreshCount = 0;
    public Button refreshButton;

    [Header("골드 UI")]
    public TextMeshProUGUI shopGoldText;

    private const int MAX_REFRESH = 1;
    public int refreshCost = 500;

    public void OpenShop(PvpBattleManager battleManager)
    {
        Debug.Log("OpenShop");
        currentBattleManager = battleManager;
        shopPanel.SetActive(true);

        localPlayer = currentBattleManager.player1;
        localSlotManager = currentBattleManager.slotManager1P;

        localPlayer.OnGoldChanged -= UpdateGoldText;
        localPlayer.OnGoldChanged += UpdateGoldText;
        UpdateGoldText(localPlayer.gold);

        refreshCount = 0;
        if (readyDimPanel != null) readyDimPanel.SetActive(false);
        if (refreshButton != null) refreshButton.interactable = true;

        RollItems();

        if (uiManager != null) uiManager.RefreshAllInventoryUI();
    }

    private void UpdateGoldText(int amount)
    {
        if (shopGoldText != null)
            shopGoldText.text = amount > 0 ? amount.ToString() + "G" : "0G";
    }

    public void RollItems()
    {
        if(allActiveItems != null && allActiveItems.Count > 0)
        {
            List<ItemData> availableActives = new List<ItemData>();
            foreach (var item in allActiveItems)
            {
                availableActives.Add(item);
                availableActives.Add(item); // 같은 아이템 2장 투입
            }

            foreach (var slot in activeSlots)
            {
                if (availableActives.Count > 0)
                {
                    Debug.Log("Setting Random Active");

                    int randomIndex = Random.Range(0, availableActives.Count);
                    ItemData randomItem = availableActives[randomIndex];

                    slot.SetupActiveSlot(randomItem, this, localPlayer);
                    availableActives.RemoveAt(randomIndex);
                }
                else
                {
                    slot.MarkAsSoldOut();
                }
            }
        }

        if (allRelicItems != null && allRelicItems.Count > 0)
        {
            List<Relic> availableRelics = new List<Relic>();
            foreach (var relic in allRelicItems)
            {
                if (!localPlayer.relics.Contains(relic))
                {
                    availableRelics.Add(relic);
                }

            }
            foreach (var slot in relicSlots)
            {
                if (availableRelics.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableRelics.Count);
                    Relic randomRelic = availableRelics[randomIndex];

                    slot.SetupRelicSlot(randomRelic, this, localPlayer, localSlotManager);

                    availableRelics.RemoveAt(randomIndex);
                }
                else
                {
                    // 더 이상 획득할 유물이 없다면 슬롯을 비활성화 (솔드아웃 또는 클리어 처리)
                    slot.MarkAsSoldOut();
                    slot.nameText.text = "ALL CLEARED";
                }
            }
        }
    }

    public void BuyActiveItem(ItemData item, PlayerManager buyer, ShopSlotUI slotUI)
    {
        if (buyer.activeInventory.Count >= buyer.maxActiveSlots) return;


        if(buyer.gold >= item.cost)
        {
            buyer.gold -= item.cost;

            buyer.activeInventory.Add(item);

            slotUI.MarkAsSoldOut();

            if (uiManager != null) uiManager.RefreshAllInventoryUI();
        }
    }

    public void BuyRelic(Relic relic, PlayerManager buyer, ShopSlotUI slotUI, SlotManager mySlotManager)
    {
        if (buyer.gold >= relic.cost)
        {
            buyer.gold -= relic.cost;

            buyer.AddRelic(relic, mySlotManager);
            slotUI.MarkAsSoldOut();

            if (uiManager != null) uiManager.RefreshAllInventoryUI();
        }
    }

    public void OnClickRefresh()
    {
        if (refreshCount >= MAX_REFRESH) return;

        if(localPlayer.gold >= refreshCost)
        {
            localPlayer.gold -= refreshCost;
            refreshCount++;

            RollItems();

            if (refreshCount >= MAX_REFRESH && refreshButton != null)
            {
                refreshButton.interactable = false;
            }

            if (uiManager != null) uiManager.RefreshAllInventoryUI();
        }
    }

    public void OnClickReady()
    {
        if (readyDimPanel != null) readyDimPanel.SetActive(true);

        ShopSyncData syncData = new ShopSyncData();

        foreach(var relic in localPlayer.relics)
        {
            syncData.relicNames.Add(relic.relicName);
        }

        foreach(var activeItem in localPlayer.activeInventory)
        {
            syncData.activeItemNames.Add(activeItem.itemName);
        }

        string jsonData = JsonUtility.ToJson(syncData);
        GamePacket packet = new GamePacket { packetType = 4, player = NetworkTest.Instance.myPlayerIndex, data = jsonData };

        // 서버로 Ready 패킷 발송
        if (NetworkTest.Instance != null)
        {
            NetworkTest.Instance.SendPacket(packet);
        }
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        if (localPlayer != null)
        {
            localPlayer.OnGoldChanged -= UpdateGoldText;
        }
    }
}
