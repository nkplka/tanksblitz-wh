using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WotBlitz
{
	// Token: 0x02000005 RID: 5
	public partial class Form1 : Form
	{
		// Token: 0x06000017 RID: 23
		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		// Token: 0x06000018 RID: 24
		[DllImport("kernel32.dll")]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

		// Token: 0x06000019 RID: 25
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

		// Token: 0x0600001A RID: 26
		[DllImport("kernel32.dll")]
		public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr dwAddress, uint nSize, Form1.MemoryProtection flProtect, out uint lpflOldProtect);

		// Token: 0x0600001B RID: 27
		[DllImport("kernel32.dll")]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr dwAddress, uint nSize, Form1.AllocationType flAllocationType, Form1.MemoryProtection flProtect);

		// Token: 0x0600001C RID: 28
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		// Token: 0x0600001D RID: 29
		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
		internal static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		// Token: 0x0600001E RID: 30 RVA: 0x0000306C File Offset: 0x0000126C
		public Form1()
		{
			this.InitializeComponent();

		}

		// Token: 0x0600001F RID: 31 RVA: 0x0000313C File Offset: 0x0000133C
		private void color_Click(object sender, EventArgs e)
		{
			ColorDialog colorDialog = new ColorDialog();
			Button button = (Button)sender;
			colorDialog.ShowDialog();
			Color color = colorDialog.Color;
			button.BackColor = color;
			bool flag = button.Text == "Team";
			if (flag)
			{
				this.m.Write<float>(this.friendly_color, Convert.ToSingle(color.R) / 255f, null);
				this.m.Write<float>(this.friendly_color + 4, Convert.ToSingle(color.G) / 255f, null);
				this.m.Write<float>(this.friendly_color + 8, Convert.ToSingle(color.B) / 255f, null);
			}
			else
			{
				this.m.Write<float>(this.enemy_color, Convert.ToSingle(color.R) / 255f, null);
				this.m.Write<float>(this.enemy_color + 4, Convert.ToSingle(color.G) / 255f, null);
				this.m.Write<float>(this.enemy_color + 8, Convert.ToSingle(color.B) / 255f, null);
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00003280 File Offset: 0x00001480
		private void bar_size_Scroll(object sender, EventArgs e)
		{
			bool flag = this.m.Read<int>(this.exebase + 768, null) == 1234;
			if (flag)
			{
				MessageBox.Show("Try after battle started.");
			}
			else
			{
				
			}
		}

		// Token: 0x06000021 RID: 33
		private void btn_attatch_Click(object sender, EventArgs e)
		{
			if (!this.m.OpenProcess("tanksblitz"))
			{
				MessageBox.Show("igra pasta ne rabotaet");
				return;
			}
			this.exedump = new byte[this.m.pTarget.MainModule.ModuleMemorySize];
			this.m.ReadBytes(this.m.pTarget.MainModule.BaseAddress, this.exedump, null);
			this.exebase = this.m.GetModuleBase("tanksblitz.exe");
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < this.exedump.Length - this.pattern1.Length; i++)
			{
				bool flag3 = true;
				for (int j = 0; j < this.pattern1.Length; j++)
				{
					flag3 = (this.exedump[i + j] == this.pattern1[j] && flag3);
					if (!flag3)
					{
						break;
					}
				}
				if (flag3)
				{
					num = i;
				}
			}
			for (int k = 0; k < this.exedump.Length - this.pattern2.Length; k++)
			{
				bool flag4 = true;
				for (int l = 0; l < this.pattern2.Length; l++)
				{
					flag4 = (this.exedump[k + l] == this.pattern2[l] && flag4);
					if (!flag4)
					{
						break;
					}
				}
				if (flag4)
				{
					num2 = k;
				}
			}
			if (num == 0 || num2 == 0)
			{
				MessageBox.Show("koder eblan po hodu");
				return;
			}

			this.m.SetProtection(this.exebase + num - 10, 512U, EMem.MemoryProtection.PAGE_EXECUTE_READWRITE);
			this.m.SetProtection(this.exebase + 768, 768U, EMem.MemoryProtection.PAGE_EXECUTE_READWRITE);
			this.m.WriteBytes(this.exebase + (num + 2), new byte[]
			{
				144,
				144
			}, null);
			this.m.WriteBytes(this.exebase + (num - 9), new byte[]
			{
				144,
				144
			}, null);
			int num3 = 0;
			do
			{
				num3++;
				if (this.m.Read<ushort>(this.exebase + num - num3, null) == 52428)
				{
					goto IL_25A;
				}
			}
			while (num3 <= 4096);
			MessageBox.Show("koder eblan po hodu");
			return;
			IL_25A:
			this.m.Write<int>(this.exebase + 768, 1234, null);
			this.m.Write<short>(this.exebase + (num - num3 + 12), 3465, null);
			this.m.Write<int>(this.exebase + (num - num3 + 12 + 2), this.exebase.ToInt32() + 768, null);
			this.friendly_color = new IntPtr(this.m.Read<int>(this.exebase + (num2 + 39), null));
			this.enemy_color = new IntPtr(this.m.Read<int>(this.exebase + num2 + 48, null));
			this.thickness = this.exebase + 768;
			if (this.m.Read<float>(this.enemy_color, null) != 1f)
			{
				MessageBox.Show("koder eblan po hodu");
				return;
			}
			this.m.SetProtection(this.enemy_color, 768U, EMem.MemoryProtection.PAGE_READWRITE);
			this.Text = num.ToString("x16") + " " + num2.ToString("x16");
		}

		// Token: 0x04000009 RID: 9
		public int bytesWritten = 0;

		// Token: 0x0400000A RID: 10
		public int bytesRead = 0;

		// Token: 0x0400000B RID: 11
		private EMem m = new EMem();

		// Token: 0x0400000C RID: 12
		private byte[] exedump;

		// Token: 0x0400000D RID: 13
		private byte[] pattern1 = new byte[]
		{
			132,
			192,
			116,
			4,
			176,
			1,
			235,
			2,
			50,
			192,
			56,
			7,
			116
		};

		// Token: 0x0400000E RID: 14
		private byte[] pattern2 = new byte[]
		{
			139,
			64,
			4,
			byte.MaxValue,
			208,
			139,
			77,
			220
		};

		// Token: 0x0400000F RID: 15
		private IntPtr friendly_color;

		// Token: 0x04000010 RID: 16
		private IntPtr enemy_color;

		// Token: 0x04000011 RID: 17
		private IntPtr exebase;

		// Token: 0x04000012 RID: 18
		private IntPtr thickness = IntPtr.Zero;

		// Token: 0x0200000B RID: 11
		[Flags]
		public enum AllocationType
		{
			// Token: 0x04000036 RID: 54
			Commit = 4096,
			// Token: 0x04000037 RID: 55
			Reserve = 8192,
			// Token: 0x04000038 RID: 56
			Decommit = 16384,
			// Token: 0x04000039 RID: 57
			Release = 32768,
			// Token: 0x0400003A RID: 58
			Reset = 524288,
			// Token: 0x0400003B RID: 59
			Physical = 4194304,
			// Token: 0x0400003C RID: 60
			TopDown = 1048576,
			// Token: 0x0400003D RID: 61
			WriteWatch = 2097152,
			// Token: 0x0400003E RID: 62
			LargePages = 536870912
		}

		// Token: 0x0200000C RID: 12
		[Flags]
		public enum MemoryProtection
		{
			// Token: 0x04000040 RID: 64
			PAGE_NOACCESS = 1,
			// Token: 0x04000041 RID: 65
			PAGE_READONLY = 2,
			// Token: 0x04000042 RID: 66
			PAGE_READWRITE = 4,
			// Token: 0x04000043 RID: 67
			PAGE_WRITECOPY = 8,
			// Token: 0x04000044 RID: 68
			PAGE_EXECUTE = 16,
			// Token: 0x04000045 RID: 69
			PAGE_EXECUTE_READ = 32,
			// Token: 0x04000046 RID: 70
			PAGE_EXECUTE_READWRITE = 64,
			// Token: 0x04000047 RID: 71
			PAGE_EXECUTE_WRITECOPY = 128,
			// Token: 0x04000048 RID: 72
			PAGE_GUARD = 256,
			// Token: 0x04000049 RID: 73
			PAGE_NOCACHE = 512,
			// Token: 0x0400004A RID: 74
			PAGE_WRITECOMBINE = 1024
		}
	}
}
