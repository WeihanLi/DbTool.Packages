// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core;
using DbTool.Core.Entity;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using System.Text;
using WeihanLi.Extensions;
using InternalDbType = DbTool.DbProvider.Oracle.OracleDbType;

namespace DbTool.DbProvider.Oracle;

public class OracleDbProvider : IDbProvider
{
    public string DbType => "Oracle";

    public virtual string QueryDbTablesSqlFormat => @"
SELECT USER AS DatabaseName,
    NULL AS TableSchema,
    t.table_name AS TableName,
    t.comments AS TableDescription
FROM user_tab_comments t
WHERE t.table_type = 'TABLE'
ORDER BY t.table_name";

    public virtual string QueryTableColumnsSqlFormat => @"
SELECT t1.Table_Name AS TableName,
       t1.Column_Name AS ColumnName,
       t2.Comments AS ColumnDescription,
       CASE WHEN t1.NullAble = 'Y' THEN 1 ELSE 0 END AS IsNullable,
       t1.DATA_TYPE AS DataType,
       t1.DATA_LENGTH AS Size,
       CASE WHEN pk.column_name IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey,
       t1.Data_Default AS DefaultValue
FROM cols t1
LEFT JOIN user_col_comments t2 
       ON t1.Table_name = t2.Table_name
      AND t1.Column_Name = t2.Column_Name
LEFT JOIN (
    SELECT cols.table_name, cols.column_name
    FROM user_constraints cons
    JOIN user_cons_columns cols 
      ON cons.constraint_name = cols.constraint_name
    WHERE cons.constraint_type = 'P'
      AND cols.table_name = :tableName
) pk ON t1.Table_Name = pk.table_name 
    AND t1.Column_Name = pk.column_name
WHERE NOT EXISTS (
    SELECT t4.Object_Name
    FROM User_objects t4
    WHERE t4.Object_Type = 'TABLE'
      AND t4.Temporary = 'Y'
      AND t4.Object_Name = t1.Table_Name
)
  AND t1.TABLE_NAME = :tableName
ORDER BY t1.Column_ID";

