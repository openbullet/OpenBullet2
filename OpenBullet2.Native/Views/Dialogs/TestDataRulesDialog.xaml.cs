using MahApps.Metro.IconPacks;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Data;
using RuriLib.Models.Data.Rules;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for TestDataRulesDialog.xaml
    /// </summary>
    public partial class TestDataRulesDialog : Page
    {
        private readonly TestDataRulesDialogViewModel vm;

        public TestDataRulesDialog(string testData, string wordlistType, IEnumerable<DataRule> rules)
        {
            vm = new TestDataRulesDialogViewModel(testData, wordlistType, rules);
            DataContext = vm;

            InitializeComponent();
        }
    }

    public class TestDataRulesDialogViewModel : ViewModelBase
    {
        public string WordlistType { get; init; }
        
        private RegexValidationViewModel regexValidation;
        public RegexValidationViewModel RegexValidation
        {
            get => regexValidation;
            set
            {
                regexValidation = value;
                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<SliceViewModel> slicesCollection;
        public ObservableCollection<SliceViewModel> SlicesCollection
        {
            get => slicesCollection;
            set
            {
                slicesCollection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ResultViewModel> resultsCollection;
        public ObservableCollection<ResultViewModel> ResultsCollection
        {
            get => resultsCollection;
            set
            {
                resultsCollection = value;
                OnPropertyChanged();
            }
        }

        public TestDataRulesDialogViewModel(string testData, string wordlistType, IEnumerable<DataRule> rules)
        {
            WordlistType = wordlistType;

            var env = SP.GetService<RuriLibSettingsService>().Environment;
            var wt = env.WordlistTypes.First(w => w.Name == wordlistType);
            var dataLine = new DataLine(testData, wt);
            var slices =dataLine.GetVariables().Select(v => new SliceViewModel(v.Name, v.AsString()));

            RegexValidation = new(dataLine.IsValid);
            SlicesCollection = new ObservableCollection<SliceViewModel>(slices);

            var results = new List<ResultViewModel>();

            foreach (var rule in rules)
            {
                var slice = slices.FirstOrDefault(v => v.Name == rule.SliceName);

                results.Add(slice == null
                    ? new ResultViewModel($"Invalid slice name: {rule.SliceName}", false)
                    : new ResultViewModel(GetRuleText(rule), rule.IsSatisfied(slice.Value)));
            }

            ResultsCollection = new ObservableCollection<ResultViewModel>(results);
        }

        private static string GetRuleText(DataRule rule)
        {
            if (rule is RegexDataRule rdr)
            {
                return $"{rdr.SliceName} must{(rdr.Invert ? " not" : "")} match regex {rdr.RegexToMatch}";
            }
            else if (rule is SimpleDataRule sdr)
            {
                return $"{sdr.SliceName} must{(sdr.Invert ? " not" : "")} respect: {sdr.Comparison} {sdr.StringToCompare}";
            }

            throw new NotImplementedException();
        }
    }

    public class RegexValidationViewModel : ViewModelBase
    {
        public bool Passed { get; set; }
        public string Result => Passed ? "Passed" : "Invalid";
        public SolidColorBrush Color => Passed ? Brushes.YellowGreen : Brushes.Tomato;
        public PackIconForkAwesomeKind Icon => Passed ? PackIconForkAwesomeKind.Check : PackIconForkAwesomeKind.Times;

        public RegexValidationViewModel(bool passed)
        {
            Passed = passed;
        }
    }

    public class SliceViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public SliceViewModel(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public class ResultViewModel : ViewModelBase
    {
        public bool Passed { get; set; }
        public string Text { get; set; }

        public SolidColorBrush Color => Passed ? Brushes.YellowGreen : Brushes.Tomato;
        public PackIconForkAwesomeKind Icon => Passed ? PackIconForkAwesomeKind.Check : PackIconForkAwesomeKind.Times;

        public ResultViewModel(string text, bool passed)
        {
            Text = text;
            Passed = passed;
        }
    }
}
