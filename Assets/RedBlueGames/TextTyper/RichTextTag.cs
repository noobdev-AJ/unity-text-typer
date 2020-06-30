namespace RedBlueGames.Tools.TextTyper
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// RichTextTags help parse text that contains HTML style tags, used by Unity's RichText text components.
    /// </summary>
    public class RichTextTag
    {
        public static readonly RichTextTag ClearColorTag = new RichTextTag("<color=#00000000>");

        private const char OpeningNodeDelimeter = '<';
        private const char CloseNodeDelimeter = '>';
        private const char EndTagDelimeter = '/';
        private const string ParameterDelimeter = "=";

        private readonly string tagText;

        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextTag"/> class.
        /// </summary>
        /// <param name="tagText">Tag text.</param>
        public RichTextTag(string tagText)
        {
            this.tagText = tagText;
            this.TagType = ParseTagType(tagText);
            this.Parameter = ParseParameter(tagText);
        }

        /// <summary>
        /// Gets the full tag text including markers.
        /// </summary>
        /// <value>The tag full text.</value>
        public string TagText
        {
            get
            {
                return this.tagText;
            }
        }

        /// <summary>
        /// Gets the text for this tag if it's used as a closing tag. Closing tags are unchanged.
        /// </summary>
        /// <value>The closing tag text.</value>
        public string ClosingTagText
        {
            get
            {
                return this.IsClosingTag ? this.TagText : string.Format("</{0}>", this.TagType);
            }
        }

        /// <summary>
        /// Gets the TagType, the body of the tag as a string
        /// </summary>
        /// <value>The type of the tag.</value>
        public string TagType {get; private set;}

        /// <summary>
        /// Gets the parameter as a string. Ex: For tag Color=#FF00FFFF the parameter would be #FF00FFFF.
        /// </summary>
        /// <value>The parameter.</value>
        public string Parameter {get; private set;}

        /// <summary>
        /// Gets a value indicating whether this instance is an opening tag.
        /// </summary>
        /// <value><c>true</c> if this instance is an opening tag; otherwise, <c>false</c>.</value>
        public bool IsOpeningTag
        {
            get
            {
                return !this.IsClosingTag;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a closing tag.
        /// </summary>
        /// <value><c>true</c> if this instance is a closing tag; otherwise, <c>false</c>.</value>
        public bool IsClosingTag
        {
            get
            {
                return this.TagText.Length > 2 && this.TagText[1] == EndTagDelimeter;
            }
        }

        /// <summary>
        /// Gets the length of the tag. Shorcut for the length of the full TagText.
        /// </summary>
        /// <value>The text length.</value>
        public int Length
        {
            get
            {
                return this.TagText.Length;
            }
        }

        /// <summary>
        /// Checks if the specified character starts with a tag.
        /// </summary>
        /// <returns><c>true</c>, if the character begins a tag <c>false</c> otherwise.</returns>
        /// <param name="character">Character to check for tags.</param>
        public static bool IsCharacterTagOpening(char character)
        {
            return character == RichTextTag.OpeningNodeDelimeter;
        }

        /// <summary>
        /// Parses the text for the next RichTextTag.
        /// </summary>
        /// <returns>The next RichTextTag in the sequence. Null if the sequence contains no RichTextTag</returns>
        /// <param name="text">Text to parse.</param>
        public static RichTextTag ParseNext(string text)
        {
            return ParseNext(text, 0, text.Length - 1);
        }

        /// <summary>
        /// Parses the text for the next RichTextTag.
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <param name="startIndex">Start index to start parsing from (inclusive)</param>
        /// <param name="endIndexInclusive">End index to parse to (inclusive)</param>
        /// /// <returns>The next RichTextTag in the sequence. Null if the sequence contains no RichTextTag</returns>
        public static RichTextTag ParseNext(string text, int startIndex, int endIndexInclusive)
        {
            var length = endIndexInclusive - startIndex;

            // Trim up to the first delimeter
            var openingDelimeterIndex = text.IndexOf(RichTextTag.OpeningNodeDelimeter, startIndex, length);

            // No opening delimeter found. Might want to throw.
            if (openingDelimeterIndex < 0)
            {
                return null;
            }

            var closingDelimeterIndex = text.IndexOf(RichTextTag.CloseNodeDelimeter, startIndex, length);

            // No closingDelimeter found. Might want to throw.
            if (closingDelimeterIndex < 0)
            {
                return null;
            }

            var tagText = text.Substring(openingDelimeterIndex, closingDelimeterIndex - openingDelimeterIndex + 1);
            return new RichTextTag(tagText);
        }

        /// <summary>
        /// Removes all copies of the tag of the specified type from the text string.
        /// </summary>
        /// <returns>The text string without any tag of the specified type.</returns>
        /// <param name="text">Text to remove Tags from.</param>
        /// <param name="tagType">Tag type to remove.</param>
        public static string RemoveTagsFromString(string text, string tagType)
        {
            var bodyWithoutTags = text;
            for (int i = 0; i < text.Length; ++i)
            {
                if (IsCharacterTagOpening(text[i]))
                {
                    var parsedTag = ParseNext(text, i, text.Length - 1);
                    if (parsedTag.TagType == tagType)
                    {
                        bodyWithoutTags = bodyWithoutTags.Replace(parsedTag.TagText, string.Empty);
                    }

                    i += parsedTag.Length - 1;
                }
            }

            return bodyWithoutTags;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="RichTextTag"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="RichTextTag"/>.</returns>
        public override string ToString()
        {
            return this.TagText;
        }

        private static string ParseTagType(string fullTag)
        {
            // Strip start and end tags
            var tagType = fullTag.Substring(1, fullTag.Length - 2);
            tagType = tagType.TrimStart(EndTagDelimeter);

            // Strip Parameter
            var parameterDelimeterIndex = tagType.IndexOf(ParameterDelimeter);
            if (parameterDelimeterIndex > 0)
            {
                tagType = tagType.Substring(0, parameterDelimeterIndex);
            }

            return tagType;
        }

        private static string ParseParameter(string fullTag)
        {
            var parameterDelimeterIndex = fullTag.IndexOf(ParameterDelimeter);
            if (parameterDelimeterIndex < 0)
            {
                return string.Empty;
            }

            // Subtract two, one for the delimeter and one for the closing character
            var parameterLength = fullTag.Length - parameterDelimeterIndex - 2;
            var parameter = fullTag.Substring(parameterDelimeterIndex + 1, parameterLength);

            // Kill optional enclosing quotes
            if (parameter.Length > 0)
            {
                if (parameter[0] == '\"' && parameter[parameter.Length - 1] == '\"')
                {
                    parameter = parameter.Substring(1, parameter.Length - 2);
                }
            }

            return parameter;
        }
    }
}