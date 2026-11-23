using System.Formats.Cbor;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Ws2.Formats.Cbor.Tests;

[TestFixture]
public sealed class CborGeneratorTests
{
    [Test]
    public void TestSemanticCollectionCopiesInputAndUsesStructuralEquality()
    {
        var input = new[] { 1, 2 };
        EquatableList<int> first = new(input);
        input[0] = 9;
        EquatableList<int> second = new([1, 2]);

        Assert.That(first[0], Is.EqualTo(1));
        Assert.That(first, Is.EqualTo(second));
        Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        Assert.That(first, Is.Not.InstanceOf<IList<int>>());
    }

    [Test]
    public void TestEmissionModelEqualityTracksKeysTypesAndConstructorOrder()
    {
        SerializableProperty first = new("First", "global::System.Int32", CborValueKind.Int32, null, cborArrayIndex: 0);

        SerializableProperty second = new("Second", "global::System.Int32", CborValueKind.Int32, null, cborArrayIndex: 1);

        var properties = new[] { first, second };
        var original = CreateContext(properties, [0, 1]);
        var equal = CreateContext(properties, [0, 1]);

        Assert.That(original, Is.EqualTo(equal));
        Assert.That(original.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
        Assert.That(
            original.Equals(CreateContext(
                [first, new SerializableProperty("Second", "global::System.String", CborValueKind.String, null, cborArrayIndex: 1)],
                [0, 1])), Is.False);

        Assert.That(
            original.Equals(CreateContext(
                [first, new SerializableProperty("Second", "global::System.Int32", CborValueKind.Int32, null, cborArrayIndex: 2)],
                [0, 1])), Is.False);

        Assert.That(original.Equals(CreateContext(properties, [1, 0])), Is.False);
    }

    private static SerializationContext CreateContext(IEnumerable<SerializableProperty> properties, IEnumerable<int> constructorOrder)
    {
        return new SerializationContext("", "Model", "class", CborSerializationFormat.Array,
            new EquatableList<SerializableProperty>(properties), new EquatableList<int>(constructorOrder),
            new EquatableList<int>([0, 1]));
    }

    [Test]
    public void TestPublicAttributesAreNonRepeatableAndNonInherited()
    {
        foreach (var attributeType in new[]
            {
                typeof(CborArrayAttribute), typeof(CborMapAttribute), typeof(CborKeyAttribute), typeof(CborConverterAttribute)
            })
        {
            var usage =
                (AttributeUsageAttribute)Attribute.GetCustomAttribute(attributeType, typeof(AttributeUsageAttribute))!;

            Assert.That(usage.AllowMultiple, Is.False, attributeType.Name);
            Assert.That(usage.Inherited, Is.False, attributeType.Name);
        }
    }

    [Test]
    public void TestModelsWithSameNameInDifferentNamespacesHaveUniqueOutputAndSafeIdentifiers()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              namespace First
                              {
                                  [CborMap] public partial class @class
                                  {
                                      [CborKey("quote\" slash\\ newline\n tab\t control\u0001")] public int @event { get; }
                                      public @class(int @event) => this.@event = @event;
                                  }
                              }
                              namespace Second
                              {
                                  [CborArray] public partial class @class
                                  {
                                      [CborKey(0)] public int @event { get; }
                                      public @class(int @event) => this.@event = @event;
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics) + Environment.NewLine +
            string.Join(Environment.NewLine, result.GeneratedSources));

        Assert.That(result.GeneratedSources, Has.Count.EqualTo(2));
        Assert.That(result.HintNames, Is.EquivalentTo([
            "global__First__class.9f5b9916ec9b72a86a53a6eab018d11067ddda8a683b49570ae424284db2f57e.Cbor.g.cs",
            "global__Second__class.0d272005be1e35698c5d00a04bd93a849d866c817a2e98d91b8ee9020bf8074c.Cbor.g.cs"
        ]));
        Assert.That(
            result.GeneratedSources.All(static generated =>
                generated.StartsWith("// <auto-generated/>\n#nullable enable", StringComparison.Ordinal)), Is.True);

        Assert.That(result.GeneratedSources.All(static generated => generated.Contains("partial class @class", StringComparison.Ordinal)),
            Is.True);

        Assert.That(result.GeneratedSources.Any(static generated => generated.Contains("\\n", StringComparison.Ordinal) &&
            generated.Contains("\\t", StringComparison.Ordinal) && generated.Contains("\\u0001", StringComparison.Ordinal)), Is.True);

        Assert.That(result.GeneratedSources.Any(static generated => generated.Contains("global::System.Linq", StringComparison.Ordinal)),
            Is.False);
    }

    [TestCase("public void Serialize(System.Formats.Cbor.CborWriter writer) { }")]
    [TestCase("public static Bad Parse(System.Formats.Cbor.CborReader reader) => new Bad(0);")]
    [TestCase("public int Parse { get; set; }")]
    [TestCase("public class Serialize { }")]
    public void TestGeneratedPublicMethodConflictsAreDiagnosedWithoutSource(string conflictingMethod)
    {
        var source = $$"""
                       using Ws2.Formats.Cbor;
                       [CborArray] public partial class Bad
                       {
                           [CborKey(0)] public int Value { get; }
                           public Bad(int value) => Value = value;
                           {{conflictingMethod}}
                       }
                       """;

        var result = GeneratorHarness.Run(source, true, true);
        AssertDiagnosticWithoutGeneratedSource(result, "SBCBOR011", source,
            conflictingMethod.Contains("Serialize", StringComparison.Ordinal) ? "Serialize" : "Parse");
    }

