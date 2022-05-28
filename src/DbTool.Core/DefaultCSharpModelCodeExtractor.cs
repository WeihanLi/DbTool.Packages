// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core.Entity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using WeihanLi.Common;
using WeihanLi.Common.Models;
using WeihanLi.Extensions;

namespace DbTool.Core;

public class DefaultCSharpModelCodeExtractor : IModelCodeExtractor
{
    private readonly IModelNameConverter _modelNameConverter;
    private readonly string _globalUsingString;

    public DefaultCSharpModelCodeExtractor(IModelNameConverter modelNameConverter)
    {
        _modelNameConverter = modelNameConverter;
        _globalUsingString = ModelCodeGenerateOptions.DefaultGlobalUsing
            .Select(ns => $"global using {ns};")
            .StringJoin(Environment.NewLine);
    }

    public Dictionary<string, string> SupportedFileExtensions { get; } = new()
    {
        { ".cs", "C# File(*.cs)" }
    };

    public virtual string CodeType => "C#";

    public virtual async Task<List<TableEntity>> GetTablesFromSourceFiles(IDbProvider dbProvider, params string[] sourceFilePaths)
    {
        if (sourceFilePaths.IsNullOrEmpty())
        {
            return new List<TableEntity>();
        }

        var sourceTextList = new string[sourceFilePaths.Length + 1];
        for (var i = 0; i < sourceFilePaths.Length; i++)
        {
            sourceTextList[i] = await File.ReadAllTextAsync(sourceFilePaths[i]);
        }
        sourceTextList[^1] = _globalUsingString;
        return await GetTablesFromSourceText(dbProvider, sourceTextList);
    }

    public virtual Task<List<TableEntity>> GetTablesFromSourceText(IDbProvider dbProvider, params string[] sourceText)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var syntaxTrees = sourceText.Select(text => CSharpSyntaxTree.ParseText(text)).ToArray();
        var nullableReferenceTypesEnabled = sourceText.Contains("string?")
            || sourceText.Contains("object?")
            || sourceText.Contains("null!")
            || sourceText.Contains("default!");
        var references = new[]
                {
                        typeof(object).Assembly,
                        typeof(TableAttribute).Assembly,
                        typeof(Result).Assembly,
                        typeof(DescriptionAttribute).Assembly,
                        Assembly.Load("netstandard"),
                        Assembly.Load("System.Runtime"),
                    }
                .Select(assembly => assembly.Location)
                .Distinct()
                .Select(l => MetadataReference.CreateFromFile(l))
                .Cast<MetadataReference>()
                .ToArray();
        var assemblyName = $"DbTool.DynamicGenerated.{GuidIdGenerator.Instance.NewId()}";
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
              .WithNullableContextOptions(nullableReferenceTypesEnabled ? NullableContextOptions.Enable : NullableContextOptions.Annotations);
        var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, compilationOptions);

        using var ms = new MemoryStream();
        var compilationResult = compilation.Emit(ms);
        if (compilationResult.Success)
        {
            var assemblyBytes = ms.ToArray();
            return Task.FromResult(GetTablesFromAssembly(Assembly.Load(assemblyBytes), dbProvider));
        }

        var error = new StringBuilder();
        foreach (var t in compilationResult.Diagnostics
            // .Where(x => x.Severity == DiagnosticSeverity.Error)
            )
        {
            var msg = CSharpDiagnosticFormatter.Instance.Format(t);
            error.AppendLine($"{msg}");
        }
        throw new ArgumentException($"Compile error:{Environment.NewLine}{error}");
    }

    protected virtual List<TableEntity> GetTablesFromAssembly(Assembly assembly, IDbProvider dbProvider)
    {
        var validTypes = assembly.GetExportedTypes().Where(x => x.IsClass && !x.IsAbstract).ToArray();
        var tables = new List<TableEntity>(validTypes.Length);
        foreach (var type in validTypes)
        {
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            var table = new TableEntity
            {
                TableName = tableAttr?.Name ?? _modelNameConverter.ConvertModelToTable(type.Name),
                TableSchema = tableAttr?.Schema,
                TableDescription = type.GetCustomAttribute<DescriptionAttribute>()?.Description
            };
            var defaultVal = Activator.CreateInstance(type);
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.IsDefined(typeof(NotMappedAttribute)))
                {
                    continue; // not mapped or is navigationProperty
                }
                if (!property.PropertyType.IsBasicType() && !property.PropertyType.IsEnum)
                {
                    continue; // none basic type
                }

                var columnInfo = new ColumnEntity
                {
                    ColumnName = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name,
                    ColumnDescription = property.GetCustomAttribute<DescriptionAttribute>()?.Description,
                    DataType = dbProvider.ClrType2DbType(
                        property.PropertyType.IsEnum
                            ? Enum.GetUnderlyingType(property.PropertyType)
                            : property.PropertyType)
                };
                var defaultPropertyValue = property.PropertyType.GetDefaultValue();
                if (defaultPropertyValue is null)
                {
                    // ReferenceType
                    columnInfo.IsNullable = !property.IsDefined(typeof(RequiredAttribute));
                }
                else
                {
                    // ValueType
                    columnInfo.IsNullable = false;
                }

                var val = property.GetValue(defaultVal);
                columnInfo.DefaultValue =
                    (columnInfo.IsNullable
                     || null == val
                     || val.Equals(defaultPropertyValue)
                     )
                    ? null : val;
                columnInfo.IsPrimaryKey = property.IsDefined(typeof(KeyAttribute))
                                          || property.Name == "Id"
                                          || columnInfo.ColumnDescription?.Contains("主键") == true;

                var stringLength = property.GetCustomAttribute<StringLengthAttribute>();
                columnInfo.Size = stringLength?.MaximumLength ?? Convert.ToInt64(dbProvider.GetDefaultSizeForDbType(columnInfo.DataType).ToString());

                table.Columns.Add(columnInfo);
            }
            tables.Add(table);
        }

        return tables;
    }
}
