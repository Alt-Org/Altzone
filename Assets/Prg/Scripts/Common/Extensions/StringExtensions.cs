using System;

namespace Prg.Scripts.Common.Extensions
{
    // See https://github.com/nhibernate/NHibernate.Mapping.Attributes

    /// <summary>
    /// Extension methods for <c>string</c> fields used in assertions.
    /// </summary>
    public static class Precondition
    {
        public static bool IsSet(this string key) => !string.IsNullOrWhiteSpace(key);

        /// <summary>
        /// Check that field is null or empty or non white space.
        /// </summary>
        public static bool IsNullOEmptyOrNonWhiteSpace(this string field) => string.IsNullOrEmpty(field) || !string.IsNullOrWhiteSpace(field);
    }
}
