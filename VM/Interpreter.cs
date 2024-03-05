using System.Reflection;
using System.Reflection.Metadata;
using ExpaIR.Exceptions;
namespace ExpaIR;
static class PointerHelpers
{
    public static int PointerToOffset(uint pointer) => (int)(pointer & 0x7fffffff); // returns the uint pointer, except the first bit is set to zero(indicies can't be negative anyways)
    public static uint OffsetToPointer(int Offset) => (uint)(Offset | 0x80000000);
    public static bool IsPointer(this uint value) => (value & 0x80000000) != 0;
}
class Vm
{
    private readonly uint[] Code;
    private uint[] RAM;
    private Stack<uint> Stack { get; } = new(); //as an address, its represented by "-1"
    private int Current = 0;
    private protected virtual int MaxMem { get; } = 6000;
    private protected const int StackMagicOffset = 1;
    private protected const int NullOffset = 0;
    private bool ABS_Malloc(int size) => __ABS_Malloc(size);
    private bool __ABS_Malloc(int size)
    {
        bool rv = size > RAM.Length;
        if(size > MaxMem || size < 0){ throw new ExpaIR_MallocError(); }
        Array.Resize(ref RAM, size);
        return rv;
    }
    private bool REL_Malloc(int newSize) => ABS_Malloc(newSize + RAM.Length);//cannot cast to int here if RAM.Length = max size of int
    private uint GetAtRAMAddress(int index) => GetAtPointer(GetAtProgramAddress(index));//gets the ram address written at the index provided in the program.
    private bool IndexOutOfBounds(int index) => index >= RAM.Length; // >= as int[2] has max index 1, so index 2 would be out of bounds
    private uint GetAtPointer(uint pointer) => GetAtOffset(PointerHelpers.PointerToOffset(pointer));
    private uint GetAtOffset(int Index)
    {
        //if index is larger than the code length, return pointer to 0(null), or pop stack if 1, else get memory stored. 
        if (Index >= MaxMem) throw new ExpaIR_OutOfMemoryException("Tried to access memory outside of maximum allocated memory"); // >= as Maxmem is 1-indexed
        else if(IndexOutOfBounds(Index)|| Index == NullOffset)
        {
            return PointerHelpers.OffsetToPointer(NullOffset);//returns null
        }
        else if(Index == StackMagicOffset) { return Stack.Pop(); }
        else if (Index < 0) { throw new MemoryAddressAccessException($"Attempted to set memory at negative index ({Index})"); }
        return RAM[Index];
    }
    private uint GetAtProgramAddress(int index)
    {
        return index > 0 ? Code[index] : throw new ExpaIR_ProgramAccessException($"Access failed due to index either exceeding program memory or being below zero({index})");
    }
    private bool SetAddressAtPointer(uint pointer, uint value) => SetAddress(PointerHelpers.PointerToOffset(pointer), value);

    private bool SetAddress(int Index, uint value)//can't be indexSetAddress because of null and stack
    {
        //returns true if address is set without any need for implicit malloc, returns false if implicit malloc is needed(and logs an error), and if SetAddress fails entirely, ABS_Malloc will throw the exception neccesary. If index is negative one specifically, then push to stack. Else, throw a memaddress exception.
        //index is zero-indexed\
        if (IndexOutOfBounds(Index))
        {
            Console.Error.Write($"Attempted to set address outside of allocated memory; automatically increasing allocated memory size from {RAM.Length} doublewords to {Index} doublewords");
            ABS_Malloc(Index + 1);//+1 to convert from index to length
            SetAddress(Index, value);
            return false;
        }
        else if (Index == StackMagicOffset) { Stack.Push(value); }
        else if (Index == NullOffset) { throw new MemoryAddressAccessException($"Attempted to set to null({NullOffset})"); }
        else if (Index < 0) { throw new MemoryAddressAccessException($"Attempted to set memory at negative index ({Index})"); }
        else { RAM[Index] = value; }
        return true;
    }
    public Vm(uint[] code)
    {
        Code = code;
        RAM = Array.Empty<uint>();
    }
    public int Process()
    {
        for(; Current < Code.Length; Current++)
        {
            ProcessOpCode((Opcode)Code[Current]);
        }
        return 0;
        
    }
    private delegate int GetAtAddress(int index);
    private delegate bool Comparator(uint value1, uint value2);
    private delegate int MOperator(int value1, int value2);

