using System;
using System.Linq;

namespace Net8080
{
    public interface IMemoryBus
    {
        int read(int addr);
        void write(int addr, int w8);
        void copy_to(int[] source, int startIndex);
        void Clear();
        Byte[] GetBytes();
    }

    public interface IInputOutputBus
    {
        void interrupt(bool v);
        int input(int devicenum);
        void output(int devicenum, int value);
    }

    public class MemoryBus : IMemoryBus
    {

        int[] mem = new int[0x10000];

        public virtual int read(int addr)
        {
            return mem[addr & 0xFFFF];
        }

        public virtual void write(int addr, int w8)
        {
            mem[addr & 0xFFFF] = w8;
        }

        public void copy_to(int[] source, int startIndex)
        {
            Array.Copy(source, 0, mem, startIndex, source.Length);
        }

        public void Clear()
        {
            mem = new int[0x10000];
        }

        public Byte[] GetBytes()
        {
            return mem.ToList().Select(i => BitConverter.GetBytes(i)[0]).ToArray();
        }

    }

    public class MarkingMemoryBus : MemoryBus
    {
        public bool[] did_read = new bool[0x10000];
        public bool[] did_write = new bool[0x10000];

        public override int read(int addr)
        {
            did_read[addr & 0xFFFF] = true;
            return base.read(addr);
        }

        public override void write(int addr, int w8)
        {
            did_write[addr & 0xFFFF] = true;
            base.write(addr, w8);
        }

        public void ClearMarks()
        {
            did_read = new bool[0x10000];
            did_write = new bool[0x10000];
        }
    }

    public class InputOutputBus : IInputOutputBus
    {

        public void interrupt(bool v)
        {

        }

        public int input(int devicenum)
        {
            return 0;
        }

        public void output(int devicenum, int value)
        {

        }
    }
}
