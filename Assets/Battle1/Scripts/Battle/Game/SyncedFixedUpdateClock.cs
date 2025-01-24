using System;
using System.Collections;
using Altzone.Scripts.Config;
/*using Battle1.PhotonUnityNetworking.Code;*/
using Battle1.Scripts.Battle.Players;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/
#if DEVELOPMENT_BUILD
using TMPro;
#endif

namespace Battle1.Scripts.Battle.Game
{
    #region Message Classes
    public class SyncedFixedUpdateClockStarted
    { }
    #endregion Message Classes

    public class SyncedFixedUpdateClock : MonoBehaviour
    {
        // Serialized Fields
        [SerializeField] private bool _offlineMode;

        // Public Constants
        public const int UpdatesPerSecond = 50; // this variable needs to be set to the number of times FixedUpdate is called per second

        // Public Properties
        public bool Synced => _synced;
        public int UpdateCount => _updateCount;

        #region Public Methods

        public int ToUpdates(int seconds)
        {
            return seconds * UpdatesPerSecond;
        }

        public int ToUpdates(float seconds)
        {
            return (int)Mathf.Ceil(seconds * UpdatesPerSecond);
        }

        public int ToUpdates(double seconds)
        {
            return (int)Math.Ceiling(seconds * UpdatesPerSecond);
        }

        public double ToSeconds(int updates)
        {
            return updates / (double)UpdatesPerSecond;
        }

        public void ExecuteOnUpdate(int updateNumber, int priority, Action action)
        {
            UpdateQueue.Add(new UpdateQueue.Event(updateNumber, priority, action));
        }

        #endregion Public Methods

        // State
        private bool _synced = false;
        private int _updateCount = 0;

        private static class UpdateQueue
        {
            /*
         * (front = index 0)
         * (back  = last index)
         *
         * s_eventArray is filled back to front starting at last index in highest to lowest UpdateNumber order
         * Events with the same UpdateNumber are ordered in lowest to highest Priority order
         *
         * s_eventArrayFirst stores the first used index
         * s_eventArraySize stores the size of s_eventArray
         *
         * new Events are added to front at s_eventArrayFirst - 1
         * and moved back until they are in the right place
         * if new Event needs to be added while events are being executed then it's added to s_eventTempArray
         * (this should only happens when Events add new Events (or at least i think so))
         *
         * s_eventTempArray is filled front to back starting at index 0 in the order the Events are added
         * s_eventTempArrayCapacity stores the size of s_eventTempArray including unused indices
         * s_eventTempArraySize stores the number of elements in s_eventTempArray not including unused indices
         *
         * Events in s_eventArray are executed front to back starting at s_eventArrayFirst
         * after Events are executed Events in s_eventTempArray are added to s_eventArray and executing is repeated in case there is new any Events to be executed
         *
         * ("executing is repeated in case there is any new Events to be executed" is done to handle events that are scheduled for the current update and added while events are being executed
         *  but this means that those Events are executed after all the other events regardless of Priority
         *
         *  scheduling Events for the current update should be fine (or at least i think so)
         *  but doing it in an another Event will guarantee the weird execution order
         *
         *  scheduling Events for later update in an another Event will be fine)
         *
         *       /------s_eventArray-------\
         *       +-------------------------+
         *       |                         | <- index 0
         *       +-------------------------+
         *       |                         |
         * ++ || +-------------------------+
         * /\ || |UpdateNumber 1 Priority 3| <- s_eventArrayFirst
         * || || +-------------------------+
         * || || |UpdateNumber 1 Priority 1|
         * || || +-------------------------+
         * || \/ |UpdateNumber 2 Priority 2| <- last index
         * || -- +-------------------------+
         */

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

            #region Update Queue - Public Static Methods

            public static void init()
            {
                s_eventArrayFirst = s_eventArraySize;
                s_eventTempArraySize = 0;
            }

