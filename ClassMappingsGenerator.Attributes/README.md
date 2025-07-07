# Mapping Attributes
A set of attributes to mark classes and properties that can be mapped between each other.

These attributes enable compile-time code generation, such as generating code to copy properties between classes, without using reflection. This helps reduce runtime overhead, improve performance, and ensure type safety.

## Benefits
🗸 Copy properties between compatible classes

🗸 Generates code at development/build time

🗸 No reflection

🗸 Lower runtime overhead

🗸 Static type checking — changes in source code are caught at compile time

## Use Case
Copy properties between a class and a DTO (Data Transfer Object), or between any two classes with compatible properties.

## Example usage
```csharp
public partial class Foo {}

// Preferred usage
[Map<Foo>]
public partial class Bar  {}


// Alternative usage
[Map(typeof(Foo)]
public partial class Bar  {}
```