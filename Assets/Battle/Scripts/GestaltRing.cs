using System;
using Altzone.Scripts.Model;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Battle.Scripts
{
    /// <summary>
    /// <c>GestaltRing</c> class manages global Gestalt <c>Defence</c> state.
    /// </summary>
    /// <remarks>
    /// When <c>Defence</c> state changes, a notification is sent through network to all players.
    /// </remarks>
    public class GestaltRing : MonoBehaviour
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase - 1;

        public static GestaltRing Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<GestaltRing>();
            }
            return _Instance;
        }

        private static GestaltRing _Instance;

        private static readonly Defence[] nextDefence =
        {
            Defence.None,
            Defence.Deflection,
            Defence.Introjection,
            Defence.Projection,
            Defence.Retroflection,
            Defence.Egotism,
            Defence.Confluence,
            Defence.Desensitisation,
        };

        [SerializeField] private Defence curDefence;
        private PhotonEventDispatcher photonEventDispatcher;

        public Defence Defence
        {
            get => curDefence;
            set
            {
                if (!PhotonWrapper.IsMasterClient)
                {
                    throw new UnityException($"Only Master Client can change {nameof(GestaltRing)} {nameof(Defence)} state");
                }
                byte payload;
                if (value == Defence.None)
                {
                    payload = (byte)nextDefence[(int)curDefence];
                }
                else
                {
                    payload = (byte)value;
                }
                Debug.Log($"set Defence {(byte)curDefence} <- {payload}");
                photonEventDispatcher.RaiseEvent(photonEventCode, payload);
            }
        }

        private void Awake()
        {
            if (_Instance == null)
            {
                _Instance = this;
            }
        }

        private void Start()
        {
            // Start with some random Defence so that we have a valid state.
            curDefence = nextDefence[Random.Range(1, (int)Defence.Confluence)];
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, data =>
            {
                var newDefence = (Defence)Enum.ToObject(typeof(Defence), data.CustomData);
                Debug.Log($"set Defence {curDefence} <- {newDefence}");
                curDefence = newDefence;
                this.Publish(new DefenceEvent(curDefence));
            });
        }

        private void OnEnable()
        {
            this.Subscribe<DefenceEvent>(onDefenceEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnDestroy()
        {
            _Instance = null;
        }

        private static void onDefenceEvent(DefenceEvent data)
        {
            // We just monitor the event!
            Debug.Log($"changed Defence {data.Defence}");
        }

        public class DefenceEvent
        {
            public readonly Defence Defence;

            public DefenceEvent(Defence defence)
            {
                Defence = defence;
            }
        }
    }
}