// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core.Entity;

namespace DbTool.Core;

public sealed class ModelCodeGenerateOptions
{
    /// <summary>
    /// Model 命名空间
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Model 前缀
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Model 后缀
    /// </summary>
    public string? Suffix { get; set; }

    /// <summary>
    /// 是否生成 Description Attribute 描述
    /// </summary>
    public bool GenerateDataAnnotation { get; set; }

    /// <summary>
    /// 是否生成私有字段
    /// </summary>
    public bool GeneratePrivateFields { get; set; }

    /// <summary>
    /// 缩进格式，默认使用两个空格 "  "
    /// </summary>
    public string Indentation { get; set; } = "  ";

    /// <summary>
    /// 是否启用可空引用类型
    /// </summary>
    public bool NullableReferenceTypesEnabled { get; set; }

    /// <summary>
    /// 是否启用 GlobalUsing
    /// </summary>
    public bool GlobalUsingEnabled { get; set; }

    /// <summary>
    /// Global using
    /// </summary>
    public HashSet<string> GlobalUsing { get; set; } = DefaultGlobalUsing;

    public static readonly HashSet<string> DefaultGlobalUsing = new()
    {
        "System",
        "System.Collections.Generic",
        "System.IO",
        "System.Linq",
        "System.Net.Http",
        "System.Threading",
        "System.Threading.Tasks"
    };
}

public interface IModelCodeGenerator
{
    string FileExtension { get; }

    /// <summary>
    /// CodeType
    /// C#/TS ...
    /// </summary>
    string CodeType { get; }

    string GenerateModelCode(TableEntity tableEntity, ModelCodeGenerateOptions options, IDbProvider dbProvider);
}
