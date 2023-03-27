using System;
using System.Reflection;

namespace Editor
{
    /// <summary>
    /// Old and deprecated command line build entry point, now it just calls the new one in an other assembly using reflection.
    /// </summary>
    internal static class TeamCity
    {
        internal static void Build()
        {
            // Fully qualified type and assembly name!
            var type = Type.GetType("Prg.Editor.Build.TeamCity, Prg.Editor");
            // Method access is 'internal static'.
            var method = type?.GetMethod("Build", BindingFlags.NonPublic | BindingFlags.Static);
            method?.Invoke(null, null);
        }
    }
}