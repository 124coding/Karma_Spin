using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("플레이어 영구 스탯")]
    public float baseDamage = 100f;
    public float lineMultiplier = 2f;
    public float clusterMultiplier = 5f;
    public float allMultiplier = 20f;
    public float taegeukMultiplier = 3f;

    [Header("인벤토리 시스템")]
    public List<ItemData> activeInventory = new List<ItemData>(); // 액티브

    public int maxActiveSlots = 3;
    public bool hasUsedItemThisTurn = false;

    public event Action<int> OnGoldChanged;

    [SerializeField]private int _gold = 0;

    public int gold
    {
        get => _gold;
        set
        {
            _gold = value;
            OnGoldChanged?.Invoke(_gold);
        }
    }

    public event Action<float> OnShieldChanged;

    [Header("버프 상태")]
    [SerializeField] private float _currentShieldHP = 0f;

    public float currentShieldHP
    {
        get => _currentShieldHP;
        set
        {
            _currentShieldHP = value;
            OnShieldChanged?.Invoke(_currentShieldHP);
        }
    }

    public void AddBaseDamage(float amount) { baseDamage += amount; }

    [Header("1턴 한정 버프")]
    public Dictionary<SymbolType, float> tempSymbolMultipliers = new Dictionary<SymbolType, float>();

    public void AddTempSymbolMultiplier(SymbolType type, float bonusRate)
    {
        if (tempSymbolMultipliers.ContainsKey(type))
        {
            tempSymbolMultipliers[type] += bonusRate;
        }
        else
        {
            tempSymbolMultipliers[type] = bonusRate;
        }
    }

    public void ResetTurnData()
    {
        hasUsedItemThisTurn = false;
        tempSymbolMultipliers.Clear();

        currentShieldHP = 0f;
    }

    [Header("유물 인벤토리")]
    public List<Relic> relics = new List<Relic>();

    public void AddRelic(Relic newRelic, SlotManager mySlotManager)
    {
        relics.Add(newRelic);

        newRelic.Equip(this, mySlotManager);
    }

    public void TriggerBattleStart(SlotManager mySlotManager)
    {
        foreach(var relic in relics)
        {
            relic.TriggerBattleStart(this, mySlotManager);
        }
    }

    public event Action OnRoundStartEvent;

    public void TriggerRoundStart()
    {
        OnRoundStartEvent?.Invoke();
    }
}
