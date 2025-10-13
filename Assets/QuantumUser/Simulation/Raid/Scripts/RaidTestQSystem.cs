using UnityEngine;
using Quantum;
using UnityEngine.Scripting;
using Input = Quantum.Input;

namespace Raid.QSimulation
{
    [Preserve]
    public unsafe class RaidTestQSystem : SystemMainThread, ISignalOnPlayerAdded
    {
        public override void OnInit(Frame f)
        {
            Debug.Log("test");
        }

        public void OnPlayerAdded(Frame f, PlayerRef playerRef, bool firstTime)
        {
            FixedArray<PlayerRef> playerRefs = f.Unsafe.GetPointerSingleton<RaidPlayerLinkQSingleton>()->PlayerRefs;
            for (int i = 0; i < playerRefs.Length; i++)
            {
                if (playerRefs[i] != PlayerRef.None) continue;

                playerRefs[i] = playerRef;
                break;
            }
        }

        public override void Update(Frame f)
        {
            FixedArray<PlayerRef> playerRefs = f.Unsafe.GetPointerSingleton<RaidPlayerLinkQSingleton>()->PlayerRefs;
            for (int i = 0; i < playerRefs.Length; i++)
            {
                PlayerRef playerRef = playerRefs[i];
                if (playerRef == PlayerRef.None) continue;
                Input* input = f.GetPlayerInput(playerRef);
                Debug.LogFormat("{0} Input: {1}", playerRef, input->RaidClickPosition);
            }
        }

    }
}
