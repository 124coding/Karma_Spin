using UnityEngine;

// 개별 아이템 기능
public abstract class ItemEffect : ScriptableObject
{
    public virtual void ExecuteEffect(BaseBattleManager battleManager, DamageReport report) { }
}