    public virtual string DbType2ClrType(string dbType, bool isNullable)
    {
        // Handle Oracle data types - convert to uppercase for comparison
        var upperDbType = dbType.ToUpperInvariant();
        
        // Try to parse as enum, but handle cases where it might not be an exact match
        InternalDbType oracleDbType;
        if (upperDbType.Contains("NUMBER") || upperDbType == "INTEGER" || upperDbType == "INT" || upperDbType == "SMALLINT")
        {
            oracleDbType = InternalDbType.Number;
        }
        else if (upperDbType.Contains("FLOAT"))
        {
            oracleDbType = upperDbType.Contains("BINARY") ? InternalDbType.BinaryFloat : InternalDbType.Float;
        }
        else if (upperDbType.Contains("DOUBLE"))
        {
            oracleDbType = InternalDbType.BinaryDouble;
        }
        else if (upperDbType.Contains("VARCHAR2"))
        {
            oracleDbType = upperDbType.StartsWith("N") ? InternalDbType.NVarchar2 : InternalDbType.Varchar2;
        }
        else if (upperDbType.Contains("CHAR") && !upperDbType.Contains("VARCHAR"))
        {
            oracleDbType = upperDbType.StartsWith("N") ? InternalDbType.NChar : InternalDbType.Char;
        }
        else if (upperDbType.Contains("CLOB"))
        {
            oracleDbType = upperDbType.StartsWith("N") ? InternalDbType.NClob : InternalDbType.Clob;
        }
        else if (upperDbType.Contains("TIMESTAMP"))
        {
            if (upperDbType.Contains("TIME ZONE"))
            {
                oracleDbType = upperDbType.Contains("LOCAL") ? InternalDbType.TimestampWithLocalTimeZone : InternalDbType.TimestampWithTimeZone;
            }
            else
            {
                oracleDbType = InternalDbType.Timestamp;
            }
        }
        else if (upperDbType.Contains("INTERVAL"))
        {
            oracleDbType = upperDbType.Contains("YEAR") ? InternalDbType.IntervalYearToMonth : InternalDbType.IntervalDayToSecond;
        }
        else if (upperDbType.Contains("BLOB"))
        {
            oracleDbType = InternalDbType.Blob;
        }
        else if (upperDbType.Contains("RAW"))
        {
            oracleDbType = upperDbType.StartsWith("LONG") ? InternalDbType.LongRaw : InternalDbType.Raw;
        }
        else if (upperDbType == "DATE")
        {
            oracleDbType = InternalDbType.Date;
        }
        else if (upperDbType == "LONG")
        {
            oracleDbType = InternalDbType.Long;
        }
        else if (upperDbType.Contains("ROWID"))
        {
            oracleDbType = upperDbType.StartsWith("U") ? InternalDbType.URowId : InternalDbType.RowId;
        }
        else if (upperDbType.Contains("XMLTYPE"))
        {
            oracleDbType = InternalDbType.XMLType;
        }
        else if (upperDbType == "BFILE")
        {
            oracleDbType = InternalDbType.BFile;
        }
        else
        {
            // Default to varchar2 for unknown types
            return "string";
        }

        var type = oracleDbType switch
        {
            InternalDbType.Number or InternalDbType.Integer or InternalDbType.Int or InternalDbType.Float => isNullable ? "decimal?" : "decimal",
            InternalDbType.SmallInt => isNullable ? "short?" : "short",
            InternalDbType.BinaryFloat => isNullable ? "float?" : "float",
            InternalDbType.BinaryDouble => isNullable ? "double?" : "double",
            InternalDbType.Char or InternalDbType.NChar or InternalDbType.Varchar or InternalDbType.Varchar2 or InternalDbType.NVarchar2 or InternalDbType.Clob or InternalDbType.NClob or InternalDbType.Long => "string",
            InternalDbType.Date or InternalDbType.Timestamp => isNullable ? "DateTime?" : "DateTime",
            InternalDbType.TimestampWithTimeZone or InternalDbType.TimestampWithLocalTimeZone => isNullable ? "DateTimeOffset?" : "DateTimeOffset",
            InternalDbType.IntervalYearToMonth or InternalDbType.IntervalDayToSecond => isNullable ? "TimeSpan?" : "TimeSpan",
            InternalDbType.Raw or InternalDbType.LongRaw or InternalDbType.Blob => "byte[]",
            InternalDbType.BFile => "byte[]",
            InternalDbType.RowId or InternalDbType.URowId => "string",
            InternalDbType.XMLType => "string",
            _ => "string"
        };
        return type;
    }

    public virtual string ClrType2DbType(Type type)
    {
        var typeFullName = type.Unwrap().FullName;
        return typeFullName switch
        {
            "System.Boolean" => InternalDbType.Number.ToString(),
            "System.Byte" => InternalDbType.Number.ToString(),
            "System.Int16" => InternalDbType.Number.ToString(),
            "System.Int32" => InternalDbType.Number.ToString(),
            "System.Int64" => InternalDbType.Number.ToString(),
            "System.Single" => InternalDbType.BinaryFloat.ToString(),
            "System.Double" => InternalDbType.BinaryDouble.ToString(),
            "System.Decimal" => InternalDbType.Number.ToString(),
            "System.DateOnly" => InternalDbType.Date.ToString(),
            "System.TimeOnly" => InternalDbType.Timestamp.ToString(),
            "System.DateTime" => InternalDbType.Date.ToString(),
            "System.DateTimeOffset" => InternalDbType.TimestampWithTimeZone.ToString(),
            "System.TimeSpan" => InternalDbType.IntervalDayToSecond.ToString(),
            "System.Guid" => InternalDbType.Raw.ToString(),
            "System.Byte[]" => InternalDbType.Blob.ToString(),
            _ => InternalDbType.Varchar2.ToString()
        };
    }

