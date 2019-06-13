﻿namespace Symbols
{
    public enum TTCALLs
    {
        INCHRW = 0, // [TTCALL 0,] INPUT CHAR AND WAIT
        OUTCHR = 1, // [TTCALL 1,] OUTPUT CHAR
        INCHRS = 2, // [TTCALL 2,] INPUT CHAR AND SKIP
        OUTSTR = 3, // [TTCALL 3,] OUTPUT STRING
        INCHWL = 4, // [TTCALL 4,] INPUT CHAR WAIT, LINE
        INCHSL = 5, // [TTCALL 5,] INPUT CHAR SKIP, LINE
        GETLCH = 6, // [TTCALL 6,] GET LINE CHARS
        SETLCH = 7, // [TTCALL 7,] SET LINE CHARS
        RESCAN = 8, // [TTCALL 10,] RESET INPUT LINE
        CLRBFI = 9, // [TTCALL 11,] CLEAR INPUT BUFFER
        CLRBFO = 10, // [TTCALL 12,] CLEAR OUTPUT BUFFER
        SKPINC = 11, // [TTCALL 13,] SKIP IF CHAR IN INPUT
        SKPINL = 12, // [TTCALL 14,] SKIP IF LINE IN INPUT
        IONEOU = 13, // [TTCALL 15,] OUTPUT IMAGE CHAR
    }
}