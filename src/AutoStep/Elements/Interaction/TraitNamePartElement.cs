using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoStep.Elements;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Elements.Interaction
{
    public class NameRefElement : PositionalElement, IEquatable<NameRefElement>, IComparable<NameRefElement>
    {
        public string Name { get; set; }

        public int CompareTo(NameRefElement other)
        {
            return string.CompareOrdinal(Name, other.Name);
        }

        public bool Equals(NameRefElement other)
        {
            return other is object &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is NameRefElement otherPart)
            {
                return Equals(otherPart);
            }

            return false;
        }

        public static bool operator ==(NameRefElement left, NameRefElement right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(NameRefElement left, NameRefElement right)
        {
            return !(left == right);
        }
    }
}
