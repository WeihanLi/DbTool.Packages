// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core;
using DbTool.Core.Entity;
using DbTool.DbProvider.SqlServer;
using Xunit;

namespace DbTool.Test;

public class DefaultCSharpModelCodeGeneratorTest
{
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
        Assert.Contains("public string? Name", code);
    }
}
