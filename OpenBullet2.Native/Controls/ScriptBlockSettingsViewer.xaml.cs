using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.Script;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for ScriptBlockSettingsViewer.xaml
    /// </summary>
    public partial class ScriptBlockSettingsViewer : UserControl
    {
        private readonly ScriptBlockSettingsViewerViewModel vm;
        private readonly OpenBulletSettingsService obSettingsService;

        public ScriptBlockSettingsViewer(BlockViewModel blockVM)
        {
            if (blockVM.Block is not ScriptBlockInstance)
            {
                throw new Exception("Wrong block type for this UC");
            }

            obSettingsService = SP.GetService<OpenBulletSettingsService>();
            vm = new ScriptBlockSettingsViewerViewModel(blockVM);
            DataContext = vm;

            InitializeComponent();

            editor.WordWrap = obSettingsService.Settings.CustomizationSettings.WordWrap;
            editor.Text = vm.Script;
            HighlightSyntax();
        }

        private void AddOutputVariable(object sender, RoutedEventArgs e) => vm.AddOutputVariable();

        private void RemoveOutputVariable(object sender, RoutedEventArgs e)
            => vm.RemoveOutputVariable((OutputVariable)(sender as Button).Tag);

        private void EditorLostFocus(object sender, RoutedEventArgs e) => vm.Script = editor.Text;
        private void EditorTextChanged(object sender, EventArgs e) => vm.Script = editor.Text;

        private void HighlightSyntax()
        {
            var xshd = vm.Interpreter switch
            {
                Interpreter.Jint or Interpreter.NodeJS => "Highlighting/JavaScript.xshd",
                Interpreter.IronPython => "Highlighting/Python.xshd",
                _ => throw new NotImplementedException()
            };

            using var reader = XmlReader.Create(xshd);
            editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            editor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            editor.TextArea.TextView.LinkTextUnderline = false;
        }

        private void InterpreterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editor is not null)
            {
                HighlightSyntax();
            }
        }
    }

    public class ScriptBlockSettingsViewerViewModel : BlockSettingsViewerViewModel
    {
        public ScriptBlockInstance ScriptBlock => Block as ScriptBlockInstance;

        public string InputVariables
        {
            get => ScriptBlock.InputVariables;
            set
            {
                ScriptBlock.InputVariables = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Interpreter> Interpreters => Enum.GetValues(typeof(Interpreter)).Cast<Interpreter>();

        public Interpreter Interpreter
        {
            get => ScriptBlock.Interpreter;
            set
            {
                ScriptBlock.Interpreter = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<VariableType> VariableTypes => Enum.GetValues(typeof(VariableType)).Cast<VariableType>();

        private ObservableCollection<OutputVariable> outputVariablesCollection;
        public ObservableCollection<OutputVariable> OutputVariablesCollection
        {
            get => outputVariablesCollection;
            set
            {
                outputVariablesCollection = value;
                OnPropertyChanged();
            }
        }

        public string Script
        {
            get => ScriptBlock.Script;
            set
            {
                ScriptBlock.Script = value;
                OnPropertyChanged();
            }
        }

        public ScriptBlockSettingsViewerViewModel(BlockViewModel block) : base(block)
        {
            CreateCollections();
        }

        public void AddOutputVariable()
        {
            OutputVariablesCollection.Add(new());
            ScriptBlock.OutputVariables = OutputVariablesCollection.ToList();
        }

        public void RemoveOutputVariable(OutputVariable variable)
        {
            OutputVariablesCollection.Remove(variable);
            ScriptBlock.OutputVariables = OutputVariablesCollection.ToList();
        }

        private void CreateCollections()
            => OutputVariablesCollection = new ObservableCollection<OutputVariable>(ScriptBlock.OutputVariables);
    }
}
