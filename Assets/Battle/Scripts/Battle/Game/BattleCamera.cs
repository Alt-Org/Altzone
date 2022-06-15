using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    internal class BattleCamera : MonoBehaviour, IBattleCamera
    {
        public Camera Camera => throw new System.NotImplementedException();

        public bool IsRotated => throw new System.NotImplementedException();

        public void DisableAudio()
        {
            throw new System.NotImplementedException();
        }
    }
}