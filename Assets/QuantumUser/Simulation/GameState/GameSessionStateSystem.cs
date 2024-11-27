using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    /**
     *  Systems that monitor game state:
     *  -ProjectileSpawnerSystem
     */
    [Preserve]
    public unsafe class GameSessionStateSystem : SystemMainThreadFilter<GameSessionStateSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public GameSession* GameSession;
        }

        public override void OnInit(Frame f)
        {
            Log.Debug("Quantum GameSessionStateSystem OnInit");
        }

        public override void Update(Frame f, ref Filter filter)
        {
            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();
            if(gameSession == null)
                return;

            //Countdown state handling
            if (gameSession->CountDownStarted)
            {
                gameSession->TimeUntilStart = gameSession->TimeUntilStart - f.DeltaTime;

            }

            if(gameSession->TimeUntilStart < 1 && gameSession->state == GameState.Countdown)
            {
                gameSession->state = GameState.Playing;

            }
        }
    }
}
