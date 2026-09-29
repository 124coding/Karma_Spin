using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    [Header("연결 설정")]
    public PlayerManager targetPlayer;
    public BaseBattleManager battleManager;

    [Header("액티브 아이템 UI (고정 슬롯)")]
    public List<InventorySlotUI> activeSlots = new List<InventorySlotUI>();

    [Header("유물 아이템 UI (동적 생성)")]
    public RelicSlotUI relicSlotPrefab;
    public Transform relicGroupParent;
    public int initialPoolSize = 5;

    private List<RelicSlotUI> relicPool = new List<RelicSlotUI>();

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for(int i = 0; i < initialPoolSize; i++)
        {
            RelicSlotUI slot = Instantiate(relicSlotPrefab, relicGroupParent);
            slot.gameObject.SetActive(false);
            relicPool.Add(slot);
        }
    }

    public void RefreshInventoryUI()
    {
        for (int i = 0; i < activeSlots.Count; i++)
        {
            if (i < targetPlayer.activeInventory.Count)
            {
                activeSlots[i].SetupSlot(targetPlayer.activeInventory[i], targetPlayer, battleManager, this);
            }
            else
            {
                activeSlots[i].ClearSlot();
            }
        }

        int currentRelicCount = targetPlayer.relics.Count;

        // 플레이어가 가진 유물 개수보다 UI 슬롯 부족 시, 모자란 만큼 프리팹 생성
        while (relicPool.Count < currentRelicCount)
        {
            RelicSlotUI newSlot = Instantiate(relicSlotPrefab, relicGroupParent);
            newSlot.gameObject.SetActive(false);
            relicPool.Add(newSlot);
        }

        for (int i = 0; i < relicPool.Count; i++)
        {
            if (i < currentRelicCount)
            {
                relicPool[i].gameObject.SetActive(true);
                relicPool[i].SetupSlot(targetPlayer.relics[i]);
            }
            else
            {
                relicPool[i].gameObject.SetActive(false);
            }
        }
    }

    public void ResetInventoryUI()
    {
        foreach (var slot in activeSlots)
        {
            slot.ClearSlot();
        }

        foreach (var slot in relicPool)
        {
            if (slot != null)
            {
                slot.ClearSlot();
                slot.gameObject.SetActive(false);
            }
        }
    }

    public void SetSlotsInteractable(bool interactable)
    {
        for(int i = 0; i< targetPlayer.activeInventory.Count; i++)
        {
            if(i < activeSlots.Count)
            {
                activeSlots[i].useButton.interactable = interactable;
            }
        }
    }
}
