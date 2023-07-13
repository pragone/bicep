// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Bicep.Core.Semantics;
using Bicep.Core.Workspaces;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using System.Text;
using Bicep.Core.Navigation;

//asdfg what folder?
namespace Bicep.Cli.Helpers;

public record PackedSourcesMetadata( //asdfg
    Uri EntryPoint
);

public class PackedSourcesAsdfg
{
    private string localSourcesFolder; //asdfg?

    private const string ZipFileName = "bicepSources.zip";

    public class PackedSources : IDisposable
    {
        public string ZipFilePath { get; }

        private string? tempFolder;

        public PackedSources(string tempFolder, string zipFilePath) {
            this.ZipFilePath = zipFilePath;
            this.tempFolder = tempFolder;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (tempFolder is not null && disposing)
            {
                Directory.Delete(this.tempFolder, recursive: true);
                this.tempFolder = null;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public PackedSourcesAsdfg(string localSourcesFolder)
    {
        this.localSourcesFolder = localSourcesFolder;
    }

    

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public static PackedSources PackSources(Compilation compilation)
    {
        //asdfg how structure hierarchy of files?
        //asdfg map of locations to filenames

        var tempFolder = Directory.CreateTempSubdirectory("biceppublish_"); //asdfg delete when done
        var zipSourceRoot = Directory.CreateDirectory(Path.Join(tempFolder.FullName, Path.GetFileNameWithoutExtension(ZipFileName)));
        var sourcesFolder = Directory.CreateDirectory(Path.Join(zipSourceRoot.FullName, "files"));
        var metadataPath = Path.Join(zipSourceRoot.FullName, "metadata.json");

        var metadata = new PackedSourcesMetadata(compilation.SourceFileGrouping.EntryFileUri);
        string metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(metadataPath, metadataJson, Encoding.UTF8);

        foreach (var file in compilation.SourceFileGrouping.SourceFiles)
        {
            string source;
            switch (file)
            {
                case BicepFile bicepFile:
                    source = bicepFile.ProgramSyntax.ToTextPreserveFormatting(); //asdfg?
                    break;
                case ArmTemplateFile armTemplateFile:
                    source = armTemplateFile.Template?.ToJson() ?? "(ARM template is null)"; //asdfg testpoint
                    break;
                case TemplateSpecFile templateSpecFile:
                    source = templateSpecFile.MainTemplateFile.Template?.ToJson() ?? "(ARM template is null)"; //asdfg testpoint
                    break;
                default:
                    throw new ArgumentException($"Unexpected source file type {file.GetType().Name}");
            }

            //asdfg map folder structure
            var sourceRelativeDestinationPath = Path.GetFileName(file.FileUri.AbsolutePath); ;
            File.WriteAllText(Path.Combine(sourcesFolder.FullName, sourceRelativeDestinationPath), source, Encoding.UTF8);
        }

        var zipPath = Path.Combine(tempFolder.FullName, ZipFileName);
        ZipFile.CreateFromDirectory(zipSourceRoot.FullName, zipPath);

        return new PackedSources(tempFolder.FullName, zipPath);
    }
}