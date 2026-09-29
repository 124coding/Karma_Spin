using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Conumable")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int cost;
    public Sprite itemIcon;

    [TextArea] public string description;

    [Header("이 아이템이 발동시킬 기능들")]
    public List<ItemEffect> effects = new List<ItemEffect>();

    public void UseItem(BaseBattleManager battleManager, DamageReport report)
    {
        foreach (var effect in effects)
        {
            effect.ExecuteEffect(battleManager, report);
        }
    }
}
