using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace VSSparkExtension
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Spark Completion Handler")]
    [ContentType("HTML")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class SparkCompletionHandlerProvider : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService;
        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }
        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            Func<SparkCompletionCommandHandler> createCommandHandler =
                () => new SparkCompletionCommandHandler(textViewAdapter, textView, this);
            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
        }
    }
}