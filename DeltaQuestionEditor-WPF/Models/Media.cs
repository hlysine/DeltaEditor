using DeltaQuestionEditor_WPF.DataSources;
using DeltaQuestionEditor_WPF.Helpers;
using Newtonsoft.Json;

namespace DeltaQuestionEditor_WPF.Models
{
    public class Media : NotifyPropertyChanged
    {
        private LocalFileDataSource dataSource;
        [JsonIgnore]
        public LocalFileDataSource DataSource { private get => dataSource; set => SetAndNotify(ref dataSource, value, new[] { nameof(FullPath) }); }

        private string id;
        public string Id
        {
            get => id;
            set => SetAndNotify(ref id, value);
        }
        private string fileName;
        public string FileName
        {
            get => fileName;
            set => SetAndNotify(ref fileName, value);
        }
        private string name;
        public string Name
        {
            get => name;
            set => SetAndNotify(ref name, value);
        }

        [JsonIgnore]
        public string FullPath => DataSource?.TempToAbsolutePath(FileName);
    }
}
