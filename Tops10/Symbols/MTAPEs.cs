namespace Symbols
{
    public enum MTAPEs
    {
        MTWAT_ = 0, //[MTAPE 0] WAIT FOR POSITIONING
        MTREW_ = 1, //[MTAPE 1] REWIND
        MTEOF_ = 3, //[MTAPE 3] WRITE END OF FILE
        MTSKR_ = 6, //[MTAPE 6] SKIP RECORD
        MTBSR_ = 7, //[MTAPE 7] BACKSPACE RECORD
        MTEOT_ = 8, //[MTAPE 10] SKIP TO END OF TAPE
        MTUNL_ = 9, //[MTAPE 11] REWIND AND UNLOAD
        MTBLK_ = 11, //[MTAPE 13] BLANK TAPE
        MTSKF_ = 14, //[MTAPE 16] SKIP FILE
        MTBSF_ = 15, //[MTAPE 17] BACKSPACE FILE
        MTDEC_ = 64, //[MTAPE 100] DEC 9-CHANNEL
        MTIND_ = 65, //[MTAPE 101] INDUSTRY STANDARD 9-CHANNEL
        MTLTH_ = 128, //[MTAPE 200] LOW THRESHOLD
    }
}