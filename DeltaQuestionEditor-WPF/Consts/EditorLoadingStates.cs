using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.Consts
{
    internal static class EditorLoadingStates
    {
        public static string EXCEL_READING = "Reading";

        public static string EXCEL_ANALYZING = "Analyzing";

        public static string EXCEL_IMPORTING_QUESTIONS = "Importing questions";

        public static string EXCEL_IMPORTING_MEDIA = "Importing media files";

        public static string FILE_OPENING = "Opening";

        public static string FILE_SAVE_AS_LOADING = "Saving";

        public static string FILE_SAVING = "Saving";

        public static string MEDIA_ADDING = "Inserting media {0}/{1}";

        public static string MEDIA_REPLACING = "Replacing media";

        public static string MEDIA_REPLACE_UNDOING = "Undoing media replacement";

        public static string MEDIA_DELETING = "Deleting media";
    }
}
