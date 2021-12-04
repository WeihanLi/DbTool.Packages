﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DbTool.Core;
using DbTool.Core.Entity;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DbTool.Test
{
    public abstract class BaseDbTest
    {
        protected readonly IDbHelperFactory DbHelperFactory;
        public abstract string DbType { get; }

        protected readonly IDbProvider DbProvider;

        public IConfiguration Configuration { get; }

        protected BaseDbTest(IConfiguration configuration, IDbHelperFactory dbHelperFactory, DbProviderFactory dbProviderFactory)
        {
            DbHelperFactory = dbHelperFactory;
            Configuration = configuration;

            DbProvider = dbProviderFactory.GetDbProvider(DbType);
        }

        protected readonly TableEntity TableEntity = new()
        {
            TableName = "tabUser111",
            TableDescription = "测试用户表",
            Columns = new List<ColumnEntity>
            {
                new ()
                {
                    ColumnName = "Id",
                    ColumnDescription = "主键",
                    IsPrimaryKey = true,
                    DataType = "int",
                    IsNullable = false,
                    Size = 4
                },
                new ()
                {
                    ColumnName = "UserName",
                    ColumnDescription = "用户名",
                    IsPrimaryKey = false,
                    DataType = "VARCHAR",
                    IsNullable = false,
                    Size = 50
                },
                new ()
                {
                    ColumnName = "NickName",
                    ColumnDescription = "昵称",
                    IsPrimaryKey = false,
                    DataType = "VARCHAR",
                    IsNullable = true,
                    Size = 50
                },
                new ()
                {
                    ColumnName = "IsAdmin",
                    ColumnDescription = "是否是管理员",
                    IsPrimaryKey = false,
                    DataType = "bit",
                    IsNullable = true,
                    Size = 1,
                    DefaultValue = 0
                },
                new ()
                {
                    ColumnName = "CreatedTime",
                    ColumnDescription = "创建时间",
                    IsPrimaryKey = false,
                    DataType = "DateTime",
                    IsNullable = false,
                    Size = 8
                }
            }
        };

        public virtual async Task QueryTest()
        {
            var dbHelper = DbHelperFactory.GetDbHelper(DbProvider, "");
            Assert.NotNull(dbHelper.DatabaseName);
            var tables = await dbHelper.GetTablesInfoAsync();
            Assert.NotNull(tables);
            Assert.NotEmpty(tables);
            foreach (var table in tables)
            {
                Assert.NotNull(table.TableName);
                var columns = await dbHelper.GetColumnsInfoAsync(table.TableName);
                Assert.NotNull(columns);
                Assert.NotEmpty(columns);
            }
        }

        public virtual void CreateTest()
        {
            var sql = DbProvider.GenerateSqlStatement(TableEntity);
            Assert.NotEmpty(sql);

            var sql1 = DbProvider.GenerateSqlStatement(TableEntity, false);
            Assert.NotEmpty(sql1);

            Assert.NotEqual(sql1, sql);
        }


        public virtual void DbType2ClrTypeTest(string dbType, bool isNullable, string expectedType)
        {
            var result = DbProvider.DbType2ClrType(dbType, isNullable);
            Assert.Equal(expectedType, result);
        }

        public virtual void ClrType2DbTypeTest(Type clrType, string expectedType)
        {
            var result = DbProvider.ClrType2DbType(clrType);
            Assert.Equal(expectedType, result);
        }
    }
}
