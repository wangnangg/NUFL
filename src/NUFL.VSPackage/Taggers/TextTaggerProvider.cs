﻿//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace OpenCover.UI.Tagger
{
	/// <summary>
	/// Background color tagger provider
	/// </summary>
	[Export(typeof(IViewTaggerProvider))]
    [ContentType("CSharp"), ContentType("Basic")]
	[TagType(typeof(ClassificationTag))]
	public class TextTaggerProvider : IViewTaggerProvider
	{
		/// <summary>
		/// The registry
		/// </summary>
		[Import]
		public IClassificationTypeRegistryService Registry;

		/// <summary>
		/// Gets or sets the text search service.
		/// </summary>
		/// <value>
		/// The text search service.
		/// </value>
		[Import]
		internal ITextSearchService TextSearchService { get; set; }

		/// <summary>
		/// Creates the tagger.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="textView">The text view.</param>
		/// <param name="buffer">The buffer.</param>
		/// <returns>Tagger for changing background color</returns>
		public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
		{
			if (buffer != textView.TextBuffer)
				return null;
			return new TextTagger
                (textView,
                Registry.GetClassificationType("nufl-text-background-covered"),
                Registry.GetClassificationType("nufl-text-background-notcovered"),
                Registry.GetClassificationType("nufl-text-background-level1"),
                Registry.GetClassificationType("nufl-text-background-level2"),
                Registry.GetClassificationType("nufl-text-background-level3"),
                Registry.GetClassificationType("nufl-text-background-level4"),
                Registry.GetClassificationType("nufl-text-background-level5")
                ) as ITagger<T>;
	
		}
	}
}
