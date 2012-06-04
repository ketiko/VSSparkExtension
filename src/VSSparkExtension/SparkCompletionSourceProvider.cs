using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace VSSparkExtension
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("HTML")]
    [Name("Spark Completion")]
    internal class SparkCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new SparkCompletionSource(this, textBuffer);
        }
    }
}