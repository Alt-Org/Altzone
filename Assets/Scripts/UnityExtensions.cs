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
        return component != null ? component : parent.AddComponent<T>();
    }

    public static T CreateGameObjectAndComponent<T>(string name, bool isDontDestroyOnLoad) where T : Component
    {
        var parent = new GameObject(name);
        if (isDontDestroyOnLoad)
        {
            Object.DontDestroyOnLoad(parent);
        }
        return parent.AddComponent<T>();
    }

    #endregion

    #region Transform

    private static readonly Quaternion NormalRotation = Quaternion.Euler(0f, 0f, 0f);
    private static readonly Quaternion UpsideDown = Quaternion.Euler(0f, 0f, 180f);

    public static void Rotate(this Transform transform, bool isUpsideDown)
    {
        var rotation = isUpsideDown ? UpsideDown : NormalRotation;
        transform.rotation = rotation;
    }

    #endregion

    #region Collision2D

    public static ContactPoint2D GetFirstContactPoint(this Collision2D collision)
    {
        if (collision == null || collision.contactCount == 0)
        {
            return new ContactPoint2D();
        }
        var contactPoint = collision.GetContact(0);
        return contactPoint;
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

    public static string GetCaption(this Button button)
    {
        var text = button.GetComponentInChildren<Text>();
        if (text != null)
        {
            return text.text;
        }
        var tmpText = button.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            return tmpText.text;
        }
        Assert.IsTrue(false, "button does not have a text component");
        return null;
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

    public static Rect InflateBlueSide(this Rect rect, Vector2 size, Vector2 size2)
    {
        return new Rect
        {
            xMin = rect.xMin - size.x,
            yMin = rect.yMin - size.y,
            xMax = rect.xMax + size2.x,
            yMax = rect.yMax + size2.y
        };
    }

    public static Rect InflateRedSide(this Rect rect, Vector2 size, Vector2 size2)
    {
        return new Rect
        {
            xMin = rect.xMin - size2.x,
            yMin = rect.yMin - size2.y,
            xMax = rect.xMax + size.x,
            yMax = rect.yMax + size.y
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
