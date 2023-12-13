using System;
using System.Windows.Forms;

namespace WotBlitz
{
	// Token: 0x02000006 RID: 6
	internal static class Program
	{
		// Token: 0x06000024 RID: 36 RVA: 0x00003B08 File Offset: 0x00001D08
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}