    [Test]
    public void TestGeneratedMethodsCanCoexistWithGenericAndByRefOverloads()
    {
        const string source = """
                              using System.Formats.Cbor;
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class Model
                              {
                                  public void Serialize<T>(CborWriter writer) { }
                                  public static Model Parse(ref CborReader reader) => new();
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty);
        Assert.That(result.CompilationDiagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty);
        Assert.That(result.GeneratedSources, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestAbstractModifierOnAnotherPartialDeclarationIsDiagnosed()
    {
        const string declaration = "using Ws2.Formats.Cbor; [CborArray] public partial class Model { }";
        var result = GeneratorHarness.Run([declaration, "public abstract partial class Model { }"], true, true);

        AssertDiagnosticWithoutGeneratedSource(result, "SBCBOR008", declaration, "Model");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void TestConverterWithInheritedRequiredMemberNeedsAttributedConstructor(bool setsRequiredMembers)
    {
        var constructor = setsRequiredMembers
            ? "[System.Diagnostics.CodeAnalysis.SetsRequiredMembers] public Converter() { Offset = 0; }"
            : "public Converter() { }";

        var source = $$"""
                       using System.Formats.Cbor;
                       using Ws2.Formats.Cbor;
                       public class BaseConverter { public required int Offset { get; init; } }
                       public sealed class Converter : BaseConverter, ICborConverter<int>
                       {
                           {{constructor}}
                           public void Write(CborWriter writer, int value) => writer.WriteInt32(value + Offset);
                           public int Read(CborReader reader) => reader.ReadInt32() - Offset;
                       }
                       [CborArray] public partial class Model
                       {
                           [CborKey(0), CborConverter(typeof(Converter))] public int Value { get; set; }
                       }
                       """;

        var result = GeneratorHarness.Run(source, true, true);
        if (setsRequiredMembers)
        {
            Assert.That(result.GeneratorDiagnostics, Is.Empty);
            Assert.That(result.CompilationDiagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty);
            Assert.That(result.GeneratedSources, Has.Count.EqualTo(1));
        }
        else
        {
            AssertDiagnosticWithoutGeneratedSource(result, "SBCBOR004", source, "CborConverter(typeof(Converter))");
        }
    }

    [Test]
    public void TestPrivateGeneratedHelperNamesAvoidModelMembersAndEmptyModelsOmitHelpers()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              public sealed class IntConverter : ICborConverter<int>
                              {
                                  public void Write(System.Formats.Cbor.CborWriter writer, int value) => writer.WriteInt32(value);
                                  public int Read(System.Formats.Cbor.CborReader reader) => reader.ReadInt32();
                              }
                              [CborArray] public partial class Model
                              {
                                  [CborKey(0), CborConverter(typeof(IntConverter))] public int Value { get; }
                                  [CborKey(1)] public int Other { get; }
                                  public Model(int value, int other) { Value = value; Other = other; }
                                  private static readonly object __cborConverter0 = new();
                                  private static System.Collections.Generic.List<T> __ReadCollection<T>(System.Formats.Cbor.CborReader reader,
                                      System.Func<System.Formats.Cbor.CborReader, T> readElement) => throw new System.NotImplementedException();
                              }
                              [CborArray] public partial class Empty { }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        Assert.That(result.GeneratedSources.Single(static generated => generated.Contains("class Empty", StringComparison.Ordinal))
            .Contains("ReadCollection", StringComparison.Ordinal), Is.False);

        var modelSource = result.GeneratedSources.Single(static generated => generated.Contains("class Model", StringComparison.Ordinal));
        Assert.That(modelSource, Does.Contain("__Cbor"));
        Assert.That(modelSource, Does.Not.Contain("__ReadCollection"));
        Assert.That(modelSource, Does.Not.Contain("__cborConverter0"));
    }

    [Test]
    public void TestIncrementalModelOutputIsCachedWhenUnrelatedFileChanges()
    {
        const string modelSource =
            "using Ws2.Formats.Cbor; [CborArray] public partial class Model { [CborKey(0)] public int First { get; } [CborKey(1)] public int Second { get; } public Model(int first, int second) { First = first; Second = second; } }";

        CSharpParseOptions parseOptions = new(LanguageVersion.CSharp12);
        var compilation = CSharpCompilation.Create("IncrementalConsumer",
            [CSharpSyntaxTree.ParseText(modelSource, parseOptions, "Model.cs")],
            GeneratorHarness.CreateReferences(true, true),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new CborSerializationIncrementalGenerator().AsSourceGenerator()],
            parseOptions: parseOptions,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true));

        driver = driver.RunGenerators(compilation);
        var unrelated = CSharpSyntaxTree.ParseText("public sealed class Unrelated { }", parseOptions, "Unrelated.cs");
        driver = driver.RunGenerators(compilation.AddSyntaxTrees(unrelated));
        var trackedSteps =
            driver.GetRunResult().Results.Single().TrackedSteps;

        Assert.That(trackedSteps.ContainsKey("CborModelAnalysis"), Is.True);
        Assert.That(trackedSteps["CborModelAnalysis"].SelectMany(static step => step.Outputs)
                .All(static output => output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged), Is.True,
            string.Join(", ", trackedSteps["CborModelAnalysis"].SelectMany(static step => step.Outputs)
                .Select(static output => output.Reason.ToString())));

        var originalOutput = driver.GetRunResult().Results.Single().GeneratedSources.Single().SourceText.ToString();
        var changedKeys = modelSource.Replace("CborKey(0)", "CborKey(9)", StringComparison.Ordinal)
            .Replace("CborKey(1)", "CborKey(0)", StringComparison.Ordinal)
            .Replace("CborKey(9)", "CborKey(1)", StringComparison.Ordinal);

        driver = RunChangedModel(driver, compilation.AddSyntaxTrees(unrelated), changedKeys, parseOptions);
        var keyChangedOutput = driver.GetRunResult().Results.Single().GeneratedSources.Single().SourceText.ToString();
        Assert.That(keyChangedOutput, Is.Not.EqualTo(originalOutput));

        var changedType = changedKeys.Replace("int Second", "long Second", StringComparison.Ordinal)
            .Replace("int second", "long second", StringComparison.Ordinal);

        driver = RunChangedModel(driver, compilation.AddSyntaxTrees(unrelated), changedType, parseOptions);
        var typeChangedOutput = driver.GetRunResult().Results.Single().GeneratedSources.Single().SourceText.ToString();
        Assert.That(typeChangedOutput, Is.Not.EqualTo(keyChangedOutput));

        var changedConstructor =
            changedType.Replace("Model(int first, long second)", "Model(long second, int first)", StringComparison.Ordinal);

        driver = RunChangedModel(driver, compilation.AddSyntaxTrees(unrelated), changedConstructor, parseOptions);
        var constructorChangedOutput = driver.GetRunResult().Results.Single().GeneratedSources.Single().SourceText.ToString();
        Assert.That(constructorChangedOutput, Is.Not.EqualTo(typeChangedOutput));

        static GeneratorDriver RunChangedModel(
            GeneratorDriver currentDriver, CSharpCompilation currentCompilation,
            string updatedSource, CSharpParseOptions options
        )
        {
            var currentTree = currentCompilation.SyntaxTrees.Single(static tree => tree.FilePath == "Model.cs");
            var updatedTree = CSharpSyntaxTree.ParseText(updatedSource, options, "Model.cs");
            return currentDriver.RunGenerators(currentCompilation.ReplaceSyntaxTree(currentTree, updatedTree));
        }
    }

    [Test]
    public void TestIncrementalDiagnosticUsesCurrentSyntaxTreeAtUnchangedSpan()
    {
        CSharpParseOptions options = new(LanguageVersion.CSharp12);
        const string source = "using Ws2.Formats.Cbor; [CborArray] public class Model { }";
        var originalTree = CSharpSyntaxTree.ParseText(source, options, "Model.cs");
        var compilation = CSharpCompilation.Create("IncrementalConsumer", [originalTree],
            GeneratorHarness.CreateReferences(true, true), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new CborSerializationIncrementalGenerator().AsSourceGenerator()], parseOptions: options);

        driver = driver.RunGenerators(compilation);
        var originalDiagnostic = driver.GetRunResult().Diagnostics.Single();
        Assert.That(originalDiagnostic.Id, Is.EqualTo("SBCBOR001"));

        var changedTree = originalTree.WithChangedText(SourceText.From(source + " // edit"));
        compilation = compilation.ReplaceSyntaxTree(originalTree, changedTree);
        driver = driver.RunGenerators(compilation);
        var changedDiagnostic = driver.GetRunResult().Diagnostics.Single();
        Assert.That(changedDiagnostic.Id, Is.EqualTo(originalDiagnostic.Id));
        Assert.That(changedDiagnostic.Location.SourceSpan, Is.EqualTo(originalDiagnostic.Location.SourceSpan));
        Assert.That(changedDiagnostic.Location.SourceTree, Is.SameAs(changedTree));

        var fixedTree = CSharpSyntaxTree.ParseText(source.Replace("public class", "public partial class"), options, "Model.cs");
        driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(changedTree, fixedTree));
        Assert.That(driver.GetRunResult().Diagnostics, Is.Empty);
        Assert.That(driver.GetRunResult().GeneratedTrees, Has.Length.EqualTo(1));
    }

    [Test]
    public void TestMapKeysAndPropertyNamesMatchingHelperPrefixesArePreserved()
    {
        const string source = """
                              using System.Formats.Cbor;
                              using Ws2.Formats.Cbor;
                              [CborMap] public partial class Model
                              {
                                  [CborKey("__cborConverter")] public int Second { get; set; }
                                  [CborKey("__Read")] public int __ReadValue { get; set; }
                              }
                              public static class EntryPoint
                              {
                                  public static bool Run()
                                  {
                                      var writer = new CborWriter();
                                      new Model { __ReadValue = 1, Second = 2 }.Serialize(writer);
                                      var reader = new CborReader(writer.Encode());
                                      if (reader.ReadStartMap() != 2 || reader.ReadTextString() != "__Read" || reader.ReadInt32() != 1 ||
                                          reader.ReadTextString() != "__cborConverter" || reader.ReadInt32() != 2)
                                          return false;
                                      reader.ReadEndMap();

                                      writer.Reset();
                                      writer.WriteStartMap(2);
                                      writer.WriteTextString("__cborConverter");
                                      writer.WriteInt32(9);
                                      writer.WriteTextString("__Read");
                                      writer.WriteInt32(8);
                                      writer.WriteEndMap();
                                      var input = new CborReader(writer.Encode());
                                      var parsed = Model.Parse(input);
                                      return reader.BytesRemaining == 0 && input.BytesRemaining == 0 &&
                                          parsed.__ReadValue == 8 && parsed.Second == 9;
                                  }
                              }
                              """;

        Assert.That(RunGeneratedSource([source]), Is.EqualTo("True"));
    }

    [Test]
    public void TestSerializeReadsCollectionGetterOnce()
    {
        const string source = """
                              using System.Formats.Cbor;
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class Model
                              {
                                  public int Calls { get; private set; }
                                  [CborKey(0)] public int[] Values => ++Calls == 1 ? new[] { 7 } : new[] { 8, 9 };
                                  public Model(int[] values) { }
                              }
                              public static class EntryPoint
                              {
                                  public static bool Run()
                                  {
                                      var model = new Model(new[] { 0 });
                                      var writer = new CborWriter();
                                      model.Serialize(writer);
                                      var reader = new CborReader(writer.Encode());
                                      if (model.Calls != 1 || reader.ReadStartArray() != 1 ||
                                          reader.ReadStartArray() != 1 || reader.ReadInt32() != 7)
                                          return false;
                                      reader.ReadEndArray();
                                      reader.ReadEndArray();
                                      return reader.BytesRemaining == 0;
                                  }
                              }
                              """;

        Assert.That(RunGeneratedSource([source]), Is.EqualTo("True"));
    }

    [Test]
    public void TestGeneratedArrayAndMapModelsCompileAndRoundTrip()
    {
        const string source = """
                              using System;
                              using Ws2.Formats.Cbor;

                              [CborArray]
                              public partial class ArrayModel
                              {
                                  [CborKey(0)] public int Number { get; }
                                  [CborKey(1)] public string Text { get; }
                                  public ArrayModel(int number, string text) { Number = number; Text = text; }
                              }

                              [CborMap]
                              public partial class MapModel
                              {
                                  [CborKey("number")] public int Number { get; }
                                  [CborKey("text")] public string Text { get; }
                                  public MapModel(int number, string text) { Number = number; Text = text; }
                              }

                              public static class EntryPoint
                              {
                                  public static byte[][] Run()
                                  {
                                      var array = new ArrayModel(42, "hi");
                                      var arrayWriter = new System.Formats.Cbor.CborWriter();
                                      array.Serialize(arrayWriter);
                                      var arrayBytes = arrayWriter.Encode();
                                      var arrayReader = new System.Formats.Cbor.CborReader(arrayBytes);
                                      var arrayCopy = ArrayModel.Parse(arrayReader);
                                      if (arrayReader.BytesRemaining != 0 || arrayCopy.Number != 42 || arrayCopy.Text != "hi") throw new Exception("Array round-trip failed.");

                                      var map = new MapModel(42, "hi");
                                      var mapWriter = new System.Formats.Cbor.CborWriter();
                                      map.Serialize(mapWriter);
                                      var mapBytes = mapWriter.Encode();
                                      var mapReader = new System.Formats.Cbor.CborReader(mapBytes);
                                      var mapCopy = MapModel.Parse(mapReader);
                                      if (mapReader.BytesRemaining != 0 || mapCopy.Number != 42 || mapCopy.Text != "hi") throw new Exception("Map round-trip failed.");
                                      return new[] { arrayBytes, mapBytes };
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);

        Assert.That(result.GeneratorDiagnostics, Is.Empty);
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        Assert.That(result.GeneratedSources, Has.Count.EqualTo(2));
        Assert.That(result.GeneratedSources.Any(static text => text.Contains("Serialize", StringComparison.Ordinal)), Is.True);
        Assert.That(result.GeneratedSources.Any(static text => text.Contains("CborArrayAttribute", StringComparison.Ordinal)), Is.False);

        using MemoryStream assemblyStream = new();
        var emitResult = result.Compilation.Emit(assemblyStream);
        Assert.That(emitResult.Success, Is.True, string.Join(Environment.NewLine, emitResult.Diagnostics));
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborGeneratedConsumer", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        byte[][] bytes;
        try
        {
            var assembly = loadContext.LoadFromStream(assemblyStream);
            bytes = (byte[][])assembly.GetType("EntryPoint")!.GetMethod("Run")!.Invoke(null, null)!;
        }
        finally
        {
            loadContext.Unload();
        }

        Assert.That(bytes[0], Is.EqualTo(new byte[] { 0x82, 0x18, 0x2a, 0x62, 0x68, 0x69 }));
        Assert.That(bytes[1],
            Is.EqualTo(new byte[]
            {
                0xa2, 0x64, 0x74, 0x65, 0x78, 0x74, 0x62, 0x68, 0x69, 0x66, 0x6e, 0x75, 0x6d, 0x62, 0x65, 0x72, 0x18, 0x2a
            }));
    }

    [Test]
    public void TestArrayAndMapWireContractsAcrossDeclarationOrders()
    {
        const string firstPart = """
                                 using Ws2.Formats.Cbor;
                                 [CborArray] public partial class IndexedArray
                                 {
                                     [CborKey(1)] public int Second { get; }
                                     public IndexedArray(int second, int first) { Second = second; First = first; }
                                 }
                                 [CborMap] public partial class OrderedMap
                                 {
                                     [CborKey("zz")] public string Zz { get; }
                                     [CborKey("a")] public string A { get; }
                                     [CborKey("λ")] public string Lambda { get; }
                                 }
                                 """;

        const string secondPart = """
                                  using Ws2.Formats.Cbor;
                                  public partial class IndexedArray { [CborKey(0)] public int First { get; } }
                                  public partial class OrderedMap
                                  {
                                      [CborKey("aaaaaaaaaaaaaaaaaaaaaaa")] public string BeforeHeaderBoundary { get; }
                                      [CborKey("aaaaaaaaaaaaaaaaaaaaaaaa")] public string AfterHeaderBoundary { get; }
                                      public OrderedMap(string zz, string a, string lambda, string beforeHeaderBoundary, string afterHeaderBoundary)
                                      {
                                          Zz = zz; A = a; Lambda = lambda; BeforeHeaderBoundary = beforeHeaderBoundary;
                                          AfterHeaderBoundary = afterHeaderBoundary;
                                      }
                                  }
                                  public static class EntryPoint
                                  {
                                      public static string Run()
                                      {
                                          var array = new IndexedArray(2, 1);
                                          var arrayWriter = new System.Formats.Cbor.CborWriter(); array.Serialize(arrayWriter);
                                          var arrayBytes = arrayWriter.Encode();
                                          var arrayReader = new System.Formats.Cbor.CborReader(arrayBytes);
                                          var arrayCopy = IndexedArray.Parse(arrayReader);
                                          if (arrayCopy.First != 1 || arrayCopy.Second != 2 || arrayReader.BytesRemaining != 0) throw new System.Exception("Array indexes changed.");

                                          var map = new OrderedMap("z", "a", "l", "before", "after");
                                          var mapWriter = new System.Formats.Cbor.CborWriter(); map.Serialize(mapWriter);
                                          var mapBytes = mapWriter.Encode();
                                          var mapReader = new System.Formats.Cbor.CborReader(mapBytes);
                                          mapReader.ReadStartMap();
                                          var keys = new System.Collections.Generic.List<string>();
                                          while (mapReader.PeekState() != System.Formats.Cbor.CborReaderState.EndMap)
                                          {
                                              keys.Add(mapReader.ReadTextString()); mapReader.SkipValue();
                                          }
                                          mapReader.ReadEndMap();
                                          var expected = new[] { "a", "zz", "λ", "aaaaaaaaaaaaaaaaaaaaaaa", "aaaaaaaaaaaaaaaaaaaaaaaa" };
                                          if (!System.Linq.Enumerable.SequenceEqual(keys, expected)) throw new System.Exception("Map key order changed: " + string.Join(",", keys));
                                          return System.Convert.ToHexString(arrayBytes) + ":" + System.Convert.ToHexString(mapBytes);
                                      }
                                  }
                                  """;

        var forward = RunGeneratedSource([firstPart, secondPart]);
        var reverse = RunGeneratedSource([secondPart, firstPart]);
        Assert.That(reverse, Is.EqualTo(forward));
        Assert.That(forward, Does.StartWith("820102:"));
    }

    [Test]
    public void TestArrayMapParsingLengthUnknownFieldsDuplicatesAndSingleValueConsumption()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class EmptyArray { }
                              [CborArray] public partial class PairArray
                              {
                                  [CborKey(0)] public int First { get; }
                                  [CborKey(1)] public int Second { get; }
                                  public PairArray(int first, int second) { First = first; Second = second; }
                              }
                              [CborMap] public partial class RequiredMap
                              {
                                  [CborKey("value")] public int Value { get; }
                                  [CborKey("maybe")] public string? Maybe { get; }
                                  public RequiredMap(int value, string? maybe) { Value = value; Maybe = maybe; }
                              }
                              [CborMap] public partial class OldMap
                              {
                                  [CborKey("value")] public int Value { get; }
                                  public OldMap(int value) => Value = value;
                              }
                              [CborMap] public partial class NewMap
                              {
                                  [CborKey("value")] public int Value { get; }
                                  [CborKey("added")] public string Added { get; }
                                  public NewMap(int value, string added) { Value = value; Added = added; }
                              }
                              public static class EntryPoint
                              {
                                  private static byte[] EncodeArray(params int[] values)
                                  {
                                      var writer = new System.Formats.Cbor.CborWriter(); writer.WriteStartArray(values.Length);
                                      foreach (var value in values) writer.WriteInt32(value);
                                      writer.WriteEndArray(); return writer.Encode();
                                  }
                                  private static byte[] EncodeMap(System.Action<System.Formats.Cbor.CborWriter> write)
                                  {
                                      var writer = new System.Formats.Cbor.CborWriter(); write(writer); return writer.Encode();
                                  }
                                  private static bool Throws(System.Action action)
                                  {
                                      try { action(); return false; }
                                      catch (System.Formats.Cbor.CborContentException) { return true; }
                                      catch (System.InvalidOperationException) { return true; }
                                      catch (System.ArgumentException) { return true; }
                                  }
                                  public static bool Run()
                                  {
                                      if (EmptyArray.Parse(new System.Formats.Cbor.CborReader(new byte[] { 0x80 })) is null) return false;
                                      if (!Throws(() => PairArray.Parse(new System.Formats.Cbor.CborReader(EncodeArray(1))))) return false;
                                      if (!Throws(() => PairArray.Parse(new System.Formats.Cbor.CborReader(EncodeArray(1, 2, 3))))) return false;
                                      var indefiniteWriter = new System.Formats.Cbor.CborWriter(System.Formats.Cbor.CborConformanceMode.Lax);
                                      indefiniteWriter.WriteStartArray(null); indefiniteWriter.WriteInt32(1); indefiniteWriter.WriteInt32(2); indefiniteWriter.WriteEndArray();
                                      if (!Throws(() => PairArray.Parse(new System.Formats.Cbor.CborReader(indefiniteWriter.Encode())))) return false;
                                      if (!Throws(() => PairArray.Parse(new System.Formats.Cbor.CborReader(new byte[] { 0x82, 0x01 })))) return false;

                                      var firstWriter = new System.Formats.Cbor.CborWriter(allowMultipleRootLevelValues: true); firstWriter.WriteStartArray(2); firstWriter.WriteInt32(3); firstWriter.WriteInt32(4); firstWriter.WriteEndArray();
                                      firstWriter.WriteStartArray(2); firstWriter.WriteInt32(5); firstWriter.WriteInt32(6); firstWriter.WriteEndArray();
                                      var sequentialReader = new System.Formats.Cbor.CborReader(firstWriter.Encode(), allowMultipleRootLevelValues: true);
                                      var first = PairArray.Parse(sequentialReader); var second = PairArray.Parse(sequentialReader);
                                      if (first.First != 3 || first.Second != 4 || second.First != 5 || second.Second != 6 || sequentialReader.BytesRemaining != 0) return false;

                                      var reordered = EncodeMap(writer =>
                                      {
                                          writer.WriteStartMap(2); writer.WriteTextString("maybe"); writer.WriteNull();
                                          writer.WriteTextString("value"); writer.WriteInt32(7); writer.WriteEndMap();
                                      });
                                      var nullable = RequiredMap.Parse(new System.Formats.Cbor.CborReader(reordered));
                                      if (nullable.Value != 7 || nullable.Maybe is not null) return false;
                                      if (!Throws(() => RequiredMap.Parse(new System.Formats.Cbor.CborReader(EncodeMap(writer =>
                                      { writer.WriteStartMap(1); writer.WriteTextString("value"); writer.WriteInt32(1); writer.WriteEndMap(); }))))) return false;
                                      if (!Throws(() => RequiredMap.Parse(new System.Formats.Cbor.CborReader(EncodeMap(writer =>
                                      { writer.WriteStartMap(2); writer.WriteTextString("value"); writer.WriteInt32(1); writer.WriteTextString("value"); writer.WriteInt32(2); writer.WriteEndMap(); }))))) return false;
                                      if (!Throws(() => RequiredMap.Parse(new System.Formats.Cbor.CborReader(EncodeMap(writer =>
                                      { writer.WriteStartMap(2); writer.WriteTextString("unknown"); writer.WriteInt32(1); writer.WriteTextString("unknown"); writer.WriteInt32(2); writer.WriteEndMap(); }))))) return false;
                                      if (!Throws(() => RequiredMap.Parse(new System.Formats.Cbor.CborReader(EncodeMap(writer =>
                                      { writer.WriteStartMap(1); writer.WriteInt32(1); writer.WriteInt32(2); writer.WriteEndMap(); }))))) return false;
                                      if (!Throws(() => RequiredMap.Parse(new System.Formats.Cbor.CborReader(new byte[] { 0xa1, 0x65, 0x76, 0x61, 0x6c, 0x75, 0x65 })))) return false;

                                      var unknownNested = EncodeMap(writer =>
                                      {
                                          writer.WriteStartMap(3); writer.WriteTextString("unknown"); writer.WriteStartArray(1);
                                          writer.WriteStartMap(1); writer.WriteTextString("nested"); writer.WriteInt32(9); writer.WriteEndMap(); writer.WriteEndArray();
                                          writer.WriteTextString("simple"); writer.WriteBoolean(true);
                                          writer.WriteTextString("value"); writer.WriteInt32(8); writer.WriteEndMap();
                                      });
                                      if (OldMap.Parse(new System.Formats.Cbor.CborReader(unknownNested)).Value != 8) return false;
                                      var indefiniteMapWriter = new System.Formats.Cbor.CborWriter(System.Formats.Cbor.CborConformanceMode.Lax);
                                      indefiniteMapWriter.WriteStartMap(null); indefiniteMapWriter.WriteTextString("value");
                                      indefiniteMapWriter.WriteInt32(9); indefiniteMapWriter.WriteEndMap();
                                      if (OldMap.Parse(new System.Formats.Cbor.CborReader(indefiniteMapWriter.Encode())).Value != 9) return false;
                                      var evolved = EncodeMap(writer =>
                                      { writer.WriteStartMap(2); writer.WriteTextString("value"); writer.WriteInt32(8); writer.WriteTextString("added"); writer.WriteTextString("new"); writer.WriteEndMap(); });
                                      if (OldMap.Parse(new System.Formats.Cbor.CborReader(evolved)).Value != 8) return false;
                                      if (!Throws(() => NewMap.Parse(new System.Formats.Cbor.CborReader(EncodeMap(writer =>
                                      { writer.WriteStartMap(1); writer.WriteTextString("value"); writer.WriteInt32(8); writer.WriteEndMap(); }))))) return false;

                                      var mapSequenceWriter = new System.Formats.Cbor.CborWriter(allowMultipleRootLevelValues: true);
                                      mapSequenceWriter.WriteStartMap(1); mapSequenceWriter.WriteTextString("value"); mapSequenceWriter.WriteInt32(1); mapSequenceWriter.WriteEndMap();
                                      mapSequenceWriter.WriteStartMap(1); mapSequenceWriter.WriteTextString("value"); mapSequenceWriter.WriteInt32(2); mapSequenceWriter.WriteEndMap();
                                      var mapReader = new System.Formats.Cbor.CborReader(mapSequenceWriter.Encode(), allowMultipleRootLevelValues: true);
                                      var map1 = OldMap.Parse(mapReader); var map2 = OldMap.Parse(mapReader);
                                      return map1.Value == 1 && map2.Value == 2 && mapReader.BytesRemaining == 0;
                                  }
                              }
                              """;

        Assert.That(RunGeneratedSource([source]), Is.EqualTo("True"));
    }

    private static string RunGeneratedSource(IEnumerable<string> sources)
    {
        var result = GeneratorHarness.Run(sources, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        var emitResult = result.Compilation.Emit(assemblyStream);
        Assert.That(emitResult.Success, Is.True, string.Join(Environment.NewLine, emitResult.Diagnostics));
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborContainerContracts", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var assembly = loadContext.LoadFromStream(assemblyStream);
            var returned = assembly.GetType("EntryPoint")!.GetMethod("Run")!.Invoke(null, null);
            return returned is bool boolean ? boolean.ToString() : (string)returned!;
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestSelectedPropertiesAndReorderedPrivateConstructorRoundTrip()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class ArrayModel
                              {
                                  [CborKey(0)] public int First { get; }
                                  [CborKey(1)] private int Second { get; }
                                  public object? Ignored => throw new System.Exception("Unselected property was evaluated.");
                                  private ArrayModel(int second, int first) { First = first; Second = second; }
                                  public static ArrayModel Create() => new(2, 1);
                                  public static string Run()
                                  {
                                      var model = Create();
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      model.Serialize(writer);
                                      var bytes = writer.Encode();
                                      var copy = Parse(new System.Formats.Cbor.CborReader(bytes));
                                      return System.Convert.ToHexString(bytes) + ":" + copy.First + "," + copy.Second;
                                  }
                              }

                              [CborMap] public partial class MapModel
                              {
                                  [CborKey("first")] public int First { get; }
                                  [CborKey("second")] private int Second { get; }
                                  public object? Ignored => throw new System.Exception("Unselected property was evaluated.");
                                  private MapModel(int second, int first) { First = first; Second = second; }
                                  public static MapModel Create() => new(2, 1);
                                  public static string Run()
                                  {
                                      var model = Create();
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      model.Serialize(writer);
                                      var bytes = writer.Encode();
                                      var copy = Parse(new System.Formats.Cbor.CborReader(bytes));
                                      return System.Convert.ToHexString(bytes) + ":" + copy.First + "," + copy.Second;
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        var emitResult = result.Compilation.Emit(assemblyStream);
        Assert.That(emitResult.Success, Is.True, string.Join(Environment.NewLine, emitResult.Diagnostics));
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborReorderedConsumer", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var assembly = loadContext.LoadFromStream(assemblyStream);
            var arrayValue = (string)assembly.GetType("ArrayModel")!.GetMethod("Run")!.Invoke(null, null)!;
            var mapValue = (string)assembly.GetType("MapModel")!.GetMethod("Run")!.Invoke(null, null)!;
            Assert.That(arrayValue, Is.EqualTo("820102:1,2"));
            Assert.That(mapValue, Is.EqualTo("A265666972737401667365636F6E6402:1,2"));
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestParameterlessSetAndInitModelsRoundTripForMapsAndArrays()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              public sealed class VersionConverter : ICborConverter<System.Version>
                              {
                                  public void Write(System.Formats.Cbor.CborWriter writer, System.Version value) => writer.WriteTextString(value.ToString());
                                  public System.Version Read(System.Formats.Cbor.CborReader reader) => System.Version.Parse(reader.ReadTextString());
                              }

                              [CborMap] public partial class MapDto
                              {
                                  [CborKey("id")] public required int Id { get; set; }
                                  [CborKey("note")] public string? Note { get; init; }
                                  [CborKey("values")] public System.Collections.Generic.IReadOnlyList<int?> Values { get; set; } = [];
                                  [CborKey("version"), CborConverter(typeof(VersionConverter))]
                                  public System.Version Version { get; init; } = new(0, 0);
                                  private MapDto() { }
                                  public static MapDto Create() => new()
                                  {
                                      Id = 12,
                                      Note = null,
                                      Values = new int?[] { 3, null, 8 },
                                      Version = new(1, 2, 3)
                                  };
                              }

                              [CborArray] public partial class ArrayDto
                              {
                                  [CborKey(0)] public string? Text { get; init; }
                                  [CborKey(1)] public int Number { get; set; }
                                  public ArrayDto() { }
                              }

                              [CborMap] public partial class EmptyDto { }

                              public static class EntryPoint
                              {
                                  public static bool Run()
                                  {
                                      var map = MapDto.Create();
                                      var mapWriter = new System.Formats.Cbor.CborWriter();
                                      map.Serialize(mapWriter);
                                      var mapReader = new System.Formats.Cbor.CborReader(mapWriter.Encode());
                                      var mapCopy = MapDto.Parse(mapReader);

                                      var array = new ArrayDto { Text = "value", Number = 42 };
                                      var arrayWriter = new System.Formats.Cbor.CborWriter();
                                      array.Serialize(arrayWriter);
                                      var arrayReader = new System.Formats.Cbor.CborReader(arrayWriter.Encode());
                                      var arrayCopy = ArrayDto.Parse(arrayReader);

                                      var emptyWriter = new System.Formats.Cbor.CborWriter();
                                      new EmptyDto().Serialize(emptyWriter);
                                      var emptyCopy = EmptyDto.Parse(new System.Formats.Cbor.CborReader(emptyWriter.Encode()));
                                      return mapReader.BytesRemaining == 0 && arrayReader.BytesRemaining == 0 &&
                                          mapCopy.Id == 12 && mapCopy.Note is null && mapCopy.Values.Count == 3 &&
                                          mapCopy.Values[0] == 3 && mapCopy.Values[1] is null && mapCopy.Values[2] == 8 &&
                                          mapCopy.Version == new System.Version(1, 2, 3) && arrayCopy.Text == "value" && arrayCopy.Number == 42 &&
                                          emptyCopy is not null;
                                  }
                              }
                              """;

        Assert.That(RunGeneratedSource([source]), Is.EqualTo("True"));
    }

