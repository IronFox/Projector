using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projector
{
	public partial class About : Form
	{
		public About()
		{
			InitializeComponent();
		}

		[DllImport("user32.dll")]
		private static extern int HideCaret(IntPtr hwnd);

		private void About_Load(object sender, EventArgs e)
		{
			HideCaret(textBox.Handle);
		}

		private void About_Shown(object sender, EventArgs e)
		{
			HideCaret(textBox.Handle);
		}

		private void textBox_MouseEnter(object sender, EventArgs e)
		{
			HideCaret(textBox.Handle);
		}
	}
}
