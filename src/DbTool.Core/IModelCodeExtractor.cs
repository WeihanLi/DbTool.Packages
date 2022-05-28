// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core.Entity;

namespace DbTool.Core;

public interface IModelCodeExtractor
{
    Dictionary<string, string> SupportedFileExtensions { get; }
    public string CodeType { get; }

    Task<List<TableEntity>> GetTablesFromSourceText(IDbProvider dbProvider, params string[] sourceText);

    Task<List<TableEntity>> GetTablesFromSourceFiles(IDbProvider dbProvider, params string[] sourceFilePaths);
}
