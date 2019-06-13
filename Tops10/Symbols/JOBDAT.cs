﻿namespace Symbols
{
    public enum JOBDAT
    {
        _JBUUO = 0x20, // [040] USER UUO TRAP LOC.(UUO STORED HERE)
        _JB41 = 0x21, // [041] USER UUO JSR LOCATION
        _JBERR = 0x22, // [042] LH UNUSED-SAVE FOR LATER THINGS - SO USER PROGRAMS
        _JBREL = 0x24, // [044] LH=0 - RH=HIGHEST REL. ADR. IN USER AREA(IE LOW SEGMENT)
        _JBBLT = 0x25, // [045] 3 WORDS USED BY LINKING LOADER TO MOVE
        _JBDDT = 0x3c, // [074] LH = FIRST ADDRESS PAST DDT (DDT END + 1)
        _JBHSO = 0x3d, // [075] HIGH SEGMENT ORIGIN PAGE NUMBER (TOPS-20 ONLY)
        _JBBPT = 0x3e, // [076] ADDRESS OF UNSOLICITED BREAKPOINT ENTRY INTO DDT
        _JBCN6 = 0x46, // [106] 6 TEMP LOCATIONS USED BY CHAIN
        _JBEDV = 0x4a, // [112] POINTER TO EXEC DATA VECTOR
        _JBHRL = 0x4d, // [115] LH IS FIRST FREE LOC IN HIGH SEG RELATIVE TO ITS ORIGIN
        _JBUSY = 0x4f, // [117] POINTER TO UNDEFINED SYMBOL TABLE
        _JBSA = 0x50, // [120] LH=INITIAL FIRST FREE LOCATION IN LOW SEG (SET BY LOADER)
        _JBFF = 0x51, // [121] (SET FROM HIGH DATA AREA ON GET IF NO LOW FILE)
        _JBPFH = 0x53, // [123] LH=LENGTH OF PAGE FAULT HANDLER
        _JBREN = 0x54, // [124] REENTER ADDRESS FOR REENTER COMMAND
        _JBAPR = 0x55, // [125] PLACE TO TRAP TO IN USER AREA ON APR TRAP
        _JBCNI = 0x56, // [126] APR IS CONIED INTO C(JOBCNI) ON APR TRAP
        _JBTPC = 0x57, // [127] PC IS STORED HERE ON USER APR TRAP
        _JBOPC = 0x58, // [130] OLD PC IS STORED HERE ON START,DDT,REENTER,
        _JBCHN = 0x59, // [131] LH=FIRST LOC AFTER FIRST FORTRAN 4 LOADED PROGRAM
        _JBINT = 0x5c, // [134] RH=LOC OF DATA BLOCK FOR ERROR INTERCEPTING
        _JBOPS = 0x5d, // [135] RESERVED TO OBJECT TIME SYSTEMS
        _JBCST = 0x5e, // [136] RESERVED TO CUSTOMERS
        _JBVER = 0x5f, // [137] CONTAINS VERSION NO.(OCTAL) OF PROGRAM BEING RUN
        _JBDA = 0x60, // [140] FIRST LOC NOT USED BY JOB DATA AREA
    }
}