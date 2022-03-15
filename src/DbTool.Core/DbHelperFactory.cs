// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace DbTool.Core;

public interface IDbHelperFactory
{
    IDbHelper GetDbHelper(IDbProvider dbProvider, string connectionString);
}

public sealed class DbHelperFactory : IDbHelperFactory
{
    public IDbHelper GetDbHelper(IDbProvider dbProvider, string connectionString)
    {
        return new DbHelper(dbProvider, connectionString);
    }
}
