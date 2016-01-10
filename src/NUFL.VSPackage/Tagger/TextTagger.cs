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
using NUFL.Framework.Setting;
using NUFL.Service;
using NUFL.Framework.Analysis;
using NUFL.Framework.Model;
namespace Buaa.NUFL_VSPackage.Tagger
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
        string _file_fullname;

        ISetting _option;

        GlobalEventManager _event_manager;

        Program _program = null;

       
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

            _textView.GotAggregateFocus += SetupSelectionChangedListener;

            _option = (ISetting)ServiceManager.Instance.GetService(typeof(ISetting));
            _event_manager = GlobalEventManager.Instance;
            _event_manager.SubscribeEventByName(EventEnum.ProgramChanged, OnProgramChanged);
            _option.SettingChanged += _option_SettingChanged;

            _file_fullname = Helpers.IDEHelper.GetFileName(_textView);


		}

        void _option_SettingChanged(string key, object value)
        {
            if (key == "show_background_color" || key == "function_mode")
            {
                RaiseAllTagsChanged();
            } 
        }

        void OnProgramChanged(GlobalEvent @event)
        {
            _program = @event.Argument as Program;
            RaiseAllTagsChanged();
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
            _option.SettingChanged -= _option_SettingChanged;
            _event_manager.UnsubscribeEventByName(EventEnum.ProgramChanged, OnProgramChanged);
            if (_textView != null)
            {
                _textView.GotAggregateFocus -= SetupSelectionChangedListener;
                _textView.LayoutChanged -= ViewLayoutChanged;
            }


            _textView = null;

        }
	

		public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection collection)
		{
            if ( !_option.GetSetting<bool>("show_background_color") || _file_fullname == null)
            {
                yield break;
            }
            if(_program == null)
            {
                yield break;
            }
            SourceFile file = _program.Files.GetFile(_file_fullname, false);
            if (file == null)
            {
                yield break;
            }
            foreach (var point in file.GetPointEnumerator())
            {
                SnapshotSpan span;
                ClassificationTag tag;
                try
                {
                    span = CreateSpan(_textView.TextSnapshot, point);
                    tag = CreateTag(point);
                } catch(Exception e)
                {
                    continue;
                }
                if (tag == null)
                {
                    continue;
                }
                yield return new TagSpan<ClassificationTag>(span, tag);
            }
            yield break;

		}

        private ClassificationTag CreateTag(InstrumentationPoint point)
        {
            if (_option.GetSetting<string>("function_mode") == "cov")
            {
                if(point.CoveredLeafChildrenCount > 0)
                {
                    return new ClassificationTag(_coveredType);
                } else
                {
                    return new ClassificationTag(_uncoveredType);
                }


            } else
            {
                switch(point.SuspLevel)
                {
                    case 1:
                        return new ClassificationTag(_level1Type);
                    case 2:
                        return new ClassificationTag(_level2Type);
                    case 3:
                        return new ClassificationTag(_level3Type);
                    case 4:
                        return new ClassificationTag(_level4Type);
                    case 5:
                        return new ClassificationTag(_level5Type);
                    default:
                        return new ClassificationTag(_level1Type);
                }
            }
        }

        private SnapshotSpan CreateSpan(ITextSnapshot snapshot, InstrumentationPoint point)
        {
 	        var start_line = snapshot.GetLineFromLineNumber(point.StartLine - 1);
            var start_position = start_line.Extent.Start.Position + point.StartColumn -1;
            SnapshotPoint start_point = new SnapshotPoint(snapshot, start_position);

            var end_line = snapshot.GetLineFromLineNumber(point.EndLine - 1);
            var end_position = end_line.Extent.Start.Position + point.EndColumn - 1;
            SnapshotPoint end_point = new SnapshotPoint(snapshot, end_position);

            return new SnapshotSpan(start_point, end_point);
        }

        void RaiseAllTagsChanged()
        {
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textView.TextBuffer.CurrentSnapshot, 0, _textView.TextBuffer.CurrentSnapshot.Length)));
        }
     
    }
}
