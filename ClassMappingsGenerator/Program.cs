// See https://aka.ms/new-console-template for more information
using vsc.ClassMappingsGenerator.Attributes;

Console.WriteLine("Hello, World!");

class Test1
{
    public void Method1()
    {
        Console.WriteLine("Method1 called");
    }
}

[Map(typeof(Test1))]
class ClassWhithMapAttribute
{
}
