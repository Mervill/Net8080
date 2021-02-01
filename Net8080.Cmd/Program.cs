using System;
using System.Linq;
using System.Diagnostics;
using System.IO;

using CommandLine;

namespace Net8080.Cmd
{
	class Program
	{
		public class Options
        {
			[Option(Required = true)]
			public string Path { get; set; }

			[Option]
			public string Mode { get; set; }
        }

		// supress printing the terminal bell (\b, 0x07) to the console
		static readonly bool supress_bell = true;

		static void Main(string[] args)
		{
			Parser.Default.ParseArguments<Options>(args).WithParsed(o => {
				if (string.IsNullOrEmpty(o.Mode))
                {
					RunFile(o.Path);
                }
				else
                {
					switch (o.Mode)
                    {
						case "run":
                        {
							RunFile(o.Path);
							break;
						}
						case "watch":
                        {
							Watch(o.Path);
							break;
                        }
						case "tests":
                        {
							BasicTests();
							break;
						}
						default:
                        {
							Console.WriteLine($"Unknown mode: {o.Mode}");
							break;
                        }
                    }
                }
			});

			Console.WriteLine("program complete");
			Console.ReadLine();
		}

		static void BasicTests()
		{
			Console.WriteLine("Intel 8080/C# test");

			string[] files = {
				"./asm/TEST.COM",
				"./asm/CPUTEST.COM",
				"./asm/8080PRE.COM",
			};

			foreach (var file in files)
				RunFile(file);

			Console.WriteLine("> press any key to run extended test (+14 min)");
			Console.ReadLine();

			//RunFile("./asm/umpire.com");
			RunFile("./asm/8080EX1.COM");
		}

		static void RunFile(string path)
		{
			var bytes = File.ReadAllBytes(path);

			Console.WriteLine($"; --------------------");
			Console.WriteLine($"; {path.Split('/').Last()}: size {bytes.Length}");

			var memBus = new PermissibleMemoryBus();
			memBus.SetRange(0x0000, 0xFFFF, PermissibleMemoryBus.Type.Both);

			var ioBus = new EmptyIOBus();

			var i8080 = new Intel8080(memBus, ioBus);
			
			// We are going to fake all the required CP/M BDOS functions by inspecting the
			// state of the Program Counter after each processor tick instead of using either a
			// custom assembly shim (maybe coming soon!) or trying to actually load CP/M itself (?!)
			//
			// A typical CP/M system call looks like this in user code:
			//
			// MOV  E, paramater ; Function param goes in E
			// MVI  C, function  ; Function index goes in C
			// CALL 5            ; Call the CP/M BDOS entrypoint
			//
			// Normally 0x0005 contains the instruction JMP 0xF200 which jumps to the higest memory
			// location in the CP/M BDOS memory area (0x0400-0xF200) where the system functions 
			// are implemented.
			//
			// For our purposes, we just need to return from 0x0005 and the user code should be none
			// the wiser.
			const int bdos_entrypoint = 0x0005;
			const int opcode_ret      = 0x00C9;
			i8080.Memory.write(bdos_entrypoint, opcode_ret); // Inject RET at 0x0005 to handle "CALL 5"

			// CP/M user programs are loaded into the CP/M TPA (Transient Program Area) starting
			// at 0x0100 and extending to a max of 0xDC00 for 64k systems. The progam counter is
			// then also set to 0x0100 and execution of the user program begins.
			const int tpa_start = 0x0100;
			i8080.Memory.copy_to(bytes.Select((b) => (int)b).ToArray(), tpa_start);
			i8080.ProgramCounter = tpa_start;
			i8080.StackPointer = 0xF1FF;

			memBus.UnsetRange(0x0000, 0xFFFF);
			memBus.SetRange(0xF200, 0xFFFF, PermissibleMemoryBus.Type.None); // BIOS
			memBus.SetRange(0xE400, 0xF1FF, PermissibleMemoryBus.Type.Both); // BDOS
			memBus.SetRange(0xDC00, 0xE3FF, PermissibleMemoryBus.Type.None); // CCP (Command Line Interpreter)
			memBus.SetRange(0x0100, 0xDBFF, PermissibleMemoryBus.Type.Both); // TPA (Transient Program Area)
			memBus.SetRange(0x0000, 0x00FF, PermissibleMemoryBus.Type.Read); // Low Storage
			
			File.Delete($"{path}.LOG");
			var writer = File.CreateText($"{path}.LOG");
			writer.AutoFlush = true;

			var masterSW = new Stopwatch();
			masterSW.Start();

			while (true)
			{
				var pc = i8080.ProgramCounter;
				const int opcode_hlt = 0x0076;
				if (i8080.Memory.read((UInt16)pc) == opcode_hlt)
				{
					writer.Write($"; HLT at {pc}");
					Console.Write($"; HLT at {pc}");
					break;
				}

				if (pc == bdos_entrypoint)
				{
					var functionIndex = i8080.REG_C;
                    switch (functionIndex)
                    {
						case 2:
                        {
							var charCode = i8080.REG_E;
							writer.Write((char)charCode);
							if (supress_bell)
							{
								if (charCode != 7)
									Console.Write($"{(char)charCode}");
							}
							else
							{
								Console.Write($"{(char)charCode}");
							}
							break;
						}
						case 9:
                        {
							for (UInt16 i = (UInt16)i8080.REG16_DE; i8080.Memory.read(i) != 0x24; i++)
							{
								var charCode = i8080.Memory.read(i);
								writer.Write((char)charCode);
								if (supress_bell)
								{
									if (charCode != 7)
										Console.Write($"{(char)charCode}");
								}
								else
								{
									Console.Write($"{(char)charCode}");
								}
							}
							break;
						}
						case 11:
                        {
							i8080.REG_A = 0;
							break;
						}
						default:
                        {
							throw new NotImplementedException();
                        }
					}
				}

				i8080.StepInstruction();
				if (i8080.ProgramCounter == 0)
				{
					writer.WriteLine();
					writer.WriteLine($"; Jump to 0000 from {pc:X4}");
					Console.WriteLine();
					Console.WriteLine($"; Jump to 0000 from {pc:X4}");
					break;
				}

			}

			masterSW.Stop();
			var time = $"; Time {new TimeSpan(masterSW.ElapsedTicks)}";
			writer.Write(time);
			Console.WriteLine(time);
		}

