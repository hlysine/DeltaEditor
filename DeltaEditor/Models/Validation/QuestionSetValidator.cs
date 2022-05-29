using DeltaEditor.Helpers;
using DeltaEditor.Models.Validation.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeltaEditor.Models.Validation
{
    public class QuestionSetValidator : NotifyPropertyChanged
    {
        private readonly List<QuestionSetValidationRule> validationRules;

        public QuestionSetValidator(QuestionSet questionSet)
        {
            QuestionSet = questionSet;
            Problems.CollectionChanged += Problems_CollectionChanged;
            validationRules = typeof(QuestionSetValidationRule).Assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(QuestionSetValidationRule)))
                .Select(x => (QuestionSetValidationRule)Activator.CreateInstance(x)).ToList();
        }

        private void Problems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyChanged(new[] { nameof(ErrorsCount), nameof(WarningsCount) });
        }

        private QuestionSet questionSet;
        public QuestionSet QuestionSet
        {
            get => questionSet;
            set => SetAndNotify(ref questionSet, value, new[] { nameof(IsValid) });
        }

        private int validationProgress;
        public int ValidationProgress
        {
            get => validationProgress;
            set => SetAndNotify(ref validationProgress, value);
        }

        public bool IsValid => questionSet != null && questionSet.Validation?.Hash == questionSet.GetHash();

        public int ErrorsCount => Problems.Count(x => x.Severity == ProblemSeverity.Error);

        public int WarningsCount => Problems.Count(x => x.Severity == ProblemSeverity.Warning);

        private ObservableCollection<ValidationProblem> problems = new ObservableCollection<ValidationProblem>();
        public ObservableCollection<ValidationProblem> Problems
        {
            get => problems;
            set => SetAndNotify(ref problems, value, new[] { nameof(ErrorsCount), nameof(WarningsCount) });
        }

        public void RefreshValidation()
        {
            NotifyChanged(nameof(IsValid));
        }

        public List<ValidationProblem> Validate()
        {
            List<ValidationProblem> problems = new List<ValidationProblem>();
            ValidationProgress = 0;
            for (int i = 0; i < validationRules.Count; i++)
            {
                QuestionSetValidationRule rule = validationRules[i];
                problems.AddRange(rule.Validate(QuestionSet));
                ValidationProgress = (i + 1) * 100 / validationRules.Count;
            }
            Problems.Clear();
            problems.ForEach(x => Problems.Add(x));
            if (!Problems.Any(x => x.Severity == ProblemSeverity.Error))
            {
                if (QuestionSet.Validation == null)
                    QuestionSet.Validation = new ValidationToken();
                QuestionSet.Validation.Timestamp = DateTime.Now;
                QuestionSet.Validation.Hash = QuestionSet.GetHash();
            }
            NotifyChanged(nameof(IsValid));
            return problems;
        }
    }
}
