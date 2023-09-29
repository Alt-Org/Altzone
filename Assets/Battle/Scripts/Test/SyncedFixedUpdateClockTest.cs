using System.Collections;
using Math = System.Math;
using Action = System.Action;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;

public class SyncedFixedUpdateClockStarted
{ }

public class SyncedFixedUpdateClockTest : MonoBehaviour
{
    [SerializeField] private bool OfflineMode;
    public const int UPDATES_PER_SECOND = 50; // this variable needs to be set to the number of times FixedUpdate is called per second
    private bool _synced = false;
    private int _updateCount = 0;

    private static class UpdateQueue
    {
        public class Event
        {
            public int UpdateNumber;
            public int Priority;
            public Action Action;

            public Event(int updateNumber, int priority, Action action)
            {
                UpdateNumber = updateNumber;
                Priority = priority;
                Action = action;
            }
        }

        /*
         * (front = index 0)
         * (back  = last index)
         *  
         * s_eventArray is filled back to front starting at last index in highest to lowest UpdateNumber order
         * Events with the same UpdateNumber are ordered in lowest to highest Priority order
         *
         * s_first stores the first used index
         * s_size stores the size of s_eventArray
         *
         * new Events are added to front at s_first - 1
         * and moved back until they are in the right place
         *
         * Events are executed front to back starting at s_first
         *
         *       +-------------------------+ 
         *       |                         | <- index 0
         *       +-------------------------+
         *       |                         |
         * ++ || +-------------------------+
         * /\ || |UpdateNumber 1 Priority 3| <- s_first
         * || || +-------------------------+
         * || || |UpdateNumber 1 Priority 1|
         * || || +-------------------------+
         * || \/ |UpdateNumber 2 Priority 2| <- last index
         * || -- +-------------------------+
         */

        private static int s_size = 10;
        private static Event[] s_eventArray = new Event[s_size];
        private static int s_first;

        public static void init()
        {
             s_first = s_size;
        }

        public static void Add(Event @event)
        {
            // allocate bigger array if full
            if (s_first == 0)
            {
                int newSize = s_size * 2;
                Event[] newEventArray = new Event[newSize];
                int newI = newSize;
                for (int i = s_size - 1; i >= s_first; i--)
                {
                    newI--;
                    newEventArray[newI] = s_eventArray[i];
                }
                s_eventArray = newEventArray;
                s_size = newSize;
                s_first = newI;
            }

            // add new event in front
            s_first--;
            s_eventArray[s_first] = @event;
            // move back until it's in the right place
            Event nextEvent;
            for (int i = s_first; i < s_size - 1; i++)
            {
                @event = s_eventArray[i];
                nextEvent = s_eventArray[i + 1];
                // this comparison can probably be done better
                if (@event.UpdateNumber <= nextEvent.UpdateNumber && (@event.Priority >= nextEvent.Priority || @event.UpdateNumber < nextEvent.UpdateNumber)) break;
                s_eventArray[i] = nextEvent;
                s_eventArray[i + 1] = @event;
            }
        }

        public static void Execute(int updateNumber)
        {
            // execute this updates events
            int i = s_first;
            Event @event;
            while(i < s_size)
            {
                @event = s_eventArray[i];
                if (@event.UpdateNumber > updateNumber) break;
                @event.Action();
                i++;
            }
            // move s_first to last executed event + 1
            s_first = i;
            // no need to delete executed events because they will be overwritten anyway
        }
    }

    private PhotonView _photonView;

#if DEVELOPMENT_BUILD
    private TextMeshPro _textMeshPro;
#endif

    private void Start()
    {
#if DEVELOPMENT_BUILD
        transform.GetChild(0).gameObject.SetActive(true);
        _textMeshPro = GetComponentInChildren<TextMeshPro>();
#endif

        UpdateQueue.init();

        _photonView = GetComponent<PhotonView>();

        if (OfflineMode)
        {
            StartClock();
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            _photonView.RPC(nameof(StartClockRpc), RpcTarget.All, PhotonNetwork.Time + 1.0);
        }
    }

    public bool Synced => _synced;

    public int UpdateCount => _updateCount;

    public int ToUpdates(int seconds)
    {
        return seconds * UPDATES_PER_SECOND;
    }

    public int ToUpdates(float seconds)
    {
        return (int)Mathf.Ceil(seconds * UPDATES_PER_SECOND);
    }

    public int ToUpdates(double seconds)
    {
        return (int)Math.Ceiling(seconds * UPDATES_PER_SECOND);
    }

    public double ToSeconds(int updates)
    {
        return updates / (double)UPDATES_PER_SECOND;
    }

    public void ExecuteOnUpdate(int updateNumber, int priority, Action action)
    {
        UpdateQueue.Add(new UpdateQueue.Event(updateNumber, priority, action));
    }

    [PunRPC]
    private void StartClockRpc(double startTimeSec)
    {
        StartCoroutine(StartClockDelayed((float)Math.Max(startTimeSec - PhotonNetwork.Time, 0.0)));
    }
    
    private IEnumerator StartClockDelayed(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        StartClock();
    }
    private void StartClock()
    {
        _synced = true;
        this.Publish(new SyncedFixedUpdateClockStarted());
    }
    private void FixedUpdate()
    {
        if (!_synced) return;

#if DEVELOPMENT_BUILD
        _textMeshPro.text = "Update\n" + _updateCount.ToString();
#endif

        UpdateQueue.Execute(_updateCount);
        _updateCount++;
    }
}
