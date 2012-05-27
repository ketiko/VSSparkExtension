namespace VSSparkExtension.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ITaggerProvider))]
    [ContentType("SparkView")]
    [TagType(typeof(SparkViewTokenTag))]
    internal sealed class SparkViewTokenTagProvider : ITaggerProvider
    {

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new SparkViewTokenTagger(buffer) as ITagger<T>;
        }
    }

    public class SparkViewTokenTag : ITag
    {
        public SparkViewTokenTypes type { get; private set; }

        public SparkViewTokenTag(SparkViewTokenTypes type)
        {
            this.type = type;
        }
    }

    internal sealed class SparkViewTokenTagger : ITagger<SparkViewTokenTag>
    {

        ITextBuffer _buffer;
        IDictionary<string, SparkViewTokenTypes> _SparkViewTypes;

        internal SparkViewTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _SparkViewTypes = new Dictionary<string, SparkViewTokenTypes>();
            _SparkViewTypes["SparkView"] = SparkViewTokenTypes.SparkView;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<SparkViewTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                int curLoc = containingLine.Start.Position;
                string[] tokens = containingLine.GetText().ToLower().Split(' ');

                foreach (string SparkViewToken in tokens)
                {
                    if (_SparkViewTypes.ContainsKey(SparkViewToken))
                    {
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc, SparkViewToken.Length));
                        if (tokenSpan.IntersectsWith(curSpan))
                            yield return new TagSpan<SparkViewTokenTag>(tokenSpan,
                                                                  new SparkViewTokenTag(_SparkViewTypes[SparkViewToken]));
                    }

                    //add an extra char location because of the space
                    curLoc += SparkViewToken.Length + 1;
                }
            }

        }
    }
}