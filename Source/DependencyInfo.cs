using System;

namespace MikhailShilkov.SimpleInjector.Visualization
{
    public class DependencyInfo
    {
        public DependencyInfo(Type parent, Type child, Type via)
        {
            this.Parent = parent;
            this.Child = child;
            this.Via = via;
        }

        public Type Parent { get; }

        public Type Child { get; }

        public Type Via { get; }

        public override bool Equals(object obj)
        {
            var other = obj as DependencyInfo;
            return other != null
                && other.Parent == this.Parent
                && other.Child == this.Child
                && other.Via == this.Via;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.Parent.GetHashCode();
                hash = hash * 23 + this.Child.GetHashCode();
                hash = hash * 23 + this.Via.GetHashCode();
                return hash;
            }
        }
    }
}