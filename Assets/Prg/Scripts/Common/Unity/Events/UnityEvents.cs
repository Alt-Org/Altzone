using System;
using UnityEngine;
using UnityEngine.Events;

namespace Prg.Scripts.Common.Unity.Events
{
    /// <summary>
    /// UNITY events to be visible in Editor
    /// </summary>
    /// <remarks>
    /// See: https://docs.unity3d.com/ScriptReference/Events.UnityEvent_1.html
    /// </remarks>
    ///
    [Serializable]
    public class UnityEventInt : UnityEvent<int>
    {
    }

    [Serializable]
    public class UnityEventInt2 : UnityEvent<int, int>
    {
    }

    [Serializable]
    public class UnityEventVector2 : UnityEvent<Vector2>
    {
    }

    [Serializable]
    public class UnityEventComponent : UnityEvent<Component>
    {
    }
}