using DeltaQuestionEditor_WPF.DataSources;
using DeltaQuestionEditor_WPF.Helpers;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace DeltaQuestionEditor_WPF.Models
{
    public class QuestionSet : NotifyPropertyChanged
    {
        private LocalFileDataSource dataSource;
        [JsonIgnore]
        public LocalFileDataSource DataSource { private get => dataSource; set => SetAndNotify(ref dataSource, value, new[] { nameof(TopicName) }); }

        private int form = 1;
        public int Form
        {
            get => form;
            set => SetAndNotify(ref form, value, new[] { nameof(TopicName) });
        }
        private int chapter = 1;
        public int Chapter
        {
            get => chapter;
            set => SetAndNotify(ref chapter, value, new[] { nameof(TopicName) });
        }
        private ValidationToken validation;
        public ValidationToken Validation
        {
            get => validation;
            set => SetAndNotify(ref validation, value);
        }

        [JsonIgnore]
        public string TopicName => DataSource?.GetTopicName(Form, Chapter) ?? "Invalid topic";

        private ObservableCollection<Question> questions = new ObservableCollection<Question>();
        public ObservableCollection<Question> Questions
        {
            get => questions;
            set
            {
                if (questions != null)
                    questions.CollectionChanged -= Questions_CollectionChanged;
                SetAndNotify(ref questions, value);
                if (questions != null)
                    questions.CollectionChanged += Questions_CollectionChanged;
            }
        }

        public QuestionSet()
        {
            questions.CollectionChanged += Questions_CollectionChanged;
        }

        private void Questions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < questions.Count; i++)
            {
                questions[i].QuestionIndex = i + 1;
            }
        }

        private ObservableCollection<Media> media = new ObservableCollection<Media>();
        public ObservableCollection<Media> Media
        {
            get => media;
            set => SetAndNotify(ref media, value);
        }

        public string GetHash()
        {
            return Helper.SHA1Hash(JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                ContractResolver = new DynamicContractResolver(new[] {
                    nameof(Validation)
                })
            }));
        }
    }
}
