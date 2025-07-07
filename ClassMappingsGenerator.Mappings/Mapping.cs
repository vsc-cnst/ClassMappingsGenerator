using System.Reflection;
using vsc.ClassMappingsGenerator.Attributes;

namespace vsc.ClassMappingsGenerator.Mappings
{

    public class ClassMapping : IEqualityComparer<ClassMapping>
    {
        private int HashCode { get; }
        public Type TypeA { get; }
        public Type TypeB { get; }

        public string? OutputNamespaceName { get; }
        public string? OutputStaticClassName { get; }
        public string? OutputRegion { get; }
        public virtual Type? ToTypeAArgsType { get; }
        public virtual Type? ToTypeBArgsType { get; }

        public ClassMapping(Type typeA, Type typeB, string? namespaceName = null,
            string? outputStaticClassName = null, string? outputRegion = null,
            Type? toTypeAArgsType = null, Type? toTypeBArgsType = null)
        {
            // apply a sort to make pairs of same type to allways be in the same order
            var arr = new Type[] { typeA, typeB }.OrderBy(t => $"{t.Assembly.FullName}.{t.FullName}").ToArray();
            this.TypeA = arr[0];
            this.TypeB = arr[1];
            this.HashCode = $"{typeA.Assembly.FullName}.{typeA.FullName}.{typeB.Assembly.FullName}.{typeB.FullName}".GetHashCode();

            this.OutputNamespaceName = namespaceName;
            this.OutputRegion = outputRegion;
            this.OutputStaticClassName = outputStaticClassName;

            this.ToTypeAArgsType = toTypeAArgsType;
            this.ToTypeBArgsType = toTypeBArgsType;
        }

        public ClassMapping(Type typeA, MapAttribute attr) : this(typeA, attr.MappedClassType,
            attr.OutputNamespaceName, attr.OutputStaticClassName, attr.OutputRegionName,
            attr.ToLocalClassArgsType, attr.ToMappedClassArgsType
        )
        {
        }

        public bool Equals(ClassMapping? x, ClassMapping? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.TypeA.Equals(y.TypeA) && x.TypeB.Equals(y.TypeB);
        }

        public int GetHashCode(ClassMapping pair) => HashCode;

        public static IEnumerable<ClassMapping> CreateMappings(IEnumerable<Type> types) {

            return types
                .SelectMany(t => t.GetCustomAttributes(typeof(MapAttribute)), (type, attr) => new ClassMapping(type, attr));

        }
    }

    public class MemberMapping
    {
        public MemberMapping(MemberInfo sourceMemberInfo, MemberInfo targetMemberInfo) { 
        }
    }

}
