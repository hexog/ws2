using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ws2.Formats.Cbor;

[Generator]
public sealed class CborSerializationIncrementalGenerator : IIncrementalGenerator
{
    private const string ArrayAttributeName = "Ws2.Formats.Cbor.CborArrayAttribute";
    private const string MapAttributeName = "Ws2.Formats.Cbor.CborMapAttribute";
    private const string ConverterAttributeName = "Ws2.Formats.Cbor.CborConverterAttribute";
    private const string KeyAttributeName = "Ws2.Formats.Cbor.CborKeyAttribute";

    private static readonly DiagnosticDescriptor MustBePartial = new(
        "SBCBOR001", "CBOR model must be partial", "CBOR model '{0}' must be declared partial",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor UnsupportedType = new(
        "SBCBOR002", "Unsupported CBOR property type",
        "{0}",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor ConflictingFormats = new(
        "SBCBOR003", "Conflicting CBOR serialization formats",
        "CBOR model '{0}' cannot have both CborArray and CborMap attributes",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor InvalidConverter = new(
        "SBCBOR004", "Invalid CBOR property converter", "{0}",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor InvalidKey = new(
        "SBCBOR005", "Invalid CBOR property key", "{0}",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor MissingRuntimeApi = new(
        "SBCBOR006", "CBOR runtime API is missing",
        "CBOR model attributes and converters require a reference to Ws2.Formats.Cbor.Abstractions",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor MissingCbor = new(
        "SBCBOR007", "System.Formats.Cbor is missing",
        "CBOR model generation requires a reference to System.Formats.Cbor",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor UnsupportedModel = new(
        "SBCBOR008", "Unsupported CBOR model", "{0}",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor InvalidMember = new(
        "SBCBOR009", "Invalid CBOR member", "{0}",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor InvalidConstructor = new(
        "SBCBOR010", "Invalid CBOR constructor", "{0}",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor GeneratedMethodConflict = new(
        "SBCBOR011", "CBOR generated method conflicts with model member",
        "{0}",
        "Ws2.Formats.Cbor", DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, static (productionContext, compilation) =>
        {
            var missingReference = GetMissingReference(compilation);
            if (missingReference is not null)
            {
                ReportMissingReference(productionContext, compilation, missingReference, "CborArray", "CborMap");
            }
        });

        RegisterModels(context, ArrayAttributeName, CborSerializationFormat.Array);
        RegisterModels(context, MapAttributeName, CborSerializationFormat.Map);
    }

    private static DiagnosticDescriptor? GetMissingReference(Compilation compilation)
    {
        string[] runtimeTypes =
        [
            ArrayAttributeName, MapAttributeName, ConverterAttributeName, KeyAttributeName,
            "Ws2.Formats.Cbor.ICborConverter`1"
        ];

        if (runtimeTypes.Any(name =>
            compilation.GetTypeByMetadataName(name)?.ContainingAssembly.Name != "Ws2.Formats.Cbor.Abstractions"))
        {
            return MissingRuntimeApi;
        }

        return compilation.GetTypeByMetadataName("System.Formats.Cbor.CborWriter") is null ||
            compilation.GetTypeByMetadataName("System.Formats.Cbor.CborReader") is null ? MissingCbor : null;
    }

    private static void ReportMissingReference(
        SourceProductionContext context, Compilation compilation, DiagnosticDescriptor descriptor,
        params string[] attributeNames
    )
    {
        foreach (var tree in compilation.SyntaxTrees)
        {
            var root = tree.GetRoot(context.CancellationToken);
            var attribute = root.DescendantNodes().OfType<AttributeSyntax>().FirstOrDefault(candidate =>
                attributeNames.Any(name => candidate.Name.ToString().EndsWith(name, StringComparison.Ordinal)));

            if (attribute is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, attribute.GetLocation()));
                return;
            }
        }
    }

    private static void RegisterModels(IncrementalGeneratorInitializationContext context, string attributeName, CborSerializationFormat format)
    {
        var analysis = context.SyntaxProvider.ForAttributeWithMetadataName(
            attributeName,
            static (node, _) => node is TypeDeclarationSyntax,
            (syntaxContext, cancellationToken) => CreateModel(
                (TypeDeclarationSyntax)syntaxContext.TargetNode,
                (INamedTypeSymbol)syntaxContext.TargetSymbol,
                format, syntaxContext.SemanticModel.Compilation, cancellationToken));

        var models = analysis
            .Where(static result => result.Model is not null)
            .Select(static (result, _) => result.Model!)
            .WithComparer(EqualityComparer<SerializationContext>.Default)
            .WithTrackingName("CborModelAnalysis");

        context.RegisterSourceOutput(models, static (productionContext, model) => Generate(productionContext, model));
        // Keep the current Roslyn Location with the analysis result. A tree can change without moving the diagnostic.
        context.RegisterSourceOutput(analysis.Where(static result => result.Diagnostic is not null),
            static (productionContext, result) => productionContext.ReportDiagnostic(result.Diagnostic!));
    }

    private static ModelAnalysis CreateModel(
        TypeDeclarationSyntax syntax, INamedTypeSymbol symbol, CborSerializationFormat format,
        Compilation compilation, CancellationToken cancellationToken
    )
    {
        if (GetMissingReference(compilation) is not null)
        {
            return new ModelAnalysis();
        }

        var diagnostic = AnalyzeModel(syntax, symbol, format, compilation, cancellationToken,
            out var selectedProperties, out var properties, out var constructorPropertyIndexes);
        if (diagnostic is not null)
        {
            return diagnostic.Id == ConflictingFormats.Id && format == CborSerializationFormat.Map
                ? new ModelAnalysis()
                : new ModelAnalysis(diagnostic: diagnostic);
        }

        foreach ((var property, var dependency) in GetModelDependencies(selectedProperties, properties, compilation))
        {
            var invalid = FindInvalidDependency(symbol, dependency, compilation, cancellationToken);
            if (invalid is not null)
            {
                return new ModelAnalysis(diagnostic: Diagnostic.Create(UnsupportedType, property.Locations.FirstOrDefault(),
                    $"Property '{property.Name}' depends on CBOR model '{invalid.ToDisplayString()}', which does not provide usable serialization methods."));
            }
        }

        var metadataIdentity = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var identityBytes = Encoding.UTF8.GetBytes(metadataIdentity);
        using var sha256 = SHA256.Create();
        var hash = BitConverter.ToString(sha256.ComputeHash(identityBytes)).Replace("-", string.Empty).ToLowerInvariant();
        string hintStem = new([.. metadataIdentity.Select(static character => char.IsLetterOrDigit(character) ? character : '_')]);
        string hintName = hintStem + "." + hash + ".Cbor.g.cs";
        var mapPropertyIndexes = format == CborSerializationFormat.Map
            ? properties.Select((property, index) => (property, index))
                .OrderBy(static item => item.property.CborMapKey!, CborSourceEmitter.CanonicalTextKeyComparer.Instance)
                .Select(static item => item.index)
            : [];

        return new ModelAnalysis(new SerializationContext(
            symbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : symbol.ContainingNamespace.ToDisplayString(),
            symbol.Name, symbol.IsRecord ? "record" : "class", format, new EquatableList<SerializableProperty>(properties),
            new EquatableList<int>(constructorPropertyIndexes ?? []),
            new EquatableList<int>(mapPropertyIndexes), hintName,
            GetGeneratedPrefix(hash.Substring(0, 12), symbol, properties),
            constructorPropertyIndexes is null));
    }

    private static Diagnostic? AnalyzeModel(
        TypeDeclarationSyntax syntax, INamedTypeSymbol symbol, CborSerializationFormat format,
        Compilation compilation, CancellationToken cancellationToken, out IPropertySymbol[] selectedProperties,
        out SerializableProperty[] properties, out int[]? constructorPropertyIndexes
    )
    {
        selectedProperties = [];
        properties = [];
        constructorPropertyIndexes = null;
        return ValidateDeclaration(syntax, symbol, compilation, cancellationToken) ??
            AnalyzeProperties(symbol, format, compilation, cancellationToken, out selectedProperties, out properties) ??
            AnalyzeConstruction(syntax, symbol, selectedProperties, compilation, cancellationToken, out constructorPropertyIndexes);
    }

    private static IEnumerable<(IPropertySymbol Property, INamedTypeSymbol Dependency)> GetModelDependencies(
        IPropertySymbol[] selectedProperties, SerializableProperty[] properties, Compilation compilation
    )
    {
        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            if (property.ConverterTypeName is not null)
            {
                continue;
            }

            var member = selectedProperties[i];
            if (property.Kind == CborValueKind.Nested)
            {
                yield return (member, (INamedTypeSymbol)member.Type);
            }
            else if (property.Kind == CborValueKind.Collection && property.ElementKind == CborValueKind.Nested)
            {
                yield return (member, (INamedTypeSymbol)GetCollectionType(member.Type, compilation)!.Value.Element);
            }
        }
    }

    private static INamedTypeSymbol? FindInvalidDependency(
        INamedTypeSymbol root, INamedTypeSymbol dependency, Compilation compilation, CancellationToken cancellationToken
    )
    {
        HashSet<INamedTypeSymbol> visited = new(SymbolEqualityComparer.Default) { root };
        Stack<INamedTypeSymbol> pending = new();
        pending.Push(dependency);
        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = pending.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(current.ContainingAssembly, compilation.Assembly))
            {
                if (!HasSerializationMethods(current, root, compilation))
                {
                    return current;
                }

                continue;
            }

            var array = FindAttribute(current, compilation.GetTypeByMetadataName(ArrayAttributeName));
            var marker = array ?? FindAttribute(current, compilation.GetTypeByMetadataName(MapAttributeName));
            if (marker?.ApplicationSyntaxReference?.GetSyntax(cancellationToken).FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } declaration ||
                AnalyzeModel(declaration, current, array is null ? CborSerializationFormat.Map : CborSerializationFormat.Array,
                    compilation, cancellationToken, out var selected, out var properties, out _) is not null)
            {
                return current;
            }

            foreach ((_, var nested) in GetModelDependencies(selected, properties, compilation))
            {
                pending.Push(nested);
            }
        }

        return null;
    }

    private static bool HasSerializationMethods(INamedTypeSymbol type, INamedTypeSymbol root, Compilation compilation)
    {
        return HasMethod("Serialize", "System.Formats.Cbor.CborWriter", false, compilation.GetSpecialType(SpecialType.System_Void)) &&
            HasMethod("Parse", "System.Formats.Cbor.CborReader", true, type);

        bool HasMethod(string name, string parameterName, bool isStatic, ITypeSymbol returnType)
        {
            var parameterType = compilation.GetTypeByMetadataName(parameterName);
            return type.GetMembers(name).OfType<IMethodSymbol>().Any(method =>
                method.MethodKind == MethodKind.Ordinary && method.Arity == 0 && method.IsStatic == isStatic &&
                method.RefKind == RefKind.None && method.Parameters.Length == 1 && method.Parameters[0].RefKind == RefKind.None &&
                SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, parameterType) &&
                SymbolEqualityComparer.Default.Equals(method.ReturnType, returnType) &&
                compilation.IsSymbolAccessibleWithin(method, root, type));
        }
    }

    private static Diagnostic? ValidateDeclaration(
        TypeDeclarationSyntax syntax, INamedTypeSymbol symbol, Compilation compilation, CancellationToken cancellationToken
    )
    {
        if (symbol.ContainingType is not null || symbol.IsAbstract || symbol.IsStatic || symbol.Arity != 0 ||
            symbol.TypeKind != TypeKind.Class || symbol.BaseType?.SpecialType != SpecialType.System_Object || symbol.IsFileLocal)
        {
            return Diagnostic.Create(UnsupportedModel, syntax.Identifier.GetLocation(),
                "CBOR models must be top-level, non-generic, non-abstract, non-static partial classes or record classes.");
        }

        var arrayAttribute = FindAttribute(symbol, compilation.GetTypeByMetadataName(ArrayAttributeName));
        if (arrayAttribute is not null && HasAttribute(symbol, compilation.GetTypeByMetadataName(MapAttributeName)))
        {
            return Diagnostic.Create(ConflictingFormats,
                arrayAttribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation(), symbol.Name);
        }

        var conflict = FindConflict("Serialize", compilation.GetTypeByMetadataName("System.Formats.Cbor.CborWriter")) ??
            FindConflict("Parse", compilation.GetTypeByMetadataName("System.Formats.Cbor.CborReader"));

        if (conflict is not null)
        {
            return Diagnostic.Create(GeneratedMethodConflict, conflict.Locations.FirstOrDefault(),
                $"CBOR model '{symbol.Name}' declares a member that conflicts with generated method '{conflict.Name}'");
        }

        if (symbol.Name is "Parse" or "Serialize")
        {
            return Diagnostic.Create(GeneratedMethodConflict, syntax.Identifier.GetLocation(),
                $"CBOR model '{symbol.Name}' has the same name as generated method '{symbol.Name}'");
        }

        return syntax.Modifiers.Any(SyntaxKind.PartialKeyword)
            ? null
            : Diagnostic.Create(MustBePartial, syntax.Identifier.GetLocation(), symbol.Name);

        ISymbol? FindConflict(string name, INamedTypeSymbol? parameterType)
        {
            return symbol.GetMembers(name).FirstOrDefault(member => member is not IMethodSymbol method ||
                method.Arity == 0 && method.Parameters.Length == 1 && method.Parameters[0].RefKind == RefKind.None &&
                SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, parameterType));
        }
    }

    private static Diagnostic? AnalyzeProperties(
        INamedTypeSymbol symbol, CborSerializationFormat format, Compilation compilation, CancellationToken cancellationToken,
        out IPropertySymbol[] selectedProperties, out SerializableProperty[] properties
    )
    {
        var keyAttributeType = compilation.GetTypeByMetadataName(KeyAttributeName);
        var converterAttributeType = compilation.GetTypeByMetadataName(ConverterAttributeName);
        var members = symbol.GetMembers();
        ISymbol[] selectedMembers = [.. members.Where(member => HasAttribute(member, keyAttributeType))];
        selectedProperties = [];
        properties = [];

        var badMember = selectedMembers.FirstOrDefault(member => member is not IPropertySymbol);
        if (badMember is not null)
        {
            return Diagnostic.Create(InvalidMember, badMember.Locations.FirstOrDefault(),
                $"CBOR member '{badMember.Name}' must be a property.");
        }

        foreach (var propertySymbol in members.OfType<IPropertySymbol>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var converterAttribute = FindAttribute(propertySymbol, converterAttributeType);
            if (converterAttribute is not null && !HasAttribute(propertySymbol, keyAttributeType))
            {
                return Diagnostic.Create(InvalidMember, converterAttribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation(),
                    $"CborConverter on property '{propertySymbol.Name}' requires CborKey.");
            }
        }

        var invalidSelectedProperty = selectedMembers.OfType<IPropertySymbol>().FirstOrDefault(property =>
            property.IsStatic || property.GetMethod is null || property.Parameters.Length != 0 || property.RefKind != RefKind.None ||
            property.ExplicitInterfaceImplementations.Length != 0);

        if (invalidSelectedProperty is not null)
        {
            return Diagnostic.Create(InvalidMember, invalidSelectedProperty.Locations.FirstOrDefault(),
                $"Selected CBOR property '{invalidSelectedProperty.Name}' must have an instance getter, no index parameters, and no explicit interface implementation or ref return.");
        }

        selectedProperties =
        [
            .. selectedMembers.OfType<IPropertySymbol>()
                .OrderBy(static property => property.Locations.FirstOrDefault()?.SourceTree?.FilePath, StringComparer.Ordinal)
                .ThenBy(static property => property.Locations.FirstOrDefault()?.SourceSpan.Start ?? int.MaxValue)
        ];

        var converterInterface = compilation.GetTypeByMetadataName("Ws2.Formats.Cbor.ICborConverter`1");
        List<CborPropertyAnalysis> analyzedProperties = [];
        foreach (var property in selectedProperties)
        {
            cancellationToken.ThrowIfCancellationRequested();
            analyzedProperties.Add(CreateProperty(property, keyAttributeType, converterAttributeType, converterInterface,
                symbol, compilation, cancellationToken));
        }

        var invalidKey = analyzedProperties.FirstOrDefault(static property => property.KeyError is not null);
        if (invalidKey is not null)
        {
            return Diagnostic.Create(InvalidKey, invalidKey.KeyLocation,
                invalidKey.KeyError!);
        }

        if (format == CborSerializationFormat.Array && analyzedProperties.Any(static property => property.Property.CborArrayIndex.HasValue))
        {
            var unindexed = analyzedProperties.FirstOrDefault(static property => !property.Property.CborArrayIndex.HasValue);
            if (unindexed is not null)
            {
                return Diagnostic.Create(InvalidKey, unindexed.Location,
                    $"Every property on CBOR array model '{symbol.Name}' must specify CborKey when any property does.");
            }

            int[] indexes =
                [.. analyzedProperties.Select(static property => property.Property.CborArrayIndex!.Value).OrderBy(static index => index)];

            var duplicateIndex = analyzedProperties
                .GroupBy(static property => property.Property.CborArrayIndex!.Value)
                .FirstOrDefault(static group => group.Count() > 1);

            if (duplicateIndex is not null)
            {
                return Diagnostic.Create(InvalidKey, duplicateIndex.Skip(1).First().KeyLocation,
                    $"CborKey indexes on CBOR array model '{symbol.Name}' must be unique and contiguous from zero.");
            }

            for (var i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] != i)
                {
                    return Diagnostic.Create(InvalidKey,
                        analyzedProperties.First(static property => property.KeyLocation is not null).KeyLocation,
                        $"CborKey indexes on CBOR array model '{symbol.Name}' must be unique and contiguous from zero.");
                }
            }
        }

