using System;

namespace Altzone.Scripts.Model.Poco.Attributes
{
    // See https://github.com/nhibernate/NHibernate.Mapping.Attributes

    /// <summary>
    /// Extension methods for <c>string</c> fields used in assertions.
    /// </summary>
    public static class Precondition
    {
        public static bool IsPrimaryKey(this string key) => !string.IsNullOrWhiteSpace(key);
        public static bool IsMandatory(this string field) => !string.IsNullOrWhiteSpace(field);

        /// <summary>
        /// Check that field is null or empty or non white space.
        /// </summary>
        public static bool IsNullOEmptyOrNonWhiteSpace(this string field) => string.IsNullOrEmpty(field) || !string.IsNullOrWhiteSpace(field);
    }

    /// <summary>
    /// ForeignKey to mark field or property as foreign key.<br />
    /// <b>Note</b> that foreign key can be either mandatory or optional!
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        public readonly string ReferencedClass;

        public ForeignKeyAttribute(string referencedClass) => ReferencedClass = referencedClass;
    }

    /// <summary>
    /// Mandatory to mark field or property as mandatory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MandatoryAttribute : Attribute
    {
    }

    /// <summary>
    /// MongoDbEntity to mark class as database ORM entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MongoDbEntityAttribute : Attribute
    {
        public readonly string DocumentName;

        public MongoDbEntityAttribute() => DocumentName = string.Empty;

        public MongoDbEntityAttribute(string documentName) => DocumentName = documentName;
    }

    /// <summary>
    /// Unique to mark field or property as unique (implicit mandatory).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UniqueAttribute : Attribute
    {
    }

    /// <summary>
    /// Optional to mark field or property as optional.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OptionalAttribute : Attribute
    {
    }

    /// <summary>
    /// PrimaryKey to mark field or property as database primary key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
    }
}