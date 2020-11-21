using DeltaQuestionEditor_WPF.DataSources;
using DeltaQuestionEditor_WPF.Helpers;
using DeltaQuestionEditor_WPF.Models;
using DeltaQuestionEditor_WPF.Views;
using ExcelDataReader;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MoreLinq;
using Squirrel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeltaQuestionEditor_WPF.ViewModels
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
    class MainViewModel : NotifyPropertyChanged, IDropTarget
    {
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


        private bool showUpdatePanel = false;
        public bool ShowUpdatePanel
        {
            get => showUpdatePanel;
            set => SetAndNotify(ref showUpdatePanel, value);
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

        DefaultDropHandler dropHandler = new DefaultDropHandler();

        public void DragOver(IDropInfo dropInfo)
        {
            DataObject dataObj = dropInfo.Data as DataObject;
            if (dataObj != null && dataObj.ContainsFileDropList())
            {
                var list = dataObj.GetFileDropList();
                if (AddMediaCommand.CanExecute(list))
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                }
            }
            else
            {
                dropHandler.DragOver(dropInfo);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            DataObject dataObj = dropInfo.Data as DataObject;
            if (dataObj != null && dataObj.ContainsFileDropList())
            {
                var list = dataObj.GetFileDropList().Cast<string>();
                if (AddMediaCommand.CanExecute(list))
                {
                    AddMediaCommand.Execute(list);
                }
            }
            else
            {
                dropHandler.Drop(dropInfo);
            }
        }

        public string AppVersion
        {
            get => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public bool UpdateFinished
        {
            get => updateFinished.CurrentCount > 0;
        }

        public Action<object> AppInitialize { get; private set; }
        public Action<object> AppClosed { get; private set; }


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


        private bool questionListEditMode;
        public bool QuestionListEditMode
        {
            get => questionListEditMode;
            set => SetAndNotify(ref questionListEditMode, value);
        }


        private Media selectedMedia;
        public Media SelectedMedia
        {
            get => selectedMedia;
            set => SetAndNotify(ref selectedMedia, value);
        }


        private bool mediaListEditMode;
        public bool MediaListEditMode
        {
            get => mediaListEditMode;
            set => SetAndNotify(ref mediaListEditMode, value);
        }


        private SnackbarMessageQueue mainMessageQueue = new SnackbarMessageQueue();
        public SnackbarMessageQueue MainMessageQueue
        {
            get => mainMessageQueue;
            set => SetAndNotify(ref mainMessageQueue, value);
        }


        private bool exitConfirmed;
        public bool ExitConfirmed
        {
            get => exitConfirmed;
            set => SetAndNotify(ref exitConfirmed, value);
        }


        private bool confirmExitDialog = false;
        public bool ConfirmExitDialog
        {
            get => confirmExitDialog;
            set => SetAndNotify(ref confirmExitDialog, value);
        }


        private bool topicSelectorDialog;
        public bool TopicSelectorDialog
        {
            get => topicSelectorDialog;
            set => SetAndNotify(ref topicSelectorDialog, value);
        }

        ICommand closeWindowCommand;
        public ICommand CloseWindowCommand
        {
            get
            {
                return closeWindowCommand ??= new RelayCommand(
                    // execute
                    async (param) =>
                    {
                        (CancelEventArgs, MainWindow) args = ((CancelEventArgs, MainWindow))param;
                        if (args.Item1 == null || args.Item2 == null) return;
                        if (DataSource?.QuestionSet == null)
                            ExitConfirmed = true;
                        if (ExitConfirmed)
                        {
                            if (!UpdateFinished)
                            {
                                args.Item1.Cancel = true;
                                // Still updating
                                ShowUpdatePanel = true;
                                await updateFinished.WaitAsync();
                                updateFinished.Release();
                                args.Item2.Close();
                            }
                            else
                            {
                                args.Item1.Cancel = false;
                            }
                        }
                        else
                        {
                            args.Item1.Cancel = true;
                            ConfirmExitDialog = true;
                        }
                    },
                    // can execute
                    (param) =>
                    {
                        return true;
                    }
                );
            }
        }


        ICommand confirmCloseCommand;
        public ICommand ConfirmCloseCommand
        {
            get
            {
                return confirmCloseCommand ??= new RelayCommand(
                    // execute
                    (param) =>
                    {
                        ConfirmExitDialog = false;
                        ExitConfirmed = true;
                        Window window = param as Window;
                        if (window == null) return;
                        window.Close();
                    },
                    // can execute
                    (param) =>
                    {
                        return !ExitConfirmed && param as Window != null;
                    }
                );
            }
        }


        ICommand cancelCloseCommand;
        public ICommand CancelCloseCommand
        {
            get
            {
                return cancelCloseCommand ??= new RelayCommand(
                    // execute
                    (param) =>
                    {
                        ConfirmExitDialog = false;
                        ExitConfirmed = false;
                    },
                    // can execute
                    (param) =>
                    {
                        return !ExitConfirmed;
                    }
                );
            }
        }


        ICommand importFromExcelCommand;
        public ICommand ImportFromExcelCommand
        {
            get
            {
                return importFromExcelCommand ??= new RelayCommand(
                    // execute
                    async (param) =>
                    {
                        async Task<string> importExcel(string path)
                        {
                            DataSource.CreateQuestionSet();
                            TopicSelectorDialog = true;
                            await WaitUntil(() => !TopicSelectorDialog);
                            LoadingState = "Reading";
                            ExcelFileDataSource importer = new ExcelFileDataSource();
                            if (!await importer.ReadFile(path))
                            {
                                return $"Failed to open {Path.GetFileName(path)}. The file is probably in use.";
                            }
                            LoadingState = "Analyzing";
                            if (!await importer.AnalyzeFile())
                            {
                                return $"Analysis of {Path.GetFileName(path)} failed: {importer.LastFailMessage}.";
                            }
                            LoadingState = "Importing questions";
                            await importer.ImportQuestions(DataSource);
                            LoadingState = "Importing media files";
                            await importer.ImportMedia(DataSource);
                            LoadingState = null;
                            EnsurePathExist(AppDataPath("Imports"));
                            string log = AppDataPath(Path.Combine("Imports", $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss} {Path.GetFileName(path)}.txt"));
                            File.WriteAllText(log, importer.GetImportReport());
                            Process.Start(log);
                            return $"Successfully imported {Path.GetFileName(path)}";
                        }

                        List<string> paths = (param as IEnumerable<string>)?.ToList();
                        if (paths == null) paths = new List<string>();
                        string paramPath = param as string;
                        if (paramPath != null) paths.Add(paramPath);
                        if (paths.Count == 0) paths.Add(null);
                        foreach (string path in paths)
                        {
                            if (DataSource.QuestionSet == null)
                            {
                                if (path == null)
                                {
                                    OpenFileDialog dialog = new OpenFileDialog();
                                    dialog.Filter = "Excel workbook|*.xls;*.xlsx;*.xlsb";
                                    dialog.Title = "Import from Excel - Choose an Excel workbook file";
                                    dialog.CheckFileExists = true;
                                    dialog.CheckPathExists = true;
                                    if (dialog.ShowDialog() == true)
                                    {
                                        MainMessageQueue.Enqueue(await importExcel(dialog.FileName));
                                    }
                                }
                                else
                                {
                                    MainMessageQueue.Enqueue(await importExcel(path));
                                }
                            }
                            else
                            {
                                if (path == null)
                                {
                                    OpenFileDialog dialog = new OpenFileDialog();
                                    dialog.Filter = "Excel workbook|*.xls;*.xlsx;*.xlsb";
                                    dialog.Title = "Import from Excel - Choose an Excel workbook file";
                                    dialog.CheckFileExists = true;
                                    dialog.CheckPathExists = true;
                                    if (dialog.ShowDialog() == true)
                                    {
                                        Process.Start(Assembly.GetEntryAssembly().Location, $"-i \"{dialog.FileName}\"");
                                    }
                                }
                                else
                                {
                                    Process.Start(Assembly.GetEntryAssembly().Location, $"-i \"{path}\"");
                                }
                            }
                        }
                    },
                    // can execute
                    (param) =>
                    {
                        return true;
                    }
                );
            }
        }

        ICommand newFileCommand;
        public ICommand NewFileCommand
        {
            get
            {
                return newFileCommand ??= new RelayCommand(
                    // execute
                    _ =>
                    {
                        if (DataSource.QuestionSet == null)
                        {
                            DataSource.CreateQuestionSet();
                            TopicSelectorDialog = true;
                        }
                        else
                        {
                            Process.Start(Assembly.GetEntryAssembly().Location);
                        }
                    },
                    // can execute
                    _ =>
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
                    async (param) =>
                    {
                        async Task openFile(string path)
                        {
                            LoadingState = "Opening";
                            if (!await DataSource.LoadQuestionSet(path))
                            {
                                MainMessageQueue.Enqueue($"Failed to open {Path.GetFileName(path)}. The file is probably in use.");
                            }
                            LoadingState = null;
                        }

                        List<string> paths = (param as IEnumerable<string>)?.ToList();
                        if (paths == null) paths = new List<string>();
                        string paramPath = param as string;
                        if (paramPath != null) paths.Add(paramPath);
                        if (paths.Count == 0) paths.Add(null);
                        foreach (string path in paths)
                        {
                            if (DataSource.QuestionSet == null)
                            {
                                if (path == null)
                                {
                                    OpenFileDialog dialog = new OpenFileDialog();
                                    dialog.Filter = "Question Set|*.qdb";
                                    dialog.Title = "Open - Choose a question set file";
                                    dialog.CheckFileExists = true;
                                    dialog.CheckPathExists = true;
                                    if (dialog.ShowDialog() == true)
                                    {
                                        await openFile(dialog.FileName);
                                    }
                                }
                                else
                                {
                                    await openFile(path);
                                }
                            }
                            else
                            {
                                if (path == null)
                                {
                                    OpenFileDialog dialog = new OpenFileDialog();
                                    dialog.Filter = "Question Set|*.qdb";
                                    dialog.Title = "Open - Choose a question set file";
                                    dialog.CheckFileExists = true;
                                    dialog.CheckPathExists = true;
                                    if (dialog.ShowDialog() == true)
                                    {
                                        Process.Start(Assembly.GetEntryAssembly().Location, $"\"{dialog.FileName}\"");
                                    }
                                }
                                else
                                {
                                    Process.Start(Assembly.GetEntryAssembly().Location, $"\"{path}\"");
                                }
                            }
                        }
                    },
                    // can execute
                    _ =>
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
                    async _ =>
                    {
                        if (DataSource.FilePath == null)
                        {
                            SaveFileDialog dialog = new SaveFileDialog();
                            dialog.Filter = "Question Set|*.qdb";
                            dialog.Title = "Save - Choose a save location";
                            if (dialog.ShowDialog() == true)
                            {
                                LoadingState = "Saving";
                                MainMessageQueue.Clear();
                                await DataSource.SaveQuestionSet(dialog.FileName);
                                MainMessageQueue.Enqueue("New file saved");
                                LoadingState = null;
                            }
                        }
                        else
                        {
                            LoadingState = "Saving";
                            MainMessageQueue.Clear();
                            await DataSource.SaveQuestionSet();
                            MainMessageQueue.Enqueue("Changes saved");
                            LoadingState = null;
                        }
                    },
                    // can execute
                    _ =>
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
                    async _ =>
                    {
                        SaveFileDialog dialog = new SaveFileDialog();
                        dialog.Filter = "Question Set|*.qdb";
                        dialog.Title = "Save As - Choose a save location";
                        if (dialog.ShowDialog() == true)
                        {
                            LoadingState = "Saving";
                            MainMessageQueue.Clear();
                            await DataSource.SaveQuestionSet(dialog.FileName);
                            MainMessageQueue.Enqueue("New file saved and loaded");
                            LoadingState = null;
                        }
                    },
                    // can execute
                    _ =>
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
                    _ =>
                    {
                        Question question = new Question();
                        for (int i = 0; i < 4; i++)
                            question.Answers.Add(null);
                        question.Id = Helper.NewGuid();
                        DataSource.QuestionSet.Questions.Add(question);
                        MainMessageQueue.Clear();
                        mainMessageQueue.Enqueue("New question added");
                    },
                    // can execute
                    _ =>
                    {
                        return DataSource?.QuestionSet?.Questions != null;
                    }
                );
            }
        }


        ICommand deleteQuestionCommand;
        public ICommand DeleteQuestionCommand
        {
            get
            {
                return deleteQuestionCommand ??= new RelayCommand(
                    // execute
                    (param) =>
                    {
                        Question question = param as Question;
                        if (question == null) throw new ArgumentNullException("Question to be deleted is null");
                        int index = DataSource.QuestionSet.Questions.IndexOf(question);
                        DataSource.QuestionSet.Questions.Remove(question);
                        MainMessageQueue.Clear();
                        MainMessageQueue.Enqueue(
                            $"Question {index + 1} deleted",
                            "UNDO",
                            (param) =>
                            {
                                DataSource?.QuestionSet?.Questions?.Insert(param.index, param.question);
                            },
                            (index, question),
                            false,
                            true,
                            TimeSpan.FromSeconds(5));
                    },
                    // can execute
                    (param) =>
                    {
                        return (DataSource?.QuestionSet?.Questions?.Contains(param)).GetValueOrDefault();
                    }
                );
            }
        }


        ICommand copyQuestionCommand;
        public ICommand CopyQuestionCommand
        {
            get
            {
                return copyQuestionCommand ??= new RelayCommand(
                    // execute
                    (param) =>
                    {
                        Question question = param as Question;
                        if (question == null) throw new ArgumentNullException("Question to be copied is null");
                        int index = DataSource.QuestionSet.Questions.IndexOf(question);
                        Question clone = (Question)question.Clone();
                        if (index >= 0)
                            DataSource.QuestionSet.Questions.Insert(index + 1, clone);
                        else
                            DataSource.QuestionSet.Questions.Add(clone);
                        MainMessageQueue.Clear();
                        MainMessageQueue.Enqueue(
                            $"Question {index + 1} copied",
                            "UNDO",
                            (param) =>
                            {
                                if (DataSource?.QuestionSet?.Questions == null) return;
                                if (DataSource.QuestionSet.Questions.Contains(clone))
                                    DataSource.QuestionSet.Questions.Remove(clone);
                            },
                            clone,
                            false,
                            true,
                            TimeSpan.FromSeconds(5));
                    },
                    // can execute
                    (param) =>
                    {
                        return param != null && DataSource?.QuestionSet?.Questions != null;
                    }
                );
            }
        }


        ICommand addMediaCommand;
        public ICommand AddMediaCommand
        {
            get
            {
                return addMediaCommand ??= new RelayCommand(
                    // execute
                    async (param) =>
                    {
                        string paramPath = param as string;
                        if (paramPath == null)
                        {
                            IEnumerable<string> paramPaths = param as IEnumerable<string>;
                            if (paramPaths == null)
                            {
                                OpenFileDialog dialog = new OpenFileDialog();
                                dialog.Filter = "All files|*.*";
                                dialog.Title = "Add Media - Choose media file(s)";
                                dialog.CheckFileExists = true;
                                dialog.Multiselect = true;
                                dialog.CheckPathExists = true;
                                if (dialog.ShowDialog() == true)
                                {
                                    for (int i = 0; i < dialog.FileNames.Length; i++)
                                    {
                                        LoadingState = $"Inserting media {i + 1}/{dialog.FileNames.Length}";
                                        await DataSource.AddMedia(dialog.FileNames[i]);
                                    }
                                    MainMessageQueue.Clear();
                                    MainMessageQueue.Enqueue($"{dialog.FileNames.Length} media files loaded");
                                    LoadingState = null;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < paramPaths.Count(); i++)
                                {
                                    LoadingState = $"Inserting media {i + 1}/{paramPaths.Count()}";
                                    await DataSource.AddMedia(paramPaths.ElementAt(i));
                                }
                                MainMessageQueue.Clear();
                                MainMessageQueue.Enqueue($"{paramPaths.Count()} media files loaded");
                                LoadingState = null;
                            }
                        }
                        else
                        {
                            LoadingState = "Inserting media 1/1";
                            await DataSource.AddMedia(paramPath);
                            MainMessageQueue.Clear();
                            MainMessageQueue.Enqueue($"1 media file loaded");
                            LoadingState = null;
                        }
                    },
                    // can execute
                    (param) =>
                    {
                        return DataSource?.QuestionSet?.Media != null;
                    }
                );
            }
        }


        ICommand copyMediaCodeCommand;
        public ICommand CopyMediaCodeCommand
        {
            get
            {
                return copyMediaCodeCommand ??= new RelayCommand(
                    // execute
                    (param) =>
                    {
                        Media media = param as Media;
                        if (media == null) throw new ArgumentNullException("Media is null when getting copy code");
                        Clipboard.SetDataObject($"![media]({media.FileName.Replace('\\', '/')})");
                        MainMessageQueue.Clear();
                        MainMessageQueue.Enqueue($"Code for {media.Name} copied");
                    },
                    // can execute
                    (param) =>
                    {
                        return param as Media != null;
                    }
                );
            }
        }


        ICommand deleteMediaCommand;
        public ICommand DeleteMediaCommand
        {
            get
            {
                return deleteMediaCommand ??= new RelayCommand(
                    // execute
                    async (param) =>
                    {
                        Media media = param as Media;
                        if (media == null) throw new ArgumentNullException("Media to be deleted is null");
                        int index = DataSource.QuestionSet.Media.IndexOf(media);
                        DataSource.QuestionSet.Media.Remove(media);
                        bool undo = false;
                        MainMessageQueue.Clear();
                        MainMessageQueue.Enqueue(
                            $"{media.Name} deleted",
                            "UNDO",
                            (param) =>
                            {
                                DataSource?.QuestionSet?.Media?.Insert(param.index, param.media);
                                undo = true;
                            },
                            (index, media),
                            false,
                            true,
                            TimeSpan.FromSeconds(5));
                        await Task.Delay(6000);
                        if (!undo && !DataSource.QuestionSet.Media.Any(x => x.FileName == media.FileName))
                            await DataSource.DeleteMedia(media);
                    },
                    // can execute
                    (param) =>
                    {
                        return (DataSource?.QuestionSet?.Media?.Contains(param)).GetValueOrDefault();
                    }
                );
            }
        }

        public MainViewModel()
        {
            AppInitialize = async _ =>
            {
                FileAssociations.EnsureAssociationsSet();
                DataSource = new LocalFileDataSource();
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1 && !args[1].IsNullOrWhiteSpace())
                {
                    if (args[1] == "-i")
                    {
                        if (args.Length > 2 && !args[2].IsNullOrWhiteSpace())
                        {
                            if (File.Exists(args[2]))
                            {
                                if (ImportFromExcelCommand.CanExecute(args[2]))
                                    ImportFromExcelCommand.Execute(args[2]);
                            }
                        }
                    }
                    else if (File.Exists(args[1]))
                    {
                        if (OpenFileCommand.CanExecute(args[1]))
                            OpenFileCommand.Execute(args[1]);
                    }
                    else
                    {
                        Logger.Log($"File not found: {args[1]}", Severity.Error);
                        MainMessageQueue.Enqueue($"Invalid commandline argument: {string.Join(" ", args.Skip(1))}");
                    }
                }
                try
                {
                    using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/Profound-Education-Centre/DeltaQuestionEditor-WPF"))
                    {
                        var updateInfo = await mgr.CheckForUpdate(false, (progress) =>
                        {
                            UpdateProgress = progress;
                            UpdateStatus = "Checking";
                        });
                        if (updateInfo.ReleasesToApply.Any())
                        {
                            string updateFile = AppDataPath("update");
                            EnsurePathExist(AppDataPath());
                            bool locked = false;
                            if (File.Exists(updateFile))
                            {
                                if (long.TryParse(File.ReadAllText(updateFile), out long res))
                                {
                                    if (TimeSpan.FromTicks(DateTime.Now.Ticks - res) < TimeSpan.FromDays(1))  // consider the update lock file as expired if its created 24 hours ago
                                    {
                                        locked = true;
                                    }
                                }
                            }
                            if (!locked)
                            {
                                File.WriteAllText(updateFile, DateTime.Now.Ticks.ToString());
                                var result = await mgr.UpdateApp((progress) =>
                                {
                                    UpdateProgress = progress;
                                    UpdateStatus = "Updating";
                                });
                                await Task.Delay(500);
                                if (File.Exists(updateFile))
                                {
                                    File.Delete(updateFile);
                                }
                                UpdateStatus = "Restart app to update";
                            }
                            else
                            {
                                UpdateStatus = "Updating elsewhere";
                            }
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
            };
            AppClosed = _ =>
            {
                DataSource.Dispose();
            };
        }
    }
}
