namespace DeltaEditor.Consts
{
    /// <summary>
    /// String literals for import failures. The import process will be aborted in these cases.
    /// </summary>
    internal static class ExcelImportFailures
    {
        /// <summary>
        /// Empty Excel workbook file
        /// </summary>
        public const string NO_WORKSHEETS_FOUND = "no worksheets found in the file";

        /// <summary>
        /// There should be at least 5 columns (1 for question text and 4 for answers) for the worksheet to be valid
        /// </summary>
        public const string TOO_FEW_COLUMNS = "all worksheets have too few columns";

        /// <summary>
        /// The worksheet selection algorithm determined that no worksheet has a suitable format in the selected Excel workbook
        /// </summary>
        public const string NO_VALID_WORKSHEET = "no valid worksheet found";
    }
}
