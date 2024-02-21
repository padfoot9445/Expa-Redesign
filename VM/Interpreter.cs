using System.Reflection.Metadata;
using ExpaIR.Exceptions;

namespace ExpaIR;
class Vm
{
    private ushort[] Code;
    private ushort ConstantsBegin { get; set; }
    private Stack<ushort> Stack { get; } = new(); //as an address, its represented by "-1"
    private ushort Current = 0;
    private protected virtual ushort MaxMem { get; } = 6000;
    private bool ABS_Malloc(ushort size) => __ABS_Malloc(size);
    private bool __ABS_Malloc(int size)
    {
        if(size > ushort.MaxValue){ throw new OutOfMemoryException("Unable to allocate memory greater than 65535 words"); }//words as the array itself stores words
        else if(size < Code.Length){ return false; }
        else if(size > MaxMem){ throw new ExpaIR_MallocError(); }
        Array.Resize(ref Code, size); 
        return true;
    }
    private bool Malloc(ushort newSize) => __ABS_Malloc(newSize + Code.Length);//cannot cast to ushort here if Code.Length = max size of ushort
    private ushort GetAddress(ushort index)
    {
        //if index is larger than the code length, return 0(null), or pop stack if -1, else get memory stored. 
        if (index > MaxMem) throw new ExpaIR_OutOfMemoryException("Tried to access memory outside of maximum allocated memory");
        else if(index > Code.Length)
        {
            return 0;
        }
        else if(index == 0) { return Stack.Pop(); }
        else if(index < 0) { throw new MemoryAddressAccessException($"Attemted to read from a negative memory address apart from -1: {index}"); }
        return Code[index - 1];
    }
    private bool SetAddress(ushort index, ushort value)
    {
        //returns true if address is set without any need for implicit malloc, returns false if implicit malloc is needed(and logs an error), and if SetAddress fails entirely, ABS_Malloc will throw the exception neccesary. If index is negative one specifically, then push to stack. Else, throw a memaddress exception.
        //index is one-indexed for obvious reasons: you cannot have line zero of code.
        if (index < Code.Length)
        {
            Console.Error.Write($"Attempted to set address outside of allocated memory; automatically increasing allocated memory size from {Code.Length} doublewords to {index} doublewords");
            ABS_Malloc(index);
            SetAddress(index, value);
            return false;
        }
        else if (index == 0) { Stack.Push(value); }
        else if (index < 0) { throw new MemoryAddressAccessException($"Attempted to access a negative memory address(that was not -1): {index}"); }
        Code[index - 1] = value;
        return true;
    }
    public Vm(ushort[] code)
    {
        ConstantsBegin = (ushort)Array.IndexOf(code, Opcode.INTERPRETER__CONSTANT);
        Code = code;
    }
    public ushort Process()
    {
        for(; Current < ConstantsBegin; Current++)
        {
            ProcessOpCode((Opcode)Code[Current]);
        }
        return 0;
    }
    private void ProcessOpCode(Opcode opcode)
    {
        switch(opcode)
        { 
            case Opcode.ADD: SetAddress(GetAddress(++Current), (ushort)((GetAddress(++Current) + GetAddress(++Current)) % ushort.MaxValue)); break; //set to the return address(first getaddress), and the value being the two other addresses' values added together modulo ushort's max value.

        }
    }
}