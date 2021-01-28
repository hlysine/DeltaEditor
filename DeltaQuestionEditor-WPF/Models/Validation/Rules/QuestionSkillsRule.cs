using DeltaQuestionEditor_WPF.Consts;
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
                                problems.Add(new ValidationProblem(ProblemSeverity.Warning, string.Format(ValidationProblems.QUESTION_SKILL_NO_CURRENT_TOPIC, i + 1, questionSet.Form, questionSet.Chapter, questionSet.TopicName), question));
                            }
                        }
                    }
                    if (invalid)
                    {
                        problems.Add(new ValidationProblem(ProblemSeverity.Error, string.Format(ValidationProblems.QUESTION_SKILL_INVALID, i + 1), question));
                    }
                }
            }
            return problems;
        }
    }
}
