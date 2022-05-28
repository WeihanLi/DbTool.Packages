// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core;
using Xunit;

namespace DbTool.Test;

public class DefaultModelCodeExtractorTest
{
    [Theory]
    [InlineData(@"
public record Post
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}
")]
    [InlineData(@"
public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}
")]
    public async Task GenerateCodeFromText(string code)
    {
        var codeExactor = new DefaultCSharpModelCodeExtractor(new DefaultModelNameConverter());
        var tables = await codeExactor.GetTablesFromSourceText(new DbProvider.SqlServer.SqlServerDbProvider(), code);
        Assert.NotNull(tables);
        Assert.NotEmpty(tables);
        Assert.Equal(4, tables[0].Columns.Count);
    }
}
