# Ws2.Formats.Cbor

`Ws2.Formats.Cbor` generates CBOR serialization methods for explicitly attributed C# models. It writes source at compile
time; serialization does not inspect model members with reflection.

## Install

Install both packages at the same version:

```xml
<PackageReference Include="Ws2.Formats.Cbor.Abstractions" Version="2.0.0" />
<PackageReference Include="Ws2.Formats.Cbor" Version="2.0.0" PrivateAssets="all" />
```

The abstractions package supplies attributes, `ICborConverter<T>`, and the `System.Formats.Cbor` dependency. The
generator package installs the compiler analyzer. Keep the analyzer package private to the project unless you intend it
to run for projects that consume your package.

## Define models

Models are top-level, non-generic, non-abstract partial classes or record classes. Mark each model as a CBOR map or
array, and mark every serialized property with a matching key. Models can be reconstructed in either of two ways: one
parameterized constructor whose parameters bind unambiguously to every selected property, or an accessible parameterless
constructor and public `set`/`init` accessors on every selected property. Parameter names are matched without regard to
case and parameter types, including nullable annotations, must match. The parameterized constructor path takes priority
when exactly one matching constructor exists. On the parameterless path, every `required` member must be selected and
initialized by its property key.

```csharp
using Ws2.Formats.Cbor;

[CborMap]
public partial class User
{
    [CborKey("id")]
    public int Id { get; }

    [CborKey("displayName")]
    public string? DisplayName { get; }

    [CborKey("scores")]
    public IReadOnlyList<int> Scores { get; }

    public User(int id, string? displayName, IReadOnlyList<int> scores)
    {
        Id = id;
        DisplayName = displayName;
        Scores = scores;
    }
}
```

DTOs with mutable or init-only properties can use the parameterless path:

```csharp
[CborMap]
public partial class UserDto
{
    [CborKey("id")] public int Id { get; set; }
    [CborKey("displayName")] public string? DisplayName { get; init; }

    public UserDto() { }
}
```

The generated parser reads each keyed value first, then assigns the values in an object initializer. Selected properties
without a public setter are not assignable through this path.

For maps, keys are text strings. For arrays, use contiguous zero-based indexes:

```csharp
[CborArray]
public partial class Point
{
    [CborKey(0)] public int X { get; }
    [CborKey(1)] public int Y { get; }

    public Point(int x, int y) { X = x; Y = y; }
}
```

The generated API provides an instance `Serialize(CborWriter)` method and a static `Parse(CborReader)` method. Nullable
properties are required fields that may contain CBOR null; nullability does not mean a map key can be omitted. Arrays
and `List<T>`/`IReadOnlyList<T>` properties are supported for supported element types. Nested generated models can be
selected properties too.

## External converters

Use a converter for a type outside the built-in set. A converter must be a concrete, closed, accessible class with an
accessible parameterless constructor and implement `ICborConverter<T>` for the property's exact type.
If the converter declares or inherits required members, mark that constructor with `[SetsRequiredMembers]`.

```csharp
public sealed class VersionConverter : ICborConverter<Version>
{
    public void Write(CborWriter writer, Version value) => writer.WriteTextString(value.ToString());
    public Version Read(CborReader reader) => Version.Parse(reader.ReadTextString());
}

[CborKey("version"), CborConverter(typeof(VersionConverter))]
public Version Version { get; }
```

The generated model caches one converter instance per property, so converter methods must be safe for concurrent calls.
The generator handles nullable values and calls the converter only for non-null values. A converter must read or write
exactly one CBOR value for its property. Converter implementations own their input size and nesting-depth behavior.

## Supported property types

