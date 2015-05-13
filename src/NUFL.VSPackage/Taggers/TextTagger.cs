//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.UI.Tagger
{
	/// <summary>
	/// Text tagger to produce tags to change background color for covered lines
	/// </summary>
    public sealed class TextTagger : ITagger<ClassificationTag>, IDisposable
	{		
        IClassificationType _coveredType;
        IClassificationType _uncoveredType;
        IClassificationType _level1Type;
        IClassificationType _level2Type;
        IClassificationType _level3Type;
        IClassificationType _level4Type;
        IClassificationType _level5Type;

        ITextView _textView;

        List<SnapshotSpan> _currentSpans;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged = delegate { };
        

		public TextTagger(
            ITextView view,
            IClassificationType coveredType,
            IClassificationType uncoveredType,
            IClassificationType level1Type,
            IClassificationType level2Type,
            IClassificationType level3Type,
            IClassificationType level4Type,
            IClassificationType level5Type)
		{
            _textView = view;
			_coveredType = coveredType;
            _uncoveredType = uncoveredType;
            _level1Type = level1Type;
            _level2Type = level2Type;
            _level3Type = level3Type;
            _level4Type = level4Type;
            _level5Type = level5Type;

            _currentSpans = GetSpans(_textView.TextSnapshot);

            _textView.GotAggregateFocus += SetupSelectionChangedListener;
		}

        private void SetupSelectionChangedListener(object sender, EventArgs e)
        {
            if (_textView != null)
            {
                _textView.LayoutChanged += ViewLayoutChanged;
                _textView.GotAggregateFocus -= SetupSelectionChangedListener;
            }
        }

        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.OldSnapshot != e.NewSnapshot)
            {
                //update currentSpans
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(e.NewSnapshot, 0, e.NewSnapshot.Length)));
            }
        }

        public void Dispose()
        {
            if (_textView != null)
            {
                _textView.GotAggregateFocus -= SetupSelectionChangedListener;
                _textView.LayoutChanged -= ViewLayoutChanged;
            }


            _textView = null;
            _currentSpans.Clear();

        }
	

		public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
            if (_currentSpans == null || _currentSpans.Count == 0)
				yield break;
		}

        void RaiseAllTagsChanged()
        {
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, 0, _textView.TextBuffer.CurrentSnapshot.Length)));
        }


        List<SnapshotSpan> GetSpans(ITextSnapshot snapshot)
        {
            var spans = new List<SnapshotSpan>();
            return spans;
        }
     
    }
}
