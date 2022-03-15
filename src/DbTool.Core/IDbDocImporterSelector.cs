// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace DbTool.Core;

public interface IDbDocImporterSelector
{
    IDbDocImporter GetImporter(string filePath);
}

public sealed class DefaultDbDocImporterSelector : IDbDocImporterSelector
{
    private readonly IDbDocImporter[] _importers;

    public DefaultDbDocImporterSelector(IEnumerable<IDbDocImporter> importers)
    {
        _importers = importers.ToArray();
    }

    public IDbDocImporter GetImporter(string filePath)
    {
        var fileExtension = Path.GetExtension(filePath);
        return _importers.LastOrDefault(i => i.SupportedFileExtensions.ContainsKey(fileExtension))
            ?? throw new InvalidOperationException($"No importer registered for {fileExtension}");
    }
}
