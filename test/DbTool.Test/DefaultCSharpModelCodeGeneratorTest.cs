// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core;
using DbTool.Core.Entity;
using DbTool.DbProvider.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace DbTool.Test;

public class DefaultCSharpModelCodeGeneratorTest
{
    private readonly ITestOutputHelper _outputHelper;

    public DefaultCSharpModelCodeGeneratorTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Theory]
    [InlineData("\t", true)]
    [InlineData("  ", false)]
    [InlineData("    ", false)]
    public void CustomIndentationTest(string indentation, bool generateField)
    {
        var table = new TableEntity()
        {
            TableName = "test",
            Columns = new List<ColumnEntity>()
            {
                new ()
                {
                    ColumnName = "Id",
                    DataType = "int",
                    ColumnDescription = "Id",
                    IsNullable = false,
                    IsPrimaryKey = true
                },
                new()
                {
                    ColumnName = "Name",
                    DataType = "nvarchar",
                    ColumnDescription = "Name",
                    IsNullable = true
                },
                new()
                {
                    ColumnName = "CreatedAt",
                    DataType = "DateTime2",
                    IsNullable = false
                }
            }
        };

        var codeGenerator = new DefaultCSharpModelCodeGenerator(new DefaultModelNameConverter());
        var code = codeGenerator.GenerateModelCode(table,
            new ModelCodeGenerateOptions()
            {
                Indentation = indentation,
                GlobalUsingEnabled = true,
                NullableReferenceTypesEnabled = false,
                GenerateDataAnnotation = false,
                GeneratePrivateFields = generateField
            }, new SqlServerDbProvider());
        Assert.NotNull(code);
        _outputHelper.WriteLine(code);
        Assert.DoesNotContain("{options.Indentation}", code);
    }

    [Fact]
    public void GlobalUsingTest()
    {
        var table = new TableEntity()
        {
            TableName = "test",
            Columns = new List<ColumnEntity>()
            {
                new ()
                {
                    ColumnName = "Id",
                    DataType = "int",
                    ColumnDescription = "Id",
                    IsNullable = false,
                    IsPrimaryKey = true
                },
                new()
                {
                    ColumnName = "Name",
                    DataType = "nvarchar",
                    ColumnDescription = "Name",
                    IsNullable = true
                },
                new()
                {
                    ColumnName = "CreatedAt",
                    DataType = "DateTime2",
                    IsNullable = false
                }
            }
        };

        var codeGenerator = new DefaultCSharpModelCodeGenerator(new DefaultModelNameConverter());
        var code = codeGenerator.GenerateModelCode(table,
            new ModelCodeGenerateOptions()
            {
                GlobalUsingEnabled = true,
                NullableReferenceTypesEnabled = false,
                GenerateDataAnnotation = false,
                GeneratePrivateFields = false
            }, new SqlServerDbProvider());
        Assert.NotNull(code);
        _outputHelper.WriteLine(code);
        Assert.DoesNotContain("using System;", code);
    }

    [Fact]
    public void NullableReferenceTypesTest()
    {
        var table = new TableEntity()
        {
            TableName = "test",
            Columns = new List<ColumnEntity>()
            {
                new ()
                {
                    ColumnName = "Id",
                    DataType = "int",
                    ColumnDescription = "Id",
                    IsNullable = false,
                    IsPrimaryKey = true
                },
                new()
                {
                    ColumnName = "Name",
                    DataType = "nvarchar",
                    ColumnDescription = "Name",
                    IsNullable = true
                }
            }
        };

        var codeGenerator = new DefaultCSharpModelCodeGenerator(new DefaultModelNameConverter());
        var code = codeGenerator.GenerateModelCode(table,
            new ModelCodeGenerateOptions()
            {
                NullableReferenceTypesEnabled = true,
                GenerateDataAnnotation = false,
                GeneratePrivateFields = false
            }, new SqlServerDbProvider());
        Assert.NotNull(code);
        _outputHelper.WriteLine(code);
        Assert.Contains("public string? Name", code);
    }

    [Fact]
    public void FileScopedTest()
    {
        var table = new TableEntity()
        {
            TableName = "test",
            Columns = new List<ColumnEntity>()
            {
                new ()
                {
                    ColumnName = "Id",
                    DataType = "int",
                    ColumnDescription = "Id",
                    IsNullable = false,
                    IsPrimaryKey = true
                },
                new()
                {
                    ColumnName = "Name",
                    DataType = "nvarchar",
                    ColumnDescription = "Name",
                    IsNullable = true
                },
                new()
                {
                    ColumnName = "CreatedAt",
                    DataType = "DateTime2",
                    IsNullable = false
                }
            }
        };

        var codeGenerator = new DefaultCSharpModelCodeGenerator(new DefaultModelNameConverter());
        var code = codeGenerator.GenerateModelCode(table,
            new ModelCodeGenerateOptions()
            {
                Namespace = "Models",
                FileScopedNamespaceEnabled = true,
                GlobalUsingEnabled = false,
                NullableReferenceTypesEnabled = false,
                GenerateDataAnnotation = false,
                GeneratePrivateFields = false
            }, new SqlServerDbProvider());
        Assert.NotNull(code);
        Assert.Contains("namespace Models;", code);
    }
}