    [Test]
    public void TestParameterlessConstructionRequiresPublicSettersAndAllRequiredMembers()
    {
        const string missingParameterless = """
                                            using Ws2.Formats.Cbor;
                                            [CborArray] public partial class MissingParameterless
                                            {
                                                [CborKey(0)] public int Value { get; set; }
                                                public MissingParameterless(string ignored) { }
                                            }
                                            """;

        var missingParameterlessResult = GeneratorHarness.Run(missingParameterless, true, true);
        AssertDiagnosticWithoutGeneratedSource(missingParameterlessResult, "SBCBOR010", missingParameterless, "MissingParameterless");

        const string getOnly = """
                               using Ws2.Formats.Cbor;
                               [CborArray] public partial class GetOnly
                               {
                                   [CborKey(0)] public int Value { get; }
                                   public GetOnly() { }
                               }
                               """;

        var getOnlyResult = GeneratorHarness.Run(getOnly, true, true);
        AssertDiagnosticWithoutGeneratedSource(getOnlyResult, "SBCBOR010", getOnly, "GetOnly");

        const string required = """
                                using Ws2.Formats.Cbor;
                                [CborMap] public partial class MissingRequired
                                {
                                    [CborKey("value")] public int Value { get; set; }
                                    public required string Name { get; init; }
                                    public MissingRequired() { }
                                }
                                """;

        var requiredResult = GeneratorHarness.Run(required, true, true);
        AssertDiagnosticWithoutGeneratedSource(requiredResult, "SBCBOR010", required, "MissingRequired");
    }

