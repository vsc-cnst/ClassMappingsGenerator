using System.Runtime.InteropServices;
using vsc.ClassMappingsGenerator.Attributes;


#pragma warning disable AttrNotImplementedYet

namespace ClassMappingsGenerator.Tests.MapAttributeTests
{
    public class ClassToMapWith
    {
    }


    [TestClass]
    public sealed class TestMapAttribute
    {
        [TestMethod]
        public void TestDefaults()
        {
            var arr = new MapAttribute[] {
                new MapAttribute(typeof(ClassToMapWith)),
                new MapAttribute<ClassToMapWith>(),
                new MapFromAttribute<ClassToMapWith>(),
                new MapToAttribute<ClassToMapWith>(),
            };

            foreach (var attr in arr)
            { 
                var msg = $"{((Type)attr.TypeId).Name} has the wrong default for prop ";
                
                Assert.IsTrue(attr.MapLocalClassDescendants == false, msg  + nameof(attr.MapLocalClassDescendants));
                Assert.IsTrue(attr.MapMappedClasssDescendants == false, msg + nameof(attr.MapMappedClasssDescendants));
                Assert.IsTrue(attr.OutputNamespaceName == null, msg + nameof(attr.OutputNamespaceName));
                Assert.IsTrue(attr.OutputStaticClassName == null, msg + nameof(attr.OutputStaticClassName));
                Assert.IsTrue(attr.OutputRegionName == null, msg + nameof(attr.OutputRegionName));
                Assert.IsTrue(attr.ToLocalClassArgsType == typeof(object), msg + nameof(attr.ToLocalClassArgsType));
                Assert.IsTrue(attr.ToMapMappedArgsType == typeof(object), msg + nameof(attr.ToMapMappedArgsType));
            }

        }

        //[TestMethod]
        //public void TestDefaults()
        //{
        //    var attrMap = typeof(ClassWhithMapAttribute).GetCustomAttributes(typeof(MapAttribute), false).Single();
        //    var attrMapFrom = typeof(ClassWhithMapFromAttribute).GetCustomAttributes(typeof(MapFromAttribute<>), false).Single();
        //    var attrMapTo = typeof(ClassWhithMapToAttribute).GetCustomAttributes(typeof(MapToAttribute<>), false).Single();
            
        //    Assert.IsTrue(attrMap is MapAttribute<ClassToMapWith>);
        //    Assert.IsTrue(attrMapFrom is MapFromAttribute<ClassToMapWith>);
        //    Assert.IsTrue(attrMapTo is MapToAttribute<ClassToMapWith>);
        //}
    }
}
