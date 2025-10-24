using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Material.Dialog;
using SuperKeywordGenerator.Infraestructure.KeywordProviderParser;
using SuperKeywordGenerator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using static SuperKeywordGenerator.ViewModels.SettingsVM;

namespace SuperKeywordGenerator.Views
{
    public partial class SettingsWnd : Window
    {
        private SettingsVM model = new SettingsVM();
        public SettingsWnd()
        {
            InitializeComponent();
            LoadSettings();
            this.DataContext = model;
#if DEBUG
            this.AttachDevTools();
#endif
        }
        private void LoadSettings()
        {
            model.MinDelay = appsettings.Default.MinDelay;
            model.MaxDelay = appsettings.Default.MaxDelay;
            model.Threads = appsettings.Default.Threads;
            model.MaxKeywords = appsettings.Default.MaxDelay;
            var StreamProviders = AssetLoader.Open(new Uri("avares://SuperKeywordGenerator/Assets/KeywordsProviders.csv"));
            KeywordProviderParser ProviderParser = new KeywordProviderParser(StreamProviders);
            var BaseProviders = ProviderParser.GetProviders();
            if (string.IsNullOrEmpty(appsettings.Default.KeywordsProviders))
            {               
                var KeyProvs = BaseProviders.Select(p=> new ViewModels.SettingsVM.KeywordProviderVM { Name = p.Name, Active =true}).ToList();
                appsettings.Default.KeywordsProviders = JsonSerializer.Serialize(KeyProvs);
                model.KeywordProviders = KeyProvs;
            }
            else
            {
                model.KeywordProviders = JsonSerializer.Deserialize<List<KeywordProviderVM>>(appsettings.Default.KeywordsProviders);
                if (model.KeywordProviders.Count != BaseProviders.Count)
                {
                    var KeyProvs = BaseProviders.Select(p => new ViewModels.SettingsVM.KeywordProviderVM { Name = p.Name, Active = true }).ToList();
                    appsettings.Default.KeywordsProviders = JsonSerializer.Serialize(KeyProvs);
                    model.KeywordProviders = KeyProvs;
                }
            }

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private async void btAcept_Click(object sender, RoutedEventArgs e)
        {
            var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
            {
                ContentHeader = LocalizationResources.SettingsRes.saveTitle,
                SupportingText = LocalizationResources.SettingsRes.saveChanges,
                StartupLocation = WindowStartupLocation.CenterOwner,
                Borderless = true,
                DialogHeaderIcon = Material.Dialog.Icons.DialogIconKind.Warning,
                NegativeResult = new DialogResult("no"),
                DialogButtons = new DialogButton[]
                {
                    new DialogButton
                    {
                        Content =LocalizationResources.SettingsRes.Yes,
                        Result = "yes"
                    },
                    new DialogButton
                    {
                        Content = LocalizationResources.SettingsRes.No,
                        Result = "no"
                    },
                },
            });
            var msg = await dialog.ShowDialog(this);           
            if (msg.GetResult =="yes")
            {
                appsettings.Default.MinDelay = model.MinDelay;
                appsettings.Default.MaxDelay = model.MaxDelay;
                appsettings.Default.Threads = model.Threads;
                appsettings.Default.KeywordsProviders = appsettings.Default.KeywordsProviders = JsonSerializer.Serialize(model.KeywordProviders);
                appsettings.Default.Save();
            }
            Close();
        }
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();

        }
    }
}
