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
    public class Question : NotifyPropertyChanged, ICloneable
    {
        private string id;
        public string Id
        {
            get => id;
            set => SetAndNotify(ref id, value);
        }

        private string text;
        public string Text
        {
            get => text;
            set => SetAndNotify(ref text, value);
        }


        private int selectedAnswerIndex;
        [JsonIgnore]
        public int SelectedAnswerIndex
        {
            get => selectedAnswerIndex;
            set => SetAndNotify(ref selectedAnswerIndex, value, new[] { nameof(SelectedAnswer) });
        }

        [JsonIgnore]
        public string SelectedAnswer
        {
            get
            {
                if (Answers == null) return null;
                if (SelectedAnswerIndex < 0 || SelectedAnswerIndex >= Answers.Count) return null;
                else return Answers[SelectedAnswerIndex];
            }
            set
            {
                if (Answers == null) return;
                if (SelectedAnswerIndex < 0 || SelectedAnswerIndex >= Answers.Count) return;
                Answers[SelectedAnswerIndex] = value;
            }
        }


        private ObservableCollection<string> answers = new ObservableCollection<string>();
        public ObservableCollection<string> Answers
        {
            get => answers;
            set => SetAndNotify(ref answers, value, new[] { nameof(SelectedAnswer) });
        }


        private ushort difficulty = 1;
        public ushort Difficulty
        {
            get => difficulty;
            set => SetAndNotify(ref difficulty, value);
        }


        private ObservableCollection<string> skills = new ObservableCollection<string>();
        public ObservableCollection<string> Skills
        {
            get => skills;
            set => SetAndNotify(ref skills, value);
        }

        public object Clone()
        {
            return new Question()
            {
                Id = Helper.NewGuid(),
                Text = Text,
                Answers = new ObservableCollection<string>(Answers ?? new ObservableCollection<string>()),
                Difficulty = Difficulty,
                Skills = new ObservableCollection<string>(Skills ?? new ObservableCollection<string>()),
                SelectedAnswerIndex = SelectedAnswerIndex,
            };
        }
    }
}
