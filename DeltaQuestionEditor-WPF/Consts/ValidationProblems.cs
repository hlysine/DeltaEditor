namespace DeltaQuestionEditor_WPF.Consts
{
    internal static class ValidationProblems
    {
        public const string MEDIA_TOO_LARGE = "The file size of {0} ({1}) is too large! Max file size is 500KB.";

        public const string CONTENT_EMPTY = "The {0} of question {1} is empty.";

        public const string WRONG_NUMBER_OF_ANSWERS = "The number of answers in question {0} is incorrect.";

        public const string WRONG_DIFFICULTY = "The difficulty of question {0} is invalid.";

        public const string MEDIA_CUSTOM_ALT_TEXT = "The media code in the {0} of question {1} contains custom alternate text. Please avoid doing so if possible.";

        public const string MEDIA_INVALID_PATH = "The media code in the {0} of question {1} contains invalid path. Please make sure that the related media files are imported and the path is correct.";

        public const string QUESTION_SET_EMPTY = "There are no questions in this question set.";

        public const string QUESTION_SET_INVALID_TOPIC = "This question set has an invalid topic code.";

        public const string QUESTION_SKILL_NO_CURRENT_TOPIC = "The skill code of question {0} does not contain skills from the current topic ({1}.{2} {3}). Please double-check for errors.";

        public const string QUESTION_SKILL_INVALID = "The skill code of question {0} is invalid.";

        public const string TEXT_ASCIIMATH_MISINTERPRETATION = "The backtick signs (`) in the {0} of question {1} seem to be misinterpreted as AsciiMath. Please double-check for errors.";

        public const string TEXT_LATEX_MISINTERPRETATION = "The dollar signs ($) in the {0} of question {1} seem to be misinterpreted as LaTeX. Please double-check for errors.";

        public const string TEXT_UNFORMATTED_MATH = "There seems to be unformatted mathematical expressions/symbols in the {0} of question {1}. Please format all symbols and expressions (including plain numbers) by wrapping them with `` or $$.";

        public const string MEDIA_UNUSED = "{0} is unused. Copy its media code to insert in any text or consider removing it.";
    }
}
