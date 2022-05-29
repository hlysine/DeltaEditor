using CefSharp;
using DeltaEditor.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DeltaEditor.Views
{
    using static DeltaEditor.Helpers.Helper;
    /// <summary>
    /// Interaction logic for QuestionPreviewPanel.xaml
    /// </summary>
    public partial class QuestionPreviewPanel : UserControl
    {
        public QuestionPreviewPanel()
        {
            InitializeComponent();

            browserPreview.RequestHandler = new ExternalRequestHandler();

            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.FileAccessFromFileUrls = CefState.Enabled;
            browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            browserSettings.BackgroundColor = ((SolidColorBrush)FindResource("MaterialDesignPaper")).Color.ToArgb();
            browserPreview.BrowserSettings = browserSettings;
        }

        public static readonly DependencyProperty QuestionProperty =
         DependencyProperty.Register("Question", typeof(string), typeof(QuestionPreviewPanel), new
            PropertyMetadata("", new PropertyChangedCallback(QuestionChanged)));

        public string Question
        {
            get => (string)GetValue(QuestionProperty);
            set => SetValue(QuestionProperty, value);
        }

        private static void QuestionChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            QuestionPreviewPanel control = d as QuestionPreviewPanel;
            control.QuestionChanged(e);
        }

        private void QuestionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!browserPreview.IsBrowserInitialized)
                return;
            if (!browserPreview.CanExecuteJavascriptInMainFrame)
                return;
            if (toggleAutoRefresh.IsChecked.GetValueOrDefault())
                setPreviewQuestionAsync(Question);
        }

        public static readonly DependencyProperty AnswersProperty =
         DependencyProperty.Register("Answers", typeof(ObservableCollection<string>), typeof(QuestionPreviewPanel), new
            PropertyMetadata(null, new PropertyChangedCallback(AnswersChanged)));

        public ObservableCollection<string> Answers
        {
            get => (ObservableCollection<string>)GetValue(AnswersProperty);
            set => SetValue(AnswersProperty, value);
        }

        private static void AnswersChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            QuestionPreviewPanel control = d as QuestionPreviewPanel;
            control.AnswersChanged(e);
        }

        private void AnswersChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!browserPreview.IsBrowserInitialized)
                return;
            if (!browserPreview.CanExecuteJavascriptInMainFrame)
                return;
            if (e.NewValue != e.OldValue)
            {
                var oldList = e.OldValue as ObservableCollection<string>;
                if (oldList != null)
                    oldList.CollectionChanged -= Answers_CollectionChanged;
                var newList = e.NewValue as ObservableCollection<string>;
                if (newList != null)
                    newList.CollectionChanged += Answers_CollectionChanged;
            }
            if (toggleAutoRefresh.IsChecked.GetValueOrDefault())
                setPreviewAnswersAsync(Answers);
        }

        public static readonly DependencyProperty TempPathProperty =
         DependencyProperty.Register("TempPath", typeof(string), typeof(QuestionPreviewPanel), new
            PropertyMetadata("", new PropertyChangedCallback(TempPathChanged)));

        public string TempPath
        {
            get => (string)GetValue(TempPathProperty);
            set => SetValue(TempPathProperty, value);
        }

        private static void TempPathChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            QuestionPreviewPanel control = d as QuestionPreviewPanel;
            control.TempPathChanged(e);
        }

        private void TempPathChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!browserPreview.IsBrowserInitialized)
                return;
            if (!browserPreview.CanExecuteJavascriptInMainFrame)
                return;
            setPreviewAnswersAsync(Answers);
            setPreviewQuestionAsync(Question);
        }

        private void Answers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (toggleAutoRefresh.IsChecked.GetValueOrDefault())
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        setPreviewAnswerAsync(e.NewStartingIndex, e.NewItems.Cast<string>().First());
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        setPreviewAnswerAsync(e.OldStartingIndex, e.OldItems.Cast<string>().First());
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        setPreviewAnswersAsync(Answers);
                        break;
                }
        }

        private void browserPreview_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            if (!e.Frame.IsMain)
                return;
            Dispatcher.Invoke(() =>
            {
                setPreviewQuestionAsync(Question);
                setPreviewAnswersAsync(Answers);
            });
        }

        private string processMarkdownForPreview(string text)
        {
            // Nothing to do here for now
            return text;
        }

        private void setPreviewQuestionAsync(string text)
        {
            browserPreview.ExecuteScriptAsync($@"setContent({processMarkdownForPreview(text).ToJSLiteral()}, '#question');");
        }

        private void setPreviewAnswerAsync(int index, string text)
        {
            browserPreview.ExecuteScriptAsync($@"setContent({processMarkdownForPreview(text).ToJSLiteral()}, '#answer{(index + 1)}');");
        }

        private void clearPreviewAnswersAsync()
        {
            browserPreview.ExecuteScriptAsync($@"for (i = 1; i < 5; i++) setContent('', '#answer' + i);");
        }

        private void setPreviewAnswersAsync(ObservableCollection<string> answers)
        {
            clearPreviewAnswersAsync();
            if (answers != null)
                for (int i = 0; i < answers.Count; i++)
                    setPreviewAnswerAsync(i, answers[i]);
        }

        private void browserPreview_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (browserPreview.IsBrowserInitialized)
            {
                browserPreview.LoadHtml(Properties.Resources.preview, $"file:///{TempPath.Replace('\\', '/')}/");
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            setPreviewAnswersAsync(Answers);
            setPreviewQuestionAsync(Question);
        }
    }
}
