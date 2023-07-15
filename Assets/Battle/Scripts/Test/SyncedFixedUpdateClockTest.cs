using System.Collections;
using Math = System.Math;
using Action = System.Action;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class SyncedFixedUpdateClockTest : MonoBehaviour
{
    public const int UPDATES_PER_SECONDS = 50; // this variable needs to be set to the number of times FixedUpdate is called per second
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
        private static int s_first = s_size;
        private static Event[] s_eventArray = new Event[s_size];

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

    private TextMeshPro _textMeshPro;

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
        _textMeshPro = GetComponentInChildren<TextMeshPro>();

        if (PhotonNetwork.IsMasterClient)
        {
            _photonView.RPC(nameof(StartClockRpc), RpcTarget.All, PhotonNetwork.Time + 1.0);
        }
    }

    public bool Synced => _synced;

    public int UpdateCount => _updateCount;

    public int ToUpdates(int seconds)
    {
        return seconds * UPDATES_PER_SECONDS;
    }

    public int ToUpdates(float seconds)
    {
        return (int)Mathf.Ceil(seconds * UPDATES_PER_SECONDS);
    }

    public int ToUpdates(double seconds)
    {
        return (int)Math.Ceiling(seconds * UPDATES_PER_SECONDS);
    }

    public double ToSeconds(int updates)
    {
        return ((double)updates) / ((double)UPDATES_PER_SECONDS);
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
        _synced = true;
    }

    private void FixedUpdate()
    {
        if (!_synced) return;
        _textMeshPro.text = "Update\n" + _updateCount.ToString();
        UpdateQueue.Execute(_updateCount);
        _updateCount++;
    }
}
