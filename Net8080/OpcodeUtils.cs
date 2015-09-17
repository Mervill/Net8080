using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net8080
{
	public class OpcodeUtils
	{
		public static string Disassemble(byte[] bytes, int dataoffset = 0) //0x100
        {
            var entrypoint = 0x100;
			//var immediates = new Dictionary<int,byte>(); //unused
            var addresses = new Dictionary<int, int>();
            var callTarget = new Dictionary<int, int>();

			for(int x = 0; x < bytes.Length; )
			{
				Opcode op = (Opcode)bytes[x];
				int fileoffset = x + entrypoint;
				int len = op.DataLength();

				/*if(len == 2)
				{
					var data8 = bytes[x + 1];
					immediates.Add(fileoffset, data8);
				}*/

				if(len == 3)
				{
					var addr16 = BitConverter.ToInt32(new byte[] { bytes[x + 1], bytes[x + 2], 0, 0 }, 0);
					addresses.Add(fileoffset, addr16);
					if(op == Opcode.CALL) // TODO: Include undocumented?
					{
						callTarget.Add(fileoffset, addr16);
					}
				}

				x += len;
			}

            var result = new StringBuilder();

			result.AppendLine(";");
			result.AppendLine("; Entrypoint: " + entrypoint.ToString("X4"));
			result.AppendLine(";");

			for(var x = 0; x < bytes.Length; )
			{
                var fileoffset = x + dataoffset;
                var op = (Opcode)bytes[x];
                var len = op.DataLength();
				
				if(callTarget.ContainsValue(fileoffset))
				{
					result.AppendLine("; Subroutine:");
				}

				// Byte display block
				result.AppendFormat("{0} ", fileoffset.ToString("X4"));
				result.AppendFormat("{0}",bytes[x].ToString("X2"));
				if(len == 1) result.Append("      ");
				if(len == 2)
				{
					var data8 = bytes[x + 1];
					result.AppendFormat(" {0} ", data8.ToString("X4"));
				}
				if(len == 3)
				{
					var addr16 = BitConverter.ToInt32(new byte[] { bytes[x + 1], bytes[x + 2], 0, 0 }, 0);
					result.AppendFormat(" {0} ", addr16.ToString("X4"));
				}
				// // //

				if(addresses.ContainsValue(fileoffset))
				{
					result.AppendFormat("Lx{0}: ", fileoffset.ToString("X4"));
				}
				else
				{
					result.Append("        ");
				}

				result.Append(op.ToString().Replace('_', ' '));

				if(len == 2)
				{
					var data8 = bytes[x + 1];
					result.AppendFormat(" 0x{0}", data8.ToString("X4"));
				}

				if(len == 3)
				{
					var addr16 = BitConverter.ToInt32(new byte[] { bytes[x + 1], bytes[x + 2], 0, 0 }, 0);
					result.AppendFormat(" Lx{0}", addr16.ToString("X4"));
				}

				switch(op)
				{
					default: 
						result.AppendLine();
						break;
					case Opcode.JMP:
					case Opcode.RET: 
						result.AppendLine();
						result.AppendLine(";;");
						break;
				}

				x += len;
			}

			return result.ToString();
		}

		public static string SimpleDisassemble(byte[] bytes, int dataoffset = 0)
		{
			StringBuilder result = new StringBuilder();

			for(int x = 0; x < bytes.Length; )
			{
				Opcode op = (Opcode)bytes[x];
				result.Append("0x" + (x + dataoffset).ToString("X4") + " " + op.ToString().Replace('_', ' '));
				int len = op.DataLength();
				if(len == 2) result.Append(" 0x" + bytes[x + 1].ToString("X4"));
				if(len == 3) result.Append(" 0x" + BitConverter.ToInt16(new[] { bytes[x + 1], bytes[x + 2] }, 0).ToString("X4"));
				result.AppendLine();
				x += len;
			}

			return result.ToString();
		}

        static HashSet<string> opmnemonics;

        public static bool IsMnemonic(string str)
        {
            if (opmnemonics == null)
            {
                var opcodes = Enum.GetValues(typeof(Opcode)).Cast<Opcode>();
                opmnemonics = new HashSet<string>();
                foreach (Opcode op in opcodes)
                    opmnemonics.Add(op.ToString().Split('_')[0]);

                opmnemonics.Remove("UNDOC");
            }

            return opmnemonics.Contains(str);
        }

    }
}
