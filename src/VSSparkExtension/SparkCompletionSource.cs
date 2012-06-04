using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace VSSparkExtension
{
    internal class SparkCompletionSource : ICompletionSource
    {
        private readonly SparkCompletionSourceProvider _sourceProvider;
        private readonly ITextBuffer _textBuffer;
        private List<Completion> _compList;

        public SparkCompletionSource(SparkCompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            _sourceProvider = sourceProvider;
            _textBuffer = textBuffer;
        }

        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var allKnown = new List<string>
                               {
                                   "var",
                                   "def",
                                   "default",
                                   "global",
                                   "viewdata",
                                   "model",
                                   "set",
                                   "for",
                                   "test",
                                   "if",
                                   "else",
                                   "elseif",
                                   "content",
                                   "use",
                                   "macro",
                                   "render",
                                   "section",
                                   "cache",
                               };
            allKnown.Sort();

            _compList = new List<Completion>();
            foreach (var known in allKnown)
                _compList.Add(new Completion(known, known, known, null, null));

            completionSets.Add(new CompletionSet(
                                   "Spark", //the non-localized title of the tab
                                   "Spark", //the display title of the tab
                                   FindTokenSpanAtPosition(session.GetTriggerPoint(_textBuffer), session), _compList,
                                   null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, IIntellisenseSession session)
        {
            var currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            var navigator = _sourceProvider.NavigatorService.GetTextStructureNavigator(_textBuffer);
            var extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed) return;

            GC.SuppressFinalize(this);
            _isDisposed = true;
        }
    }
}