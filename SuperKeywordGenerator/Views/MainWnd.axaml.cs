using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using CsvHelper;
using Material.Dialog;
using SuperKeywordGenerator.Infraestructure;
using SuperKeywordGenerator.Infraestructure.KeywordParser;
using SuperKeywordGenerator.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using static SuperKeywordGenerator.LocalizationResources.MainRes;
using static SuperKeywordGenerator.ViewModels.SettingsVM;

namespace SuperKeywordGenerator.Views
{
    public partial class MainWnd : Window
    {
        private CancellationTokenSource tokenSource;
        private MainVM model;
        private bool launched;
        public MainWnd()
        {
            InitializeComponent();
            model = new MainVM();           
            this.DataContext = model;

 
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        
        private List<string> GetKeywordProviders()
        {
            List<string> ActiveKeyProviders = new List<string>();
            var StreamProviders = AssetLoader.Open(new Uri("avares://SuperKeywordGenerator/Assets/KeywordsProviders.csv"));
            Infraestructure.KeywordProviderParser.KeywordProviderParser ProviderParser = new Infraestructure.KeywordProviderParser.KeywordProviderParser(StreamProviders);
            var BaseProviders = ProviderParser.GetProviders();
            List<KeywordProviderVM> KeysProvActive = new List<KeywordProviderVM>();
            if (!string.IsNullOrEmpty(appsettings.Default.KeywordsProviders))
            {
                try
                {
                    KeysProvActive = JsonSerializer.Deserialize<List<KeywordProviderVM>>(appsettings.Default.KeywordsProviders);
                }
                catch
                {

                }
                
            }
            if (KeysProvActive == null || KeysProvActive.Count==0)
            {
                KeysProvActive = BaseProviders.Select(d => new SettingsVM.KeywordProviderVM() { Active = true, Name = d.Name }).ToList();
                foreach (var KeyProv in KeysProvActive)
                {
                    var url = BaseProviders.First(d => d.Name == KeyProv.Name).Url;
                    ActiveKeyProviders.Add(url);
                }
            }
            else
            {
                foreach (var KeyProv in KeysProvActive)
                {
                    if (KeyProv.Active)
                    {
                        var url = BaseProviders.First(d => d.Name == KeyProv.Name).Url;
                        ActiveKeyProviders.Add(url);
                    }                    
                }
            }
            
            return (ActiveKeyProviders);
        }
        private async Task Launch()
        {
            tokenSource = new CancellationTokenSource();
            launched = true;
            model.StopEnabled = true;
            model.LaunchEnabled = false;
            Random rnd = new Random();
            int sourceIdx = 1;
            var KeywordProviders = GetKeywordProviders();

            KeywordParserCore kParser = new KeywordParserCore(model.BaseKeywords.Select(d => d).ToList(), appsettings.Default.MinDelay, appsettings.Default.MaxDelay, appsettings.Default.Threads);

            var throttler = new SemaphoreSlim(appsettings.Default.Threads);
            var tasks = KeywordProviders.Select(async ProvUrl =>
            {
                await throttler.WaitAsync();
                var task = kParser.GetKeywordsAsync(ProvUrl, tokenSource.Token);

                _ = task.ContinueWith(async s =>
                {
                    await Task.Delay(rnd.Next(appsettings.Default.MinDelay, appsettings.Default.MaxDelay) * 1000);
                    throttler.Release();
                });

                var ResultKeywords = await task;
                sourceIdx++;
                if (ResultKeywords.Any())
                {
                    foreach (var w in ResultKeywords)
                    {
                        if (!model.GeneratedKeywords.Contains(w) && !model.BaseKeywords.Contains(w))
                            model.GeneratedKeywords.Add(w);
                    }
                    model.TotalKeywords = model.GeneratedKeywords.Count;
                }
            });
            try
            {
                await Task.WhenAny(Task.WhenAll(tasks), tokenSource.Token.AsTask());
            }
            catch
            {

            }
            model.TotalKeywords = model.GeneratedKeywords.Count();
            model.LaunchEnabled = true;
            model.StopEnabled = false;

            var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
            {
                ContentHeader = completedTitle,
                SupportingText = completedMsg,
                StartupLocation = WindowStartupLocation.CenterOwner,
                Borderless = true,
                DialogHeaderIcon = Material.Dialog.Icons.DialogIconKind.Info,                
            });
            var result = await dialog.ShowDialog(this);           
            launched = false;
            
        }
        private void SetBaseKeywords()
        {
            model.BaseKeywords.Clear();
            var lsBaseKeys = model.PlainBaseKeywords.Split(new[] { "," }, StringSplitOptions.None).ToList();
           
            foreach (var key in lsBaseKeys)
            {
                if (!string.IsNullOrEmpty(key))
                    model.BaseKeywords.Add(key.Trim());
            }

            model.PlainBaseKeywords = String.Join(", ", model.BaseKeywords);
            model.TotalBaseKeywords = model.BaseKeywords.Count;
        }


        private async void btLaunch_Click(object sender, RoutedEventArgs e)
        {
            SetBaseKeywords();
            if (model.BaseKeywords.Count > 0)
            { 
                if (!launched)
                    await Launch();
            }
        }
        private void btStop_Click(object sender, RoutedEventArgs e)
        {
            tokenSource.Cancel();            
            model.LaunchEnabled = true;
            model.StopEnabled = false;
        }
        private async void btExport_Click(object sender, RoutedEventArgs e)
        {

            var dlg =    new SaveFileDialog();
            dlg.DefaultExtension = "csv";
            dlg.Filters.Add(new FileDialogFilter() { Name = csvFileName, Extensions = { csvExt } });
            dlg.InitialFileName = Infraestructure.Files.FilesUtils.MakeValidFileName("Generated Keywords " + DateTime.Now.ToShortDateString() + DateTime.Now.ToShortDateString() + ".csv");
            var result = await dlg.ShowAsync(this);
            if (result != null)
            {
                var destinationPath = result;
                var Keywords = model.GeneratedKeywords.Select(d => new Domain.Keyword() { Value = d }).ToList();
                using (var writer = new StreamWriter(destinationPath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(Keywords);
                }
                var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
                {
                    ContentHeader = msgExportedTitle,
                    SupportingText = exportedOk,
                    StartupLocation = WindowStartupLocation.CenterOwner,
                    Borderless = true,
                    DialogHeaderIcon = Material.Dialog.Icons.DialogIconKind.Info,
                });
                var msg = await dialog.ShowDialog(this);
            }
        }
        private void mnSettings_Click(object sender, RoutedEventArgs e)
        {

            SettingsWnd sett = new SettingsWnd();
            sett.ShowDialog(this);
        }


        
        private void mnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWnd about = new AboutWnd();
            about.Show(this);

        }
        private void mnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            model.TotalKeywords = 0;
            model.GeneratedKeywords.Clear();
        }
    }
}
