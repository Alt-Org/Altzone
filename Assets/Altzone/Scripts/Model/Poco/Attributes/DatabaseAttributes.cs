using System;

namespace Altzone.Scripts.Model.Poco.Attributes
{
    // See https://github.com/nhibernate/NHibernate.Mapping.Attributes

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