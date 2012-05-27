using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace VSSparkExtension.VisualStudio.Classification
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("SparkView")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class SparkViewClassifierProvider : ITaggerProvider
    {
        [Export]
        [Name("SparkView")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition SparkViewContentType = null;

        [Export]
        [FileExtension(".spark")]
        [ContentType("SparkView")]
        internal static FileExtensionToContentTypeDefinition SparkViewFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<SparkViewTokenTag> ookTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<SparkViewTokenTag>(buffer);

            return new SparkViewClassifier(buffer, ookTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

  internal sealed class SparkViewClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<SparkViewTokenTag> _aggregator;
        IDictionary<SparkViewTokenTypes, IClassificationType> _ookTypes;

        internal OokClassifier(ITextBuffer buffer, 
                               ITagAggregator<SparkViewTokenTag> ookTagAggregator, 
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = ookTagAggregator;
            _ookTypes = new Dictionary<OokTokenTypes, IClassificationType>();
            _ookTypes[SparkViewTokenTypes.SparkView] = typeService.GetClassificationType("SparkView");
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            foreach (var tagSpan in this._aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return 
                    new TagSpan<ClassificationTag>(tagSpans[0], 
                                                   new ClassificationTag(_ookTypes[tagSpan.Tag.type]));
            }
        }
    }
}