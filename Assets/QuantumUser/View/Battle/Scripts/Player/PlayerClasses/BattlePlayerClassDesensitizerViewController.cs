using UnityEngine;
using Quantum;
using Photon.Deterministic;
using Battle.QSimulation;
using Battle.View.Game;

namespace Battle.View.Player
{
    
    public class BattlePlayerClassDesensitizerViewController : BattlePlayerClassBaseViewController
    {
        [SerializeField] private GameObject _aimIndicator;
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Desensitizer;

        public override void OnUpdateView()
        {
            
        }

        protected override void OnViewInitOverride(BattlePlayerSlot slot, int characterId)
        {
            QuantumEvent.Subscribe<EventBattlePlayerClassDesensitizerAimIndicatorUpdate>(_parent, QEventOnAimIndicatorUpdate);
        }

        private void QEventOnAimIndicatorUpdate(EventBattlePlayerClassDesensitizerAimIndicatorUpdate e)
        {
            if (e.ERef != _entityRef) return;
            if (e.Slot != BattleGameViewController.LocalPlayerSlot) return;
            if (!e.Show)
            {
                _aimIndicator.SetActive(false);
                return;
            }
            // Set indicator active
            _aimIndicator.SetActive(true);
            // Rotate indicator in degrees
            float angleDeg = (float)(FPVector2.RadiansSigned(FPVector2.Up, e.Direction) * -FP.Rad2Deg);
            if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta) angleDeg += 180;
            // Indicator's new rotation in degrees.
            _aimIndicator.transform.rotation = Quaternion.Euler(0, angleDeg, 0);
            BattleDebugLogger.WarningFormat(nameof(BattlePlayerClassDesensitizerViewController), "Angle ( state: {0})", angleDeg);
        }

        
    }
}
