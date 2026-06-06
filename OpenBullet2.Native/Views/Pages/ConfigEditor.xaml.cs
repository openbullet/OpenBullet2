using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Pages.Shared;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for ConfigEditor.xaml
/// </summary>
public partial class ConfigEditor : Page
{
    private readonly MainWindow mainWindow;
    private readonly ConfigEditorViewModel vm;
    private readonly Debugger debugger;
    private readonly ConfigStacker stackerPage;
    private readonly ConfigLoliCode loliCodePage;
    private readonly ConfigCSharpCode cSharpPage;
    private readonly ConfigLoliScript loliScriptPage;

    public ConfigEditor(
        MainWindow mainWindow,
        ConfigEditorViewModel vm,
        Debugger debugger,
        ConfigStacker stackerPage,
        ConfigLoliCode loliCodePage,
        ConfigCSharpCode cSharpPage,
        ConfigLoliScript loliScriptPage)
    {
        this.mainWindow = mainWindow;
        this.vm = vm;
        this.debugger = debugger;
        this.stackerPage = stackerPage;
        this.loliCodePage = loliCodePage;
        this.cSharpPage = cSharpPage;
        this.loliScriptPage = loliScriptPage;
        DataContext = vm;

        InitializeComponent();

        editorFrame.Navigated += (_, _) => UpdateButtonsVisibility();

        debuggerFrame.Content = debugger;
    }

    public void NavigateTo(ConfigEditorSection section)
    {
        int? lineToFocus = null;
        int? blockIndexToSelect = null;

        if (editorFrame.Content == loliCodePage && section == ConfigEditorSection.Stacker)
        {
            blockIndexToSelect = StackLoliPositionMapper.GetBlockIndexAtLine(vm.Config.LoliCodeScript, loliCodePage.GetCaretLine());
        }
        else if (editorFrame.Content == stackerPage && section == ConfigEditorSection.LoliCode)
        {
            var selectedBlockIndex = stackerPage.GetSelectedBlockIndex();

            if (selectedBlockIndex.HasValue)
            {
                lineToFocus = StackLoliPositionMapper.GetLineNumberForBlock(vm.Config.Stack, selectedBlockIndex.Value);
            }
        }

        switch (section)
        {
            case ConfigEditorSection.Stacker:
                stackerPage.UpdateViewModel();
                editorFrame.Content = stackerPage;

                if (blockIndexToSelect.HasValue)
                {
                    stackerPage.SelectBlockByIndex(blockIndexToSelect.Value);
                }

                break;

            case ConfigEditorSection.LoliCode:
                loliCodePage.UpdateViewModel();
                editorFrame.Content = loliCodePage;

                if (lineToFocus.HasValue)
                {
                    loliCodePage.MoveCaretToLine(lineToFocus.Value);
                }

                break;

            case ConfigEditorSection.CSharp:
                cSharpPage.UpdateViewModel();
                editorFrame.Content = cSharpPage;
                break;

            case ConfigEditorSection.LoliScript:
                loliScriptPage.UpdateViewModel();
                editorFrame.Content = loliScriptPage;
                break;

            default:
                break;
        }
    }

    private void UpdateButtonsVisibility()
    {
        if (vm.Config.Mode == ConfigMode.Stack || vm.Config.Mode == ConfigMode.LoliCode)
        {
            stackerButton.Visibility = editorFrame.Content != stackerPage
                ? Visibility.Visible : Visibility.Collapsed;

            loliCodeButton.Visibility = editorFrame.Content != loliCodePage
                ? Visibility.Visible : Visibility.Collapsed;

            cSharpButton.Visibility = editorFrame.Content != cSharpPage ? Visibility.Visible : Visibility.Collapsed;
        }
        else // C# only mode OR LoliScript mode
        {
            stackerButton.Visibility = Visibility.Collapsed;
            loliCodeButton.Visibility = Visibility.Collapsed;
            cSharpButton.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Call this when changing page via the dropdown menu otherwise it
    /// will not save the content of the LoliCode editor.
    /// </summary>
    public void OnPageChanged()
    {
        if (editorFrame.Content == loliCodePage)
        {
            loliCodePage.OnPageChanged();
        }
    }

    private void OpenStacker(object sender, RoutedEventArgs e) => mainWindow.NavigateTo(MainWindowPage.ConfigStacker);
    private void OpenLoliCode(object sender, RoutedEventArgs e) => mainWindow.NavigateTo(MainWindowPage.ConfigLoliCode);
    private void OpenCSharpCode(object sender, RoutedEventArgs e) => mainWindow.NavigateTo(MainWindowPage.ConfigCSharpCode);

    private async void Save(object sender, RoutedEventArgs e)
    {
        try
        {
            await vm.Save();
            Alert.ToastSuccess("Saved", $"{vm.Config.Metadata.Name} was saved successfully!");
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }
}

public enum ConfigEditorSection
{
    Stacker,
    LoliCode,
    CSharp,
    LoliScript
}

public class ConfigEditorViewModel : ViewModelBase
{
    private readonly IConfigRepository configRepo;
    private readonly ConfigService configService;
    public Config Config => configService.SelectedConfig;

    public ConfigEditorViewModel(IConfigRepository configRepo, ConfigService configService)
    {
        this.configRepo = configRepo;
        this.configService = configService;
    }

    public Task Save() => configRepo.SaveAsync(Config);
}
