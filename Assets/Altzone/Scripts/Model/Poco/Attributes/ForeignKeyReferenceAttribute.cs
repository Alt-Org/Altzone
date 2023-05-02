using System;

namespace Altzone.Scripts.Model.Poco.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ForeignKeyReferenceAttribute : Attribute
    {
        public readonly string ReferencedClass;

        public ForeignKeyReferenceAttribute(string referencedClass) => ReferencedClass = referencedClass;
    }
}