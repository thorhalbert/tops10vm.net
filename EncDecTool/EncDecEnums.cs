namespace EncDecTool
{
    public enum WordTypes
    {
        PDP10, // 36 Bit pdp-10
    }

    public enum NamingStyles
    {
        DEC, // Upper case variables, includes ., $, % (.=_, $=d %=p so .FRED$BLA% is _FREDdBLAp
    }

    public enum Bases
    {
        Bin = 2,
        Oct = 8,
        Dec = 10,
        Hex = 16,
        B32 = 32,
        B36 = 36,
        B64 = 64,
        Rad50_10 = 1000,
        Rad50_11 = 1001,
        SixBit = 1002,
    }
}