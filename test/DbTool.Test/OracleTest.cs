// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DbTool.Test;

public class OracleTest : BaseDbTest
{
    public override string DbType => "Oracle";

    [Fact]
    public override Task QueryTest()
    {
        return base.QueryTest();
    }

    [Fact]
    public override void CreateTest()
    {
        base.CreateTest();
    }

    public OracleTest(IConfiguration configuration, IDbHelperFactory dbHelperFactory, DbProviderFactory dbProviderFactory) : base(configuration, dbHelperFactory, dbProviderFactory)
    {
    }

    [Theory]
    [InlineData("VARCHAR2", true, "string")]
    [InlineData("NVARCHAR2", true, "string")]
    [InlineData("CHAR", true, "string")]
    [InlineData("NCHAR", true, "string")]
    [InlineData("CLOB", true, "string")]
    [InlineData("NUMBER", true, "decimal?")]
    [InlineData("NUMBER", false, "decimal")]
    [InlineData("INTEGER", true, "decimal?")]
    [InlineData("INTEGER", false, "decimal")]
    [InlineData("FLOAT", true, "decimal?")]
    [InlineData("FLOAT", false, "decimal")]
    [InlineData("BINARY_FLOAT", true, "float?")]
    [InlineData("BINARY_FLOAT", false, "float")]
    [InlineData("BINARY_DOUBLE", true, "double?")]
    [InlineData("BINARY_DOUBLE", false, "double")]
    [InlineData("DATE", true, "DateTime?")]
    [InlineData("DATE", false, "DateTime")]
    [InlineData("TIMESTAMP", true, "DateTime?")]
    [InlineData("TIMESTAMP", false, "DateTime")]
    public override void DbType2ClrTypeTest(string dbType, bool isNullable, string expectedType)
    {
        base.DbType2ClrTypeTest(dbType, isNullable, expectedType);
    }

    [Theory]
    [InlineData(typeof(string), "Varchar2")]
    [InlineData(typeof(int), "Number")]
    [InlineData(typeof(long), "Number")]
    [InlineData(typeof(decimal), "Number")]
    [InlineData(typeof(float), "BinaryFloat")]
    [InlineData(typeof(double), "BinaryDouble")]
    [InlineData(typeof(DateTime), "Date")]
    [InlineData(typeof(DateTimeOffset), "TimestampWithTimeZone")]
    [InlineData(typeof(bool), "Number")]
    [InlineData(typeof(byte[]), "Blob")]
    public override void ClrType2DbTypeTest(Type clrType, string expectedType)
    {
        base.ClrType2DbTypeTest(clrType, expectedType);
    }
}
