using UnityEngine;
#if USE_LOOTLOCKER
using LootLocker.Requests;
#endif

public class GameManager : MonoBehaviour
{
#if USE_LOOTLOCKER
    void Start()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (!response.success)
            {
                Debug.Log("error starting LootLocker session");

                return;
            }

            Debug.Log("successfully started LootLocker session");
        });
    }
#endif
}