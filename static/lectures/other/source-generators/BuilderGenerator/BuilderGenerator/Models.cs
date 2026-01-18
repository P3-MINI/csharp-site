using System;
using System.Collections.Generic;
using System.Linq;

namespace BuilderGenerator
{
    /// <summary>
    /// Represents a class to generate a Builder for.
    /// Implements IEquatable to ensure the Incremental Generator cache works correctly.
    /// </summary>
    public class ClassModel : IEquatable<ClassModel>
    {
        public string ClassName { get; }
        public string Namespace { get; }
        public List<PropertyModel> Properties { get; }

        public ClassModel(string className, string @namespace, List<PropertyModel> properties)
        {
            ClassName = className;
            Namespace = @namespace;
            Properties = properties;
        }

        public bool Equals(ClassModel? other)
        {
            if (other is null) return false;
            
            return ClassName == other.ClassName && 
                   Namespace == other.Namespace && 
                   Properties.SequenceEqual(other.Properties); 
        }

        public override bool Equals(object? obj) => Equals(obj as ClassModel);

        public override int GetHashCode()
        {
            var hc = new HashCode();
            hc.Add(ClassName);
            hc.Add(Namespace);
            
            foreach (var p in Properties)
            {
                hc.Add(p);
            }
            
            return hc.ToHashCode();
        }
    }

    /// <summary>
    /// Represents a property of the class.
    /// </summary>
    public class PropertyModel : IEquatable<PropertyModel>
    {
        public string Name { get; }
        public string Type { get; }

        public PropertyModel(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public bool Equals(PropertyModel? other)
        {
            return other != null && Name == other.Name && Type == other.Type;
        }

        public override bool Equals(object? obj) => Equals(obj as PropertyModel);

        public override int GetHashCode() => HashCode.Combine(Name, Type);
    }
}