    [Test]
    public void TestUnsupportedModelAndConstructorDiagnosticsHaveLocationsAndNoSource()
    {
        const string unsupportedModel = "using Ws2.Formats.Cbor; [CborArray] public partial struct Bad { }";
        var unsupportedResult = GeneratorHarness.Run(unsupportedModel, true, true);
        AssertDiagnosticWithoutGeneratedSource(unsupportedResult, "SBCBOR008", unsupportedModel, "Bad");

        const string invalidConstructor = """
                                          using Ws2.Formats.Cbor;
                                          [CborArray] public partial class Bad
                                          {
                                              [CborKey(0)] public int Value { get; }
                                              public Bad(string wrong) { Value = 0; }
                                          }
                                          """;

        var constructorResult = GeneratorHarness.Run(invalidConstructor, true, true);
        AssertDiagnosticWithoutGeneratedSource(constructorResult, "SBCBOR010", invalidConstructor, "Bad");
    }

    [Test]
    public void TestConverterWithoutKeyReportsMemberDiagnostic()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class Bad
                              {
                                  [CborConverter(typeof(string))] public int Value { get; }
                                  public Bad(int value) => Value = value;
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        AssertDiagnosticWithoutGeneratedSource(result, "SBCBOR009", source, "CborConverter(typeof(string))");
    }

