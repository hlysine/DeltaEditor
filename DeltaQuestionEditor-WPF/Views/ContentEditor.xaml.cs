using CefSharp;
using DeltaQuestionEditor_WPF.Helpers;
using DeltaQuestionEditor_WPF.Logging;
using MyScript.IInk;
using MyScript.IInk.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DeltaQuestionEditor_WPF.Views
{
    using static DeltaQuestionEditor_WPF.Helpers.Helper;
    /// <summary>
    /// Interaction logic for ContentEditor.xaml
    /// </summary>
    public partial class ContentEditor : UserControl
    {
        EditorCodeProxy codeProxy;

        private int _suppressDPEvent = 0;
        public bool suppressDPEvent
        {
            get => (Interlocked.CompareExchange(ref _suppressDPEvent, 1, 1) == 1);
            set
            {
                if (value)
                    Interlocked.CompareExchange(ref _suppressDPEvent, 1, 0);
                else
                    Interlocked.CompareExchange(ref _suppressDPEvent, 0, 1);
            }
        }

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
            if (!e.Frame.IsMain)
                return;
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
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void TextChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            ContentEditor control = d as ContentEditor;
            control.TextChanged(e);
        }

        private void TextChanged(DependencyPropertyChangedEventArgs e)
        {
            if (suppressDPEvent)
                return;
            if (!browserEditor.IsBrowserInitialized)
                return;
            if (!browserEditor.CanExecuteJavascriptInMainFrame)
                return;
            setEditorTextAsync(Text);
        }

        private void setEditorTextAsync(string text)
        {
            if (text == null)
                text = "";
            browserEditor.ExecuteScriptAsync($@"edit.getModel().setValue({text.ToJSLiteral()});");
        }

        private void browserEditor_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (browserEditor.IsBrowserInitialized)
                browserEditor.LoadHtml(Properties.Resources.editor, "http://localhtml/");
        }

        private void btnBold_Click(object sender, RoutedEventArgs e)
        {
            if (!browserEditor.IsBrowserInitialized)
                return;
            if (!browserEditor.CanExecuteJavascriptInMainFrame)
                return;
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
            if (!browserEditor.IsBrowserInitialized)
                return;
            if (!browserEditor.CanExecuteJavascriptInMainFrame)
                return;
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

        private void btnHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (!browserEditor.IsBrowserInitialized)
                return;
            if (!browserEditor.CanExecuteJavascriptInMainFrame)
                return;
            browserEditor.ExecuteScriptAsync(@"
                var selections = edit.getSelections();
                var edits = selections.map(selection => {
                    var text = `[${edit.getModel().getValueInRange(selection)}](<link address here...>)`;
                    return { range: selection, text: text, forceMoveMarkers: true };
                });
                edit.executeEdits('C# Toolbar', edits,
                    inverseEditOperations => inverseEditOperations.map(operation => {
                        return new monaco.Selection(operation.range.endLineNumber, operation.range.endColumn - 23, operation.range.endLineNumber, operation.range.endColumn - 1);
                    })
                );
            ");
        }

        private void btnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (!browserEditor.IsBrowserInitialized)
                return;
            if (!browserEditor.CanExecuteJavascriptInMainFrame)
                return;
            browserEditor.ExecuteScriptAsync(@"
                var selections = edit.getSelections();
                var edits = selections.map(selection => {
                    var line = new monaco.Selection(selection.startLineNumber, 0, selection.endLineNumber, Number.MAX_SAFE_INTEGER);
                    var text = edit.getModel().getValueInRange(line);
                    var matches = [...text.replaceAll('\r\n','\n').matchAll(/^(#*) ?(.*?)$/gm)];
                    return matches.map((match, idx) => {
                        var newText;
                        if (match[1].length >= 6) {
                            newText = match[2];
                        }
                        else if (match[1].length == 0) {
                            newText = '# ' + match[0];
                        }
                        else {
                            newText = '#' + match[0];
                        }
                        return { range: new monaco.Selection(selection.startLineNumber + idx, 0, selection.startLineNumber + idx, Number.MAX_SAFE_INTEGER), text: newText, forceMoveMarkers: true };
                    });
                }).flat().filter((item, i, arr) => arr.findIndex(x => x.range.startLineNumber == item.range.startLineNumber) == i);
                edit.executeEdits('C# Toolbar', edits,
                    inverseEditOperations => inverseEditOperations.map(operation => {
                        return new monaco.Selection(operation.range.startLineNumber, operation.range.startColumn, operation.range.endLineNumber, operation.range.endColumn);
                    })
                );
            ");
        }

        private void btnBullets_Click(object sender, RoutedEventArgs e)
        {
            if (!browserEditor.IsBrowserInitialized)
                return;
            if (!browserEditor.CanExecuteJavascriptInMainFrame)
                return;
            browserEditor.ExecuteScriptAsync(@"
                var selections = edit.getSelections();
                var edits = selections.map(selection => {
                    var line = new monaco.Selection(selection.startLineNumber, 0, selection.endLineNumber, Number.MAX_SAFE_INTEGER);
                    var text = edit.getModel().getValueInRange(line);
                    var matches = [...text.replaceAll('\r\n','\n').matchAll(/^( *)([*-] |[0-9]+\. )? *(.*?)$/gm)];
                    return matches.map((match, idx) => {
                        var newText;
                        if (!match[2] || match[2].length == 0) {
                            newText = match[1] + '- ' + match[3];
                        }
                        else if (/[0-9]/.test(match[2])) {
                            newText = match[1] + match[3];
                        }
                        else {
                            newText = match[1] + (idx + 1) + '. ' + match[3];
                        }
                        return { range: new monaco.Selection(selection.startLineNumber + idx, 0, selection.startLineNumber + idx, Number.MAX_SAFE_INTEGER), text: newText, forceMoveMarkers: true };
                    });
                }).flat().filter((item, i, arr) => arr.findIndex(x => x.range.startLineNumber == item.range.startLineNumber) == i);
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
            switch (UcEditor.InputMode)
            {
                case InputMode.PEN:
                    iconInput.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pen;
                    break;
                case InputMode.TOUCH:
                    iconInput.Kind = MaterialDesignThemes.Wpf.PackIconKind.CursorMove;
                    break;
                case InputMode.AUTO:
                    iconInput.Kind = MaterialDesignThemes.Wpf.PackIconKind.LetterA;
                    break;
            }
        }

        private void btnPen_Click(object sender, RoutedEventArgs e)
        {
            switch (UcEditor.InputMode)
            {
                case InputMode.AUTO:
                    SetInputMode(InputMode.PEN);
                    break;
                case InputMode.PEN:
                    SetInputMode(InputMode.TOUCH);
                    break;
                case InputMode.TOUCH:
                    SetInputMode(InputMode.AUTO);
                    break;
            }
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
                    if (latex == "$$")
                        latex = "";
                    if (!browserEditor.IsBrowserInitialized)
                        return;
                    if (!browserEditor.CanExecuteJavascriptInMainFrame)
                        return;
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

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (toggleMath.IsSelected)
            //{
            //    // TODO: is it possible to clear the undo stack of the editor?
            //    _editor.Clear();
            //    string selection = (string)(await browserEditor.EvaluateScriptAsync(@"edit.getModel().getValueInRange(selection)")).Result;
            //    if (selection.IsNullOrWhiteSpace()) return;
            //    Match match = Regex.Match(selection, @"^\$(.*)\$$");
            //    if (match.Success)
            //    {
            //        _editor.Import_(MimeType.LATEX, match.Groups[1].Value, null);
            //    }
            //}
        }
    }
}
