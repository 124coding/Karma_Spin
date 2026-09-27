using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Relic")]
public class Relic : ScriptableObject
{
    [Header("유물 정보")]
    public string relicName;
    [TextArea] public string description;
    public Sprite icon;
    public int cost;

    [Header("조립된 효과들")]
    public List<RelicEffect> effects = new List<RelicEffect>();

    // 획득 시 모든 효과의 OnEquip 실행
    public void Equip(PlayerManager player, SlotManager slotManager)
    {
        foreach (var effect in effects)
        {
            effect.OnEquip(player, slotManager);
        }
    }

    // 전투 시작 시 모든 효과의 OnBattleStart 실행
    public void TriggerBattleStart(PlayerManager player, SlotManager slotManager)
    {
        foreach (var effect in effects)
        {
            effect.OnBattleStart(player, slotManager);
        }
    }
}
