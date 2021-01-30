using System;
using System.Linq;
using System.Collections.Generic;

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

    public class PermissibleMemoryBus : MemoryBus
    {
        [Flags]
        public enum Type
        {
            None,
            Read,
            Write,
            Both = Read | Write
        }

        public struct Range
        {
            public int Lower { get; private set; }
            public int Upper { get; private set; }

            public Range(int lower, int upper)
            {
                if (lower >= upper)
                    throw new ArgumentException($"A range must be {nameof(lower)} < {nameof(upper)}", nameof(lower));

                Lower = lower;
                Upper = upper;
            }

            public bool InRange(int value)
                => Lower <= value && value <= Upper;
        }

        public Dictionary<Range, Type> Areas = new Dictionary<Range, Type>();

        public void SetRange(int lower, int upper, Type flags)
        {
            Areas.Add(new Range(lower, upper), flags);
        }

        public void UnsetRange(int lower, int upper)
        {
            Areas.Remove(new Range(lower, upper));
        }

        public override int read(int addr)
        {
            if (HasFlag(addr, Type.Read))
            {
                return base.read(addr);
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public override void write(int addr, int w8)
        {
            if (HasFlag(addr, Type.Write))
            {
                base.write(addr, w8);
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public bool HasFlag(int addr, Type flag)
        {
            foreach (var kvp in Areas)
            {
                if (kvp.Key.InRange(addr))
                {
                    return kvp.Value.HasFlag(flag);
                }
            }
            return false;
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

    public class EmptyIOBus : IInputOutputBus
    {
        public virtual void interrupt(bool v)
        {
        }

        public virtual int input(int devicenum)
        {
            return 0;
        }

        public virtual void output(int devicenum, int value)
        {
        }
    }

}