        if (format == CborSerializationFormat.Map)
        {
            var wrongKind = analyzedProperties.FirstOrDefault(static property => property.Property.CborArrayIndex.HasValue);
            if (wrongKind is not null)
            {
                return Diagnostic.Create(InvalidKey, wrongKind.KeyLocation,
                    $"CborKey on Map property '{wrongKind.Property.Name}' must specify a string name.");
            }

            var duplicateKey = analyzedProperties
                .GroupBy(static property => property.Property.CborMapKey ?? property.Property.Name, StringComparer.Ordinal)
                .FirstOrDefault(static group => group.Count() > 1);

            if (duplicateKey is not null)
            {
                return Diagnostic.Create(InvalidKey, duplicateKey.Skip(1).First().KeyLocation,
                    $"Duplicate CBOR map key '{duplicateKey.Key}' on model '{symbol.Name}'.");
            }
        }
        else
        {
            var wrongKind = analyzedProperties.FirstOrDefault(static property => property.Property.CborMapKey is not null);
            if (wrongKind is not null)
            {
                return Diagnostic.Create(InvalidKey, wrongKind.KeyLocation,
                    $"CborKey on Array property '{wrongKind.Property.Name}' must specify an integer index.");
            }
        }

        var invalidConverter =
            analyzedProperties.FirstOrDefault(static property => property.ConverterError is not null);

