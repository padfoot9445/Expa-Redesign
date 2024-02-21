using System.Reflection.Metadata;

namespace VM;
class Vm
{
    private int[] Code;
    private int ConstantsBegin { get; set; }
    private Stack<int> Stack { get; } = new(); //as an address, its represented by "-1"
    private int i = 0;
    private protected virtual int MaxMem { get; } = 6000;
    private bool Malloc(int size)
    {
        if(size < Code.Length){ return true; }
        else if(size > MaxMem){ return false; }//TODO: THROW EXCEPTION
        Array.Resize(ref Code, size); 
        return true;
    }
    private int GetInstruction(int index)
    {
        if(index > Code.Length)
        {
            return 0;
        }
        return index;
    }
    public Vm(int[] code)
    {
        ConstantsBegin = Array.IndexOf(code, Opcode.INTERPRETER__CONSTANT);
        Code = code;
    }
    public int Process()
    {
        for(; i < ConstantsBegin; i++)
        {
            ProcessOpCode((Opcode)Code[i]);
        }
        return 0;
    }
    private void ProcessOpCode(Opcode opcode)
    {
        switch(opcode)
        {
            case Opcode.ADD:


        }
    }
}