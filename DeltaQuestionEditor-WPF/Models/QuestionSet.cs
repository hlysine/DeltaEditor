using DeltaQuestionEditor_WPF.DataSources;
using DeltaQuestionEditor_WPF.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [JsonIgnore]
        public string TopicName
        {
            get => DataSource?.GetTopicName(Form, Chapter) ?? "Invalid topic";
        }

        private ObservableCollection<Question> questions = new ObservableCollection<Question>();
        public ObservableCollection<Question> Questions
        {
            get => questions;
            set => SetAndNotify(ref questions, value);
        }


        private ObservableCollection<Media> media = new ObservableCollection<Media>();
        public ObservableCollection<Media> Media
        {
            get => media;
            set => SetAndNotify(ref media, value);
        }
    }
}
