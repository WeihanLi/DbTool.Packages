// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace DbTool.DbProvider.Oracle;

/// <summary>
/// Oracle 数据库类型
/// </summary>
internal enum OracleDbType
{
    // Numeric types
    Number,
    Float,
    BinaryFloat,
    BinaryDouble,
    
    // Integer types (aliases for NUMBER)
    Integer,
    Int,
    SmallInt,
    
    // String types
    Char,
    NChar,
    Varchar,
    Varchar2,
    NVarchar2,
    Clob,
    NClob,
    Long,
    
    // Date/Time types
    Date,
    Timestamp,
    TimestampWithTimeZone,
    TimestampWithLocalTimeZone,
    IntervalYearToMonth,
    IntervalDayToSecond,
    
    // Binary types
    Raw,
    LongRaw,
    Blob,
    BFile,
    
    // Other types
    RowId,
    URowId,
    XMLType
}
