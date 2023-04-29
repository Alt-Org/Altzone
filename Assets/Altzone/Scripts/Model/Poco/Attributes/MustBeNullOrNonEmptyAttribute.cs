using System;

namespace Altzone.Scripts.Model.Poco.Attributes
{
    /// <summary>
    /// Marker <c>Attribute</c> for (<c>string</c>) parameters than <b>must be</b> either <c>null</c> or 'non empty'.
    /// </summary>
    /// <remarks>
    /// See also: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class MustBeNullOrNonEmptyAttribute : Attribute
    {
    }
}