| Type                                                               | CBOR value                                  |
|--------------------------------------------------------------------|---------------------------------------------|
| `bool`                                                             | Boolean                                     |
| `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong` | Integer, range checked on read              |
| `float`, `double`                                                  | Floating point                              |
| `string`                                                           | Text string                                 |
| `byte[]`                                                           | Byte string                                 |
| Enum                                                               | Integer using the enum's underlying type    |
| Another generated model                                            | Nested model value                          |
| `T?` / nullable reference                                          | CBOR null or the underlying value           |
| `T[]`, `List<T>`, `IReadOnlyList<T>`                               | Definite-length array of supported elements |
| `byte[][]`                                                         | Array of byte strings                       |
| Any type with a valid property converter                           | One value defined by the converter          |

Nested collections other than `byte[][]` are unsupported. Collection matching is limited to the listed array and generic
interface/class definitions; arbitrary enumerable types do not opt into serialization.

## Wire format and schema changes

Treat map names and array indexes as persistent wire identifiers. Selected fields are required, including nullable
fields. Readers skip unknown map keys and reject duplicate keys, including duplicate unknown keys. Arrays must have
exactly the schema's item count. Generated maps are written in a stable order based on encoded text-key length and then
UTF-8 bytes; this does not promise canonical encoding for arbitrary CBOR documents.

Adding a map field is compatible only when old and new readers/writers are coordinated, because generated readers
require every selected field. Renaming a map key, changing a property's type, or changing an array index changes the
wire schema. Adding an array item changes the required array length. Preserve old keys and indexes or introduce an
explicit versioning strategy when data must remain readable across schema versions.

## Reader and writer ownership

The caller creates, configures, and owns `CborReader` and `CborWriter`. Generated methods do not change their options or
modes. `Serialize` writes exactly one CBOR value; `Parse` reads exactly one. The caller decides whether to check for
trailing input after parsing, for example by checking `reader.BytesRemaining`.

The generator writes definite-length containers. Reads require definite-length arrays; map reading accepts the modes
supported by the configured `CborReader`. Generated collection reads append items as they are read and add no count
limit. The input owner is responsible for suitable byte and depth limits. Object reference cycles are not detected.

## Diagnostics

| ID          | Meaning and fix                                                                                                                                                                                                                                                                                                                                  |
|-------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `SBCBOR001` | Model declaration is not partial; add the `partial` modifier to each declaration.                                                                                                                                                                                                                                                                |
| `SBCBOR002` | Selected property type is unsupported; use a supported type or add a valid converter.                                                                                                                                                                                                                                                            |
| `SBCBOR003` | Model has both `[CborArray]` and `[CborMap]`; choose one wire format.                                                                                                                                                                                                                                                                            |
| `SBCBOR004` | Converter declaration is invalid; implement `ICborConverter<T>` for the exact property type and provide an accessible parameterless constructor.                                                                                                                                                                                                 |
| `SBCBOR005` | Property key is invalid or duplicated; use a unique string key for maps or unique contiguous zero-based indexes for arrays.                                                                                                                                                                                                                      |
| `SBCBOR006` | CBOR attributes/converter API are missing; reference `Ws2.Formats.Cbor.Abstractions`.                                                                                                                                                                                                                                                            |
| `SBCBOR007` | `System.Formats.Cbor` is missing; reference the runtime package and use its reader/writer types.                                                                                                                                                                                                                                                 |
| `SBCBOR008` | Model declaration shape is unsupported; use a top-level, non-generic, non-abstract, non-static class or record class.                                                                                                                                                                                                                            |
| `SBCBOR009` | Selected member or converter is missing a valid key; add the appropriate `[CborKey]` or remove the orphan converter attribute.                                                                                                                                                                                                                   |
| `SBCBOR010` | No supported construction path is available; provide one matching parameterized constructor or an accessible parameterless constructor with public setters for all selected properties. The parameterless path must select every `required` member; the parameterized path must mark its constructor with `[SetsRequiredMembers]` when required. |
| `SBCBOR011` | Generated method conflicts with a model member; rename the member that conflicts with `Serialize` or `Parse`.                                                                                                                                                                                                                                    |

The diagnostic is reported at the offending model, member, or converter. The compilation will not receive generated
source for an invalid model.
