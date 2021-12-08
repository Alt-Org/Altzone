using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class UnityExtensions
{
    #region GameObjects and Components

    public static T GetOrAddComponent<T>(this GameObject parent) where T : Component
    {
        var component = parent.GetComponent<T>();
        if (component == null)
        {
            component = parent.AddComponent<T>();
        }
        return component;
    }

    public static T CreateGameObjectAndComponent<T>(string name, bool isDontDestroyOnLoad) where T : Component
    {
        var parent = GameObject.Find(name);
        if (parent == null)
        {
            parent = new GameObject(name);
            if (isDontDestroyOnLoad)
            {
                Object.DontDestroyOnLoad(parent);
            }
            return parent.AddComponent<T>();
        }
        return parent.GetComponent<T>();
    }

    #endregion

    #region Button

    public static void SetCaption(this Button button, string caption)
    {
        var text = button.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = caption;
            return;
        }
        var tmpText = button.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = caption;
            return;
        }
        Assert.IsTrue(false, "button does not have a text component");
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Execute an action as coroutine on next frame.
    /// </summary>
    public static void ExecuteOnNextFrame(this MonoBehaviour component, Action action)
    {
        ExecuteAsCoroutine(component, null, action);
    }

    /// <summary>
    /// Execute an action as coroutine with delay.
    /// </summary>
    public static void ExecuteAsCoroutine(this MonoBehaviour component, YieldInstruction wait, Action action)
    {
        IEnumerator DelayedExecute(YieldInstruction localWait, Action localAction)
        {
            yield return localWait;
            localAction();
        }

        component.StartCoroutine(DelayedExecute(wait, action));
    }

    #endregion

    #region Rect

    public static Rect Inflate(this Rect rect, Vector2 size)
    {
        return new Rect
        {
            xMin = rect.xMin - size.x,
            yMin = rect.yMin - size.y,
            xMax = rect.xMax + size.x,
            yMax = rect.yMax + size.y
        };
    }

    public static Rect Inflate(this Rect rect, float left, float top, float right, float bottom)
    {
        return new Rect
        {
            xMin = rect.xMin - left,
            yMin = rect.yMin - top,
            xMax = rect.xMax + right,
            yMax = rect.yMax + bottom
        };
    }

    #endregion

    #region Debugging

    public static string GetFullPath(this Transform transform)
    {
        return transform == null ? string.Empty : GetFullPath(transform.gameObject);
    }

    public static string GetFullPath(this Component component)
    {
        return component == null ? string.Empty : GetFullPath(component.gameObject);
    }

    public static string GetFullPath(this GameObject gameObject)
    {
        if (gameObject == null)
        {
            return string.Empty;
        }
        var path = new StringBuilder("\\").Append(gameObject.name);
        while (gameObject.transform.parent != null)
        {
            gameObject = gameObject.transform.parent.gameObject;
            path.Insert(0, gameObject.name).Insert(0, '\\');
        }
        return path.ToString();
    }

    #endregion
}