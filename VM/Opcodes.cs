using System.Runtime.CompilerServices;

namespace ExpaIR;
enum Opcode: ushort
{
    JMP,
    JEQ,
    JLT,//unsigned, so the sign bit is interepreted as a numberical bit as well
    JEL,//Jump Equal to or Less than (not Jump Less than or Equal because that would be JLE and confusing with Jump LEss)
    JGT,
    JEG,
    SJEQ,//signed
    SJLT,
    SJEL,
    SJGT,
    SJEG,
    ADD,
    //TODO

    OUT,
    //TODO

    MALLOC,
    //TODO
    
    LOAD,
    CPC,
    EXIT
}