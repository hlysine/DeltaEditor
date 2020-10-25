using DeltaQuestionEditor_WPF.Helpers;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.ViewModels
{
    class MainViewModel : NotifyPropertyChanged
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

        public Action<object> AppInitialize { get; private set; }

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
            };
        }
    }
}
