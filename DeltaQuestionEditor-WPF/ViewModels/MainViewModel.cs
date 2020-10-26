using DeltaQuestionEditor_WPF.DataSources;
using DeltaQuestionEditor_WPF.Helpers;
using DeltaQuestionEditor_WPF.Models;
using Microsoft.Win32;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeltaQuestionEditor_WPF.ViewModels
{
    class MainViewModel : NotifyPropertyChanged
    {
        #region App Update

        int updateProgress = 0;
        public int UpdateProgress
        {
            get => updateProgress;
            set => SetAndNotify(ref updateProgress, value, new[] { nameof(UpdateString) });
        }

        string updateStatus = "";
        public string UpdateStatus
        {
            get => updateStatus;
            set => SetAndNotify(ref updateStatus, value, new[] { nameof(UpdateString) });
        }

        public string UpdateString
        {
            get
            {
                if (UpdateStatus.IsNullOrWhiteSpace())
                {
                    return "";
                }
                else
                {
                    if (UpdateFinished)
                    {
                        return $" - {UpdateStatus}";
                    }
                    else
                    {
                        return $" - {UpdateStatus} {UpdateProgress}%";
                    }
                }
            }
        }

        SemaphoreSlim updateFinished = new SemaphoreSlim(0, 1);

        public async Task AwaitUpdateFinish()
        {
            await updateFinished.WaitAsync();
        }

        public string AppVersion
        {
            get => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public bool UpdateFinished
        {
            get => updateFinished.CurrentCount > 0;
        }

        #endregion

        public Action<object> AppInitialize { get; private set; }
        public Action<object> AppClosing { get; private set; }


        private LocalFileDataSource dataSource;
        public LocalFileDataSource DataSource
        {
            get => dataSource;
            set => SetAndNotify(ref dataSource, value);
        }


        private string loadingState;
        public string LoadingState
        {
            get => loadingState;
            set => SetAndNotify(ref loadingState, value);
        }


        private Question selectedQuestion;
        public Question SelectedQuestion
        {
            get => selectedQuestion;
            set => SetAndNotify(ref selectedQuestion, value);
        }

        ICommand newFileCommand;
        public ICommand NewFileCommand
        {
            get
            {
                return newFileCommand ??= new RelayCommand(
                    // execute
                    () =>
                    {
                        if (DataSource.QuestionSet == null)
                        {
                            DataSource.CreateQuestionSet();
                        }
                        else
                        {
                            // TODO
                        }
                    },
                    // can execute
                    () =>
                    {
                        return true;
                    }
                );
            }
        }

        ICommand openFileCommand;
        public ICommand OpenFileCommand
        {
            get
            {
                return openFileCommand ??= new RelayCommand(
                    // execute
                    async () =>
                    {
                        if (DataSource.QuestionSet == null)
                        {
                            OpenFileDialog dialog = new OpenFileDialog();
                            dialog.Filter = "Question Set (.qdb)|*.qdb";
                            dialog.Title = "Choose a question set file";
                            dialog.CheckFileExists = true;
                            dialog.CheckPathExists = true;
                            if (dialog.ShowDialog() == true)
                            {
                                LoadingState = "Opening";
                                await DataSource.LoadQuestionSet(dialog.FileName);
                                LoadingState = null;
                            }
                        }
                        else
                        {
                            // TODO
                        }
                    },
                    // can execute
                    () =>
                    {
                        return true;
                    }
                );
            }
        }

        ICommand saveFileCommand;
        public ICommand SaveFileCommand
        {
            get
            {
                return saveFileCommand ??= new RelayCommand(
                    // execute
                    async () =>
                    {
                        if (DataSource.FilePath == null)
                        {
                            SaveFileDialog dialog = new SaveFileDialog();
                            dialog.Filter = "Question Set (.qdb)|*.qdb";
                            dialog.Title = "Choose a save location";
                            if (dialog.ShowDialog() == true)
                            {
                                LoadingState = "Saving";
                                await DataSource.SaveQuestionSet(dialog.FileName);
                                LoadingState = null;
                            }
                        }
                        else
                        {
                            LoadingState = "Saving";
                            await DataSource.SaveQuestionSet();
                            LoadingState = null;
                        }
                    },
                    // can execute
                    () =>
                    {
                        return DataSource.QuestionSet != null;
                    }
                );
            }
        }

        ICommand saveAsCommand;
        public ICommand SaveAsCommand
        {
            get
            {
                return saveAsCommand ??= new RelayCommand(
                    // execute
                    async () =>
                    {
                        SaveFileDialog dialog = new SaveFileDialog();
                        dialog.Filter = "Question Set (.qdb)|*.qdb";
                        dialog.Title = "Choose a save location";
                        if (dialog.ShowDialog() == true)
                        {
                            LoadingState = "Saving";
                            await DataSource.SaveQuestionSet(dialog.FileName);
                            LoadingState = null;
                        }
                    },
                    // can execute
                    () =>
                    {
                        return DataSource.QuestionSet != null;
                    }
                );
            }
        }

        ICommand addQuestionCommand;
        public ICommand AddQuestionCommand
        {
            get
            {
                return addQuestionCommand ??= new RelayCommand(
                    // execute
                    () =>
                    {
                        Question question = new Question();
                        for (int i = 0; i < 4; i++)
                            question.Answers.Add(null);
                        question.Id = Guid.NewGuid().ToString("N");
                        DataSource.QuestionSet.Questions.Add(question);
                    },
                    // can execute
                    () =>
                    {
                        return DataSource?.QuestionSet?.Questions != null;
                    }
                );
            }
        }

        public MainViewModel()
        {
            AppInitialize = async _ =>
            {
                try
                {
                    // TODO: github link
                    using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/Henry-YSLin/DeltaQuestionEditor-WPF"))
                    {
                        var updateInfo = await mgr.CheckForUpdate(false, (progress) =>
                        {
                            UpdateProgress = progress;
                            UpdateStatus = "Checking";
                        });
                        if (updateInfo.ReleasesToApply.Any())
                        {
                            var result = await mgr.UpdateApp((progress) =>
                            {
                                UpdateProgress = progress;
                                UpdateStatus = "Updating";
                            });
                            await Task.Delay(500);
                            UpdateStatus = "Restart app to update";
                        }
                        else
                        {
                            UpdateStatus = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, ex.Source);
                }
                updateFinished.Release();
                NotifyChanged(nameof(UpdateFinished));
                DataSource = new LocalFileDataSource();
            };
            AppClosing = _ =>
            {
                DataSource.Dispose();
            };
        }
    }
}