		static void Watch(string path)
		{
			var bytes = File.ReadAllBytes(path);

			Console.WriteLine($"; --------------------");
			Console.WriteLine($"{path.Split('/').Last()}: size {bytes.Length}");

			var memBus = new MemoryBus();
			var ioBus = new EmptyIOBus();
			var intel8080 = new Intel8080(memBus, ioBus);

			const int bdos_entrypoint = 0x0005;
			const int opcode_ret = 0x00C9;
			intel8080.Memory.write(bdos_entrypoint, opcode_ret);

			const int tpa_start = 0x0100;
			intel8080.Memory.copy_to(bytes.Select((b) => (int)b).ToArray(), tpa_start);
			intel8080.ProgramCounter = tpa_start;
			intel8080.StackPointer = 0xF1FF;

			File.Delete($"{path}.LOG");
			var writer = File.CreateText($"{path}.LOG");
			writer.AutoFlush = true;

			Console.CursorVisible = false;

			Stopwatch masterSW = new Stopwatch();
			masterSW.Start();

			int clockCycles = 0;
			int totalTicks = 0;
			while (true)
			{
				clockCycles++;

				var pc = intel8080.ProgramCounter;
				const int opcode_hlt = 0x0076;
				if (intel8080.Memory.read((UInt16)pc) == opcode_hlt)
				{
					writer.Write($"; HLT at {pc}");
					Console.Write($"; HLT at {pc}");
					break;
				}

				/*if (pc == 0x0005)
				{
					if (i8080.REG_C == 9)
						for (UInt16 i = (UInt16)i8080.REG16_DE; i8080.memory.read(i) != 0x24; i++)
						{
							writer.Write((char)i8080.memory.read(i));
							Console.Write((char)i8080.memory.read(i));
						}


					if (i8080.REG_C == 2)
					{
						writer.Write((char)i8080.REG_E);
						Console.Write((char)i8080.REG_E);
					}
				}*/

				totalTicks += intel8080.StepInstruction();

				//System.Threading.Thread.Sleep(1);
				//Console.Clear();
				Console.SetCursorPosition(0, 0);

				Console.WriteLine(string.Format(
					"PC {0,5} {0:X4} SP {1,5} {1:X4}",
					(UInt16)intel8080.ProgramCounter,
					(UInt16)intel8080.StackPointer));

				Console.WriteLine(string.Format(
					"A {0,4} {0:X2} | B    {1,4} {1:X2}   | D    {2,4} {2:X2}   | H    {3,4} {3:X2}",
					intel8080.REG_A,
					intel8080.REG_B,
					intel8080.REG_D,
					intel8080.REG_H));

				Console.WriteLine(string.Format(
					"          | C    {0,4}   {0:X2} | E    {1,4}   {1:X2} | L    {2,4}   {2:X2}",
					intel8080.REG_C,
					intel8080.REG_E,
					intel8080.REG_L));

				Console.WriteLine(string.Format(
					"          | BC {0,6} {0:X4} | DE {1,6} {1:X4} | HL {2,6} {2:X4}",
					intel8080.REG16_BC,
					intel8080.REG16_DE,
					intel8080.REG16_HL));

				Console.WriteLine($"IC: {clockCycles,6} CC: {totalTicks,6} | SW: {new TimeSpan(masterSW.ElapsedTicks)}");

				if (intel8080.ProgramCounter == 0)
				{
					writer.WriteLine();
					writer.WriteLine($"[!] Jump to 0000 from {pc:X4}");
					Console.WriteLine();
					Console.WriteLine($"[!] Jump to 0000 from {pc:X4}");
					break;
				}

			}

			Console.CursorVisible = true;

			Console.WriteLine($"Instruction Cycles: {clockCycles}");
			Console.WriteLine($"Clock Cycles:       {totalTicks}");

			masterSW.Stop();
			Console.WriteLine($"Stopwatch: {masterSW.ElapsedMilliseconds} ms / {masterSW.ElapsedTicks} ticks");
		}

	}
}
