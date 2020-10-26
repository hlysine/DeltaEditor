using DeltaQuestionEditor_WPF.Helpers;
using DeltaQuestionEditor_WPF.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.DataSources
{
    using static Helper;
    public class LocalFileDataSource : NotifyPropertyChanged, IDataSource, IDisposable
    {
        private QuestionSet questionSet = null;
        public QuestionSet QuestionSet
        {
            get => questionSet;
            private set => SetAndNotify(ref questionSet, value);
        }


        private string filePath = null;
        public string FilePath
        {
            get => filePath;
            private set => SetAndNotify(ref filePath, value);
        }


        private DateTime lastSaved;
        public DateTime LastSaved
        {
            get => lastSaved;
            set => SetAndNotify(ref lastSaved, value);
        }

        private string tempPath;

        public LocalFileDataSource()
        {
            tempPath = Path.Combine(Path.GetTempPath(), "DeltaQuestionEditor", Guid.NewGuid().ToString());
            EnsurePathExist(tempPath);
            EnsurePathExist(Path.Combine(tempPath, "Media"));
        }

        public void Dispose()
        {
            Directory.Delete(tempPath, true);
        }

        private static readonly Dictionary<int, Dictionary<int, string>> topicNames = new Dictionary<int, Dictionary<int, string>>
        {
            [1] = new Dictionary<int, string>
            {
                [0]= "Basic Mathematics",
                [1]= "Directed Numbers and the Number Line",
                [2]= "Introduction to Algebra",
                [3]= "Algebraic Equations in One Unknown",
                [4]= "Percentages (I)",
                [5]= "Estimation in Numbers and Measurement",
                [6]= "Introduction to Geometry",
                [7]= "Symmetry and Transformation",
                [8]= "Areas and Volumes (I)",
                [9]= "Congruence and Similarity",
                [10]= "Introduction to Coordinates",
                [11]= "Angles related to Lines",
                [12]= "Manipulation of Simple Polynomials",
                [13]= "Introduction to Various Stages of Statistics",
                [14]= "Simple Statistical Diagrams and Graphs (I)",
            },
            [2] = new Dictionary<int, string>
            {
                [1]= "Rate and Ratio",
                [2]= "Identities and Factorization",
                [3]= "Algebraic Fractions and Formulas",
                [4]= "More about Factorization of Polynomials",
                [5]= "Approximation and Errors",
                [6]= "Angles related to Rectilinear Figures",
                [7]= "Simple Statistical Diagrams and Graphs (II)",
                [8]= "Linear Equations in Two Unknowns",
                [9]= "Laws of Integral Indices",
                [10]= "Introduction to Deductive Geometry",
                [11]= "Rational and Irrational Numbers",
                [12]= "Pythagoras' Theorem",
                [13]= "Areas and Volumes (II)",
                [14]= "Trigonometric Ratios",
            },
            [3] = new Dictionary<int, string>
            {
                [1]= "Linear Inequalities in One Unknown",
                [2]= "Percentages (II)",
                [3]= "Special Lines and Centres in a Triangle",
                [4]= "Quadrilaterals",
                [5]= "More about 3D Figures",
                [6]= "Measures of Central Tendency",
                [7]= "Areas and Volumes (III)",
                [8]= "Coordinate Geometry of Straight Lines",
                [9]= "Trigonometric Relations",
                [10]= "Applications of Trigonometry",
                [11]= "Introduction to Probability",
            },
            [4] = new Dictionary<int, string>
            {
                [1]= "Number System",
                [2]= "Equation of Straight Lines",
                [3]= "Quadratic Equations in One unknown ",
                [4]= "Basic Knowledge of Functions",
                [5]= "Quadratic Functions",
                [6]= "More about Polynomials",
                [7]= "Exponential Functions",
                [8]= "Logarithmic Functions",
                [9]= "Rational Functions",
                [10]= "Basic Properties of Circles",
                [11]= "More about Basic Properties of Circles",
                [12]= "Basic Trigonometry",
            },
            [5] = new Dictionary<int, string>
            {
                [1]= "More about Equations",
                [2]= "Inequalities in One Unknown",
                [3]= "More about Graphs of Functions",
                [4]= "Permutation and Combination",
                [5]= "More about Probability",
                [6]= "Variations  ",
                [7]= "Equations of Circles",
                [8]= "Locus",
                [9]= "Solving Triangles",
                [10]= "Applications in Trigonometry",
                [11]= "Measures of Dispersion",
                [12]= "More about Dispersion",
            },
            [6] = new Dictionary<int, string>
            {
                [1]= "Arithmetic Sequences",
                [2]= "Geometric Sequences",
                [3]= "Linear Inequalities in Two Unknowns and Linear Programming",
                [4]= "Uses and Abuses of Statistics",
            }
        };

        /// <summary>
        /// Get the topic name by form and chapter number
        /// </summary>
        public string GetTopicName(int form, int chapter)
        {
            if (topicNames.TryGetValue(form, out var dict))
            {
                if (dict.TryGetValue(chapter, out string name))
                {
                    return name;
                }
            }
            return null;
        }

        /// <summary>
        /// Create a new, empty question set.
        /// </summary>
        public void CreateQuestionSet()
        {
            QuestionSet = new QuestionSet();
            QuestionSet.DataSource = this;
            FilePath = null;
        }

        /// <summary>
        /// Load a question set (.qdb) file.
        /// </summary>
        /// <param name="path">Path to qdb file.</param>
        public async Task LoadQuestionSet(string path)
        {
            FilePath = path;
            await Task.Run(() =>
            {
                ClearDirectory(tempPath);
                ZipFile.ExtractToDirectory(path, tempPath);
                QuestionSet = JsonConvert.DeserializeObject<QuestionSet>(File.ReadAllText(Path.Combine(tempPath, "questionset.json")));
                QuestionSet.DataSource = this;
            });
            LastSaved = DateTime.Now;
        }

        /// <summary>
        /// Save the question set to a qdb file.
        /// </summary>
        /// <param name="path">Save location of the qdb file. Can be omitted if the question set is loaded through a path.</param>
        /// <exception cref="ArgumentNullException">Thrown if path is null and the question set is never saved.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <code>QuestionSet</code> is null.</exception>
        public async Task SaveQuestionSet(string path = null)
        {
            if (QuestionSet == null) throw new InvalidOperationException("QuestionSet is null");
            if (path != null) FilePath = path;
            else if (FilePath == null) throw new ArgumentNullException("path is null and there is no saved file path");
            await Task.Run(() =>
            {
                File.WriteAllText(Path.Combine(tempPath, "questionset.json"), JsonConvert.SerializeObject(QuestionSet));
                EnsurePathExist(Path.Combine(tempPath, "Media"));
                // TODO: overwrite?
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                ZipFile.CreateFromDirectory(tempPath, FilePath);
            });
            LastSaved = DateTime.Now;
        }

        /// <summary>
        /// Add a media file to the question set
        /// </summary>
        /// <param name="path">Path to the media file</param>
        /// <exception cref="InvalidOperationException">Thrown if <code>QuestionSet</code> is null.</exception>
        public async Task AddMedia(string path)
        {
            if (QuestionSet == null) throw new InvalidOperationException("QuestionSet is null");

            Media media = new Media();
            await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (SHA1Managed sha1 = new SHA1Managed())
                    {
                        media.Id = BitConverter.ToString(sha1.ComputeHash(bs)).Replace("-", "");
                        media.FileName = Path.Combine("Media", $"{media.Id}.{Path.GetExtension(path)}");
                    }
                }
                File.Copy(path, Path.Combine(tempPath, media.FileName));
            });
            QuestionSet.Media.Add(media);
        }

        /// <summary>
        /// Delete a media file from the question set
        /// </summary>
        /// <param name="id">Id of the media file</param>
        /// <exception cref="ArgumentException">Thrown if the id does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <code>QuestionSet</code> is null.</exception>
        public async Task DeleteMedia(string id)
        {
            if (QuestionSet == null) throw new InvalidOperationException("QuestionSet is null");
            Media media = QuestionSet.Media.FirstOrDefault(x => x.Id == id);
            if (media == null) throw new ArgumentException("Media id not found");
            await Task.Run(() =>
            {
                File.Delete(Path.Combine(tempPath, media.FileName));
            });
            QuestionSet.Media.Remove(media);
        }
    }
}