    private uint GetAtPointerIfPointer(uint PointerOrValue) => PointerOrValue.IsPointer() ? GetAtPointer(PointerOrValue) : PointerOrValue;
    private uint GetSuppliedValue(int programCounter) => GetAtPointerIfPointer(GetAtProgramAddress(programCounter)); //returns either the constant value at the program-counter supplied, or the value that the constant argument points to in RAM
    private void ProcessOpCode(Opcode opcode)
    {
        
        uint arg;
        /// <summary>
        /// Jumps to the Jump Destination if the condition is true. JumpDestination may be either a pointer or value.
        /// </summary>
        /// <param name="JumpDestination">May be either pointer or value</param>
        /// <param name="condition"></param>
        bool JIF(uint JumpDestination, bool condition)
        {
            //JumpDestination may be a pointer
            if(condition)
            {
                Current = (int)GetAtPointerIfPointer(JumpDestination) - 1;//-1 because process will increment current by one
                if(Current < -1)
                {
                    throw new ExpaIR_ProgramAccessException("Jumped to a negative program address, which is impossible");
                }
            }
            return condition;
        }
        void JCMP(Comparator comparator)
        {

            if (!JIF
            (
                GetAtProgramAddress(Current + 1),
                comparator
                (
                    GetSuppliedValue(Current + 2),
                    GetSuppliedValue(Current + 3)
                )

            )) { Current += 3; }
        }
        switch (opcode)
        {
            //when adding to current to handle arguments for opcodes, at the end of processing the opcode current needs to point to the last argument consumed.
            case Opcode.JMP:
                JIF(GetAtProgramAddress(Current + 1), true);
                Current += 1;
                break;
            case Opcode.JEQ: JCMP((uint x, uint y) => x == y); break;
            case Opcode.JLT: JCMP((uint x, uint y) => x < y); break; //unsigned
            case Opcode.JEL: JCMP((uint x, uint y) => x <= y); break; 
            case Opcode.JGT: JCMP((uint x, uint y) => x > y); break;
            case Opcode.JEG: JCMP((uint x, uint y) => x >= y); break;
            case Opcode.SJEQ: JCMP((uint x, uint y) => ((int)x) == ((int)y) ); break;
            case Opcode.SJLT: JCMP((uint x, uint y) => ((int)x) < ((int)y) ); break; //unsigned
            case Opcode.SJEL: JCMP((uint x, uint y) => ((int)x) <= ((int)y) ); break; 
            case Opcode.SJGT: JCMP((uint x, uint y) => ((int)x) > ((int)y) ); break;
            case Opcode.SJEG: JCMP((uint x, uint y) => ((int)x) >= ((int)y) ); break;
            case Opcode.ADD:
                SetAddress
                (
                    (int)GetSuppliedValue(Current + 1),
                    (GetSuppliedValue(Current + 2) + GetSuppliedValue(Current + 3)) % uint.MaxValue
                );
                Current += 3;
                break;
            case Opcode.OUT:
                Console.WriteLine(GetSuppliedValue(Current + 1));
                Current++;
                break;
            case Opcode.MALLOC:
                REL_Malloc((int)GetSuppliedValue(++Current)); break;
            case Opcode.LOAD:
                SetAddress((int)GetAtProgramAddress(++Current), GetSuppliedValue(++Current)); break;
            case Opcode.CPC:
                SetAddress((int)GetAtProgramAddress(++Current), (uint)Current); break;
            case Opcode.EXIT:
                Current = Code.Length + 1; break;
        }
    }
}
public class Program
{
    static void Main()
    {
        uint[] Code = {
            (uint)Opcode.MALLOC, 20,
            (uint)Opcode.LOAD, 1, 1, //pushes 1 onto the stack
            (uint)Opcode.LOAD, 2, 2, //loads 2 into ram
            (uint)Opcode.ADD, 2, 1, PointerHelpers.OffsetToPointer(2), //sets 1 + 2 to the ram
            (uint)Opcode.LOAD,1,PointerHelpers.OffsetToPointer(2), //pushes that result onto the stack
            (uint)Opcode.OUT, PointerHelpers.OffsetToPointer(2),//outputs the result,
            (uint)Opcode.JEQ,24, PointerHelpers.OffsetToPointer(1), 3, //if the result on the stack is equal to 3, print true, else false
            (uint)Opcode.OUT, 0,
            (uint)Opcode.EXIT,//return
            (uint)Opcode.OUT, 1,
            (uint)Opcode.EXIT
            
            };
        Vm vm = new(Code);
        vm.Process();
    }
}