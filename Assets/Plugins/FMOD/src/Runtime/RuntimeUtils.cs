using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FMODUnity
{
    public class EventNotFoundException : Exception
    {
        public Guid Guid;
        public string Path;
        public EventNotFoundException(string path)
            : base("[FMOD] Event not found '" + path + "'")
        {
            Path = path;
        }

        public EventNotFoundException(Guid guid)
            : base("[FMOD] Event not found " + guid.ToString("b") + "")
        {
            Guid = guid;
        }
    }

    public class BusNotFoundException : Exception
    {
        public string Path;
        public BusNotFoundException(string path)
            : base("[FMOD] Bus not found '" + path + "'")
        {
            Path = path;
        }
    }

    public class VCANotFoundException : Exception
    {
        public string Path;
        public VCANotFoundException(string path)
            : base("[FMOD] VCA not found '" + path + "'")
        {
            Path = path;
        }
    }

    public class BankLoadException : Exception
    {
        public string Path;
        public FMOD.RESULT Result;

        public BankLoadException(string path, FMOD.RESULT result)
            : base(string.Format("[FMOD] Could not load bank '{0}' : {1} : {2}", path, result.ToString(), FMOD.Error.String(result)))
        {
            Path = path;
            Result = result;
        }
        public BankLoadException(string path, string error)
            : base(string.Format("[FMOD] Could not load bank '{0}' : {1}", path, error))
        {
            Path = path;
            Result = FMOD.RESULT.ERR_INTERNAL;
        }
    }

    public class SystemNotInitializedException : Exception
    {
        public FMOD.RESULT Result;
        public string Location;

        public SystemNotInitializedException(FMOD.RESULT result, string location)
            : base(string.Format("[FMOD] Initialization failed : {2} : {0} : {1}", result.ToString(), FMOD.Error.String(result), location))
        {
            Result = result;
            Location = location;
        }

        public SystemNotInitializedException(Exception inner)
            : base("[FMOD] Initialization failed", inner)
        {
        }
    }

    public enum EmitterGameEvent : int
    {
        None,
        ObjectStart,
        ObjectDestroy,
        TriggerEnter,
        TriggerExit,
        TriggerEnter2D,
        TriggerExit2D,
        CollisionEnter,
        CollisionExit,
        CollisionEnter2D,
        CollisionExit2D,
        ObjectEnable,
        ObjectDisable,
        MouseEnter,
        MouseExit,
        MouseDown,
        MouseUp,
    }

    public enum LoaderGameEvent : int
    {
        None,
        ObjectStart,
        ObjectDestroy,
        TriggerEnter,
        TriggerExit,
        TriggerEnter2D,
        TriggerExit2D,
        ObjectEnable,
        ObjectDisable,
    }

    public static class RuntimeUtils
    {
        public static string GetCommonPlatformPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return path.Replace('\\', '/');
        }

        public static FMOD.VECTOR ToFMODVector(this Vector3 vec)
        {
            FMOD.VECTOR temp;
            temp.x = vec.x;
            temp.y = vec.y;
            temp.z = vec.z;

            return temp;
        }

        public static FMOD.ATTRIBUTES_3D To3DAttributes(this Vector3 pos)
        {
            FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D();
            attributes.forward = ToFMODVector(Vector3.forward);
            attributes.up = ToFMODVector(Vector3.up);
            attributes.position = ToFMODVector(pos);

            return attributes;
        }

        public static FMOD.ATTRIBUTES_3D To3DAttributes(this Transform transform)
        {
            FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D();
            attributes.forward = transform.forward.ToFMODVector();
            attributes.up = transform.up.ToFMODVector();
            attributes.position = transform.position.ToFMODVector();

            return attributes;
        }

        public static FMOD.ATTRIBUTES_3D To3DAttributes(Transform transform, Rigidbody rigidbody = null)
        {
            FMOD.ATTRIBUTES_3D attributes = transform.To3DAttributes();

            if (rigidbody)
            {
                attributes.velocity = rigidbody.velocity.ToFMODVector();
            }

            return attributes;
        }

        public static FMOD.ATTRIBUTES_3D To3DAttributes(GameObject go, Rigidbody rigidbody = null)
        {
            FMOD.ATTRIBUTES_3D attributes = go.transform.To3DAttributes();

            if (rigidbody)
            {
                attributes.velocity = rigidbody.velocity.ToFMODVector();
            }

            return attributes;
        }

        public static FMOD.ATTRIBUTES_3D To3DAttributes(Transform transform, Rigidbody2D rigidbody)
        {
            FMOD.ATTRIBUTES_3D attributes = transform.To3DAttributes();

            if (rigidbody)
            {
                FMOD.VECTOR vel;
                vel.x = rigidbody.velocity.x;
                vel.y = rigidbody.velocity.y;
                vel.z = 0;
                attributes.velocity = vel;
            }

            return attributes;
        }

        public static FMOD.ATTRIBUTES_3D To3DAttributes(GameObject go, Rigidbody2D rigidbody)
        {
            FMOD.ATTRIBUTES_3D attributes = go.transform.To3DAttributes();

            if (rigidbody)
            {
                FMOD.VECTOR vel;
                vel.x = rigidbody.velocity.x;
                vel.y = rigidbody.velocity.y;
                vel.z = 0;
                attributes.velocity = vel;
            }

            return attributes;
        }

        public static void EnforceLibraryOrder()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR

            AndroidJavaClass jSystem = new AndroidJavaClass("java.lang.System");
            jSystem.CallStatic("loadLibrary", FMOD.VERSION.dll);
            jSystem.CallStatic("loadLibrary", FMOD.Studio.STUDIO_VERSION.dll);

            #endif

            // Call a function in fmod.dll to make sure it's loaded before fmodstudio.dll
            int temp1, temp2;
            FMOD.Memory.GetStats(out temp1, out temp2);

            Guid temp3;
            FMOD.Studio.Util.parseID("", out temp3);
        }

#if !UNITY_EDITOR
        public static void SetThreadAffinity(Action<FMOD.RESULT, string> reportResult)
        {
            FMOD.RESULT result = FMOD.Thread.SetAttributes(FMOD.THREAD_TYPE.MIXER, FMOD.THREAD_AFFINITY.CORE_2);
            reportResult(result, "FMOD.Thread.SetAttributes(Mixer)");

            result = FMOD.Thread.SetAttributes(FMOD.THREAD_TYPE.STUDIO_UPDATE, FMOD.THREAD_AFFINITY.CORE_4);
            reportResult(result, "FMOD.Thread.SetAttributes(Update)");

            result = FMOD.Thread.SetAttributes(FMOD.THREAD_TYPE.STUDIO_LOAD_BANK, FMOD.THREAD_AFFINITY.CORE_4);
            reportResult(result, "FMOD.Thread.SetAttributes(Load_Bank)");

            result = FMOD.Thread.SetAttributes(FMOD.THREAD_TYPE.STUDIO_LOAD_SAMPLE, FMOD.THREAD_AFFINITY.CORE_4);
            reportResult(result, "FMOD.Thread.SetAttributes(Load_Sample)");
        }
#endif
    }
}