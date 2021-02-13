using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Console
{
    public static class Out
    {
        public static TextBox TextBox
        {
            get;
            set;
        }
        public static void WriteLine(string Text)
        {
            TextBox.Dispatcher.BeginInvoke((Action)(() =>
                                TextBox.Text += Text + "\n"));
            TextBox.Dispatcher.BeginInvoke((Action)(() =>
                                TextBox.ScrollToEnd()));
            System.Console.Out.WriteLine(Text);
        }
    }
}