    public virtual uint GetDefaultSizeForDbType(string dbType, uint defaultLength = 64)
    {
        var upperDbType = dbType.ToUpperInvariant();
        
        // Handle Oracle data types
        if (upperDbType.Contains("NUMBER") || upperDbType == "INTEGER" || upperDbType == "INT")
        {
            return 22; // Oracle NUMBER default precision
        }
        else if (upperDbType == "SMALLINT")
        {
            return 5;
        }
        else if (upperDbType.Contains("FLOAT") || upperDbType.Contains("DOUBLE"))
        {
            return 8;
        }
        else if (upperDbType == "DATE" || upperDbType.Contains("TIMESTAMP"))
        {
            return 7;
        }
        else if (upperDbType.Contains("CHAR") && !upperDbType.Contains("VARCHAR"))
        {
            return 2000;
        }
        else if (upperDbType.Contains("VARCHAR2"))
        {
            return 4000;
        }
        else if (upperDbType.Contains("CLOB") || upperDbType.Contains("BLOB"))
        {
            return 0; // LOBs don't have a fixed size
        }
        else if (upperDbType.Contains("RAW"))
        {
            return upperDbType.StartsWith("LONG") ? 0u : 2000u;
        }
        else if (upperDbType.Contains("ROWID"))
        {
            return 10;
        }
        
        return defaultLength;
    }

    public virtual DbConnection GetDbConnection(string connectionString) => new OracleConnection(connectionString);

    public virtual string GenerateSqlStatement(TableEntity tableEntity, bool generateDescription = true)
    {
        if (string.IsNullOrWhiteSpace(tableEntity.TableName))
        {
            return string.Empty;
        }
        
        var sbSqlText = new StringBuilder();
        sbSqlText.AppendLine($"-- ---------- Create Table 【{tableEntity.TableName}】 Sql -----------");
        sbSqlText.Append($"CREATE TABLE {tableEntity.TableName}(");

        if (tableEntity.Columns.Count > 0)
        {
            foreach (var col in tableEntity.Columns)
            {
                sbSqlText.AppendLine();
                sbSqlText.Append($"    {col.ColumnName} {col.DataType}");
                
                // Add size for character and binary types
                if (col.DataType.ToUpperInvariant().Contains("CHAR") || col.DataType.ToUpperInvariant().Contains("RAW"))
                {
                    if (!col.DataType.ToUpperInvariant().Contains("CLOB") && !col.DataType.ToUpperInvariant().Contains("LONG"))
                    {
                        var size = col.Size == 0 ? GetDefaultSizeForDbType(col.DataType, 100) : col.Size;
                        sbSqlText.Append($"({size})");
                    }
                }
                else if (col.DataType.ToUpperInvariant() == "NUMBER" && !col.IsPrimaryKey)
                {
                    // Default NUMBER precision/scale
                    sbSqlText.Append("(18,2)");
                }
                
                // Primary key constraint
                if (col.IsPrimaryKey)
                {
                    sbSqlText.Append(" PRIMARY KEY");
                }
                
                // Nullable
                if (!col.IsNullable)
                {
                    sbSqlText.Append(" NOT NULL");
                }
                
                // Default Value
                var defaultValueStr = col.DefaultValue?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(defaultValueStr))
                {
                    if (!col.IsPrimaryKey)
                    {
                        if ((col.DataType.ToUpperInvariant().Contains("CHAR") || col.DataType.ToUpperInvariant().Contains("CLOB"))
                            && !defaultValueStr.StartsWith("'"))
                        {
                            sbSqlText.AppendFormat(" DEFAULT '{0}'", defaultValueStr);
                        }
                        else
                        {
                            sbSqlText.AppendFormat(" DEFAULT {0}", defaultValueStr);
                        }
                    }
                }

                sbSqlText.Append(',');
            }
            sbSqlText.Remove(sbSqlText.Length - 1, 1);
            sbSqlText.AppendLine();
        }

        sbSqlText.AppendLine(");");
        
        // Add table comments
        if (generateDescription && !string.IsNullOrWhiteSpace(tableEntity.TableDescription))
        {
            sbSqlText.AppendLine();
            sbSqlText.AppendLine($"COMMENT ON TABLE {tableEntity.TableName} IS '{tableEntity.TableDescription.Replace("'", "''")}';");
        }
        
        // Add column comments
        if (generateDescription)
        {
            foreach (var col in tableEntity.Columns.Where(c => !string.IsNullOrWhiteSpace(c.ColumnDescription)))
            {
                sbSqlText.AppendLine($"COMMENT ON COLUMN {tableEntity.TableName}.{col.ColumnName} IS '{col.ColumnDescription!.Replace("'", "''")}';");
            }
        }
        
        sbSqlText.AppendLine();
        return sbSqlText.ToString();
    }
}
