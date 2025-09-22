using Quantum;
using UnityEngine;

namespace Battle.View.Player
{
    public abstract class BattlePlayerClassBaseViewController : MonoBehaviour
    {
        public abstract BattlePlayerCharacterClass Class { get; }

        public void OnViewInit(BattlePlayerViewController parent, EntityRef entityRef, BattlePlayerSlot slot, int characterId)
        {
            _parent = parent;
            _entityRef = entityRef;
            OnViewInitOverride(slot, characterId);
        }

        public virtual void OnCharacterTakeDamage(EventBattleCharacterTakeDamage e) { }
        public virtual void OnShieldTakeDamage(EventBattleShieldTakeDamage e) { }
        public virtual void OnUpdateView() { }

        protected BattlePlayerViewController _parent;
        protected EntityRef _entityRef;

        protected virtual void OnViewInitOverride(BattlePlayerSlot slot, int characterId) { }
    }
}
