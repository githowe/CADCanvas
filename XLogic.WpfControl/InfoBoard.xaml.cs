using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace XLogic.WpfControl
{
    public partial class InfoBoard : UserControl
    {
        public InfoBoard() => InitializeComponent();

        public Brush BoardBackColor
        {
            get => MainGrid.Background;
            set => MainGrid.Background = value;
        }

        public void AddInfo(params string[] titleArray)
        {
            foreach (var title in titleArray)
            {
                if (title == "")
                {
                    AddSplit();
                    continue;
                }

                InfoBar bar = new InfoBar();
                bar.Title.Text = title + "：";
                bar.Info.Text = "";
                if (InfoPanel.Children.Count > 0)
                {
                    bar.Margin = new Thickness(0, 5, 0, 0);
                }
                InfoPanel.Children.Add(bar);
                _infoDict.Add(title, bar.Info);
            }
        }

        public void UpdateInfo(string title, string info)
        {
            if (_infoDict.ContainsKey(title))
                _infoDict[title].Text = info;
        }

        public void AddSplit()
        {
            InfoPanel.Children.Add(new Grid { Height = 5 });
        }

        public void ClearInfo()
        {
            InfoPanel.Children.Clear();
            _infoDict.Clear();
        }

        private readonly Dictionary<string, Run> _infoDict = new Dictionary<string, Run>();
    }
}