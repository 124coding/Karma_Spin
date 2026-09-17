using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int cost;

    [Header("이 아이템이 발동시킬 기능들")]
    public List<ItemEffect> effects;

    public void UseItem(BaseBattleManager battleManager, DamageReport report)
    {
        foreach (var effect in effects)
        {
            effect.ExecuteEffect(battleManager, report);
        }
    }
}
