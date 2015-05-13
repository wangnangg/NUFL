//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace Buaa.NUFL_VSPackage.Taggers
{

	public static class CoveredTextBackgroundTypeExports
	{
		[Export(typeof(ClassificationTypeDefinition))]
		[Name("nufl-text-background-covered")]
		public static ClassificationTypeDefinition OrdinaryClassificationType;
	}


	public static class NotCoveredTextBackgroundTypeExports
	{
		[Export(typeof(ClassificationTypeDefinition))]
        [Name("nufl-text-background-notcovered")]
		public static ClassificationTypeDefinition OrdinaryClassificationType;
	}


    public static class Level1TextBackgroundTypeExports
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("nufl-text-background-level1")]
        public static ClassificationTypeDefinition OrdinaryClassificationType;
    }


    public static class Level2TextBackgroundTypeExports
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("nufl-text-background-level2")]
        public static ClassificationTypeDefinition OrdinaryClassificationType;
    }


    public static class Level3TextBackgroundTypeExports
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("nufl-text-background-level3")]
        public static ClassificationTypeDefinition OrdinaryClassificationType;
    }


    public static class Level4TextBackgroundTypeExports
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("nufl-text-background-level4")]
        public static ClassificationTypeDefinition OrdinaryClassificationType;
    }


    public static class Level5TextBackgroundTypeExports
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("nufl-text-background-level5")]
        public static ClassificationTypeDefinition OrdinaryClassificationType;
    }


	[Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "nufl-text-background-covered")]
    [Name("nufl-text-background-covered")]
	[UserVisible(true)]
	[Order(After = Priority.High)]
	public sealed class CoveredTextBackground : ClassificationFormatDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CoveredTextBackground"/> class.
		/// </summary>
		public CoveredTextBackground()
		{
			DisplayName = "Covered Text Background";
            BackgroundColor = Color.FromRgb(207, 231, 209);	
		}
	}


	[Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "nufl-text-background-notcovered")]
    [Name("nufl-text-background-notcovered")]
	[UserVisible(true)]
	[Order(After = Priority.High)]
	public sealed class NotCoveredTextBackground : ClassificationFormatDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CoveredTextBackground"/> class.
		/// </summary>
		public NotCoveredTextBackground()
		{
			DisplayName = "Not Covered Text Background";
            BackgroundColor = Color.FromRgb(255, 217, 217);			
		}
	}



    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "nufl-text-background-level1")]
    [Name("nufl-text-background-level1")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class Level1TextBackground : ClassificationFormatDefinition
    {

        public Level1TextBackground()
        {
            DisplayName = "Level1 Text Background";
            BackgroundColor = Color.FromRgb(207, 231, 209);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "nufl-text-background-level2")]
    [Name("nufl-text-background-level2")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class Level2TextBackground : ClassificationFormatDefinition
    {

        public Level2TextBackground()
        {
            DisplayName = "Level2 Text Background";
            BackgroundColor = Color.FromRgb(255, 217, 217);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "nufl-text-background-level3")]
    [Name("nufl-text-background-level3")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class Level3TextBackground : ClassificationFormatDefinition
    {

        public Level3TextBackground()
        {
            DisplayName = "Level3 Text Background";
            BackgroundColor = Color.FromRgb(255, 217, 217);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "nufl-text-background-level4")]
    [Name("nufl-text-background-level4")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class Level4TextBackground : ClassificationFormatDefinition
    {

        public Level4TextBackground()
        {
            DisplayName = "Level4 Text Background";
            BackgroundColor = Color.FromRgb(255, 217, 217);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "nufl-text-background-level5")]
    [Name("nufl-text-background-level5")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    public sealed class Level5TextBackground : ClassificationFormatDefinition
    {

        public Level5TextBackground()
        {
            DisplayName = "Level5 Text Background";
            BackgroundColor = Color.FromRgb(255, 217, 217);
        }
    }
}