        if (invalidConverter is not null)
        {
            return Diagnostic.Create(InvalidConverter, invalidConverter.Location,
                invalidConverter.ConverterError!);
        }

        var unsupported = analyzedProperties.FirstOrDefault(static property =>
            property.Property.Kind == CborValueKind.Unsupported &&
            property.Property.ConverterTypeName is null);

        var unsupportedCollectionElement = analyzedProperties.FirstOrDefault(static property =>
            property.Property.Kind == CborValueKind.Collection &&
            property.Property.ConverterTypeName is null &&
            property.Property.ElementKind is CborValueKind.Unsupported or CborValueKind.Collection);

        if (unsupported is not null)
        {
            return Diagnostic.Create(UnsupportedType, unsupported.Location,
                $"Property '{unsupported.Property.Name}' on CBOR model '{symbol.Name}' has unsupported type '{unsupported.Property.TypeName}'");
        }

        if (unsupportedCollectionElement is not null)
        {
            return Diagnostic.Create(UnsupportedType, unsupportedCollectionElement.Location,
                $"Collection property '{unsupportedCollectionElement.Property.Name}' on CBOR model '{symbol.Name}' has unsupported element type '{unsupportedCollectionElement.Property.ElementTypeName}'.");
        }

        properties = [.. analyzedProperties.Select(static item => item.Property)];
        return null;
    }

    private static Diagnostic? AnalyzeConstruction(
        TypeDeclarationSyntax syntax, INamedTypeSymbol symbol, IPropertySymbol[] selectedProperties,
        Compilation compilation, CancellationToken cancellationToken, out int[]? constructorPropertyIndexes
    )
    {
        List<(IMethodSymbol Constructor, int[] PropertyIndexes)> candidates = [];
        foreach (var constructor in symbol.InstanceConstructors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (constructor.IsStatic || constructor.Parameters.Length != selectedProperties.Length ||
                constructor.Parameters.Any(static parameter => parameter.RefKind != RefKind.None))
            {
                continue;
            }

            var indexes = new int[constructor.Parameters.Length];
            HashSet<int> used = [];
            var matched = true;
            for (var i = 0; i < constructor.Parameters.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var parameter = constructor.Parameters[i];
                (IPropertySymbol property, int index)[] matching =
                [
                    .. selectedProperties.Select((property, index) => (property, index))
                        .Where(item => string.Equals(item.property.Name, parameter.Name, StringComparison.OrdinalIgnoreCase) &&
                            SymbolEqualityComparer.IncludeNullability.Equals(item.property.Type, parameter.Type))
                ];

                if (matching.Length != 1 || !used.Add(matching[0].index))
                {
                    matched = false;
                    break;
                }

                indexes[i] = matching[0].index;
            }

            if (matched)
            {
                candidates.Add((constructor, indexes));
            }
        }

        constructorPropertyIndexes = null;
        ISymbol[] requiredMembers = [.. GetRequiredMembers(symbol)];

        if (candidates.Count != 1)
        {
            var hasAccessibleParameterlessConstructor = symbol.InstanceConstructors.Any(constructor =>
                constructor.Parameters.Length == 0 && compilation.IsSymbolAccessibleWithin(constructor, symbol));

            var allPropertiesAreAssignable = selectedProperties.All(static property =>
                property.SetMethod is { DeclaredAccessibility: Accessibility.Public });

            var allRequiredMembersAreSelected = requiredMembers.All(requiredMember =>
                requiredMember is IPropertySymbol property && selectedProperties.Any(selectedProperty =>
                    SymbolEqualityComparer.Default.Equals(selectedProperty, property)));

            if (!hasAccessibleParameterlessConstructor || !allPropertiesAreAssignable || !allRequiredMembersAreSelected)
            {
                return Diagnostic.Create(InvalidConstructor, syntax.Identifier.GetLocation(),
                    $"CBOR model '{symbol.Name}' must have exactly one constructor whose parameters match all selected properties, or an accessible parameterless constructor and public set/init accessors for every selected property; all required members must be selected properties.");
            }

            return null;
        }

        (var selectedConstructor, var indexesInParameterOrder) = candidates[0];
        if (requiredMembers.Length > 0 && !SetsRequiredMembers(selectedConstructor, compilation))
        {
            return Diagnostic.Create(InvalidConstructor, selectedConstructor.Locations.FirstOrDefault(),
                $"Constructor for CBOR model '{symbol.Name}' must have SetsRequiredMembersAttribute because the model declares required members.");
        }

        constructorPropertyIndexes = indexesInParameterOrder;
        return null;
    }

    private static AttributeData? FindAttribute(ISymbol member, INamedTypeSymbol? attributeType)
    {
        return attributeType is null
            ? null
            : member.GetAttributes().FirstOrDefault(attribute =>
                SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType));
    }

    private static bool HasAttribute(ISymbol member, INamedTypeSymbol? attributeType)
    {
        return FindAttribute(member, attributeType) is not null;
    }

    private static bool SetsRequiredMembers(IMethodSymbol constructor, Compilation compilation)
    {
        return HasAttribute(constructor,
            compilation.GetTypeByMetadataName("System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute"));
    }

    private static IEnumerable<ISymbol> GetRequiredMembers(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol { IsRequired: true } or IFieldSymbol { IsRequired: true })
                {
                    yield return member;
                }
            }
        }
    }

    private static string GetGeneratedPrefix(
        string hashPrefix, INamedTypeSymbol symbol,
        IReadOnlyList<SerializableProperty> properties
    )
    {
        HashSet<string> memberNames = new(symbol.GetMembers().Select(static member => member.Name), StringComparer.Ordinal);
        int[] converterIndexes =
        [
            .. properties.Select((property, index) => (property, index))
                .Where(static item => item.property.ConverterTypeName is not null).Select(static item => item.index)
        ];

        string[] helperNames =
        [
            "ReadCollection", "ReadChecked", "ReadNullableValue", "ReadNullableReference", "ReadNullableConverted", "ReadNonNull",
            "ReadConverted"
        ];

        var suffix = 0;
        while (true)
        {
            var prefix = "__Cbor" + hashPrefix + (suffix == 0 ? "_" : suffix + "_");
            if (!helperNames.Any(name => memberNames.Contains(prefix + name)) &&
                !converterIndexes.Any(index => memberNames.Contains(prefix + "Converter" + index)))
            {
                return prefix;
            }

            suffix++;
        }
    }

    private static CborPropertyAnalysis CreateProperty(
        IPropertySymbol property, INamedTypeSymbol? keyAttributeType,
        INamedTypeSymbol? converterAttributeType, INamedTypeSymbol? converterInterfaceType, INamedTypeSymbol model,
        Compilation compilation,
        CancellationToken cancellationToken
    )
    {
        var displayFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        var typeName = property.Type.ToDisplayString(displayFormat);
        var keyAttribute = FindAttribute(property, keyAttributeType);

        var keyLocation = keyAttribute?.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation();
        string? mapKey = null;
        int? arrayIndex = null;
        string? keyError = null;
        if (keyAttribute is not null)
        {
            if (keyAttribute.ConstructorArguments.Length != 1)
            {
                keyError = $"CborKey on property '{property.Name}' requires a string name or integer index.";
            }
            else if (keyAttribute.ConstructorArguments[0].Value is string name)
            {
                mapKey = name;
            }
            else if (keyAttribute.ConstructorArguments[0].Value is int index)
            {
                arrayIndex = index;
                if (index < 0)
                {
                    keyError = $"CborKey index on property '{property.Name}' cannot be negative.";
                }
            }
            else
            {
                keyError = $"CborKey on property '{property.Name}' requires a string name or integer index.";
            }
        }

        var converterAttribute = FindAttribute(property, converterAttributeType);

        if (converterAttribute is null)
        {
            return Create();
        }

        var location = converterAttribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ??
            property.Locations.FirstOrDefault();

        var converter = converterAttribute.ConstructorArguments.Length == 1
            ? converterAttribute.ConstructorArguments[0].Value as INamedTypeSymbol
            : null;

        if (converter is null || converter.TypeKind != TypeKind.Class || converter.IsAbstract || converter.IsStatic ||
            IsOpenType(converter))
        {
            return Create(location: location,
                converterError: $"Property '{property.Name}' has a CBOR converter that must be a concrete, closed class.");
        }

        if (!compilation.IsSymbolAccessibleWithin(converter, model))
        {
            return Create(location: location,
                converterError:
                $"CBOR converter '{converter.ToDisplayString()}' on property '{property.Name}' must be accessible from model '{model.ToDisplayString()}'.");
        }

        var hasMatchingInterface = converterInterfaceType is not null && converter.AllInterfaces.Any(@interface =>
            SymbolEqualityComparer.Default.Equals(@interface.OriginalDefinition, converterInterfaceType) &&
            SymbolEqualityComparer.IncludeNullability.Equals(@interface.TypeArguments[0], property.Type));

        if (!hasMatchingInterface)
        {
            return Create(location: location,
                converterError:
                $"CBOR converter '{converter.ToDisplayString()}' on property '{property.Name}' must implement ICborConverter<{typeName}>.");
        }

        var parameterlessConstructor = converter.InstanceConstructors.FirstOrDefault(constructor =>
            constructor.Parameters.Length == 0 && compilation.IsSymbolAccessibleWithin(constructor, model));

        if (parameterlessConstructor is null)
        {
            return Create(location: location,
                converterError:
                $"CBOR converter '{converter.ToDisplayString()}' on property '{property.Name}' must have a parameterless constructor accessible from model '{model.ToDisplayString()}'.");
        }

        if (GetRequiredMembers(converter).Any() && !SetsRequiredMembers(parameterlessConstructor, compilation))
        {
            return Create(location: location,
                converterError:
                $"CBOR converter '{converter.ToDisplayString()}' on property '{property.Name}' must mark its parameterless constructor with SetsRequiredMembersAttribute because the converter has required members.");
        }

        return Create(converter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

        CborPropertyAnalysis Create(string? converterName = null, Location? location = null, string? converterError = null)
        {
            var nullableType = property.Type as INamedTypeSymbol;
            var nullableValueType = nullableType?.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
                nullableType.TypeArguments.Length == 1;

            var valueType = nullableValueType ? nullableType!.TypeArguments[0] : property.Type;
            var collection = GetCollectionType(valueType, compilation);
            var enumUnderlyingType = valueType is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType
                ? enumType.EnumUnderlyingType
                : null;

            var isNullable = property.Type.NullableAnnotation == NullableAnnotation.Annotated ||
                nullableValueType;

            var element = collection?.Element;
            var nullableElementType = element as INamedTypeSymbol;
            var nullableElementValueType = nullableElementType?.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
                nullableElementType.TypeArguments.Length == 1;

            var elementValueType = nullableElementValueType ? nullableElementType!.TypeArguments[0] : element;
            var isElementNullable = element is not null && (element.NullableAnnotation == NullableAnnotation.Annotated ||
                nullableElementValueType);

            var elementEnumUnderlyingType = elementValueType is INamedTypeSymbol { TypeKind: TypeKind.Enum } elementEnum
                ? elementEnum.EnumUnderlyingType
                : null;

            var valueTypeName = valueType.IsReferenceType
                ? valueType.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(displayFormat)
                : valueType.ToDisplayString(displayFormat);

            var elementValueTypeName = elementValueType is null
                ? null
                : elementValueType.IsReferenceType
                    ? elementValueType.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(displayFormat)
                    : elementValueType.ToDisplayString(displayFormat);

            SerializableProperty serializableProperty = new(property.Name, typeName, GetKind(valueType, compilation), converterName,
                mapKey, arrayIndex, collection?.Shape,
                collection?.Element.ToDisplayString(displayFormat),
                elementValueType is null ? null : GetKind(elementValueType, compilation), isNullable,
                enumUnderlyingType is null ? null : GetKind(enumUnderlyingType, compilation), valueTypeName,
                valueType.IsReferenceType, nullableValueType, isElementNullable, element?.IsReferenceType ?? false,
                nullableElementValueType,
                elementEnumUnderlyingType is null ? null : GetKind(elementEnumUnderlyingType, compilation), elementValueTypeName);

            return new CborPropertyAnalysis(serializableProperty, location ?? property.Locations.FirstOrDefault(),
                converterError, keyLocation, keyError);
        }
    }

    private static bool IsOpenType(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (current.IsUnboundGenericType || current.TypeArguments.Any(static argument => argument.TypeKind == TypeKind.TypeParameter))
            {
                return true;
            }
        }

        return false;
    }

    private static CborValueKind GetKind(ITypeSymbol type, Compilation compilation)
    {
        if (type is INamedTypeSymbol nullable && nullable.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            type = nullable.TypeArguments[0];
        }

        if (type.TypeKind == TypeKind.Enum)
        {
            return CborValueKind.Enum;
        }

        var specialKind = type.SpecialType switch
        {
            SpecialType.System_Boolean => CborValueKind.Boolean,
            SpecialType.System_Byte => CborValueKind.Byte,
            SpecialType.System_SByte => CborValueKind.SByte,
            SpecialType.System_Int16 => CborValueKind.Int16,
            SpecialType.System_UInt16 => CborValueKind.UInt16,
            SpecialType.System_Int32 => CborValueKind.Int32,
            SpecialType.System_UInt32 => CborValueKind.UInt32,
            SpecialType.System_Int64 => CborValueKind.Int64,
            SpecialType.System_UInt64 => CborValueKind.UInt64,
            SpecialType.System_Single => CborValueKind.Single,
            SpecialType.System_Double => CborValueKind.Double,
            SpecialType.System_String => CborValueKind.String,
            _ => CborValueKind.Unsupported
        };

        if (specialKind != CborValueKind.Unsupported)
        {
            return specialKind;
        }

        if (type is INamedTypeSymbol readOnlyMemory &&
            SymbolEqualityComparer.Default.Equals(readOnlyMemory.OriginalDefinition,
                compilation.GetTypeByMetadataName("System.ReadOnlyMemory`1")) &&
            readOnlyMemory.TypeArguments[0].SpecialType == SpecialType.System_Byte)
        {
            return CborValueKind.Bytes;
        }

        if (type is IArrayTypeSymbol { Rank: 1, ElementType.SpecialType: SpecialType.System_Byte })
        {
            return CborValueKind.ByteArray;
        }

        if (GetCollectionType(type, compilation) is not null)
        {
            return CborValueKind.Collection;
        }

        if (type is INamedTypeSymbol named && named.GetAttributes().Any(attribute =>
            SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.GetTypeByMetadataName(ArrayAttributeName)) ||
            SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.GetTypeByMetadataName(MapAttributeName))))
        {
            return CborValueKind.Nested;
        }

        return CborValueKind.Unsupported;
    }

    private static (CborCollectionKind Shape, ITypeSymbol Element)? GetCollectionType(ITypeSymbol type, Compilation compilation)
    {
        if (type is IArrayTypeSymbol array)
        {
            return array.Rank == 1 ? (CborCollectionKind.Array, array.ElementType) : null;
        }

        INamedTypeSymbol?[] supported =
        [
            .. new[] { "System.Collections.Generic.List`1", "System.Collections.Generic.IReadOnlyList`1" }
                .Select(compilation.GetTypeByMetadataName).Where(static symbol => symbol is not null)
        ];

        if (type is not INamedTypeSymbol named)
        {
            return null;
        }

        var candidate =
            named.IsGenericType && supported.Any(definition => SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, definition))
                ? named
                : null;

        if (candidate is null)
        {
            return null;
        }

        var listDefinition = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
        return (SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, listDefinition)
            ? CborCollectionKind.List
            : CborCollectionKind.Interface, candidate.TypeArguments[0]);
    }

    private static void Generate(SourceProductionContext context, SerializationContext model)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        context.AddSource(model.HintName, CborSourceEmitter.Emit(model, context.CancellationToken));
    }

    private sealed class ModelAnalysis(SerializationContext? model = null, Diagnostic? diagnostic = null)
    {
        public SerializationContext? Model { get; } = model;

        public Diagnostic? Diagnostic { get; } = diagnostic;
    }

    private sealed class CborPropertyAnalysis(
        SerializableProperty property,
        Location? location,
        string? converterError,
        Location? keyLocation,
        string? keyError
    )
    {
        public SerializableProperty Property { get; } = property;

        public Location? Location { get; } = location;

        public string? ConverterError { get; } = converterError;

        public Location? KeyLocation { get; } = keyLocation;

        public string? KeyError { get; } = keyError;
    }
}