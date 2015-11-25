using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Pretwa.Gui.Annotations;
using Pretwa.Gui.Common;
using Pretwa.Gui.Models;

namespace Pretwa.Gui.Views
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : UserControl
    {
        public MainMenu()
        {
            InitializeComponent();
            this.DataContext = new NewGameModel();
        }

        private void BtnSinglePlayer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var ctx = (NewGameModel) this.DataContext;
            ctx.GameMode = GameMode.SinglePlayer;
            Trace.WriteLine("Starting new SinglePlayer game");
        }

        private void BtnMultiPlayer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var ctx = (NewGameModel)this.DataContext;
            ctx.GameMode = GameMode.MultiPlayer;
            Trace.WriteLine("Starting new MultiPlayer game");
        }
    }
}
