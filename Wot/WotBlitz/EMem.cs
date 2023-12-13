using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WotBlitz
{
	// Token: 0x02000004 RID: 4
	public class EMem
	{
		// Token: 0x06000008 RID: 8
		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		// Token: 0x06000009 RID: 9
		[DllImport("kernel32.dll")]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

		// Token: 0x0600000A RID: 10
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

		// Token: 0x0600000B RID: 11
		[DllImport("kernel32.dll")]
		public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr dwAddress, uint nSize, EMem.MemoryProtection flProtect, out uint lpflOldProtect);

		// Token: 0x0600000C RID: 12
		[DllImport("kernel32.dll")]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr dwAddress, uint nSize, EMem.AllocationType flAllocationType, EMem.MemoryProtection flProtect);

        // Token: 0x0600000D RID: 13 RVA: 0x000020FC File Offset: 0x000002FC
        public void ReadBytes(IntPtr Address, byte[] buf, int[] Offsets = null)
        {
            try
            {
                int numBytesRead = 0;
                byte[] buffer = new byte[buf.Length];
                if (Offsets == null)
                {
                    ReadProcessMemory(this.handle, Address, buf, buffer.Length, ref numBytesRead);
                }
                else
                {
                    IntPtr lpBaseAddress = Address;
                    ReadProcessMemory(this.handle, Address, buffer, buffer.Length, ref numBytesRead);
                    for (int i = 0; i < Offsets.Length; i++)
                    {
                        int offset = BitConverter.ToInt32(buffer, 0) + Offsets[i];
                        lpBaseAddress = new IntPtr(offset);
                        ReadProcessMemory(this.handle, lpBaseAddress, buffer, buffer.Length, ref numBytesRead);
                    }
                    Array.Copy(buffer, buf, buffer.Length);
                }
            }
            catch
            {
                // Handle or log the exception as needed.
            }
        }

		// Token: 0x0600000E RID: 14 RVA: 0x000021B0 File Offset: 0x000003B0
		public void WriteBytes(IntPtr Address, byte[] Data, int[] Offsets = null)
		{
			int num = 0;
			bool flag = Offsets == null;
			if (flag)
			{
				EMem.WriteProcessMemory(this.handle, Address, Data, Data.Length, ref num);
			}
			else
			{
				byte[] array = new byte[8];
				int num2 = 1;
                IntPtr lpBaseAddress = IntPtr.Zero;
                EMem.ReadProcessMemory(this.handle, Address, array, array.Length, ref num2);
				for (int i = 0; i < Offsets.Length - 1; i++)
				{
					lpBaseAddress = new IntPtr(BitConverter.ToInt32(array, 0)) + Offsets[i];
					EMem.ReadProcessMemory(this.handle, lpBaseAddress, array, array.Length, ref num2);
				}
				lpBaseAddress = new IntPtr(BitConverter.ToInt32(array, 0)) + Offsets[Offsets.Length - 1];
				EMem.WriteProcessMemory(this.handle, lpBaseAddress, Data, Data.Length, ref num);
			}
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002280 File Offset: 0x00000480
		public IntPtr GetModuleBase(string Modulename)
		{
            IntPtr intPtr = IntPtr.Zero;
            bool flag = !this.isOpened;
			IntPtr result;
			if (flag)
			{
				result = new IntPtr(-2);
			}
			else
			{
				try
				{
					foreach (object obj in this.pTarget.Modules)
					{
						ProcessModule processModule = (ProcessModule)obj;
						bool flag2 = processModule.ModuleName == Modulename;
						if (flag2)
						{
							intPtr = processModule.BaseAddress;
							return intPtr;
						}
					}
					bool flag3 = intPtr != IntPtr.Zero;
					if (flag3)
					{
						result = new IntPtr(-1);
					}
					else
					{
						result = intPtr;
					}
				}
				catch
				{
					result = new IntPtr(-2);
				}
			}
			return result;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x0000235C File Offset: 0x0000055C
		public bool SetProtection(IntPtr start, uint size, EMem.MemoryProtection newprotection)
		{
			uint num = 0U;
			return EMem.VirtualProtectEx(this.handle, start, size, newprotection, out num);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002380 File Offset: 0x00000580
		public IntPtr AllocateMemory(IntPtr Address, uint size, EMem.AllocationType allocationtype, EMem.MemoryProtection protection)
		{
			return EMem.VirtualAllocEx(this.handle, Address, size, allocationtype, protection);
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000023A4 File Offset: 0x000005A4
		public bool OpenProcess(string Procname)
		{
			try
			{
				this.pTarget = Process.GetProcessesByName(Procname)[0];
				this.handle = EMem.OpenProcess(this.PROCESS_ALL_ACCESS, false, this.pTarget.Id);
			}
			catch
			{
				this.isOpened = false;
				return false;
			}
			bool flag = this.pTarget == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				this.isOpened = true;
				result = true;
			}
			return result;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x0000241C File Offset: 0x0000061C
		public T Read<T>(IntPtr Address, int[] Offsets = null)
		{
			try
			{
				int num = 0;
				byte[] array = new byte[8];
				bool flag = Offsets == null;
				if (flag)
				{
					EMem.ReadProcessMemory(this.handle, Address, array, array.Length, ref num);
					bool flag2 = typeof(T) == typeof(int);
					if (flag2)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToInt32(array, 0), typeof(T)));
					}
					bool flag3 = typeof(T) == typeof(float);
					if (flag3)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToSingle(array, 0), typeof(T)));
					}
					bool flag4 = typeof(T) == typeof(double);
					if (flag4)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToDouble(array, 0), typeof(T)));
					}
					bool flag5 = typeof(T) == typeof(long);
					if (flag5)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToInt64(array, 0), typeof(T)));
					}
					bool flag6 = typeof(T) == typeof(short);
					if (flag6)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToInt16(array, 0), typeof(T)));
					}
					bool flag7 = typeof(T) == typeof(ushort);
					if (flag7)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToUInt16(array, 0), typeof(T)));
					}
					bool flag8 = typeof(T) == typeof(ulong);
					if (flag8)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToUInt64(array, 0), typeof(T)));
					}
					bool flag9 = typeof(T) == typeof(uint);
					if (flag9)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToUInt32(array, 0), typeof(T)));
					}
					bool flag10 = typeof(T) == typeof(byte);
					if (flag10)
					{
						return (T)((object)Convert.ChangeType(array[0], typeof(T)));
					}
				}
				else
				{
                    IntPtr lpBaseAddress = IntPtr.Zero;
                    EMem.ReadProcessMemory(this.handle, Address, array, array.Length, ref num);
					for (int i = 0; i < Offsets.Length; i++)
					{
						lpBaseAddress = new IntPtr(BitConverter.ToInt32(array, 0)) + Offsets[i];
						EMem.ReadProcessMemory(this.handle, lpBaseAddress, array, array.Length, ref num);
					}
					bool flag11 = typeof(T) == typeof(int);
					if (flag11)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToInt32(array, 0), typeof(T)));
					}
					bool flag12 = typeof(T) == typeof(float);
					if (flag12)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToSingle(array, 0), typeof(T)));
					}
					bool flag13 = typeof(T) == typeof(double);
					if (flag13)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToDouble(array, 0), typeof(T)));
					}
					bool flag14 = typeof(T) == typeof(long);
					if (flag14)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToInt64(array, 0), typeof(T)));
					}
					bool flag15 = typeof(T) == typeof(short);
					if (flag15)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToInt16(array, 0), typeof(T)));
					}
					bool flag16 = typeof(T) == typeof(ushort);
					if (flag16)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToUInt16(array, 0), typeof(T)));
					}
					bool flag17 = typeof(T) == typeof(ulong);
					if (flag17)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToUInt64(array, 0), typeof(T)));
					}
					bool flag18 = typeof(T) == typeof(uint);
					if (flag18)
					{
						return (T)((object)Convert.ChangeType(BitConverter.ToUInt32(array, 0), typeof(T)));
					}
					bool flag19 = typeof(T) == typeof(byte);
					if (flag19)
					{
						return (T)((object)Convert.ChangeType(array[0], typeof(T)));
					}
				}
			}
			catch
			{
			}
			return default(T);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000029CC File Offset: 0x00000BCC
		public void Write<T>(IntPtr Address, T Data, int[] Offsets = null)
		{
			int num = 0;
			bool flag = Offsets == null;
			if (flag)
			{
				bool flag2 = typeof(T) == typeof(int);
				if (flag2)
				{
					EMem.WriteProcessMemory(this.handle, Address, BitConverter.GetBytes(Convert.ToInt32(Data)), 4, ref num);
				}
				bool flag3 = typeof(T) == typeof(float);
				if (flag3)
				{
					EMem.WriteProcessMemory(this.handle, Address, BitConverter.GetBytes(Convert.ToSingle(Data)), 4, ref num);
				}
				bool flag4 = typeof(T) == typeof(double);
				if (flag4)
				{
					EMem.WriteProcessMemory(this.handle, Address, BitConverter.GetBytes(Convert.ToDouble(Data)), 8, ref num);
				}
				bool flag5 = typeof(T) == typeof(long);
				if (flag5)
				{
					EMem.WriteProcessMemory(this.handle, Address, BitConverter.GetBytes(Convert.ToInt64(Data)), 8, ref num);
				}
				bool flag6 = typeof(T) == typeof(short);
				if (flag6)
				{
					EMem.WriteProcessMemory(this.handle, Address, BitConverter.GetBytes(Convert.ToInt16(Data)), 2, ref num);
				}
				bool flag7 = typeof(T) == typeof(ushort);
				if (flag7)
				{
					EMem.WriteProcessMemory(this.handle, Address, BitConverter.GetBytes(Convert.ToUInt16(Data)), 2, ref num);
				}
				bool flag8 = typeof(T) == typeof(ulong);
				if (flag8)
				{
					EMem.WriteProcessMemory(this.handle, Address, BitConverter.GetBytes(Convert.ToUInt64(Data)), 8, ref num);
				}
				bool flag9 = typeof(T) == typeof(uint);
				if (flag9)
				{
					EMem.WriteProcessMemory(this.handle, Address, BitConverter.GetBytes(Convert.ToUInt32(Data)), 4, ref num);
				}
				bool flag10 = typeof(T) == typeof(byte);
				if (flag10)
				{
					EMem.WriteProcessMemory(this.handle, Address, BitConverter.GetBytes((short)Convert.ToByte(Data)), 1, ref num);
				}
			}
			else
			{
				byte[] array = new byte[8];
				int num2 = 1;
                IntPtr lpBaseAddress = IntPtr.Zero;
                EMem.ReadProcessMemory(this.handle, Address, array, array.Length, ref num2);
				for (int i = 0; i < Offsets.Length - 1; i++)
				{
					lpBaseAddress = new IntPtr(BitConverter.ToInt32(array, 0)) + Offsets[i];
					EMem.ReadProcessMemory(this.handle, lpBaseAddress, array, array.Length, ref num2);
				}
				lpBaseAddress = new IntPtr(BitConverter.ToInt32(array, 0)) + Offsets[Offsets.Length - 1];
				bool flag11 = typeof(T) == typeof(int);
				if (flag11)
				{
					EMem.WriteProcessMemory(this.handle, lpBaseAddress, BitConverter.GetBytes(Convert.ToInt32(Data)), 4, ref num);
				}
				bool flag12 = typeof(T) == typeof(float);
				if (flag12)
				{
					EMem.WriteProcessMemory(this.handle, lpBaseAddress, BitConverter.GetBytes(Convert.ToSingle(Data)), 4, ref num);
				}
				bool flag13 = typeof(T) == typeof(double);
				if (flag13)
				{
					EMem.WriteProcessMemory(this.handle, lpBaseAddress, BitConverter.GetBytes(Convert.ToDouble(Data)), 8, ref num);
				}
				bool flag14 = typeof(T) == typeof(long);
				if (flag14)
				{
					EMem.WriteProcessMemory(this.handle, lpBaseAddress, BitConverter.GetBytes(Convert.ToInt64(Data)), 8, ref num);
				}
				bool flag15 = typeof(T) == typeof(short);
				if (flag15)
				{
					EMem.WriteProcessMemory(this.handle, lpBaseAddress, BitConverter.GetBytes(Convert.ToInt16(Data)), 2, ref num);
				}
				bool flag16 = typeof(T) == typeof(ushort);
				if (flag16)
				{
					EMem.WriteProcessMemory(this.handle, lpBaseAddress, BitConverter.GetBytes(Convert.ToUInt16(Data)), 2, ref num);
				}
				bool flag17 = typeof(T) == typeof(ulong);
				if (flag17)
				{
					EMem.WriteProcessMemory(this.handle, lpBaseAddress, BitConverter.GetBytes(Convert.ToUInt64(Data)), 8, ref num);
				}
				bool flag18 = typeof(T) == typeof(uint);
				if (flag18)
				{
					EMem.WriteProcessMemory(this.handle, lpBaseAddress, BitConverter.GetBytes(Convert.ToUInt32(Data)), 4, ref num);
				}
				bool flag19 = typeof(T) == typeof(byte);
				if (flag19)
				{
					EMem.WriteProcessMemory(this.handle, lpBaseAddress, BitConverter.GetBytes((short)Convert.ToByte(Data)), 1, ref num);
				}
			}
		}

		
		public string Readstring(IntPtr Address, uint stringlength, EMem.STringEncoding encoding, int[] Offsets = null)
		{
			int num = 1;
			byte[] array = new byte[stringlength * 2U];
			bool flag = Offsets == null;
			string result;
			if (flag)
			{
				EMem.ReadProcessMemory(this.handle, Address, array, array.Length, ref num);
				string text;
				switch (encoding)
				{
				case EMem.STringEncoding.UTF8:
					text = Encoding.UTF8.GetString(array);
					break;
				case EMem.STringEncoding.ASCII:
					text = Encoding.ASCII.GetString(array);
					break;
				case EMem.STringEncoding.Unicode:
					text = Encoding.Unicode.GetString(array);
					break;
				case EMem.STringEncoding.ANSI:
					text = Encoding.Default.GetString(array);
					break;
				default:
					text = null;
					break;
				}
				result = text;
			}
			else
			{
				IntPtr lpBaseAddress = IntPtr.Zero;
				EMem.ReadProcessMemory(this.handle, Address, array, array.Length, ref num);
				for (int i = 0; i < Offsets.Length; i++)
				{
					lpBaseAddress = new IntPtr(BitConverter.ToInt32(array, 0)) + Offsets[i];
					EMem.ReadProcessMemory(this.handle, lpBaseAddress, array, array.Length, ref num);
				}
				string text;
				switch (encoding)
				{
				case EMem.STringEncoding.UTF8:
					text = Encoding.UTF8.GetString(array);
					break;
				case EMem.STringEncoding.ASCII:
					text = Encoding.ASCII.GetString(array);
					break;
				case EMem.STringEncoding.Unicode:
					text = Encoding.Unicode.GetString(array);
					break;
				case EMem.STringEncoding.ANSI:
					text = Encoding.Default.GetString(array);
					break;
				default:
					text = null;
					break;
				}
				result = text;
			}
			return result;
		}

		// Token: 0x04000004 RID: 4
		private const int PAGE_NOACCESS = 1;

		// Token: 0x04000005 RID: 5
		private int PROCESS_ALL_ACCESS = 2035711;

		// Token: 0x04000006 RID: 6
		public Process pTarget;

		// Token: 0x04000007 RID: 7
		private IntPtr handle;

		// Token: 0x04000008 RID: 8
		private bool isOpened = false;

		// Token: 0x02000008 RID: 8
		[Flags]
		public enum AllocationType
		{
			// Token: 0x0400001B RID: 27
			Commit = 4096,
			// Token: 0x0400001C RID: 28
			Reserve = 8192,
			// Token: 0x0400001D RID: 29
			Decommit = 16384,
			// Token: 0x0400001E RID: 30
			Release = 32768,
			// Token: 0x0400001F RID: 31
			Reset = 524288,
			// Token: 0x04000020 RID: 32
			Physical = 4194304,
			// Token: 0x04000021 RID: 33
			TopDown = 1048576,
			// Token: 0x04000022 RID: 34
			WriteWatch = 2097152,
			// Token: 0x04000023 RID: 35
			LargePages = 536870912
		}

		// Token: 0x02000009 RID: 9
		[Flags]
		public enum MemoryProtection
		{
			// Token: 0x04000025 RID: 37
			PAGE_NOACCESS = 1,
			// Token: 0x04000026 RID: 38
			PAGE_READONLY = 2,
			// Token: 0x04000027 RID: 39
			PAGE_READWRITE = 4,
			// Token: 0x04000028 RID: 40
			PAGE_WRITECOPY = 8,
			// Token: 0x04000029 RID: 41
			PAGE_EXECUTE = 16,
			// Token: 0x0400002A RID: 42
			PAGE_EXECUTE_READ = 32,
			// Token: 0x0400002B RID: 43
			PAGE_EXECUTE_READWRITE = 64,
			// Token: 0x0400002C RID: 44
			PAGE_EXECUTE_WRITECOPY = 128,
			// Token: 0x0400002D RID: 45
			PAGE_GUARD = 256,
			// Token: 0x0400002E RID: 46
			PAGE_NOCACHE = 512,
			// Token: 0x0400002F RID: 47
			PAGE_WRITECOMBINE = 1024
		}

		// Token: 0x0200000A RID: 10
		[Flags]
		public enum STringEncoding
		{
			// Token: 0x04000031 RID: 49
			UTF8 = 0,
			// Token: 0x04000032 RID: 50
			ASCII = 1,
			// Token: 0x04000033 RID: 51
			Unicode = 2,
			// Token: 0x04000034 RID: 52
			ANSI = 3
		}
	}
}
