using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.Consts
{
    internal static class EditorSnackMessages
    {
        public const string EXCEL_OPEN_FAIL_FILE_IN_USE = "Failed to open {0}. The file is probably in use.";

        public const string EXCEL_ANALYSIS_FAIL = "Analysis of {0} failed: {1}.";

        public const string EXCEL_IMPORT_SUCCESS = "Successfully imported {0}.";

        public const string FILE_OPEN_FAIL_FILE_IN_USE = "Failed to open {0}. The file is probably in use.";

        public const string FILE_OPEN_FAIL_INVALID_FILE = "Failed to open {0}. The file is invalid.";

        public const string FILE_OPEN_SUCCESS_AUTO_FIXED = "An automatic fix is available for {0}. Please save the file manually to apply the fix.";

        public const string FILE_SAVE_SUCCESS_NEW = "New file saved.";

        public const string FILE_SAVE_SUCCESS = "Changes saved.";

        public const string FILE_SAVE_AS_SUCCESS = "New file saved and loaded.";

        public const string QUESTION_NEW_SUCCESS = "New question added.";

        public const string QUESTION_DELETE_SUCCESS = "Question {0} deleted.";

        public const string QUESTION_COPY_SUCCESS = "Question {0} copied.";

        public const string MEDIA_ADD_SUCCESS = "{0} media file{1} loaded. Copy media code to insert in text.";

        public const string MEDIA_COPY_CODE_SUCCESS = "Code for {0} copied. Paste in any text to insert media.";

        public const string MEDIA_REPLACE_FAIL_SAME_MEDIA = "The new media file is the same as the old one. Replacement skipped.";

        public const string MEDIA_REPLACE_SUCCESS = "{0} is replaced with {1}.";

        public const string MEDIA_DELETE_SUCCESS = "{0} deleted.";

        public const string VALIDATION_COMPLETE = "Validation completed.";

        public const string INVALID_COMMANDLINE_ARGS = "Invalid commandline argument: {0}";
    }
}
