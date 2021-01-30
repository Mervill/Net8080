﻿using System;
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

                }
			});
			BasicTests();
			//Watch("./ASM/CPUTEST.COM");
			Console.WriteLine("program complete");
			Console.ReadLine();
		}

		static void BasicTests()
		{
			Console.WriteLine("Intel 8080/C# test");

			string[] files = {
				"./ASM/TEST.COM",
				"./ASM/CPUTEST.COM",
				"./ASM/8080PRE.COM",
			};

			foreach (var file in files)
				RunFile(file);

			Console.WriteLine("> press any key to run extended test (+14 min)");
			Console.ReadLine();

			RunFile("./ASM/8080EX1.COM");
		}

		static void RunFile(string path)
		{
			var i8080 = new Intel8080(new MemoryBus(), new InputOutputBus());
			var bytes = File.ReadAllBytes(path);

			Console.WriteLine($"; --------------------");
			Console.WriteLine($"; {path.Split('/').Last()}: size {bytes.Length}");

			// Copy the program to memory
			i8080.Memory.copy_to(bytes.Select((b) => (int)b).ToArray(), 0x0100);

			// The first 256 bytes of the 8080 usually contain the interrupt table (RST 0 - 7) 
			// and system functions for CP/M or some other BDOS. The interface into BDOS involves
			// setting the 'C' register to a software interrupt, then unconditionally CALL
			// memory location 5 (the CP/M entrypoint). The test scripts make use of interrupt
			// 0x02, which takes the value of E as a char code and prints it to the screen. 
			//
			// As asm:
			// 
			//  MOV	E, A    ; Move the value of A (a char code) into E
			//  MVI C, 2    ; Set the value of C to 2 (CP/M software interrupt for print)
			//  CALL 5      ; Return to the CP/M BDOS
			//
			//
			i8080.Memory.write(5, 0xC9); // Inject RET at 0x0005 to handle "CALL 5".

			// First 256 bytes of memory are reserved for system
			// use, so classic 8080 programs usually start at 0x100
			i8080.ProgramCounter = (0x100); // Jump to entrypoint

			File.Delete($"{path}.LOG");
			var writer = File.CreateText($"{path}.LOG");
			writer.AutoFlush = true;

			Stopwatch masterSW = new Stopwatch();
			masterSW.Start();

			while (true)
			{
				var pc = i8080.ProgramCounter;
				if (i8080.Memory.read((UInt16)pc) == 0x76)
				{
					writer.Write($"; HLT at {pc}");
					Console.Write($"; HLT at {pc}");
					break;
				}

				if (pc == 0x0005)
				{
					if (i8080.REG_C == 9)
                    {
						for (UInt16 i = (UInt16)i8080.REG16_DE; i8080.Memory.read(i) != 0x24; i++)
						{
							writer.Write((char)i8080.Memory.read(i));
							var mem = i8080.Memory.read(i);
							if (supress_bell)
							{
								if (mem != 7)
									Console.Write($"{(char)mem}");
							}
							else
							{
								Console.Write($"{(char)mem}");
							}
						}
					}
					else if (i8080.REG_C == 2)
					{
						writer.Write((char)i8080.REG_E);
						var mem = i8080.REG_E;
						if (supress_bell)
						{
							if (mem != 7)
								Console.Write($"{(char)mem}");
						}
						else
						{
							Console.Write($"{(char)mem}");
						}
					}
					else
                    {
						throw new NotImplementedException();
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
			Console.WriteLine($"; Time {new TimeSpan(masterSW.ElapsedTicks)}");

			GC.Collect();
		}

		static void Watch(string path)
		{
			var intel8080 = new Intel8080(new MemoryBus(), new InputOutputBus());
			var bytes = System.IO.File.ReadAllBytes(path);

			Console.WriteLine($"{path.Split('/').Last()}: size {bytes.Length}");

			intel8080.Memory.copy_to(bytes.Select((b) => (int)b).ToArray(), 0x0100);
			intel8080.Memory.write(5, 0xC9);

			intel8080.ProgramCounter = (0x100);

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
				if (intel8080.Memory.read((UInt16)pc) == 0x76)
				{
					writer.Write($"HLT at {pc}");
					Console.Write($"HLT at {pc}");
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

			GC.Collect();
		}

	}
}
