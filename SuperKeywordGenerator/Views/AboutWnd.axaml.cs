using Avalonia.Controls;
using SuperKeywordGenerator.ViewModels;

namespace SuperKeywordGenerator.Views
{
    public partial class AboutWnd : Window
    {
        private AboutVM model = new AboutVM();
        public AboutWnd()
        {
            InitializeComponent();
            LoadModel();
            this.DataContext = model;
 
        }

       
        private void LoadModel()
        {
            model.version = GetType().Assembly.GetName().Version.ToString();
        }
       
    }
}
