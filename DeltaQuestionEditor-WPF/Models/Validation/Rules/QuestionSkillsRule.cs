using DeltaQuestionEditor_WPF.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.Models.Validation.Rules
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
    public class QuestionSkillsRule : QuestionSetValidationRule
    {
        public override List<ValidationProblem> Validate(QuestionSet questionSet)
        {
            List<ValidationProblem> problems = new List<ValidationProblem>();
            for (int i = 0; i < questionSet.Questions.Count; i++)
            {
                Question question = questionSet.Questions[i];
                if (question.Skills != null && question.Skills.Count > 0)
                {
                    bool invalid = false;
                    foreach (string skill in question.Skills)
                    {
                        string skills = Regex.Replace(skill, @"\s", "");
                        if (!Regex.IsMatch(skills, @"^(?:\d+\.){2,4}\d+$"))
                        {
                            invalid = true;
                        }
                        else
                        {
                            string[] sk = skills.Split(',');
                            bool containsTopic = false;
                            foreach (string s in sk)
                            {
                                if (s.StartsWith($"{questionSet.Form}.{questionSet.Chapter}"))
                                    containsTopic = true;
                            }
                            if (!containsTopic)
                            {
                                problems.Add(new ValidationProblem(ProblemSeverity.Warning, $"The skill code of question {i + 1} does not contain skills from the current topic ({questionSet.Form}.{questionSet.Chapter} {questionSet.TopicName}). Please double-check for errors.", question));
                            }
                        }
                    }
                    if (invalid)
                    {
                        problems.Add(new ValidationProblem(ProblemSeverity.Error, $"The skill code of question {i + 1} is invalid.", question));
                    }
                }
            }
            return problems;
        }
    }
}
