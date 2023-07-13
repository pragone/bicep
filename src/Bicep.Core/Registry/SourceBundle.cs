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
using System.IO.Abstractions;
using System.Linq;

namespace Bicep.Core.Registry;

public class SourceBundle
{
    public static class SourceKinds {
        const string Bicep = "bicep";
        const string ArmTemplate = "armTemplate";
        const string TemplateSpec = "templateSpec";
        // IF ADDING TO THIS: Remember both forwards and backwards compatibility.
        // Previous versions must be able to deal with unrecognized source kinds.   asdfg test
    }

//asdfg how test forwards compat?
    public static class SourceInfoKeys {
        const string Uri = "uri"; // Required for all versions
        const string LocalPath = "localPath"; // Required for all versions
        const string Kind = "kind"; // Required for all versions
        // IF ADDING TO THIS: Remember both forwards and backwards compatibility.
        //   Previous versions of Bicep must be able to ignore what is added.
    }

    public record FileMetadata( //asdfg
        Uri Uri,
        string LocalPath,
        string Kind
    );

// asdfg test that deserializing this with unknown properties works
    public record Metadata( //asdfg
        Uri EntryPoint, //asdfg?
        List<Dictionary<string, string>> SourceFiles
    );

    private IFileSystem fileSystem;
    private string localSourcesFolder; //asdfg?

    private const string ZipFileName = "bicepSources.zip";

    public class PackResult : IDisposable
    {
        public string ZipFilePath { get; }

        private string? tempFolder;

        public PackResult(string tempFolder, string zipFilePath)
        {
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

    public SourceBundle(IFileSystem fileSystem, string localSourcesFolder)
    {
        this.localSourcesFolder = localSourcesFolder;
        this.fileSystem = fileSystem;
    }

    // public IEnumerable<(Uri uri, FileMetadata metadata, string contents)> GetSourceFiles()
    // {
    //     var metadataPath = Path.Join(localSourcesFolder, "metadata.json"); //asdfg constant
    //     var metadataJson = File.ReadAllText(metadataPath, Encoding.UTF8);
    //     var metadata = JsonSerializer.Deserialize<Metadata>(metadataJson);
    //     if (metadata is null)
    //     {
    //         throw new ArgumentException($"Unable to deserialize metadata from {metadataPath}");
    //     }

    //     // foreach (var file in metadata.SourceFiles)
    //     // {
    //     //     switch (file)
    //     //     {
    //     //         case BicepFile bicepFile:
    //     //             source = bicepFile.ProgramSyntax.ToTextPreserveFormatting(); //asdfg?
    //     //             break;
    //     //         case ArmTemplateFile armTemplateFile:
    //     //             source = armTemplateFile.Template?.ToJson() ?? "(ARM template is null)"; //asdfg testpoint
    //     //             break;
    //     //         case TemplateSpecFile templateSpecFile:
    //     //             source = templateSpecFile.MainTemplateFile.Template?.ToJson() ?? "(ARM template is null)"; //asdfg testpoint
    //     //             break;
    //     //         default:
    //     //             throw new ArgumentException($"Unexpected source file type {file.GetType().Name}");
    //     //     }

    //     //     //asdfg map folder structure
    //     //     var sourceRelativeDestinationPath = Path.GetFileName(file.FileUri.AbsolutePath); ;
    //     //     File.WriteAllText(Path.Combine(sourcesFolder.FullName, sourceRelativeDestinationPath), source, Encoding.UTF8);
    //     // }

    //     // var zipPath = Path.Combine(tempFolder.FullName, ZipFileName);
    //     // ZipFile.CreateFromDirectory(zipSourceRoot.FullName, zipPath);

    //     // return new PackResult(tempFolder.FullName, zipPath);
    // }

    public static PackResult PackSources(IFileSystem fileSystem, SourceFileGrouping sourceFileGrouping) {
        return PackSources(sourceFileGrouping.EntryFileUri, sourceFileGrouping.SourceFiles.ToArray());
    }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public static PackResult PackSources(IFileSystem fileSystem, Uri entryFileUri, params ISourceFile[] sourceFiles)
    {
        //asdfg how structure hierarchy of files?
        //asdfg map of locations to filenames

        var tempFolder = Directory.CreateTempSubdirectory("biceppublish_");
        var zipSourceRoot = Directory.CreateDirectory(Path.Join(tempFolder.FullName, Path.GetFileNameWithoutExtension(ZipFileName)));
        var sourcesFolder = Directory.CreateDirectory(Path.Join(zipSourceRoot.FullName, "files"));
        var metadataPath = Path.Join(zipSourceRoot.FullName, "metadata.json");

        var filesMetadata = new List<file
        var metadata = new Metadata(entryFileUri, sourceFiles);
        string metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(metadataPath, metadataJson, Encoding.UTF8);

        foreach (var file in sourceFiles)
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
        //asdfg ZipFile.CreateFromDirectory(zipSourceRoot.FullName, zipPath);

        return new PackResult(tempFolder.FullName, zipPath);
    }

    public static void UnpackSources(IFileSystem fileSystem, Stream sourcesStream, string localSourcesFolder)
    {
        var zipFile = fileSystem.Path.GetTempFileName(); //asdfg delete when done
        using (var fileStream = fileSystem.File.OpenWrite(zipFile))
        {
            sourcesStream.CopyTo(fileStream);
        }

        ZipFile.ExtractToDirectory(zipFile, localSourcesFolder); //asdfg won't work with filessytem?  use ZipArchive
    }

    public static void UnpackSources(IFileSystem fileSystem, string zipFilePath, string localSourcesFolder)
    {
        fileSystem.File.Copy(zipFilePath, Path.Combine(localSourcesFolder, ZipFileName), overwrite: true);
        //asdfg ZipFile.ExtractToDirectory(zipFilePath, localSourcesFolder);
    }
}
