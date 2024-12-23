using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedAsync.ConfigureAwaitFalse
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            var button = new Button {Text = "Click me!"};
            button.Click += (_, __) => LongLongAsyncOperation().Wait(); 
            Controls.Add(button);
        }

        private async Task LongLongAsyncOperation()
        {
            await Task.Delay(100);

            Text = "Clicked!";
        }
    }
}