            public static void Add(Event @event)
            {
                if (s_executing)
                {
                    AddToTemp(@event);
                    return;
                }

                int i; // initialize i so we can use it later to place the event to the correct position in the array (initializing this later causes problems with the array largenings code)

                // allocate bigger array if full
                if (s_eventArrayFirst == 0)
                {
                    int newSize = s_eventArraySize * 2;
                    Event[] newEventArray = new Event[newSize];
                    int newI = newSize;
                    for (i = s_eventArraySize - 1; i >= s_eventArrayFirst; i--)
                    {
                        newI--;
                        newEventArray[newI] = s_eventArray[i];
                    }
                    s_eventArray = newEventArray;
                    s_eventArraySize = newSize;
                    s_eventArrayFirst = newI;
                }

                // initially select the position in front of the first element in array
                s_eventArrayFirst--;
                // compare the new event to preexisting events in array to find the correct position for the new event by moving the events that trigger earlier forward
                Event nextEvent;
                for (i = s_eventArrayFirst; i < s_eventArraySize - 1; i++)
                {
                    nextEvent = s_eventArray[i + 1];
                    //if (@event.UpdateNumber <= nextEvent.UpdateNumber && (@event.Priority >= nextEvent.Priority || @event.UpdateNumber < nextEvent.UpdateNumber)) break;
                    if (@event.UpdateNumber > nextEvent.UpdateNumber || (@event.UpdateNumber == nextEvent.UpdateNumber && @event.Priority <= nextEvent.Priority))
                        s_eventArray[i] = nextEvent;
                    else break;
                }
                // place the event into selected position
                s_eventArray[i] = @event;
            }

            public static void Execute(int updateNumber)
            {
                int i;
                Event @event;
                while (true)
                {
                    // execute this updates events
                    s_executing = true;
                    i = s_eventArrayFirst;
                    while (i < s_eventArraySize)
                    {
                        @event = s_eventArray[i];
                        if (@event.UpdateNumber > updateNumber) break;
                        @event.Action();
                        i++;
                    }
                    // move s_eventArrayFirst to last executed event + 1
                    s_eventArrayFirst = i;
                    // no need to delete executed events because they will be overwritten anyway

                    s_executing = false;

                    // check for new events
                    if (s_eventTempArraySize == 0) break;

                    // add new events to queue
                    for (i = 0; i < s_eventTempArraySize; i++)
                    {
                        Add(s_eventTempArray[i]);
                    }
                    s_eventTempArraySize = 0;

                    // repeat in case any of the new events need to be executed in this update
                }
            }

            #endregion Update Queue - Public Static Methods

            // State
            private static bool s_executing = false;

            // Event Array
            private static int s_eventArraySize = 10;
            private static int s_eventArrayFirst;
            private static Event[] s_eventArray = new Event[s_eventArraySize];

            // Event Temp Array
            private static int s_eventTempArrayCapacity = 5;
            private static int s_eventTempArraySize;
            private static Event[] s_eventTempArray = new Event[s_eventTempArrayCapacity];

            #region Update Queue - Private Static Methods
            private static void AddToTemp(Event @event)
            {
                // allocate bigger array if full
                if (s_eventTempArraySize == s_eventTempArrayCapacity)
                {
                    int newEventTempArrayCapacity = s_eventTempArrayCapacity * 2;
                    Event[] newEventTempArray = new Event[newEventTempArrayCapacity];
                    for (int i = 0; i < s_eventTempArrayCapacity; i++)
                    {
                        newEventTempArray[i] = s_eventTempArray[i];
                    }
                    s_eventTempArray = newEventTempArray;
                    s_eventTempArrayCapacity = newEventTempArrayCapacity;
                }

                // add event
                s_eventTempArray[s_eventTempArraySize] = @event;
                s_eventTempArraySize++;
            }
            #endregion Update Queue - Private Static Methods
        }

        // Components
        /*private PhotonView _photonView;*/

        #region DEBUG
#if DEVELOPMENT_BUILD
    private TextMeshPro _textMeshPro;
#endif
        #endregion DEBUG

        private void Start()
        {
            #region DEBUG
#if DEVELOPMENT_BUILD
        transform.GetChild(0).gameObject.SetActive(true);
        _textMeshPro = GetComponentInChildren<TextMeshPro>();
#endif
            #endregion DEBUG

            UpdateQueue.init();

            // get components
            /*_photonView = GetComponent<PhotonView>();*/

            // subscribe to messages
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);
        }

        #region Message Listeners
        private void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            if (_offlineMode)
            {
                StartClock();
                return;
            }
/*
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(StartClockRpc), RpcTarget.All, PhotonNetwork.Time + GameConfig.Get().Variables._networkDelay);
            }*/
        }
        #endregion Message Listeners

        #region Clock Startup

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

        #endregion Clock Startup

        private void FixedUpdate()
        {
            if (!_synced) return;

            #region DEBUG
#if DEVELOPMENT_BUILD
        _textMeshPro.text = "Update\n" + _updateCount.ToString();
#endif
            #endregion DEBUG

            UpdateQueue.Execute(_updateCount);
            _updateCount++;
        }

        #region Photon RPC - Clock Startup
       /* [PunRPC]
        private void StartClockRpc(double startTimeSec)
        {
            StartCoroutine(StartClockDelayed((float)Math.Max(startTimeSec - PhotonNetwork.Time, 0.0)));
        }*/
        #endregion Photon RPC - Clock Startup
    }
}
