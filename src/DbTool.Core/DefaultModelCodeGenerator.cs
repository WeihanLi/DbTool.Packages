// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core.Entity;
using System.Text;
using WeihanLi.Extensions;

namespace DbTool.Core;

public class DefaultCSharpModelCodeGenerator : IModelCodeGenerator
{
    private readonly IModelNameConverter _modelNameConverter;

    public string FileExtension => ".cs";

    public string CodeType => "C#";

    public DefaultCSharpModelCodeGenerator(IModelNameConverter modelNameConverter)
    {
        _modelNameConverter = modelNameConverter;
    }

    public virtual string GenerateModelCode(TableEntity tableEntity, ModelCodeGenerateOptions options, IDbProvider dbProvider)
    {
        if (tableEntity == null)
        {
            throw new ArgumentNullException(nameof(tableEntity));
        }
        if (dbProvider == null)
        {
            throw new ArgumentNullException(nameof(dbProvider));
        }

        var sbText = new StringBuilder();
        if (!(options.GlobalUsingEnabled && options.GlobalUsing.Contains("System")))
        {
            sbText.AppendLine("using System;");
        }
        if (options.GenerateDataAnnotation)
        {
            sbText.AppendLine("using System.ComponentModel;");
            sbText.AppendLine("using System.ComponentModel.DataAnnotations;");
            sbText.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
        }
        sbText.AppendLine();
        var namespaceText = $"namespace {options.Namespace}";
        var classIndentation = string.Empty;
        var memberIndentation = options.Indentation;
        if (options.FileScopedNamespaceEnabled)
        {
            namespaceText += ";";
        }
        else
        {
            classIndentation = options.Indentation;
            memberIndentation = options.Indentation.Repeat(2);
        }
        sbText.AppendLine(namespaceText);
        if (!options.FileScopedNamespaceEnabled)
            sbText.AppendLine("{");
        else
            sbText.AppendLine();

        if (options.GenerateDataAnnotation && !string.IsNullOrEmpty(tableEntity.TableDescription))
        {
            sbText.AppendLine(
                $"{classIndentation}/// <summary>{Environment.NewLine}{options.Indentation}/// {tableEntity.TableDescription.Replace(Environment.NewLine, " ")}{Environment.NewLine}{options.Indentation}/// </summary>");
            sbText.AppendLine($"{classIndentation}[Table(\"{tableEntity.TableName}\")]");
            sbText.AppendLine($"{classIndentation}[Description(\"{tableEntity.TableDescription.Replace(Environment.NewLine, " ")}\")]");
        }
        sbText.AppendLine($"{classIndentation}public class {options.Prefix}{_modelNameConverter.ConvertTableToModel(tableEntity.TableName)}{options.Suffix}");
        sbText.AppendLine($"{classIndentation}{{");
        var index = 0;
        if (options.GeneratePrivateFields)
        {
            foreach (var item in tableEntity.Columns)
            {
                if (index > 0)
                {
                    sbText.AppendLine();
                }
                else
                {
                    index++;
                }
                var fclType = dbProvider.DbType2ClrType(item.DataType, item.IsNullable);
                if (item.IsNullable && fclType[^1] != '?' && options.NullableReferenceTypesEnabled)
                {
                    fclType += "?";
                }
                var tmpColName = ToPrivateFieldName(item.ColumnName);
                sbText.AppendLine($"{memberIndentation}private {fclType} {tmpColName};");
                if (options.GenerateDataAnnotation)
                {
                    if (!string.IsNullOrEmpty(item.ColumnDescription))
                    {
                        sbText.AppendLine(
                            $"{memberIndentation}/// <summary>{Environment.NewLine}{options.Indentation}{options.Indentation}/// {item.ColumnDescription.Replace(Environment.NewLine, " ")}{Environment.NewLine}{options.Indentation}{options.Indentation}/// </summary>");
                        if (options.GenerateDataAnnotation)
                        {
                            sbText.AppendLine($"{memberIndentation}[Description(\"{item.ColumnDescription.Replace(Environment.NewLine, " ")}\")]");
                        }
                    }
                    else
                    {
                        if (item.IsPrimaryKey)
                        {
                            sbText.AppendLine($"{memberIndentation}[Description(\"PrimaryKey\")]");
                        }
                    }
                    if (item.IsPrimaryKey)
                    {
                        sbText.AppendLine($"{memberIndentation}[Key]");
                    }
                    if (fclType == "string" && item.Size is > 0 and < int.MaxValue)
                    {
                        sbText.AppendLine($"{memberIndentation}[StringLength({item.Size})]");
                    }
                    sbText.AppendLine($"{memberIndentation}[Column(\"{item.ColumnName}\")]");
                }
                sbText.AppendLine($"{memberIndentation}public {fclType} {item.ColumnName}");
                sbText.AppendLine($"{memberIndentation}{{");
                sbText.AppendLine($"{memberIndentation}{options.Indentation}get {{ return {tmpColName}; }}");
                sbText.AppendLine($"{memberIndentation}{options.Indentation}set {{ {tmpColName} = value; }}");
                sbText.AppendLine($"{memberIndentation}}}");
                sbText.AppendLine();
            }
        }
        else
        {
            foreach (var item in tableEntity.Columns)
            {
                if (index > 0)
                {
                    sbText.AppendLine();
                }
                else
                {
                    index++;
                }
                var fclType = dbProvider.DbType2ClrType(item.DataType, item.IsNullable);
                if (item.IsNullable && fclType[^1] != '?' && options.NullableReferenceTypesEnabled)
                {
                    fclType += "?";
                }
                if (options.GenerateDataAnnotation)
                {
                    if (!string.IsNullOrEmpty(item.ColumnDescription))
                    {
                        sbText.AppendLine(
                            $"{memberIndentation}/// <summary>{Environment.NewLine}{options.Indentation}{options.Indentation}/// {item.ColumnDescription.Replace(Environment.NewLine, " ")}{Environment.NewLine}{options.Indentation}{options.Indentation}/// </summary>");
                        if (options.GenerateDataAnnotation)
                        {
                            sbText.AppendLine($"{memberIndentation}[Description(\"{item.ColumnDescription.Replace(Environment.NewLine, " ")}\")]");
                        }
                    }
                    if (item.IsPrimaryKey)
                    {
                        sbText.AppendLine($"{memberIndentation}[Key]");
                    }
                    if (fclType == "string" && item.Size is > 0 and < int.MaxValue)
                    {
                        sbText.AppendLine($"{memberIndentation}[StringLength({item.Size})]");
                    }
                    sbText.AppendLine($"{memberIndentation}[Column(\"{item.ColumnName}\")]");
                }
                sbText.AppendLine($"{memberIndentation}public {fclType} {item.ColumnName} {{ get; set; }}");
            }
        }
        sbText.AppendLine($"{classIndentation}}}");
        if (!options.FileScopedNamespaceEnabled)
            sbText.AppendLine("}");
        return sbText.ToString().Trim();
    }

    /// <summary>
    /// 将属性名称转换为私有字段名称
    /// </summary>
    /// <param name="propertyName"> 属性名称 </param>
    /// <returns> 私有字段名称 </returns>
    protected virtual string ToPrivateFieldName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return string.Empty;
        }
        // 全部大写的专有名词
        if (propertyName.Equals(propertyName.ToUpperInvariant()))
        {
            return propertyName.ToLowerInvariant();
        }
        // 首字母大写转成小写
        if (char.IsUpper(propertyName[0]))
        {
            return $"{char.ToLower(propertyName[0])}{propertyName[1..]}";
        }
        return $"_{propertyName}";
    }
}