    [Test]
    public void TestSelectedPropertiesCanBeDeclaredAcrossPartialFiles()
    {
        var result = GeneratorHarness.Run(
        [
            "using Ws2.Formats.Cbor; [CborMap] public partial class Model { public Model(int value) => Value = value; }",
                "using Ws2.Formats.Cbor; public partial class Model { [CborKey(\"value\")] private int Value { get; } public object? Ignored => new object(); }"
        ], true, true);

        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        Assert.That(result.GeneratedSources, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestPositionalRecordAndEmptyArrayModelsCompile()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial record class Pair([property: CborKey(0)] int First, [property: CborKey(1)] int Second);
                              [CborArray] public partial class Empty { }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        Assert.That(result.GeneratedSources, Has.Count.EqualTo(2));
        Assert.That(
            result.GeneratedSources.Any(static text =>
                text.Contains("return new Pair(\n            property0,\n            property1);", StringComparison.Ordinal)), Is.True);

        Assert.That(result.GeneratedSources.Any(static text => text.Contains("return new Empty();", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void TestRequiredMembersNeedSetsRequiredMembersOnSelectedConstructor()
    {
        const string accepted = """
                                using System.Diagnostics.CodeAnalysis;
                                using Ws2.Formats.Cbor;
                                [CborArray] public partial class Accepted
                                {
                                    [CborKey(0)] public required int Value { get; init; }
                                    [SetsRequiredMembers] public Accepted(int value) => Value = value;
                                }
                                """;

        var acceptedResult = GeneratorHarness.Run(accepted, true, true);
        Assert.That(acceptedResult.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, acceptedResult.GeneratorDiagnostics));
        Assert.That(acceptedResult.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, acceptedResult.CompilationDiagnostics));

        const string rejected = """
                                using Ws2.Formats.Cbor;
                                [CborArray] public partial class Rejected
                                {
                                    [CborKey(0)] public required int Value { get; init; }
                                    public Rejected(int value) => Value = value;
                                }
                                """;

        var rejectedResult = GeneratorHarness.Run(rejected, true, true);
        AssertDiagnosticWithoutGeneratedSource(rejectedResult, "SBCBOR010", rejected, "Rejected", 1);
    }

    [Test]
    public void TestConstructorBindingIncludesNullableAnnotations()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class Bad
                              {
                                  [CborKey(0)] public string? Value { get; }
                                  public Bad(string value) => Value = value;
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        AssertDiagnosticWithoutGeneratedSource(result, "SBCBOR010", source, "Bad");
    }

    [Test]
    public void TestMultipleMatchingConstructorsAreRejected()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class Ambiguous
                              {
                                  [CborKey(0)] public int Count { get; }
                                  [CborKey(1)] public string Name { get; }
                                  public Ambiguous(int count, string name) { Count = count; Name = name; }
                                  private Ambiguous(string name, int count) { Count = count; Name = name; }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        AssertDiagnosticWithoutGeneratedSource(result, "SBCBOR010", source, "Ambiguous");
    }

    [Test]
    public void TestFormatConflictAndInvalidArrayKeyHavePreciseLocations()
    {
        const string conflictingFormats = "using Ws2.Formats.Cbor; [CborArray, CborMap] public partial class Conflict { }";
        var conflictResult = GeneratorHarness.Run(conflictingFormats, true, true);
        AssertDiagnosticWithoutGeneratedSource(conflictResult, "SBCBOR003", conflictingFormats, "CborArray");

        const string invalidKey = """
                                  using Ws2.Formats.Cbor;
                                  [CborArray] public partial class InvalidKeyModel
                                  {
                                      [CborKey(-1)] public int Value { get; }
                                      public InvalidKeyModel(int value) => Value = value;
                                  }
                                  """;

        var invalidKeyResult = GeneratorHarness.Run(invalidKey, true, true);
        AssertDiagnosticWithoutGeneratedSource(invalidKeyResult, "SBCBOR005", invalidKey, "CborKey(-1)");
    }

    [Test]
    public void TestNullableScalarsAndUnsignedEnumRoundTrip()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              public enum LargeEnum : ulong { MaxValue = ulong.MaxValue }
                              [CborArray] public partial class ScalarModel
                              {
                                  [CborKey(0)] public uint Unsigned { get; }
                                  [CborKey(1)] public ulong Large { get; }
                                  [CborKey(2)] public LargeEnum EnumValue { get; }
                                  [CborKey(3)] public short Signed { get; }
                                  [CborKey(4)] public int? OptionalNumber { get; }
                                  [CborKey(5)] public string? OptionalText { get; }
                                  [CborKey(6)] public byte[]? OptionalBytes { get; }
                                  [CborKey(7)] public LargeEnum? OptionalEnum { get; }
                                  public ScalarModel(uint unsigned, ulong large, LargeEnum enumValue, short signed,
                                      int? optionalNumber, string? optionalText, byte[]? optionalBytes, LargeEnum? optionalEnum)
                                  {
                                      Unsigned = unsigned; Large = large; EnumValue = enumValue; Signed = signed;
                                      OptionalNumber = optionalNumber; OptionalText = optionalText; OptionalBytes = optionalBytes; OptionalEnum = optionalEnum;
                                  }
                                  public static string Run()
                                  {
                                      var model = new ScalarModel(uint.MaxValue, ulong.MaxValue, LargeEnum.MaxValue, short.MinValue, null, null, null, null);
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      model.Serialize(writer);
                                      var bytes = writer.Encode();
                                      var copy = Parse(new System.Formats.Cbor.CborReader(bytes));
                                      return System.Convert.ToHexString(bytes) + ":" + copy.Unsigned + "," + copy.Large + "," +
                                          (ulong)copy.EnumValue + "," + copy.Signed + "," + (copy.OptionalNumber is null) + "," +
                                          (copy.OptionalText is null) + "," + (copy.OptionalBytes is null) + "," + (copy.OptionalEnum is null);
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        var emitResult = result.Compilation.Emit(assemblyStream);
        Assert.That(emitResult.Success, Is.True, string.Join(Environment.NewLine, emitResult.Diagnostics));
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborNullableScalars", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var assembly = loadContext.LoadFromStream(assemblyStream);
            var actual = (string)assembly.GetType("ScalarModel")!.GetMethod("Run")!.Invoke(null, null)!;
            Assert.That(actual, Does.StartWith("881AFFFFFFFF1BFFFFFFFFFFFFFFFF1BFFFFFFFFFFFFFFFF397FFFF6F6F6F6:"));
            Assert.That(actual, Does.EndWith(":4294967295,18446744073709551615,18446744073709551615,-32768,True,True,True,True"));
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestSmallIntegerOverflowRetainsCauseAndPropertyName()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class ByteModel
                              {
                                  [CborKey(0)] public byte Value { get; }
                                  public ByteModel(byte value) => Value = value;
                                  private static bool ThrowsRangeError(int value)
                                  {
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      writer.WriteStartArray(1); writer.WriteInt32(value); writer.WriteEndArray();
                                      try { Parse(new System.Formats.Cbor.CborReader(writer.Encode())); }
                                      catch (System.Formats.Cbor.CborContentException exception)
                                      {
                                          return exception.InnerException is System.OverflowException && exception.Message.Contains("Value");
                                      }
                                      return false;
                                  }
                                  public static void Run()
                                  {
                                      if (!ThrowsRangeError(256) || !ThrowsRangeError(-1)) throw new System.Exception("Expected range errors for overflow and negative unsigned value.");
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        Assert.That(result.Compilation.Emit(assemblyStream).Success, Is.True);
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborIntegerOverflow", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            loadContext.LoadFromStream(assemblyStream).GetType("ByteModel")!.GetMethod("Run")!.Invoke(null, null);
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestNullContractForNonNullableReferenceProperties()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class RequiredText
                              {
                                  [CborKey(0)] public string Value { get; }
                                  public RequiredText(string value) => Value = value;
                                  public static void Run()
                                  {
                                      try { new RequiredText(null!).Serialize(new System.Formats.Cbor.CborWriter()); }
                                      catch (System.ArgumentNullException exception)
                                      {
                                          if (exception.ParamName != "Value") throw;
                                      }
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      writer.WriteStartArray(1); writer.WriteNull(); writer.WriteEndArray();
                                      try { Parse(new System.Formats.Cbor.CborReader(writer.Encode())); }
                                      catch (System.Formats.Cbor.CborContentException exception)
                                      {
                                          if (!exception.Message.Contains("Value")) throw;
                                          return;
                                      }
                                      throw new System.Exception("Expected null content to be rejected.");
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        Assert.That(result.Compilation.Emit(assemblyStream).Success, Is.True);
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborNonNullableNull", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            loadContext.LoadFromStream(assemblyStream).GetType("RequiredText")!.GetMethod("Run")!.Invoke(null, null);
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestNullableValueConverterIsSkippedForNull()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              public sealed class NullableIntConverter : ICborConverter<int?>
                              {
                                  public static int Calls;
                                  public void Write(System.Formats.Cbor.CborWriter writer, int? value)
                                  {
                                      Calls++;
                                      writer.WriteInt32(value!.Value);
                                  }
                                  public int? Read(System.Formats.Cbor.CborReader reader)
                                  {
                                      Calls++;
                                      return reader.ReadInt32();
                                  }
                              }
                              [CborArray] public partial class ConvertedModel
                              {
                                  [CborKey(0), CborConverter(typeof(NullableIntConverter))] public int? Value { get; }
                                  public ConvertedModel(int? value) => Value = value;
                                  public static bool Run()
                                  {
                                      var nullModel = new ConvertedModel(null);
                                      var nullWriter = new System.Formats.Cbor.CborWriter();
                                      nullModel.Serialize(nullWriter);
                                      var nullCopy = Parse(new System.Formats.Cbor.CborReader(nullWriter.Encode()));
                                      if (NullableIntConverter.Calls != 0 || nullCopy.Value is not null) return false;
                                      var valueModel = new ConvertedModel(42);
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      valueModel.Serialize(writer);
                                      var copy = Parse(new System.Formats.Cbor.CborReader(writer.Encode()));
                                      return NullableIntConverter.Calls == 2 && copy.Value == 42;
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        Assert.That(result.Compilation.Emit(assemblyStream).Success, Is.True);
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborNullableConverter", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var returned = loadContext.LoadFromStream(assemblyStream).GetType("ConvertedModel")!
                .GetMethod("Run")!.Invoke(null, null);

            Assert.That(returned, Is.True);
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestConverterValidationRequiresClosedAccessibleClassAndExactNullableType()
    {
        var invalidSources = new[]
        {
            """
            using Ws2.Formats.Cbor;
            public abstract class ValueConverter : ICborConverter<int> { public abstract void Write(System.Formats.Cbor.CborWriter w, int v); public abstract int Read(System.Formats.Cbor.CborReader r); }
            [CborArray] public partial class Model { [CborKey(0), CborConverter(typeof(ValueConverter))] public int Value { get; } public Model(int value) => Value = value; }
            """,
            """
            using Ws2.Formats.Cbor;
            public sealed class ValueConverter<T> : ICborConverter<int> { public void Write(System.Formats.Cbor.CborWriter w, int v) => w.WriteInt32(v); public int Read(System.Formats.Cbor.CborReader r) => r.ReadInt32(); }
            [CborArray] public partial class Model { [CborKey(0), CborConverter(typeof(ValueConverter<>))] public int Value { get; } public Model(int value) => Value = value; }
            """,
            """
            #nullable enable
            using Ws2.Formats.Cbor;
            public sealed class ValueConverter : ICborConverter<string> { public void Write(System.Formats.Cbor.CborWriter w, string v) => w.WriteTextString(v); public string Read(System.Formats.Cbor.CborReader r) => r.ReadTextString(); }
            [CborArray] public partial class Model { [CborKey(0), CborConverter(typeof(ValueConverter))] public string? Value { get; } public Model(string? value) => Value = value; }
            """,
            """
            using Ws2.Formats.Cbor;
            public sealed class ValueConverter : ICborConverter<int> { private ValueConverter() { } public void Write(System.Formats.Cbor.CborWriter w, int v) => w.WriteInt32(v); public int Read(System.Formats.Cbor.CborReader r) => r.ReadInt32(); }
            [CborArray] public partial class Model { [CborKey(0), CborConverter(typeof(ValueConverter))] public int Value { get; } public Model(int value) => Value = value; }
            """
        };

        foreach (var source in invalidSources)
        {
            var result = GeneratorHarness.Run(source, true, true);
            Assert.That(result.GeneratorDiagnostics.Any(static diagnostic => diagnostic.Id == "SBCBOR004"), Is.True,
                string.Join(Environment.NewLine, result.GeneratorDiagnostics));

            Assert.That(result.GeneratedSources, Is.Empty);
        }

        const string internalConverterSource = """
                                               using Ws2.Formats.Cbor;
                                               internal sealed class InternalConverter : ICborConverter<int>
                                               {
                                                   internal InternalConverter() { }
                                                   public void Write(System.Formats.Cbor.CborWriter writer, int value) => writer.WriteInt32(value);
                                                   public int Read(System.Formats.Cbor.CborReader reader) => reader.ReadInt32();
                                               }
                                               [CborArray] public partial class Model
                                               {
                                                   [CborKey(0), CborConverter(typeof(InternalConverter))] public int Value { get; }
                                                   public Model(int value) => Value = value;
                                               }
                                               """;

        var internalConverterResult = GeneratorHarness.Run(internalConverterSource, true, true);
        Assert.That(internalConverterResult.GeneratorDiagnostics, Is.Empty,
            string.Join(Environment.NewLine, internalConverterResult.GeneratorDiagnostics));

        Assert.That(internalConverterResult.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error),
            Is.Empty,
            string.Join(Environment.NewLine, internalConverterResult.CompilationDiagnostics));

        const string inaccessibleConverterSource = """
                                                   using Ws2.Formats.Cbor;
                                                   internal sealed class InternalExternalConverter : ICborConverter<int>
                                                   {
                                                       public void Write(System.Formats.Cbor.CborWriter writer, int value) => writer.WriteInt32(value);
                                                       public int Read(System.Formats.Cbor.CborReader reader) => reader.ReadInt32();
                                                   }
                                                   """;

        var externalCompilation = CSharpCompilation.Create("InaccessibleConverter",
            [CSharpSyntaxTree.ParseText(inaccessibleConverterSource, new CSharpParseOptions(LanguageVersion.CSharp12))],
            GeneratorHarness.CreateReferences(true, true),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        using MemoryStream externalStream = new();
        Assert.That(externalCompilation.Emit(externalStream).Success, Is.True);
        var inaccessibleSource = """
                                 using Ws2.Formats.Cbor;
                                 [CborArray] public partial class Model
                                 {
                                     [CborKey(0), CborConverter(typeof(InternalExternalConverter))] public int Value { get; }
                                     public Model(int value) => Value = value;
                                 }
                                 """;

        var inaccessibleResult = GeneratorHarness.Run(inaccessibleSource, true, true,
            [MetadataReference.CreateFromImage(externalStream.ToArray())]);

        Assert.That(inaccessibleResult.GeneratorDiagnostics.Any(static diagnostic => diagnostic.Id == "SBCBOR004"), Is.True,
            string.Join(Environment.NewLine, inaccessibleResult.GeneratorDiagnostics));

        Assert.That(inaccessibleResult.GeneratedSources, Is.Empty);
    }

    [Test]
    public void TestConverterOverridesUnsupportedTypesAndVersionAndIsReusedConcurrently()
    {
        const string source = """
                              using System;
                              using System.Threading;
                              using System.Threading.Tasks;
                              using Ws2.Formats.Cbor;
                              public sealed class VersionTextConverter : ICborConverter<Version>
                              {
                                  public static int Instances;
                                  public VersionTextConverter() => Interlocked.Increment(ref Instances);
                                  public void Write(System.Formats.Cbor.CborWriter writer, Version value) => writer.WriteTextString(value.ToString());
                                  public Version Read(System.Formats.Cbor.CborReader reader) => new(reader.ReadTextString());
                              }
                              [CborArray] public partial class ConvertedModel
                              {
                                  [CborKey(0), CborConverter(typeof(VersionTextConverter))] public Version Value { get; }
                                  public ConvertedModel(Version value) => Value = value;
                                  public static string Run()
                                  {
                                      var model = new ConvertedModel(new Version(1, 2, 3));
                                      var writer = new System.Formats.Cbor.CborWriter(); model.Serialize(writer);
                                      if (writer.Encode()[1] >> 5 != 3) return "not text: " + Convert.ToHexString(writer.Encode());
                                      if (!Parse(new System.Formats.Cbor.CborReader(writer.Encode())).Value.Equals(model.Value)) return "version mismatch";
                                      var ok = 1;
                                      Parallel.For(0, 64, _ =>
                                      {
                                          var localWriter = new System.Formats.Cbor.CborWriter(); model.Serialize(localWriter);
                                          var copy = Parse(new System.Formats.Cbor.CborReader(localWriter.Encode()));
                                          if (!copy.Value.Equals(model.Value)) Interlocked.Exchange(ref ok, 0);
                                      });
                                      return ok == 1 && VersionTextConverter.Instances == 1
                                          ? "ok"
                                          : "parallel=" + ok + "; instances=" + VersionTextConverter.Instances;
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        Assert.That(result.Compilation.Emit(assemblyStream).Success, Is.True);
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborVersionConverter", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var returned = loadContext.LoadFromStream(assemblyStream).GetType("ConvertedModel")!
                .GetMethod("Run")!.Invoke(null, null);

            Assert.That(returned, Is.EqualTo("ok"));
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestNullableReferenceConverterSkipsNullAndRejectsNullForNonNullableProperty()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              public sealed class TextConverter : ICborConverter<string?>
                              {
                                  public static int Calls;
                                  public void Write(System.Formats.Cbor.CborWriter writer, string? value) { Calls++; writer.WriteTextString(value!); }
                                  public string? Read(System.Formats.Cbor.CborReader reader) { Calls++; _ = reader.ReadTextString(); return null; }
                              }
                              [CborArray] public partial class Model
                              {
                                  [CborKey(0), CborConverter(typeof(TextConverter))] public string? Value { get; }
                                  public Model(string? value) => Value = value;
                                  public static bool Run()
                                  {
                                      var nullModel = new Model(null);
                                      var nullWriter = new System.Formats.Cbor.CborWriter(); nullModel.Serialize(nullWriter);
                                      if (Parse(new System.Formats.Cbor.CborReader(nullWriter.Encode())).Value is not null || TextConverter.Calls != 0) return false;
                                      var writer = new System.Formats.Cbor.CborWriter(); new Model("value").Serialize(writer);
                                      return Parse(new System.Formats.Cbor.CborReader(writer.Encode())).Value is null && TextConverter.Calls == 2;
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        Assert.That(result.Compilation.Emit(assemblyStream).Success, Is.True);
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborNullableReferenceConverter", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var returned = loadContext.LoadFromStream(assemblyStream).GetType("Model")!.GetMethod("Run")!.Invoke(null, null);
            Assert.That(returned, Is.True);
        }
        finally
        {
            loadContext.Unload();
        }

        const string badResultSource = """
                                       #nullable enable
                                       using Ws2.Formats.Cbor;
                                       public sealed class NullConverter : ICborConverter<string> { public void Write(System.Formats.Cbor.CborWriter w, string v) => w.WriteTextString(v); public string Read(System.Formats.Cbor.CborReader r) => null!; }
                                       [CborArray] public partial class NonNullableModel
                                       {
                                           [CborKey(0), CborConverter(typeof(NullConverter))] public string Value { get; }
                                           public NonNullableModel(string value) => Value = value;
                                           public static bool Run()
                                           {
                                               var writer = new System.Formats.Cbor.CborWriter(); writer.WriteStartArray(1); writer.WriteTextString("value"); writer.WriteEndArray();
                                               try { Parse(new System.Formats.Cbor.CborReader(writer.Encode())); }
                                               catch (System.Formats.Cbor.CborContentException) { return true; }
                                               return false;
                                           }
                                       }
                                       """;

        var badResult = GeneratorHarness.Run(badResultSource, true, true);
        Assert.That(badResult.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, badResult.GeneratorDiagnostics));
        Assert.That(badResult.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, badResult.CompilationDiagnostics));

        using MemoryStream badAssemblyStream = new();
        Assert.That(badResult.Compilation.Emit(badAssemblyStream).Success, Is.True);
        badAssemblyStream.Position = 0;
        AssemblyLoadContext badLoadContext = new("CborNullConverterResult", true);
        badLoadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var returned = badLoadContext.LoadFromStream(badAssemblyStream).GetType("NonNullableModel")!
                .GetMethod("Run")!.Invoke(null, null);

            Assert.That(returned, Is.True);
        }
        finally
        {
            badLoadContext.Unload();
        }
    }

    [Test]
    public void TestAllIntegerTypesAndEnumUnderlyingBoundariesRoundTrip()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              public enum SignedEnum : sbyte { MinValue = sbyte.MinValue }
                              public enum UnsignedEnum : ulong { MaxValue = ulong.MaxValue }
                              [CborArray] public partial class IntegerModel
                              {
                                  [CborKey(0)] public byte ByteValue { get; }
                                  [CborKey(1)] public sbyte SByteValue { get; }
                                  [CborKey(2)] public short Int16Value { get; }
                                  [CborKey(3)] public ushort UInt16Value { get; }
                                  [CborKey(4)] public int Int32Value { get; }
                                  [CborKey(5)] public uint UInt32Value { get; }
                                  [CborKey(6)] public long Int64Value { get; }
                                  [CborKey(7)] public ulong UInt64Value { get; }
                                  [CborKey(8)] public SignedEnum SignedEnumValue { get; }
                                  [CborKey(9)] public UnsignedEnum UnsignedEnumValue { get; }
                                  public IntegerModel(byte byteValue, sbyte sbyteValue, short int16Value, ushort uint16Value,
                                      int int32Value, uint uint32Value, long int64Value, ulong uint64Value,
                                      SignedEnum signedEnumValue, UnsignedEnum unsignedEnumValue)
                                  {
                                      ByteValue = byteValue; SByteValue = sbyteValue; Int16Value = int16Value; UInt16Value = uint16Value;
                                      Int32Value = int32Value; UInt32Value = uint32Value; Int64Value = int64Value; UInt64Value = uint64Value;
                                      SignedEnumValue = signedEnumValue; UnsignedEnumValue = unsignedEnumValue;
                                  }
                                  public static string Run()
                                  {
                                      var model = new IntegerModel(byte.MaxValue, sbyte.MinValue, short.MinValue, ushort.MaxValue,
                                          int.MinValue, uint.MaxValue, long.MinValue, ulong.MaxValue, SignedEnum.MinValue, UnsignedEnum.MaxValue);
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      model.Serialize(writer);
                                      var bytes = writer.Encode();
                                      var copy = Parse(new System.Formats.Cbor.CborReader(bytes));
                                      if (copy.ByteValue != byte.MaxValue || copy.SByteValue != sbyte.MinValue ||
                                          copy.Int16Value != short.MinValue || copy.UInt16Value != ushort.MaxValue ||
                                          copy.Int32Value != int.MinValue || copy.UInt32Value != uint.MaxValue ||
                                          copy.Int64Value != long.MinValue || copy.UInt64Value != ulong.MaxValue ||
                                          copy.SignedEnumValue != SignedEnum.MinValue || copy.UnsignedEnumValue != UnsignedEnum.MaxValue)
                                          throw new System.Exception("Integer round-trip changed a boundary value.");
                                      return System.Convert.ToHexString(bytes);
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        Assert.That(result.Compilation.Emit(assemblyStream).Success, Is.True);
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborIntegerBoundaries", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var actual = (string)loadContext.LoadFromStream(assemblyStream).GetType("IntegerModel")!
                .GetMethod("Run")!.Invoke(null, null)!;

            Assert.That(actual,
                Is.EqualTo("8A18FF387F397FFF19FFFF3A7FFFFFFF1AFFFFFFFF3B7FFFFFFFFFFFFFFF1BFFFFFFFFFFFFFFFF387F1BFFFFFFFFFFFFFFFF"));
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestFloatingPointSpecialValuesUseCborReaderAndWriterSemantics()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class FloatModel
                              {
                                  [CborKey(0)] public float NaN { get; }
                                  [CborKey(1)] public double PositiveInfinity { get; }
                                  [CborKey(2)] public double NegativeInfinity { get; }
                                  [CborKey(3)] public double NegativeZero { get; }
                                  public FloatModel(float nan, double positiveInfinity, double negativeInfinity, double negativeZero)
                                  {
                                      NaN = nan; PositiveInfinity = positiveInfinity; NegativeInfinity = negativeInfinity; NegativeZero = negativeZero;
                                  }
                                  public static bool Run()
                                  {
                                      var model = new FloatModel(float.NaN, double.PositiveInfinity, double.NegativeInfinity, -0.0d);
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      model.Serialize(writer);
                                      var copy = Parse(new System.Formats.Cbor.CborReader(writer.Encode()));
                                      return float.IsNaN(copy.NaN) && double.IsPositiveInfinity(copy.PositiveInfinity) &&
                                          double.IsNegativeInfinity(copy.NegativeInfinity) &&
                                          System.BitConverter.DoubleToInt64Bits(copy.NegativeZero) == long.MinValue;
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        Assert.That(result.Compilation.Emit(assemblyStream).Success, Is.True);
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborFloatingPointSpecials", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var returned = loadContext.LoadFromStream(assemblyStream).GetType("FloatModel")!
                .GetMethod("Run")!.Invoke(null, null);

            Assert.That(returned, Is.True);
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestEmptyUnicodeTextAndByteStringsRoundTrip()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class TextBytesModel
                              {
                                  [CborKey(0)] public string Text { get; }
                                  [CborKey(1)] public byte[] Bytes { get; }
                                  public TextBytesModel(string text, byte[] bytes) { Text = text; Bytes = bytes; }
                                  public static bool Run()
                                  {
                                      var empty = new TextBytesModel(string.Empty, System.Array.Empty<byte>());
                                      var emptyWriter = new System.Formats.Cbor.CborWriter();
                                      empty.Serialize(emptyWriter);
                                      var emptyCopy = Parse(new System.Formats.Cbor.CborReader(emptyWriter.Encode()));
                                      if (emptyCopy.Text != string.Empty || emptyCopy.Bytes.Length != 0) return false;
                                      const string unicode = "λ雪🙂";
                                      var value = new TextBytesModel(unicode, new byte[] { 0, 127, 255 });
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      value.Serialize(writer);
                                      var copy = Parse(new System.Formats.Cbor.CborReader(writer.Encode()));
                                      return copy.Text == unicode && System.Linq.Enumerable.SequenceEqual(value.Bytes, copy.Bytes);
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        Assert.That(result.Compilation.Emit(assemblyStream).Success, Is.True);
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborUnicodeTextBytes", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var returned = loadContext.LoadFromStream(assemblyStream).GetType("TextBytesModel")!
                .GetMethod("Run")!.Invoke(null, null);

            Assert.That(returned, Is.True);
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestNullableNestedModelRoundTrip()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class ChildModel
                              {
                                  [CborKey(0)] public string Text { get; }
                                  public ChildModel(string text) => Text = text;
                              }
                              [CborArray] public partial class ParentModel
                              {
                                  [CborKey(0)] public ChildModel? Child { get; }
                                  public ParentModel(ChildModel? child) => Child = child;
                                  public static bool Run()
                                  {
                                      var nullModel = new ParentModel(null);
                                      var nullWriter = new System.Formats.Cbor.CborWriter();
                                      nullModel.Serialize(nullWriter);
                                      if (Parse(new System.Formats.Cbor.CborReader(nullWriter.Encode())).Child is not null) return false;
                                      var valueModel = new ParentModel(new ChildModel("nested"));
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      valueModel.Serialize(writer);
                                      return Parse(new System.Formats.Cbor.CborReader(writer.Encode())).Child?.Text == "nested";
                                  }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        using MemoryStream assemblyStream = new();
        Assert.That(result.Compilation.Emit(assemblyStream).Success, Is.True);
        assemblyStream.Position = 0;
        AssemblyLoadContext loadContext = new("CborNullableNestedModel", true);
        loadContext.Resolving += static (_, name) => AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        try
        {
            var returned = loadContext.LoadFromStream(assemblyStream).GetType("ParentModel")!
                .GetMethod("Run")!.Invoke(null, null);

            Assert.That(returned, Is.True);
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Test]
    public void TestSupportedCollectionsAndNullableElementsRoundTrip()
    {
        const string source = """
                              #nullable enable
                              using System.Collections.Generic;
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class CollectionsModel
                              {
                                  [CborKey(0)] public int[] Array { get; }
                                  [CborKey(1)] public List<string?> List { get; }
                                  [CborKey(2)] public IReadOnlyList<int?> ReadOnlyList { get; }
                                  [CborKey(3)] public byte[][] ByteStrings { get; }
                                  [CborKey(4)] public int[]? Optional { get; }
                                  public CollectionsModel(int[] array, List<string?> list, IReadOnlyList<int?> readOnlyList,
                                      byte[][] byteStrings, int[]? optional)
                                  { Array = array; List = list; ReadOnlyList = readOnlyList; ByteStrings = byteStrings; Optional = optional; }
                                  public static bool Run()
                                  {
                                      var model = new CollectionsModel(new[] { 1, 2 }, new List<string?> { "x", null },
                                          new int?[] { 3, null }, new[] { new byte[] { 4, 5 }, System.Array.Empty<byte>() }, null);
                                      var writer = new System.Formats.Cbor.CborWriter();
                                      model.Serialize(writer);
                                      var copy = Parse(new System.Formats.Cbor.CborReader(writer.Encode()));
                                      if (copy.Array.Length != 2 || copy.Array[1] != 2 || copy.List[0] != "x" || copy.List[1] is not null ||
                                          copy.ReadOnlyList[0] != 3 || copy.ReadOnlyList[1] is not null || copy.ByteStrings[0][1] != 5 ||
                                          copy.ByteStrings[1].Length != 0 || copy.Optional is not null) return false;

                                      var emptyWriter = new System.Formats.Cbor.CborWriter();
                                      new CollectionsModel(System.Array.Empty<int>(), new List<string?>(), System.Array.Empty<int?>(),
                                          System.Array.Empty<byte[]>(), System.Array.Empty<int>()).Serialize(emptyWriter);
                                      var empty = Parse(new System.Formats.Cbor.CborReader(emptyWriter.Encode()));
                                      return empty.Array.Length == 0 && empty.List.Count == 0 && empty.ReadOnlyList.Count == 0 &&
                                          empty.ByteStrings.Length == 0 && empty.Optional is { Length: 0 };
                                  }
                              }
                              public static class EntryPoint { public static bool Run() => CollectionsModel.Run(); }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, result.GeneratorDiagnostics));
        Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, result.CompilationDiagnostics));

        Assert.That(result.GeneratedSources.Any(static generated => generated.Contains("ToArray(this.", StringComparison.Ordinal)), Is.False);
        Assert.That(RunGeneratedSource([source]), Is.EqualTo("True"));
    }

    [Test]
    public void TestUnsupportedCollectionShapesAreDiagnosedAndByteJaggedArrayIsAccepted()
    {
        var unsupportedTypes = new[]
        {
            "System.Collections.Generic.HashSet<int>", "System.Collections.Generic.Dictionary<int, int>",
            "System.Collections.Generic.IEnumerable<int>", "System.Collections.Generic.List<int>[]", "int[,]", "CustomCollection"
        };

        foreach (var type in unsupportedTypes)
        {
            var source = $$"""
                           using Ws2.Formats.Cbor;
                           public sealed class CustomCollection : System.Collections.Generic.List<int> { }
                           [CborArray] public partial class Bad
                           {
                               [CborKey(0)] public {{type}} Value { get; }
                               public Bad({{type}} value) => Value = value;
                           }
                           """;

            var result = GeneratorHarness.Run(source, true, true);
            Assert.That(result.GeneratorDiagnostics.Select(static diagnostic => diagnostic.Id), Does.Contain("SBCBOR002"), type);
            Assert.That(result.GeneratedSources, Is.Empty, type);
        }

        const string supported = """
                                 using Ws2.Formats.Cbor;
                                 [CborArray] public partial class ByteStrings
                                 {
                                     [CborKey(0)] public byte[][] Values { get; }
                                     public ByteStrings(byte[][] values) => Values = values;
                                 }
                                 """;

        var accepted = GeneratorHarness.Run(supported, true, true);
        Assert.That(accepted.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, accepted.GeneratorDiagnostics));
        Assert.That(accepted.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, accepted.CompilationDiagnostics));
    }

    [Test]
    public void TestNamespacedNullableRecursiveModelsRoundTrip()
    {
        const string source = """
                              #nullable enable
                              using Ws2.Formats.Cbor;
                              namespace Example.Models
                              {
                                  [CborMap] public partial class Node
                                  {
                                      [CborKey("value")] public string Value { get; }
                                      [CborKey("next")] public Node? Next { get; }
                                      public Node(string value, Node? next) { Value = value; Next = next; }
                                  }
                              }
                              public static class EntryPoint
                              {
                                  public static bool Run()
                                  {
                                      var root = new Example.Models.Node("one", new Example.Models.Node("two", null));
                                      var writer = new System.Formats.Cbor.CborWriter(); root.Serialize(writer);
                                      var copy = Example.Models.Node.Parse(new System.Formats.Cbor.CborReader(writer.Encode()));
                                      return copy.Value == "one" && copy.Next?.Value == "two" && copy.Next.Next is null;
                                  }
                              }
                              """;

        Assert.That(RunGeneratedSource([source]), Is.EqualTo("True"));
    }

    [Test]
    public void TestNestedModelCanBeConsumedFromAnotherAssembly()
    {
        const string childSource = """
                                   using Ws2.Formats.Cbor;
                                   namespace Shared.Models
                                   {
                                       [CborArray] public partial class Child
                                       {
                                           [CborKey(0)] public int Value { get; }
                                           public Child(int value) => Value = value;
                                       }
                                   }
                                   """;

        var childResult = GeneratorHarness.Run(childSource, true, true);
        Assert.That(childResult.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, childResult.GeneratorDiagnostics));
        Assert.That(childResult.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, childResult.CompilationDiagnostics));

        using MemoryStream childAssembly = new();
        Assert.That(childResult.Compilation.Emit(childAssembly).Success, Is.True);

        const string parentSource = """
                                    using Ws2.Formats.Cbor;
                                    [CborArray] public partial class Parent
                                    {
                                        [CborKey(0)] public Shared.Models.Child? Child { get; }
                                        public Parent(Shared.Models.Child? child) => Child = child;
                                    }
                                    """;

        var parentResult = GeneratorHarness.Run(parentSource, true, true,
            [MetadataReference.CreateFromImage(childAssembly.ToArray())]);

        Assert.That(parentResult.GeneratorDiagnostics, Is.Empty, string.Join(Environment.NewLine, parentResult.GeneratorDiagnostics));
        Assert.That(parentResult.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
            string.Join(Environment.NewLine, parentResult.CompilationDiagnostics));

        Assert.That(parentResult.GeneratedSources, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestCollectionMalformedInputFailsWithoutDeclaredLengthPreallocation()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class Values
                              {
                                  [CborKey(0)] public int[] Items { get; }
                                  public Values(int[] items) => Items = items;
                                  public static bool Run()
                                  {
                                      var largeLengthRejected = false;
                                      try { Parse(new System.Formats.Cbor.CborReader(new byte[] { 0x81, 0x1a, 0x7f, 0xff, 0xff, 0xff })); }
                                      catch (System.InvalidOperationException) { largeLengthRejected = true; }
                                      var indefiniteLengthRejected = false;
                                      try { Parse(new System.Formats.Cbor.CborReader(new byte[] { 0x81, 0x9f, 0xff })); }
                                      catch (System.Formats.Cbor.CborContentException) { indefiniteLengthRejected = true; }
                                      return largeLengthRejected && indefiniteLengthRejected;
                                  }
                              }
                              public static class EntryPoint { public static bool Run() => Values.Run(); }
                              """;

        Assert.That(RunGeneratedSource([source]), Is.EqualTo("True"));
    }

    [Test]
    public void TestInvalidNestedModelSuppressesDependentSources()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborMap] public class Invalid { }
                              [CborArray] public partial class Direct
                              {
                                  [CborKey(0)] public Invalid Item { get; set; } = new();
                              }
                              [CborArray] public partial class Collection
                              {
                                  [CborKey(0)] public Invalid[] Items { get; set; } = [];
                              }
                              [CborArray] public partial class Transitive
                              {
                                  [CborKey(0)] public Direct? Next { get; set; }
                              }
                              [CborMap] public partial class Independent { }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        Assert.That(result.GeneratorDiagnostics, Has.Count.EqualTo(4));
        AssertDiagnosticHasPreciseSpan(result, "SBCBOR001", source, "Invalid");
        Assert.That(result.GeneratorDiagnostics.Where(d => d.Id == "SBCBOR002")
            .Select(d => source.Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length)),
            Is.EquivalentTo(["Item", "Items", "Next"]));
        Assert.That(result.GeneratedSources.Single(), Does.Contain("partial class Independent"));
        Assert.That(result.CompilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty);
    }

    [Test]
    public void TestReferencedNestedModelWithoutSerializationMethodsIsDiagnosed()
    {
        const string external = "using Ws2.Formats.Cbor; namespace External; [CborMap] public class Child { }";
        var compilation = CSharpCompilation.Create("ExternalModels",
            [CSharpSyntaxTree.ParseText(external)], GeneratorHarness.CreateReferences(true, true),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using MemoryStream assembly = new();
        Assert.That(compilation.Emit(assembly).Success, Is.True);

        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class Parent
                              {
                                  [CborKey(0)] public External.Child? Value { get; set; }
                              }
                              """;
        var result = GeneratorHarness.Run(source, true, true,
            [MetadataReference.CreateFromImage(assembly.ToArray())]);
        AssertDiagnosticWithoutGeneratedSource(result, "SBCBOR002", source, "Value");
        Assert.That(result.CompilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty);
    }

    [Test]
    public void TestMatchingConstructorTakesPriorityOverPropertyInitializer()
    {
        const string source = """
                              using System.Formats.Cbor;
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class Model
                              {
                                  [CborKey(0)] public int Value { get; set; }
                                  public bool UsedMatchingConstructor { get; }
                                  public Model() { }
                                  public Model(int value) { Value = value; UsedMatchingConstructor = true; }
                              }
                              public static class EntryPoint
                              {
                                  public static bool Run()
                                  {
                                      var writer = new CborWriter();
                                      new Model { Value = 42 }.Serialize(writer);
                                      var parsed = Model.Parse(new CborReader(writer.Encode()));
                                      return parsed.UsedMatchingConstructor && parsed.Value == 42;
                                  }
                              }
                              """;

        Assert.That(RunGeneratedSource([source]), Is.EqualTo("True"));
    }

    [TestCase("private")]
    [TestCase("protected")]
    public void TestParameterlessConstructionRejectsNonPublicSetter(string access)
    {
        var source = $$"""
                       using Ws2.Formats.Cbor;
                       [CborArray] public partial class Model
                       {
                           [CborKey(0)] public int Value { get; {{access}} set; }
                       }
                       """;
        AssertDiagnosticWithoutGeneratedSource(GeneratorHarness.Run(source, true, true), "SBCBOR010", source, "Model");
    }

    [TestCase("CborMap", "CborKey(\"same\")", "CborKey(\"same\")", "CborKey(\"same\")", 1)]
    [TestCase("CborArray", "CborKey(0)", "CborKey(0)", "CborKey(0)", 1)]
    [TestCase("CborArray", "CborKey(0)", "CborKey(2)", "CborKey(0)", 0)]
    [TestCase("CborMap", "CborKey(\"first\")", "CborKey(0)", "CborKey(0)", 0)]
    [TestCase("CborArray", "CborKey(\"first\")", "CborKey(\"second\")", "CborKey(\"first\")", 0)]
    public void TestInvalidKeyDeclarationsPreventGeneration(string format, string first, string second, string location, int occurrence)
    {
        var source = $$"""
                       using Ws2.Formats.Cbor;
                       [{{format}}] public partial class Model
                       {
                           [{{first}}] public int First { get; set; }
                           [{{second}}] public int Second { get; set; }
                       }
                       """;
        AssertDiagnosticWithoutGeneratedSource(GeneratorHarness.Run(source, true, true), "SBCBOR005", source, location, occurrence);
    }

    [Test]
    public void TestReadOnlyMemoryWritesOnlyTheSliceAsByteString()
    {
        const string source = """
                              using System;
                              using System.Formats.Cbor;
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class Model
                              {
                                  [CborKey(0)] public ReadOnlyMemory<byte> Value { get; set; }
                              }
                              public static class EntryPoint
                              {
                                  public static bool Run()
                                  {
                                      return Check(new ReadOnlyMemory<byte>(new byte[] { 9, 1, 2, 8 }, 1, 2), new byte[] { 1, 2 }) &&
                                          Check(ReadOnlyMemory<byte>.Empty, Array.Empty<byte>());
                                  }

                                  private static bool Check(ReadOnlyMemory<byte> value, byte[] expected)
                                  {
                                      var writer = new CborWriter();
                                      new Model { Value = value }.Serialize(writer);
                                      var bytes = writer.Encode();
                                      var reader = new CborReader(bytes);
                                      if (reader.ReadStartArray() != 1 || reader.PeekState() != CborReaderState.ByteString ||
                                          !reader.ReadByteString().AsSpan().SequenceEqual(expected))
                                          return false;
                                      reader.ReadEndArray();
                                      var input = new CborReader(bytes);
                                      return reader.BytesRemaining == 0 && Model.Parse(input).Value.Span.SequenceEqual(expected) &&
                                          input.BytesRemaining == 0;
                                  }
                              }
                              """;

        Assert.That(RunGeneratedSource([source]), Is.EqualTo("True"));
    }

    [Test]
    public void TestPartialAndUnsupportedPropertyDiagnosticsHavePreciseLocations()
    {
        const string nonPartial = "using Ws2.Formats.Cbor; [CborArray] public class NonPartial { }";
        var nonPartialResult = GeneratorHarness.Run(nonPartial, true, true);
        AssertDiagnosticWithoutGeneratedSource(nonPartialResult, "SBCBOR001", nonPartial, "NonPartial");

        const string unsupportedProperty = """
                                           using Ws2.Formats.Cbor;
                                           [CborArray] public partial class Unsupported
                                           {
                                               [CborKey(0)] public object Value { get; }
                                               public Unsupported(object value) => Value = value;
                                           }
                                           """;

        var unsupportedResult = GeneratorHarness.Run(unsupportedProperty, true, true);
        AssertDiagnosticWithoutGeneratedSource(unsupportedResult, "SBCBOR002", unsupportedProperty, "Value");
    }

    private static void AssertDiagnosticWithoutGeneratedSource(
        GeneratorRun result, string id, string source, string locationText,
        int occurrence = 0
    )
    {
        AssertDiagnosticHasPreciseSpan(result, id, source, locationText, occurrence);
        Assert.That(result.GeneratedSources, Is.Empty);
    }

    private static void AssertDiagnosticHasPreciseSpan(
        GeneratorRun result, string id, string source, string locationText,
        int occurrence = 0
    )
    {
        var diagnostic = result.GeneratorDiagnostics.Single(item => item.Id == id);
        var expectedStart = -1;
        for (var i = 0; i <= occurrence; i++)
        {
            expectedStart = source.IndexOf(locationText, expectedStart + 1, StringComparison.Ordinal);
        }

        Assert.That(expectedStart, Is.GreaterThanOrEqualTo(0));
        Assert.That(diagnostic.Location.SourceSpan.Start, Is.EqualTo(expectedStart));
        Assert.That(diagnostic.Location.SourceSpan.Length, Is.EqualTo(locationText.Length));
    }

    [Test]
    public Task TestGeneratedSourcesSnapshot()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class SnapshotModel
                              {
                                  [CborKey(0)] public int Number { get; }
                                  [CborKey(1)] public string Text { get; }
                                  public SnapshotModel(int number, string text) { Number = number; Text = text; }
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        return Verify(result.GeneratedSources);
    }

    [Test]
    public void TestMissingRuntimeApiDiagnosticHasStableIdAndLocation()
    {
        const string source = "[Ws2.Formats.Cbor.CborArray] public partial class Model { }";
        var result = GeneratorHarness.Run(source, false, true);
        AssertDiagnosticWithoutGeneratedSource(result, "SBCBOR006", source, "Ws2.Formats.Cbor.CborArray");
    }

    [Test]
    public void TestMissingCborLibraryDiagnosticHasStableIdAndLocation()
    {
        const string source = "[Ws2.Formats.Cbor.CborArray] public partial class Model { }";
        var result = GeneratorHarness.Run(source, true, false);
        AssertDiagnosticWithoutGeneratedSource(result, "SBCBOR007", source, "Ws2.Formats.Cbor.CborArray");
    }

    [Test]
    public void TestInvalidConverterDiagnosticHasStableIdAndLocation()
    {
        const string source = """
                              using Ws2.Formats.Cbor;
                              [CborArray] public partial class Model
                              {
                                  [CborKey(0), CborConverter(typeof(string))] public int Value { get; }
                                  public Model(int value) => Value = value;
                              }
                              """;

        var result = GeneratorHarness.Run(source, true, true);
        AssertDiagnosticWithoutGeneratedSource(result, "SBCBOR004", source, "CborConverter(typeof(string))");
    }

    [Test]
    public void TestExternalConverterCanBeSharedBySeparateConsumerCompilations()
    {
        const string converterSource = """
                                       #nullable enable
                                       using Ws2.Formats.Cbor;
                                       public sealed class Value { public int Number { get; } public Value(int number) => Number = number; }
                                       public sealed class ValueConverter : ICborConverter<Value>
                                       {
                                           public static int Instances;
                                           public ValueConverter() => System.Threading.Interlocked.Increment(ref Instances);
                                           public void Write(System.Formats.Cbor.CborWriter writer, Value value) => writer.WriteInt32(value.Number);
                                           public Value Read(System.Formats.Cbor.CborReader reader) => new(reader.ReadInt32());
                                       }
                                       """;

        var baseReferences = GeneratorHarness.CreateReferences(true, true);
        var converterCompilation = CSharpCompilation.Create("ExternalConverter",
            [CSharpSyntaxTree.ParseText(converterSource, new CSharpParseOptions(LanguageVersion.CSharp12))],
            baseReferences, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream converterStream = new();
        var converterEmit = converterCompilation.Emit(converterStream);
        Assert.That(converterEmit.Success, Is.True, string.Join(Environment.NewLine, converterEmit.Diagnostics));
        var converterReference = MetadataReference.CreateFromImage(converterStream.ToArray());

        foreach (var modelName in new[] { "FirstConsumer", "SecondConsumer" })
        {
            var source = $$"""
                           using System.Threading;
                           using System.Threading.Tasks;
                           using Ws2.Formats.Cbor;
                           [CborArray] public partial class {{modelName}}
                           {
                               [CborKey(0), CborConverter(typeof(ValueConverter))] public Value Item { get; }
                               public {{modelName}}(Value item) => Item = item;
                               public static bool Run()
                               {
                                   var value = new {{modelName}}(new Value(42));
                                   var writer = new System.Formats.Cbor.CborWriter(); value.Serialize(writer);
                                   if (Parse(new System.Formats.Cbor.CborReader(writer.Encode())).Item.Number != 42) return false;
                                   var ok = 1;
                                   Parallel.For(0, 64, _ =>
                                   {
                                       var localWriter = new System.Formats.Cbor.CborWriter(); value.Serialize(localWriter);
                                       if (Parse(new System.Formats.Cbor.CborReader(localWriter.Encode())).Item.Number != 42) Interlocked.Exchange(ref ok, 0);
                                   });
                                   return ok == 1 && ValueConverter.Instances == 1;
                               }
                           }
                           """;

            var result = GeneratorHarness.Run(source, true, true,
                [converterReference]);

            Assert.That(result.GeneratorDiagnostics, Is.Empty);
            Assert.That(result.CompilationDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Is.Empty,
                string.Join(Environment.NewLine, result.CompilationDiagnostics));

            if (modelName == "FirstConsumer")
            {
                using MemoryStream consumerStream = new();
                Assert.That(result.Compilation.Emit(consumerStream).Success, Is.True);
                consumerStream.Position = 0;
                var externalBytes = converterStream.ToArray();
                AssemblyLoadContext loadContext = new("ExternalCborConverterConsumer", true);
                loadContext.Resolving += (_, name) =>
                {
                    if (name.Name == "ExternalConverter")
                    {
                        return loadContext.LoadFromStream(new MemoryStream(externalBytes));
                    }

                    return AssemblyLoadContext.Default.LoadFromAssemblyName(name);
                };

                try
                {
                    var returned = loadContext.LoadFromStream(consumerStream).GetType(modelName)!
                        .GetMethod("Run")!.Invoke(null, null);

                    Assert.That(returned, Is.True);
                }
                finally
                {
                    loadContext.Unload();
                }
            }
        }
    }
}

internal sealed record GeneratorRun(
    CSharpCompilation Compilation,
    IReadOnlyList<Diagnostic> GeneratorDiagnostics,
    IReadOnlyList<Diagnostic> CompilationDiagnostics,
    IReadOnlyList<string> GeneratedSources,
    IReadOnlyList<string> HintNames
);

internal static class GeneratorHarness
{
    public static GeneratorRun Run(
        string source, bool includeAbstractions, bool includeCbor,
        IEnumerable<MetadataReference>? additionalReferences = null
    )
    {
        return Run([source], includeAbstractions, includeCbor, additionalReferences);
    }

    public static GeneratorRun Run(
        IEnumerable<string> sources, bool includeAbstractions, bool includeCbor,
        IEnumerable<MetadataReference>? additionalReferences = null
    )
    {
        var references = CreateReferences(includeAbstractions, includeCbor);
        if (additionalReferences is not null)
        {
            references.AddRange(additionalReferences);
        }

        var syntaxTrees = sources.Select((source, index) => CSharpSyntaxTree.ParseText(source,
            new CSharpParseOptions(LanguageVersion.CSharp12), "Source" + index + ".cs")).ToArray();

        var compilation = CSharpCompilation.Create("CborGeneratedConsumer", syntaxTrees, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new CborSerializationIncrementalGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation,
            out var driverDiagnostics);

        var runResult = driver.GetRunResult();
        var generatedSources = runResult.Results.SelectMany(static result => result.GeneratedSources).ToArray();
        return new GeneratorRun((CSharpCompilation)outputCompilation,
            [.. runResult.Diagnostics.Concat(driverDiagnostics).Distinct()], outputCompilation.GetDiagnostics(),
            [.. generatedSources.Select(static generated => generated.SourceText.ToString())],
            [.. generatedSources.Select(static generated => generated.HintName)]);
    }

    public static List<MetadataReference> CreateReferences(bool includeAbstractions, bool includeCbor)
    {
        var frameworkMajor = Environment.Version.Major;
        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? "/usr/share/dotnet";
        var referencePackRoot = Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref");
        var referencePack = Directory.GetDirectories(referencePackRoot, frameworkMajor + ".0.*")
            .OrderByDescending(path => Version.Parse(Path.GetFileName(path)))
            .First();

        var referenceDirectory = Path.Combine(referencePack, "ref", "net" + frameworkMajor + ".0");
        var references = Directory.GetFiles(referenceDirectory, "*.dll")
            .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToList();

        AddReference(typeof(CborArrayAttribute).Assembly.Location, includeAbstractions);
        AddReference(typeof(CborReader).Assembly.Location, includeCbor);
        return references;

        void AddReference(string path, bool include)
        {
            if (include && references.All(reference => reference.Display != path))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }
    }
}