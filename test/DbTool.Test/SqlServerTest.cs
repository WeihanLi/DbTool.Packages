// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DbTool.Test;

public class SqlServerTest : BaseDbTest
{
    public override string DbType => "SqlServer";

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

    public SqlServerTest(IConfiguration configuration, IDbHelperFactory dbHelperFactory, DbProviderFactory dbProviderFactory) : base(configuration, dbHelperFactory, dbProviderFactory)
    {
    }
}
