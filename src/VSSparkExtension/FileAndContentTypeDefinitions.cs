using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace VSSparkExtension
{
    internal static class FileAndContentTypeDefinitions
    {
        [Export]
        [Name("Spark")]
        [BaseDefinition("HTML")]
        internal static ContentTypeDefinition SparkContentTypeDefinition;

        [Export]
        [FileExtension(".spark")]
        [ContentType("Spark")]
        internal static FileExtensionToContentTypeDefinition SparkFileExtensionDefinition;
    }
}