using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using vsc.ClassMappingsGenerator.Attributes;

namespace vsc.ClassMappingsGenerator.Mappings
{
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// if <b>property</b> then return <b>property type</b> <br/>
        /// if <b>field</b> then return <b>field type</b> <br/>
        /// if <b>method</b> then return <b>return type</b> <br/>
        /// else return null
        /// </summary>
        /// <param name="member"></param>
        /// <returns>
        /// <see cref="Type"/> or <see langword="null"/>
        /// </returns>
        public static Type? GetMemberDataType(this MemberInfo member)
            => member switch
            {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                MethodInfo method => method.ReturnType,
                _ => null
            };

        public static bool MemberIsBasic(this MemberInfo? member)
            => member != null && (
                    (member is FieldInfo f && f.FieldType.IsBasicType())
                    || (member is PropertyInfo p && p.PropertyType.IsBasicType())
                );

        public static bool MemberIsClass(this MemberInfo? member)
            => member != null
                && (
                        (member is FieldInfo f && f.FieldType.IsClass)
                        || (member is PropertyInfo p && p.PropertyType.IsClass)
                    )
                && !member.MemberIsIEnumerable();

        public static bool MemberIsList(this MemberInfo? member)
            => member != null && (
                    (member is FieldInfo f && f.FieldType.IsList())
                    || (member is PropertyInfo p && p.PropertyType.IsList())
                );

        public static bool MemberIsICollection(this MemberInfo? member)
            => member != null && (
                    (member is FieldInfo f && f.FieldType.IsICollection())
                    || (member is PropertyInfo p && p.PropertyType.IsICollection())
                );

        public static bool MemberIsIEnumerable(this MemberInfo? member)
            => member != null && (
                    (member is FieldInfo f && f.FieldType.IsIEnumerable())
                    || (member is PropertyInfo p && p.PropertyType.IsIEnumerable())
                );

        public static bool MemberIsMethod(this MemberInfo? member)
            => member != null && (member is MethodInfo);

    }

    public class MemberInfoNameTypeComparer : IEqualityComparer<MemberInfo>
    {
        public bool Equals(MemberInfo? x, MemberInfo? y)
        {
            if (x == null || y == null)
                return false;

            return x.Name == y.Name && x.DeclaringType == y.DeclaringType;
        }

        public int GetHashCode(MemberInfo obj)
        {
            using (MD5 md5 = MD5.Create())
                unchecked
                {
                    byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(obj.Name + obj.DeclaringType?.FullName ?? ""));
                    int positiveHash = Math.Abs(BitConverter.ToInt32(bytes, 0));
                    return (positiveHash == int.MinValue) ? 0 : positiveHash;
                }
        }
    }
}
