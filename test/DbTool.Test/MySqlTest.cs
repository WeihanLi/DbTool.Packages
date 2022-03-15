// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DbTool.Test;

public class MySqlTest : BaseDbTest
{
    public override string DbType => "MySql";

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

    public MySqlTest(IConfiguration configuration, IDbHelperFactory dbHelperFactory, DbProviderFactory dbProviderFactory) : base(configuration, dbHelperFactory, dbProviderFactory)
    {
    }

    [Theory]
    [InlineData("mediumText", true, "string")]
    [InlineData("nchar", true, "string")]
    [InlineData("char", true, "string")]
    [InlineData("text", true, "string")]
    [InlineData("int", true, "int?")]
    [InlineData("int", false, "int")]
    public override void DbType2ClrTypeTest(string dbType, bool isNullable, string expectedType)
    {
        base.DbType2ClrTypeTest(dbType, isNullable, expectedType);
    }
}
