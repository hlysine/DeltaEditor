using CefSharp;
using DeltaQuestionEditor_WPF.Helpers;
using MyScript.IInk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.IO;
using MyScript.IInk.UIReferenceImplementation;

namespace DeltaQuestionEditor_WPF.Views
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
    /// <summary>
    /// Interaction logic for ContentEditor.xaml
    /// </summary>
    public partial class ContentEditor : UserControl
    {
        EditorCodeProxy codeProxy;
        bool suppressDPEvent;

        private const string PART_TYPE = "Math";
        private Engine _engine;
        private Editor _editor => UcEditor.Editor;

        public ContentEditor()
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
         DependencyProperty.Register("Text", typeof(string), typeof(ContentEditor), new
            PropertyMetadata("", new PropertyChangedCallback(TextChanged)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void TextChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            ContentEditor control = d as ContentEditor;
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
            if (text == null) text = "";
            browserEditor.ExecuteScriptAsync($@"edit.getModel().setValue({text.ToJSLiteral()});");
        }

        private void browserEditor_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (browserEditor.IsBrowserInitialized)
                browserEditor.LoadHtml(Properties.Resources.editor, "http://localhtml/");
        }

        private void btnBold_Click(object sender, RoutedEventArgs e)
        {
            if (!browserEditor.IsBrowserInitialized) return;
            if (!browserEditor.CanExecuteJavascriptInMainFrame) return;
            browserEditor.ExecuteScriptAsync(@"
                var selections = edit.getSelections();
                var edits = selections.map(selection => {
                    var text = edit.getModel().getValueInRange(selection);
                    var match = /^(\**)(.*?)(\**)$/.exec(text);
                    if (match[1].length >= 2 && match[1].length == match[3].length || match[1].length >= 4 && match[1].length != match[3].length) {
                        text = /^\*\*(.*)\*\*$/.exec(text)[1];
                    }
                    else {
                        text = '**' + text + '**';
                    }
                    return { range: selection, text: text, forceMoveMarkers: true };
                });
                edit.executeEdits('C# Toolbar', edits, 
                    inverseEditOperations => inverseEditOperations.map(operation => {
                        return new monaco.Selection(operation.range.startLineNumber, operation.range.startColumn, operation.range.endLineNumber, operation.range.endColumn);
                    })
                );
            ");
        }

        private void btnItalic_Click(object sender, RoutedEventArgs e)
        {
            if (!browserEditor.IsBrowserInitialized) return;
            if (!browserEditor.CanExecuteJavascriptInMainFrame) return;
            browserEditor.ExecuteScriptAsync(@"
                var selections = edit.getSelections();
                var edits = selections.map(selection => {
                    var text = edit.getModel().getValueInRange(selection);
                    var match = /^(\**)(.*?)(\**)$/.exec(text);
                    if (match[1].length >= 1 && (match[1].length != 1 || match[3].length == match[1].length) && (match[1].length != 2 || match[3].length != match[1].length) && (match[1].length != 4 || match[3].length == match[1].length)) {
                        text = /^\*(.*)\*$/.exec(text)[1];
                    }
                    else {
                        text = '*' + text + '*';
                    }
                    return { range: selection, text: text, forceMoveMarkers: true };
                });
                edit.executeEdits('C# Toolbar', edits, 
                    inverseEditOperations => inverseEditOperations.map(operation => {
                        return new monaco.Selection(operation.range.startLineNumber, operation.range.startColumn, operation.range.endLineNumber, operation.range.endColumn);
                    })
                );
            ");
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_editor != null)
            {
                var part = _editor.Part;
                var package = part?.Package;

                _editor.Part = null;

                part?.Dispose();
                package?.Dispose();

                _editor.Dispose();
            }

            UcEditor?.Closing();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    // Initialize Interactive Ink runtime environment
                    _engine = Engine.Create(MyScript.Certificate.MyCertificate.Bytes);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "MyScript Engine Creation");
                    return;
                }
            });

            // Folders "conf" and "resources" are currently parts of the layout
            // (for each conf/res file of the project => properties => "Build Action = content")
            string[] confDirs = new string[1];
            confDirs[0] = "Assets/conf";
            _engine.Configuration.SetStringArray("configuration-manager.search-path", confDirs);
            _engine.Configuration.SetBoolean("math.solver.enable", false);

            var localFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var tempFolder = Path.Combine(localFolder, "MyScript", "tmp");
            _engine.Configuration.SetString("content-package.temp-folder", tempFolder);

            // Initialize the editor with the engine
            UcEditor.Engine = _engine;
            UcEditor.Initialize(Application.Current.MainWindow);

            // Force pointer to be a pen, for an automatic detection, set InputMode to AUTO
            SetInputMode(InputMode.PEN);

            NewFile();
        }


        private void ClosePackage()
        {
            var part = _editor.Part;
            var package = part?.Package;
            _editor.Part = null;
            part?.Dispose();
            package?.Dispose();
        }

        public void NewFile()
        {
            try
            {
                // Close current package
                ClosePackage();

                // Create package and part
                var packageName = MakeUntitledFilename();
                var package = _engine.CreatePackage(packageName);
                var part = package.CreatePart(PART_TYPE);
                _editor.Part = part;
            }
            catch (Exception ex)
            {
                ClosePackage();
                Logger.LogException(ex, "MyScript NewFile");
            }
        }

        private string MakeUntitledFilename()
        {
            var localFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(localFolder, "MyScript", NewGuid() + ".iink");
        }

        private void SetInputMode(InputMode inputMode)
        {
            UcEditor.InputMode = inputMode;
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            _editor.Undo();
        }

        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            _editor.Redo();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _editor.Clear();
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var supportedStates = _editor.GetSupportedTargetConversionStates(null);

                if ((supportedStates != null) && (supportedStates.Count() > 0))
                {
                    _editor.Convert(null, supportedStates[0]);
                    string latex = $"${_editor.Export_(null, MimeType.LATEX)}$";
                    if (latex == "$$") latex = "";
                    if (!browserEditor.IsBrowserInitialized) return;
                    if (!browserEditor.CanExecuteJavascriptInMainFrame) return;
                    browserEditor.ExecuteScriptAsync(@"
                        var selections = edit.getSelections();
                        var edits = selections.map(selection => {
                            return { range: selection, text: " + latex.ToJSLiteral() + @", forceMoveMarkers: true };
                        });
                        edit.executeEdits('C# Toolbar', edits, 
                            inverseEditOperations => inverseEditOperations.map(operation => {
                                return new monaco.Selection(operation.range.startLineNumber, operation.range.startColumn, operation.range.endLineNumber, operation.range.endColumn);
                            })
                        );
                    ");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "MyScript Convert");
            }
        }
    }
}
