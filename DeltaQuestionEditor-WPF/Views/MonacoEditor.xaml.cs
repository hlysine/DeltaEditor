using CefSharp;
using DeltaQuestionEditor_WPF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeltaQuestionEditor_WPF.Views
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
    /// <summary>
    /// Interaction logic for MonacoEditor.xaml
    /// </summary>
    public partial class MonacoEditor : UserControl
    {
        EditorCodeProxy codeProxy;
        bool suppressDPEvent;

        public MonacoEditor()
        {
            InitializeComponent();

            browserEditor.RequestHandler = new ExternalRequestHandler();

            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.FileAccessFromFileUrls = CefState.Enabled;
            browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            browserSettings.BackgroundColor = ((SolidColorBrush)FindResource("MaterialDesignPaper")).Color.ToArgb();
            browserEditor.BrowserSettings = browserSettings;

            codeProxy = new EditorCodeProxy();
            browserEditor.JavascriptObjectRepository.Register("cefCode", codeProxy, true);
            codeProxy.CodeChanged += CodeProxy_CodeChanged;
        }

        private void BrowserEditor_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (!e.Frame.IsMain) return;
            Dispatcher.Invoke(() => setEditorTextAsync(Text));
        }

        private void CodeProxy_CodeChanged(EditorCodeProxy proxy)
        {
            suppressDPEvent = true;
            Dispatcher.Invoke(() => Text = proxy.Content);
            suppressDPEvent = false;
        }

        public static readonly DependencyProperty TextProperty =
         DependencyProperty.Register("Text", typeof(string), typeof(MonacoEditor), new
            PropertyMetadata("", new PropertyChangedCallback(TextChanged)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void TextChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            MonacoEditor control = d as MonacoEditor;
            control.TextChanged(e);
        }

        private void TextChanged(DependencyPropertyChangedEventArgs e)
        {
            if (suppressDPEvent) return;
            if (!browserEditor.IsBrowserInitialized) return;
            if (!browserEditor.CanExecuteJavascriptInMainFrame) return;
            setEditorTextAsync(Text);
        }

        private void setEditorTextAsync(string text)
        {
            browserEditor.ExecuteScriptAsync($@"edit.getModel().setValue({text.ToJSLiteral()});");
        }

        class EditorCodeProxy
        {
            public string Content { get; set; }

            public delegate void CodeChangedHandler(EditorCodeProxy proxy);
            public event CodeChangedHandler CodeChanged;

            public void setCode(string code)
            {
                Content = code;
                CodeChanged?.Invoke(this);
            }
        }

        private void browserEditor_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (browserEditor.IsBrowserInitialized)
                browserEditor.LoadHtml(Properties.Resources.editor, "http://localhtml/");
        }
    }
}
