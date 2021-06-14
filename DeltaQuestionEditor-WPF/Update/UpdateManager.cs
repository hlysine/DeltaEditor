using DeltaQuestionEditor_WPF.Helpers;
using Squirrel;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeltaQuestionEditor_WPF.Update
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
    class UpdateManager : NotifyPropertyChanged
    {
        public string AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        int updateProgress = 0;
        public int UpdateProgress
        {
            get => updateProgress;
            set => SetAndNotify(ref updateProgress, value);
        }

        string updateStatus = "";
        public string UpdateStatus
        {
            get => updateStatus;
            set => SetAndNotify(ref updateStatus, value);
        }

        SemaphoreSlim updateFinished = new SemaphoreSlim(0, 1);

        public bool UpdateFinished => updateFinished.CurrentCount > 0;
        private bool upToDate = false;
        public bool UpToDate
        {
            get => upToDate;
            set => SetAndNotify(ref upToDate, value);
        }

        public async Task WaitForUpdateAsync()
        {
            await updateFinished.WaitAsync();
            updateFinished.Release();
        }

        public async Task PerformUpdate()
        {
            try
            {
                UpdateStatus = "Checking for updates";
                using (var mgr = await Squirrel.UpdateManager.GitHubUpdateManager("https://github.com/Profound-Education-Centre/DeltaQuestionEditor-WPF"))
                {
                    var updateInfo = await mgr.CheckForUpdate(false, (progress) =>
                    {
                        UpdateProgress = progress;
                        UpdateStatus = "Checking";
                    });
                    if (updateInfo.ReleasesToApply.Any())
                    {
                        UpdateStatus = "Preparing for update";
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
                        UpToDate = true;
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus = "Update error";
                Logger.LogException(ex, ex.Source);
            }
            updateFinished.Release();
            NotifyChanged(nameof(UpdateFinished));
        }
    }
}
