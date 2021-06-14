namespace DeltaQuestionEditor_WPF.Consts
{
    internal static class ExcelImportProblems
    {
        /// <summary>
        /// Worksheets with no headers are still allowed, in which case the editor will guess the columns based on the data type and text format
        /// </summary>
        public const string NO_HEADERS = "This table has no headers. Detection of columns may be less accurate. Be sure to check the import summary carefully.";

        public const string MISSING_COLUMN_REQUIRED_SKILLS = "There isn't a column for \"Required Skills\" in the worksheet. Please enter the skills for each question manually in the editor.";

        /// <summary>
        /// All difficulty ratings will default to 2 if there isn't a difficulty column
        /// </summary>
        public const string MISSING_COLUMN_DIFFICULTY = "There isn't a column for \"Difficulty\" in the worksheet. Please rate the difficulty of each question manually in the editor.";

        /// <summary>
        /// Instead of using Markdown to insert media, some files have a separate column for media files. The editor will then put the specified media in the question text.
        /// </summary>
        public const string EXTRA_COLUMN_MEDIA = "A separate column for media files has been found. Media files in this column will be inserted to the question text automatically. Edit the questions manually if this isn't what you want.";

        public const string UNUSED_COLUMN = "Column {0}{1} is not used when importing. If this column contains useful information, please copy it manually in the editor.";

        public const string EMPTY_QUESTION_TEXT = "Question text of question {0} is empty!";

        public const string EMPTY_CORRECT_ANSWER = "The correct answer of question {0} is empty!";

        public const string EMPTY_WRONG_ANSWER = "Wrong answer {0} of question {1} is empty!";

        public const string EMPTY_DIFFICULTY = "There is no difficulty rating in question {0}!";

        public const string INVALID_DIFFICULTY = "Difficulty rating of question {0} is invalid! Please rate the difficulty manually in the editor.";

        public const string EMPTY_SKILLS = "There are no skills in question {0}!";

        /// <summary>
        /// Skills should have at least 2 dots (#.#.#). If there is 1 or 0 dot, then the form and chapter numbers are probably omitted. The editor will thus append the form and chapter number back.
        /// </summary>
        public const string SKILLS_TOO_SHORT = "Skills in question {0} are too short. The topic code ({1}.{2}) has been appended in front of the skills codes. Please check if this fix is correct.";

        public const string INVALID_SKILLS = "Skills in question {0} are invalid. They have been imported nonetheless. Please fix them manually in the editor.";

        /// <summary>
        /// If the media file is not found in the specified path, the editor will then search for the file in the same directory as the Excel file recursively (so as to fix incorrect relative paths). If the file is still not found, this problem will be issued.
        /// </summary>
        public const string MEDIA_NOT_FOUND = "Media not found: \"{0}\". Please add the media file manually in the editor.";

        public const string MEDIA_TOO_LARGE = "The file \"{0}\" is too large. Maximum file size is 500KB. The file will still be imported but you have to replace this file for the question set to be successfully validated.";
    }
}
