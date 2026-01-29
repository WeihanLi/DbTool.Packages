// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

#:package WeihanLi.Common@1.0.84

using WeihanLi.Common.Helpers;

string[] srcProjects = [ 
    "./src/DbTool.Core/DbTool.Core.csproj",
    "./src/DbTool.MySql/DbTool.MySql.csproj",
    "./src/DbTool.Oracle/DbTool.Oracle.csproj",
    "./src/DbTool.PostgreSql/DbTool.PostgreSql.csproj",
    "./src/DbTool.SqlServer/DbTool.SqlServer.csproj"
];
string[] testProjects = [ 
    "./test/DbTool.Test/DbTool.Test.csproj"
];

await DotNetPackageBuildProcess
    .Create(options => 
    {
        options.SolutionPath = "./DbTool.Packages.slnx";
        options.SrcProjects = srcProjects;
        options.TestProjects = testProjects;
    })
    .ExecuteAsync(args);
