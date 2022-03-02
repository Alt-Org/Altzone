using Battle.Scripts.Battle.PlayerConnect;
using UnityEngine;

namespace Battle.Scripts.Battle.Factory
{
    internal class HelpersCollection : MonoBehaviour
    {
        [SerializeField] private PlayerLineConnector _teamRedLineConnector;
        [SerializeField] private PlayerLineConnector _teamBlueLineConnector;

        public PlayerLineConnector TeamRedLineConnector => _teamRedLineConnector;
        public PlayerLineConnector TeamBlueLineConnector => _teamBlueLineConnector;
    }
}