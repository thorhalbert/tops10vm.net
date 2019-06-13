using System;
using System.Collections.Generic;
using PDP10CPU.BreakPoints;
using PDP10CPU.Enums;
using PDP10CPU.Events;
using PDP10CPU.Memory;
using Symbols;
using ThirtySixBits;

namespace PDP10CPU.CPU
{
    public partial class SimhPDP10CPU
    {
        // pdp10_cpu.c: PDP-10 CPU simulator

        //Copyright (c) 1993-2007, Robert M. Supnik

        //Permission is hereby granted, free of charge, to any person obtaining a
        //copy of this software and associated documentation files (the "Software"),
        //to deal in the Software without restriction, including without limitation
        //the rights to use, copy, modify, merge, publish, distribute, sublicense,
        //and/or sell copies of the Software, and to permit persons to whom the
        //Software is furnished to do so, subject to the following conditions:

        //The above copyright notice and this permission notice shall be included in
        //all copies or substantial portions of the Software.

        //THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        //IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        //FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL
        //ROBERT M SUPNIK BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
        //IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
        //CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

        //Except as contained in this notice, the name of Robert M Supnik shall not be
        //used in advertising or otherwise to promote the sale, use or other dealings
        //in this Software without prior written authorization from Robert M Supnik.

        //cpu          KS10 central processor

        //17-Jul-07    RMS     Fixed non-portable usage in SHOW HISTORY
        //28-Apr-07    RMS     Removed clock initialization
        //22-Sep-05    RMS     Fixed declarations (from Sterling Garwood)
        //                     Fixed warning in MOVNI
        //16-Aug-05    RMS     Fixed C++ declaration and cast problems
        //10-Nov-04    RMS     Added instruction history
        //08-Oct-02    RMS     Revised to build dib_tab dynamically
        //                     Added SHOW IOSPACE
        //30-Dec-01    RMS     Added old PC queue
        //25-Dec-01    RMS     Cleaned up sim_inst declarations
        //07-Dec-01    RMS     Revised to use new breakpoint package
        //21-Nov-01    RMS     Implemented ITS 1-proceed hack
        //31-Aug-01    RMS     Changed int64 to t_int64 for Windoze
        //10-Aug-01    RMS     Removed register in declarations
        //17-Jul-01    RMS     Moved function prototype
        //19-May-01    RMS     Added workaround for TOPS-20 V4.1 boot bug
        //29-Apr-01    RMS     Fixed modifier naming conflict
        //                     Fixed XCTR/XCTRI, UMOVE/UMOVEM, BLTUB/BLTBU for ITS
        //                     Added CLRCSH for ITS

        //The 36b system family had six different implementions: PDP-6, KA10, KI10,
        //L10, KL10 extended, and KS10.  This simulator implements the KS10.

        //The register state for the KS10 is:

        //AC[8][16]                    accumulators
        //PC                           program counter
        //flags<0:11>                  state flags
        //pi_enb<1:7>                  enabled PI levels
        //pi_act<1:7>                  active PI levels
        //pi_prq<1:7>                  program PI requests
        //apr_enb<0:7>                 enabled system flags
        //apr_flg<0:7>                 system flags
        //ebr                          executive base register
        //ubr                          user base register
        //hsb                          halt status block address
        //spt                          SPT base
        //cst                          CST base
        //pur                          process use register
        //cstm                         CST mask

        //The PDP-10 had just two instruction formats: memory reference
        //and I/O.

        // 000000000 0111 1 1111 112222222222333333
        // 012345678 9012 3 4567 890123456789012345
        //+---------+----+-+----+------------------+
        //|  opcode | ac |i| idx|     address      | memory reference
        //+---------+----+-+----+------------------+

        // 000 0000000 111 1 1111 112222222222333333
        // 012 3456789 012 3 4567 890123456789012345
        //+---+-------+---+-+----+------------------+
        //|111|device |iop|i| idx|     address      | I/O
        //+---+-------+---+-+----+------------------+

        //This routine is the instruction decode routine for the PDP-10.
        //It is called from the simulator control program to execute
        //instructions in simulated memory, starting at the simulated PC.
        //It runs until an abort occurs.

        //General notes:

        //1. Reasons to stop.  The simulator can be stopped by:

        //     HALT instruction
        //     MUUO instruction in executive mode
        //     pager error in interrupt sequence
        //     invalid vector table in interrupt sequence
        //     illegal instruction in interrupt sequence
        //     breakpoint encountered
        //     nested indirects exceeding limit
        //     nested XCT's exceeding limit
        //     I/O error in I/O simulator

        //2. Interrupts.  PDP-10's have a seven level priority interrupt
        //   system.  Interrupt requests can come from internal sources,
        //   such as APR program requests, or external sources, such as
        //   I/O devices.  The requests are stored in pi_prq for program
        //   requests, pi_apr for other internal flags, and pi_ioq for
        //   I/O device flags.  Internal and device (but not program)
        //   interrupts must be enabled on a level by level basis.  When
        //   an interrupt is granted on a level, interrupts at that level
        //   and below are masked until the interrupt is dismissed.

        //   The I/O device interrupt system is taken from the PDP-11.
        //   int_req stores the interrupt requests for Unibus I/O devices.
        //   Routines in the Unibus adapter map requests in int_req to
        //   PDP-10 levels.  The Unibus adapter also calculates which
        //   device to get a vector from when a PDP-10 interrupt is granted.

        //3. Arithmetic.  The PDP-10 is a 2's complement system.

        //4. Adding I/O devices.  These modules must be modified:

        //     pdp10_defs.h    add device address and interrupt definitions
        //     pdp10_sys.c     add sim_devices table entry

        //A note on ITS 1-proceed.  The simulator follows the implementation
        //on the KS10, keeping 1-proceed as a side flag (its_1pr) rather than
        //as flags<8>.  This simplifies the flag saving instructions, which
        //don't have to clear flags<8> before saving it.  Instead, the page
        //fail and interrupt code must restore flags<8> from its_1pr.  Unlike
        //the KS10, the simulator will not lose the 1-proceed trap if the
        //1-proceeded instructions clears 1-proceed.

        #region Nested type: cpu_exit

        #endregion

        private const int XctMax = 32;
        private const int IndMax = 32;

        public event EventHandler<PCChangedEvent> PCChanged;
        public event EventHandler<ProcFlagChangedEvent> ProcFlagChanged;
        public event EventHandler<LightsChangedEvent> LightsChanged;

        public event EventHandler<EffectiveAddressCalculatedEvent> EffectiveAddressCalculated;

        public UserModeCore CORE { get; private set; }
        public Accumulators AC { get; private set; }
        public OSTypes RunOS { get; set; }
        public ProcessorTypes ProcessorType { get; set; }
        public RunModes Runmode { get; set; }

        public ulong InstructionsExecuted { get; private set; }

        public Word36 Switches { get; set; }
        private Word36 lights;

        public Word36 Lights
        {
            get { return lights; }
            set
            {
                if (LightsChanged != null &&
                    lights.Equals(value))
                    LightsChanged(this, new LightsChangedEvent(lights, value));
                lights = value;
            }
        }

        private ulong procFlags;

        public ulong ProcFlags
        {
            get { return procFlags; }
            set
            {
                if (ProcFlagChanged != null &&
                    procFlags != value)
                    ProcFlagChanged(this, new ProcFlagChangedEvent(procFlags, value, serializeProcFlags(value)));
                procFlags = value;
            }
        }

        private ulong pc;

        public ulong PC
        {
            get { return pc; }
            set
            {
                if (PCChanged != null &&
                    pc != value)
                    PCChanged(this, new PCChangedEvent(pc, value));
                pc = value;
            }
        }

        private int CurrentSegment { get; set; }

        private ulong mb;

        private ulong pxctFlags;

        private ulong Im { get; set; }

        private ulong Ims
        {
            // #define IMS             (((d10) ea) << 18)
            get { return Im << 18; }
        }

        private readonly Dictionary<ulong, BreakContext> breakPoints =
            new Dictionary<ulong, BreakContext>();

        public Dictionary<ulong, BreakContext> BreakPoints
        {
            get { return breakPoints; }
        }

        public delegate InstructionExit ProcessInstruction(SimhPDP10CPU processor,
                                                           ulong instruction,
                                                           OpCodes opcode, int ac, ulong ea);

        private readonly ProcessInstruction[] uuoHandlers = new ProcessInstruction[512];

        private readonly ulong[] dwrs = new ulong[2];

        private ulong pagerPC;
        //private ulong epta; // proc tbl addr (dyn) 
        private ulong upta; // proc tbl addr (dyn) 
        private ulong ubr; // User base register

        //private ulong pi_on; // pi system enable 
        //private ulong pi_enb; // pi enabled levels 
        //private ulong pi_act; // pi active levels 
        //private ulong pi_ioq; // pi io requests 
        //private ulong pi_apr; // pi apr requests 
        //private ulong pi_prq; // pi prog requests 
        //private ulong apr_enb; // apr enables 
        //private ulong apr_flg; // apr flags 
        //private ulong apr_lvl; // apr level 

        private bool rlog;

        private int xctCnt;
        private ulong inst;
        private bool T20PAG;
        private bool pagerTc;
        private int its2Pr;

        public SimhPDP10CPU(UserModeCore mainCore, OSTypes osType)
        {
            CORE = mainCore;
            AC = new Accumulators(CORE);
            RunOS = osType;
            CurrentSegment = 0;
        }

        public InstructionExit ProcessorMainloop()
        {
            var xct = false;

            while (true)
            {
                if (!xct) // Don't overwrite inst if XCT
                {
                    inst = read(pagerPC = PC, MmCur); /* get instruction */
                    incpc();

                    xctCnt = 0; // Not recursing
                }
                xct = false;

                var sts = cpuXct();
                switch (sts)
                {
                    case InstructionExit.MUUO:
                    case InstructionExit.Normal:
                        break; // Go do next
                    case InstructionExit.XCT:
                        xct = true;
                        break;
                    default:
                        return sts;
                }

                if (xct) continue;

                if (Runmode == RunModes.SingleStep)
                    return InstructionExit.SingleStep;

                if (breakPoints.ContainsKey(PC) &&
                    breakPoints[PC].DoIPause(CORE, PC))
                    return InstructionExit.BreakPoint;
            }
        }

        private InstructionExit cpuXct()
        {
            InstructionsExecuted++;

            var op = getOp(inst); /* get opcode */
            var ac = getAC(inst); /* get AC */

            bool loop;
            Im = calcEffective(inst, out loop);
            if (loop)
                return InstructionExit.RegisterIndirectionLimitExceeded;

            if (EffectiveAddressCalculated != null)
                EffectiveAddressCalculated(this, new EffectiveAddressCalculatedEvent(Im));

            var ex = executeInstruction(op, ac);
            if (ex == InstructionExit.MUUO)
                ex = procMUUO(op, ac);

            return ex;
        }

        private ulong calcEffective(ulong instr, out bool loop)
        {
            var ind = 0;
            ulong calEA = 0;
            for (var indrct = instr; ind < IndMax; ind++)
            {
                /* calc eff addr */
                calEA = indrct.RWD();
                var xr = getXr(indrct);
                if (xr != 0)
                    calEA = (calEA + (xrIdxReg(xr, MmEA))) & B36.AMASK;
                if (tstInd(indrct))
                    indrct = read(calEA, MmEA);
                else break;
            }
            loop = (ind >= IndMax);
            return calEA;
        }

        private InstructionExit executeInstruction(int op, int ac)
        {
            var opcode = (OpCodes) op;

            if (uuoHandlers[op] != null)
                return uuoHandlers[op](this, inst, opcode, ac, Im);

            switch (opcode)
            {
                    // case on opcode 

                    #region UUO's (0000 - 0077) - checked against KS10 ucode

                case OpCodes.MUUO0:
                    return InstructionExit.IllegalInstructionZero;
                case OpCodes.LUUO1: // local UUO's 
                case OpCodes.LUUO2:
                case OpCodes.LUUO3:
                case OpCodes.LUUO4:
                case OpCodes.LUUO5:
                case OpCodes.LUUO6:
                case OpCodes.LUUO7:
                case OpCodes.LUUO10:
                case OpCodes.LUUO11:
                case OpCodes.LUUO12:
                case OpCodes.LUUO13:
                case OpCodes.LUUO14:
                case OpCodes.LUUO15:
                case OpCodes.LUUO16:
                case OpCodes.LUUO17:
                case OpCodes.LUUO20:
                case OpCodes.LUUO21:
                case OpCodes.LUUO22:
                case OpCodes.LUUO23:
                case OpCodes.LUUO24:
                case OpCodes.LUUO25:
                case OpCodes.LUUO26:
                case OpCodes.LUUO27:
                case OpCodes.LUUO30:
                case OpCodes.LUUO31:
                case OpCodes.LUUO32:
                case OpCodes.LUUO33:
                case OpCodes.LUUO34:
                case OpCodes.LUUO35:
                case OpCodes.LUUO36:
                case OpCodes.LUUO37:
                    write(B36.OctUL(040), uuoword(op, ac), MmCur); // store op, ac, ea 

                    inst = read(B36.OctUL(041), MmCur);
                    return InstructionExit.XCT;

                    // case X0040 - 0077: MUUO's, handled by default at end of case 

                    #endregion

                    #region Floating point, bytes, multiple precision (0100 - 0177)

                    // case X0100:   MUUO                                    // UJEN 
                    // case X0101:   MUUO                                    // unassigned 
                case OpCodes.X0102:
                    if ((RunOS == OSTypes.ITS) && !IsUsr)
                    {
                        // GFAD (KL), XCTRI (ITS) 
                        inst = readEA();
                        pxctFlags |= ((uint) ac);
                        return InstructionExit.XCT;
                    }
                    return InstructionExit.MUUO;
                case OpCodes.X0103:
                    if ((RunOS == OSTypes.ITS) && !IsUsr)
                    {
                        // GFSB (KL), XCTR (ITS) 
                        inst = readEA();
                        pxctFlags |= ((uint) ac);
                        return InstructionExit.XCT;
                    }
                    return InstructionExit.MUUO;
                    // case X0104:   MUUO                                    // JSYS (T20) 
                case OpCodes.ADJSP:
                    AC[ac] = adjsp(AC[ac], Im);
                    break; // ADJSP 
                    // case X0106:   MUUO                                    // GFMP (KL)
                    // case X0107:   MUUO                                    // GFDV (KL) 
                case OpCodes.DFAD:
                    dwrs[0] = readEA();
                    dfad(ac, dwrs, 0);
                    break; // DFAD 
                case OpCodes.DFSB:
                    dwrs[0] = readEA();
                    dfad(ac, dwrs, 1);
                    break; // DFSB 
                case OpCodes.DFMP:
                    dwrs[0] = readEA();
                    dfmp(ac, dwrs);
                    break; // DFMP 
                case OpCodes.DFDV:
                    dwrs[0] = readEA();
                    dfdv(ac, dwrs);
                    break; // DFDV 
                case OpCodes.DADD:
                    dwrs[0] = readEA();
                    dadd(ac, dwrs);
                    break; // DADD 
                case OpCodes.DSUB:
                    dwrs[0] = readEA();
                    dsub(ac, dwrs);
                    break; // DSUB 
                case OpCodes.DMUL:
                    dwrs[0] = readEA();
                    dmul(ac, dwrs);
                    break; // DMUL 
                case OpCodes.DDIV:
                    dwrs[0] = readEA();
                    ddiv(ac, dwrs);
                    break; // DDIV 
                case OpCodes.DMOVE:
                    dwrs[0] = readEA();
                    s2AC(ac);
                    break; // DMOVE 
                case OpCodes.DMOVN:
                    dwrs[0] = readEA();
                    dmovn(dwrs);
                    s2AC(ac);
                    dmovnf();
                    break; // DMOVN 
                case OpCodes.FIX:
                    mb = readEA();
                    fix(ac, mb, false);
                    break; // FIX 
                case OpCodes.EXTEND:
                    var st = xtend(ac, Im, pxctFlags); // EXTEND 
                    rlog = false; // clear log 
                    switch (st)
                    {
                        case EXtendCodes.XtSkip:
                            incpc();
                            break;
                        case EXtendCodes.XtNosk:
                            break;
                        default:
                            return InstructionExit.MUUO;
                    }
                    break;
                case OpCodes.DMOVEM:
                    g2AC(ac);
                    wr2();
                    break; // DMOVEM 
                case OpCodes.DMOVNM:
                    g2AC(ac);
                    dmovn(dwrs);
                    wr2();
                    dmovnf();
                    break; // DMOVNM 
                case OpCodes.FIXR:
                    mb = readEA();
                    fix(ac, mb, true);
                    break; // FIXR 
                case OpCodes.FLTR:
                    mb = readEA();
                    AC[ac] = fltr(mb);
                    break; // FLTR 
                    // case X0130:   MUUO                                    // UFA 
                    // case X0131:   MUUO                                    // DFN 
                case OpCodes.FSC:
                    AC[ac] = fsc(AC[ac], Im);
                    break; // FSC 
                case OpCodes.IBP:
                    if (ac == 0)
                        ibp(Im, pxctFlags); // IBP 
                    else
                        adjbp(ac, Im, pxctFlags);
                    break;
                case OpCodes.ILBP:
                    cibp();
                    LDB(ac);
                    clrf(_fFpd);
                    break; // ILBP 
                case OpCodes.LDB:
                    LDB(ac);
                    break; // LDB 
                case OpCodes.IDBP:
                    cibp();
                    DPB(ac);
                    clrf(_fFpd);
                    break; // IDBP 
                case OpCodes.DPB:
                    DPB(ac);
                    break; // DPB 
                case OpCodes.FAD:
                    mb = readEA();
                    AC[ac] = FAD(ac, mb);
                    break; // FAD 
                    // case X0141:   MUUO                                    // FADL 
                case OpCodes.FADM:
                    mb = readMea();
                    mb = FAD(ac, mb);
                    WriteEA(mb);
                    break; // FADM 
                case OpCodes.FADB:
                    mb = readMea();
                    AC[ac] = FAD(ac, mb);
                    WriteEA(AC[ac]);
                    break; // FADB 
                case OpCodes.FADR:
                    mb = readEA();
                    AC[ac] = fadr(ac, mb);
                    break; // FADR 
                case OpCodes.FADRI:
                    AC[ac] = fadr(ac, Ims);
                    break; // FADRI 
                case OpCodes.FADRM:
                    mb = readMea();
                    mb = fadr(ac, mb);
                    WriteEA(mb);
                    break; // FADRM 
                case OpCodes.FADRB:
                    mb = readMea();
                    AC[ac] = fadr(ac, mb);
                    WriteEA(AC[ac]);
                    break; // FADRB 
                case OpCodes.FSB:
                    mb = readEA();
                    AC[ac] = fsb(ac, mb);
                    break; // FSB 
                    // case X0151:   MUUO                                    // FSBL 
                case OpCodes.FSBM:
                    mb = readMea();
                    mb = fsb(ac, mb);
                    WriteEA(mb);
                    break; // FSBM 
                case OpCodes.FSBB:
                    mb = readMea();
                    AC[ac] = fsb(ac, mb);
                    WriteEA(AC[ac]);
                    break; // FSBB 
                case OpCodes.FSBR:
                    mb = readEA();
                    AC[ac] = fsbr(ac, mb);
                    break; // FSBR 
                case OpCodes.FSBRI:
                    AC[ac] = fsbr(ac, Ims);
                    break; // FSBRI 
                case OpCodes.FSBRM:
                    mb = readMea();
                    mb = fsbr(ac, mb);
                    WriteEA(mb);
                    break; // FSBRM 
                case OpCodes.FSBRB:
                    mb = readMea();
                    AC[ac] = fsbr(ac, mb);
                    WriteEA(AC[ac]);
                    break; // FSBRB 
                case OpCodes.FMP:
                    mb = readEA();
                    AC[ac] = FMP(ac, mb);
                    break; // FMP 
                    // case X0161:   MUUO                                    // FMPL 
                case OpCodes.FMPM:
                    mb = readMea();
                    mb = FMP(ac, mb);
                    WriteEA(mb);
                    break; // FMPM 
                case OpCodes.FMPB:
                    mb = readMea();
                    AC[ac] = FMP(ac, mb);
                    WriteEA(AC[ac]);
                    break; // FMPB 
                case OpCodes.FMPR:
                    mb = readEA();
                    AC[ac] = fmpr(ac, mb);
                    break; // FMPR 
                case OpCodes.FMPRI:
                    AC[ac] = fmpr(ac, Ims);
                    break; // FMPRI 
                case OpCodes.FMPRM:
                    mb = readMea();
                    mb = fmpr(ac, mb);
                    WriteEA(mb);
                    break; // FMPRM 
                case OpCodes.FMPRB:
                    mb = readMea();
                    AC[ac] = fmpr(ac, mb);
                    WriteEA(AC[ac]);
                    break; // FMPRB 
                case OpCodes.FDV:
                    mb = readEA();
                    if (FDV(ac, mb))
                        s1AC(ac);
                    break; // FDV 
                    // case X0171:   MUUO                                    // FDVL 
                case OpCodes.FDVM:
                    mb = readMea();
                    if (FDV(ac, mb))
                        WriteEA(dwrs[0]);
                    break; // FDVM 
                case OpCodes.FDVB:
                    mb = readMea();
                    if (FDV(ac, mb))
                    {
                        s1AC(ac);
                        WriteEA(AC[ac]);
                    }
                    break; // FDVB 
                case OpCodes.FDVR:
                    mb = readEA();
                    if (fdvr(ac, mb))
                        s1AC(ac);
                    break; // FDVR 
                case OpCodes.FDVRI:
                    if (fdvr(ac, Ims))
                        s1AC(ac);
                    break; // FDVRI 
                case OpCodes.FDVRM:
                    mb = readMea();
                    if (fdvr(ac, mb))
                        WriteEA(dwrs[0]);
                    break; // FDVRM 
                case OpCodes.FDVRB:
                    mb = readMea();
                    if (fdvr(ac, mb))
                    {
                        s1AC(ac);
                        WriteEA(AC[ac]);
                    }
                    break; // FDVRB 

                    #endregion

                    #region Move, arithmetic, shift, and jump (0200 - 0277)

                    //   Note that instructions which modify the flags and store a
                    //  result in memory must prove the writeability of the result
                    //  location before modifying the flags.  Also, 0247 and 0257,
                    //  if not implemented, are nops, not MUUO's.

                case OpCodes.MOVE:
                    AC[ac] = readEA();
                    break; // MOVE 
                case OpCodes.MOVEI:
                    AC[ac] = Im;
                    break; // MOVEI 
                case OpCodes.MOVEM:
                    WriteEA(AC[ac]);
                    break; // MOVEM 
                case OpCodes.MOVES:
                    mb = readMea();
                    lac(ac);
                    break; // MOVES 
                case OpCodes.MOVS:
                    mb = readEA();
                    AC[ac] = swp(mb);
                    break; // MOVS 
                case OpCodes.MOVSI:
                    AC[ac] = Ims;
                    break; // MOVSI 
                case OpCodes.MOVSM:
                    mb = swp(AC[ac]);
                    WriteEA(mb);
                    break; // MOVSM 
                case OpCodes.MOVSS:
                    mb = readMea();
                    mb = swp(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // MOVSS 
                case OpCodes.MOVN:
                    mb = readEA();
                    AC[ac] = movn(mb);
                    break; // MOVN 
                case OpCodes.MOVNI:
                    AC[ac] = neg(Im); // MOVNI 
                    if (AC[ac] == 0) setf(_fC0 | _fC1);
                    break;
                case OpCodes.MOVNM:
                    mb = readMea();
                    mb = movn(AC[ac]);
                    WriteEA(mb);
                    break; // MOVNM 
                case OpCodes.MOVNS:
                    mb = readMea();
                    mb = movn(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // MOVNS 
                case OpCodes.MOVM:
                    mb = readEA();
                    AC[ac] = movm(mb);
                    break; // MOVM 
                case OpCodes.MOVMI:
                    AC[ac] = Im;
                    break; // MOVMI 
                case OpCodes.MOVMM:
                    mb = readMea();
                    mb = movm(AC[ac]);
                    WriteEA(mb);
                    break; // MOVMM 
                case OpCodes.MOVMS:
                    mb = readMea();
                    mb = movm(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // MOVMS 
                case OpCodes.IMUL:
                    mb = readEA();
                    AC[ac] = IMUL(ac, mb);
                    break; // IMUL 
                case OpCodes.IMULI:
                    AC[ac] = IMUL(ac, Im);
                    break; // IMULI 
                case OpCodes.IMULM:
                    mb = readMea();
                    mb = IMUL(ac, mb);
                    WriteEA(mb);
                    break; // IMULM 
                case OpCodes.IMULB:
                    mb = readMea();
                    AC[ac] = IMUL(ac, mb);
                    WriteEA(AC[ac]);
                    break; // IMULB 
                case OpCodes.MUL:
                    mb = readEA();
                    MUL(ac, mb);
                    s2AC(ac);
                    break; // MUL 
                case OpCodes.MULI:
                    MUL(ac, Im);
                    s2AC(ac);
                    break; // MULI 
                case OpCodes.MULM:
                    mb = readMea();
                    MUL(ac, mb);
                    WriteEA(dwrs[0]);
                    break; // MULM 
                case OpCodes.MULB:
                    mb = readMea();
                    MUL(ac, mb);
                    WriteEA(dwrs[0]);
                    s2AC(ac);
                    break; // MULB 
                case OpCodes.IDIV:
                    mb = readEA();
                    if (IDIV(ac, mb))
                        s2AC(ac);
                    break; // IDIV 
                case OpCodes.IDIVI:
                    if (IDIV(ac, Im))
                        s2AC(ac);
                    break; // IDIVI 
                case OpCodes.IDIVM:
                    mb = readMea();
                    if (IDIV(ac, mb))
                        WriteEA(dwrs[0]);
                    break; // IDIVM 
                case OpCodes.IDIVB:
                    mb = readMea();
                    if (IDIV(ac, mb))
                    {
                        WriteEA(dwrs[0]);
                        s2AC(ac);
                    }
                    break; // IDIVB 
                case OpCodes.DIV:
                    mb = readEA();
                    if (div(ac, mb))
                        s2AC(ac);
                    break; // DIV 
                case OpCodes.DIVI:
                    if (div(ac, Im))
                        s2AC(ac);
                    break; // DIVI 
                case OpCodes.DIVM:
                    mb = readMea();
                    if (div(ac, mb))
                        WriteEA(dwrs[0]);
                    break; // DIVM 
                case OpCodes.DIVB:
                    mb = readMea();
                    if (div(ac, mb))
                    {
                        WriteEA(dwrs[0]);
                        s2AC(ac);
                    }
                    break; // DIVB 
                case OpCodes.ASH:
                    AC[ac] = ash(AC[ac], Im);
                    break; // ASH 
                case OpCodes.ROT:
                    AC[ac] = rot(AC[ac], Im);
                    break; // ROT 
                case OpCodes.LSH:
                    AC[ac] = lsh(AC[ac], Im);
                    break; // LSH 
                case OpCodes.JFFO:
                    AC[addac(ac, 1)] = (ulong) jffo(AC[ac]); // JFFO 
                    if (AC[ac] != 0)
                        jump(Im);
                    break;
                case OpCodes.ASHC:
                    ashc(ac, Im);
                    break; // ASHC 
                case OpCodes.ROTC:
                    rotc(ac, Im);
                    break; // ROTC 
                case OpCodes.LSHC:
                    lshc(ac, Im);
                    break; // LSHC 
                case OpCodes.CIRC:
                    if ((RunOS == OSTypes.ITS)) circ(ac, (int) Im);
                    break; // (ITS) CIRC 
                case OpCodes.EXCH:
                    mb = readMea();
                    WriteEA(AC[ac]);
                    AC[ac] = mb;
                    break; // EXCH 
                case OpCodes.BLT:
                    blt(ac, Im, pxctFlags);
                    break; // BLT 
                case OpCodes.AOBJP:
                    aobac(ac);
                    if (tge(AC[ac]))
                        jump(Im);
                    break; // AOBJP 
                case OpCodes.AOBJN:
                    aobac(ac);
                    if (tl(AC[ac]))
                        jump(Im);
                    break; // AOBJN 
                case OpCodes.JFCL:
                    var cm = ((ulong) ac) << 14;
                    if ((ProcFlags & cm).NZ())
                    {
                        // JFCL 
                        jump(Im);
                        clrf(cm);
                    }
                    break;
                case OpCodes.XCT:
                    if (xctCnt++ >= XctMax) // XCT 
                        return InstructionExit.XCTMaxDepthExceeded;
                    inst = readEA();
                    if ((ac != 0) && !IsUsr && RunOS != OSTypes.ITS)
                        pxctFlags |= (uint) ac;
                    return InstructionExit.XCT;
                case OpCodes.MAP:
                    if ((RunOS == OSTypes.ITS))
                        return InstructionExit.MUUO; // MAP 
                    AC[ac] = map(Im, MmOpnd);
                    break;
                case OpCodes.PUSHJ:
                    wrp(ac, Flpc);
                    aobac(ac); // PUSHJ 
                    subj(Im);
                    pushf(ac);
                    break;
                case OpCodes.PUSH:
                    mb = readEA();
                    wrp(ac, mb);
                    aobac(ac);
                    pushf(ac);
                    break; // PUSH 
                case OpCodes.POP:
                    rdp(ac);
                    WriteEA(mb);
                    sobac(ac);
                    popf(ac);
                    break; // POP 
                case OpCodes.POPJ:
                    rdp(ac);
                    jump(mb);
                    sobac(ac);
                    popf(ac);
                    break; // POPJ 
                case OpCodes.JSR:
                    write(Im, Flpc, MmOpnd); // JSR 
                    subj(incr(Im));
                    break;
                case OpCodes.JSP:
                    AC[ac] = Flpc;
                    subj(Im);
                    break; // JSP 
                case OpCodes.JSA:
                    WriteEA(AC[ac]);
                    AC[ac] = xwd(Im, PC); // JSA 
                    jump(incr(Im));
                    break;
                case OpCodes.JRA:
                    AC[ac] = read(lrz(AC[ac]), MmOpnd); // JRA 
                    jump(Im);
                    break;
                case OpCodes.ADD:
                    mb = readEA();
                    AC[ac] = ADD(ac, mb);
                    break; // ADD 
                case OpCodes.ADDI:
                    AC[ac] = ADD(ac, Im);
                    break; // ADDI 
                case OpCodes.ADDM:
                    mb = readMea();
                    mb = ADD(ac, mb);
                    WriteEA(mb);
                    break; // ADDM 
                case OpCodes.ADDB:
                    mb = readMea();
                    AC[ac] = ADD(ac, mb);
                    WriteEA(AC[ac]);
                    break; // ADDB 
                case OpCodes.SUB:
                    mb = readEA();
                    AC[ac] = SUB(ac, mb);
                    break; // SUB 
                case OpCodes.SUBI:
                    AC[ac] = SUB(ac, Im);
                    break; // SUBI 
                case OpCodes.SUBM:
                    mb = readMea();
                    mb = SUB(ac, mb);
                    WriteEA(mb);
                    break; // SUBM 
                case OpCodes.SUBB:
                    mb = readMea();
                    AC[ac] = SUB(ac, mb);
                    WriteEA(AC[ac]);
                    break; // SUBB 

                    #endregion

                    #region  Compare, jump, skip instructions (0300 - 0377) - checked against KS10 ucode

                case OpCodes.CAI:
                    break; // CAI 
                case OpCodes.CAIL:
                    if (cl(ac, Im))
                        incpc();
                    break; // CAIL 
                case OpCodes.CAIE:
                    if (ce(ac, Im))
                        incpc();
                    break; // CAIE 
                case OpCodes.CAILE:
                    if (cle(ac, Im))
                        incpc();
                    break; // CAILE 
                case OpCodes.CAIA:
                    incpc();
                    break; // CAIA 
                case OpCodes.CAIGE:
                    if (cge(ac, Im))
                        incpc();
                    break; // CAIGE 
                case OpCodes.CAIN:
                    if (cn(ac, Im))
                        incpc();
                    break; // CAIN 
                case OpCodes.CAIG:
                    if (cg(ac, Im))
                        incpc();
                    break; // CAIG 
                case OpCodes.CAM:
                    mb = readEA();
                    break; // CAM 
                case OpCodes.CAML:
                    mb = readEA();
                    if (cl(ac, mb))
                        incpc();
                    break; // CAML 
                case OpCodes.CAME:
                    mb = readEA();
                    if (ce(ac, mb))
                        incpc();
                    break; // CAME 
                case OpCodes.CAMLE:
                    mb = readEA();
                    if (cle(ac, mb))
                        incpc();
                    break; // CAMLE 
                case OpCodes.CAMA:
                    mb = readEA();
                    incpc();
                    break; // CAMA 
                case OpCodes.CAMGE:
                    mb = readEA();
                    if (cge(ac, mb))
                        incpc();
                    break; // CAMGE 
                case OpCodes.CAMN:
                    mb = readEA();
                    if (cn(ac, mb))
                        incpc();
                    break; // CAMN 
                case OpCodes.CAMG:
                    mb = readEA();
                    if (cg(ac, mb))
                        incpc();
                    break; // CAMG 
                case OpCodes.JUMP:
                    break; // JUMP 
                case OpCodes.JUMPL:
                    if (tl(AC[ac]))
                        jump(Im);
                    break; // JUMPL 
                case OpCodes.JUMPE:
                    if (te(AC[ac]))
                        jump(Im);
                    break; // JUMPE 
                case OpCodes.JUMPLE:
                    if (tle(AC[ac]))
                        jump(Im);
                    break; // JUMPLE 
                case OpCodes.JUMPA:
                    jump(Im);
                    break; // JUMPA 
                case OpCodes.JUMPGE:
                    if (tge(AC[ac]))
                        jump(Im);
                    break; // JUMPGE 
                case OpCodes.JUMPN:
                    if (tn(AC[ac]))
                        jump(Im);
                    break; // JUMPN 
                case OpCodes.JUMPG:
                    if (tg(AC[ac]))
                        jump(Im);
                    break; // JUMPG 
                case OpCodes.SKIP:
                    mb = readEA();
                    lac(ac);
                    break; // SKIP 
                case OpCodes.SKIPL:
                    mb = readEA();
                    lac(ac);
                    if (tl(mb))
                        incpc();
                    break; // SKIPL 
                case OpCodes.SKIPE:
                    mb = readEA();
                    lac(ac);
                    if (te(mb))

                        incpc();
                    break; // SKIPE 
                case OpCodes.SKIPLE:
                    mb = readEA();
                    lac(ac);
                    if (tle(mb))
                        incpc();
                    break; // SKIPLE 
                case OpCodes.SKIPA:
                    mb = readEA();
                    lac(ac);
                    incpc();
                    break; // SKIPA 
                case OpCodes.SKIPGE:
                    mb = readEA();
                    lac(ac);
                    if (tge(mb)) incpc();
                    break; // SKIPGE 
                case OpCodes.SKIPN:
                    mb = readEA();
                    lac(ac);
                    if (tn(mb))
                        incpc();
                    break; // SKIPN 
                case OpCodes.SKIPG:
                    mb = readEA();
                    lac(ac);
                    if (tg(mb))
                        incpc();
                    break; // SKIPG 
                case OpCodes.AOJ:
                    aoj(ac);
                    break; // AOJ 
                case OpCodes.AOJL:
                    aoj(ac);
                    if (tl(AC[ac])) jump(Im);
                    break; // AOJL 
                case OpCodes.AOJE:
                    aoj(ac);
                    if (te(AC[ac])) jump(Im);
                    break; // AOJE 
                case OpCodes.AOJLE:
                    aoj(ac);
                    if (tle(AC[ac])) jump(Im);
                    break; // AOJLE 
                case OpCodes.AOJA:
                    aoj(ac);
                    jump(Im); // AOJA 
                    //if ((RunOS==OSTypes.ITS) && Q_IDLE && // ITS idle? 
                    //    isUSR && (pager_PC == 017) && // user mode, loc 17? 
                    //    (ac == 0) && (ea == 017)) // AOJA 0,17? 
                    //    sim_idle(0, false);
                    break;
                case OpCodes.AOJGE:
                    aoj(ac);
                    if (tge(AC[ac])) jump(Im);
                    break; // AOJGE 
                case OpCodes.AOJN:
                    aoj(ac);
                    if (tn(AC[ac])) jump(Im);
                    break; // AOJN 
                case OpCodes.AOJG:
                    aoj(ac);
                    if (tg(AC[ac])) jump(Im);
                    break; // AOJG 
                case OpCodes.AOS:
                    aos(ac);
                    break; // AOS 
                case OpCodes.AOSL:
                    aos(ac);
                    if (tl(mb)) incpc();
                    break; // AOSL 
                case OpCodes.AOSE:
                    aos(ac);
                    if (te(mb)) incpc();
                    break; // AOSE 
                case OpCodes.AOSLE:
                    aos(ac);
                    if (tle(mb)) incpc();
                    break; // AOSLE 
                case OpCodes.AOSA:
                    aos(ac);
                    incpc();
                    break; // AOSA 
                case OpCodes.AOSGE:
                    aos(ac);
                    if (tge(mb)) incpc();
                    break; // AOSGE 
                case OpCodes.AOSN:
                    aos(ac);
                    if (tn(mb)) incpc();
                    break; // AOSN 
                case OpCodes.AOSG:
                    aos(ac);
                    if (tg(mb)) incpc();
                    break; // AOSG 
                case OpCodes.SOJ:
                    soj(ac);
                    break; // SOJ 
                case OpCodes.SOJL:
                    soj(ac);
                    if (tl(AC[ac])) jump(Im);
                    break; // SOJL 
                case OpCodes.SOJE:
                    soj(ac);
                    if (te(AC[ac])) jump(Im);
                    break; // SOJE 
                case OpCodes.SOJLE:
                    soj(ac);
                    if (tle(AC[ac])) jump(Im);
                    break; // SOJLE 
                case OpCodes.SOJA:
                    soj(ac);
                    jump(Im);
                    break; // SOJA 
                case OpCodes.SOJGE:
                    soj(ac);
                    if (tge(AC[ac])) jump(Im);
                    break; // SOJGE 
                case OpCodes.SOJN:
                    soj(ac);
                    if (tn(AC[ac])) jump(Im);
                    break; // SOJN 
                case OpCodes.SOJG:
                    soj(ac);
                    if (tg(AC[ac])) jump(Im); // SOJG 
                    //if ((ea == pager_PC) && Q_IDLE)
                    //{
                    //    // to self, idle enab? 

                    //    int tmr_poll;
                    //    if ((ac == 6) && (ea == 1) && // SOJG 6,1? 
                    //        isUSR && Q_T10) // T10, user mode? 
                    //        sim_idle(0, false);
                    //    else
                    //        if (!t20_idlelock && // interlock off? 
                    //            (ac == 2) && (ea == 3) && // SOJG 2,3? 
                    //            !isUSR && Q_T20 && // T20, mon mode? 
                    //            (sim_interval > (tmr_poll >> 1)))
                    //        {
                    //            // >= half clock? 
                    //            t20_idlelock = 1; // set interlock 
                    //            if (sim_os_ms_sleep(1)) // sleep 1ms 
                    //                sim_interval = 0; // if ok, sched event 
                    //        }
                    //}
                    break;
                case OpCodes.SOS:
                    sos(ac);
                    break; // SOS 
                case OpCodes.SOSL:
                    sos(ac);
                    if (tl(mb)) incpc();
                    break; // SOSL 
                case OpCodes.SOSE:
                    sos(ac);
                    if (te(mb)) incpc();
                    break; // SOSE 
                case OpCodes.SOSLE:
                    sos(ac);
                    if (tle(mb)) incpc();
                    break; // SOSLE 
                case OpCodes.SOSA:
                    sos(ac);
                    incpc();
                    break; // SOSA 
                case OpCodes.SOSGE:
                    sos(ac);
                    if (tge(mb)) incpc();
                    break; // SOSGE 
                case OpCodes.SOSN:
                    sos(ac);
                    if (tn(mb)) incpc();
                    break; // SOSN 
                case OpCodes.SOSG:
                    sos(ac);
                    if (tg(mb)) incpc();
                    break; // SOSG 

                    #endregion

                    #region Boolean instructions (0400 - 0477) - checked against KS10 ucode

                    //Note that for boolean B, the initial read checks writeability of
                    //the memory operand; hence, it is safe to modify the AC.

                case OpCodes.SETZ:
                    AC[ac] = 0;
                    break; // SETZ 
                case OpCodes.SETZI:
                    AC[ac] = 0;
                    break; // SETZI 
                case OpCodes.SETZM:
                    mb = 0;
                    WriteEA(mb);
                    break; // SETZM 
                case OpCodes.SETZB:
                    mb = 0;
                    WriteEA(mb);
                    AC[ac] = 0;
                    break; // SETZB 
                case OpCodes.AND:
                    mb = readEA();
                    AC[ac] = and(ac, mb);
                    break; // AND 
                case OpCodes.ANDI:
                    AC[ac] = and(ac, Im);
                    break; // ANDI 
                case OpCodes.ANDM:
                    mb = readMea();
                    mb = and(ac, mb);
                    WriteEA(mb);
                    break; // ANDM 
                case OpCodes.ANDB:
                    mb = readMea();
                    AC[ac] = and(ac, mb);
                    WriteEA(AC[ac]);
                    break; // ANDB 
                case OpCodes.ANDCA:
                    mb = readEA();
                    AC[ac] = andca(ac, mb);
                    break; // ANDCA 
                case OpCodes.ANDCAI:
                    AC[ac] = andca(ac, Im);
                    break; // ANDCAI 
                case OpCodes.ANDCAM:
                    mb = readMea();
                    mb = andca(ac, mb);
                    WriteEA(mb);
                    break; // ANDCAM 
                case OpCodes.ANDCAB:
                    mb = readMea();
                    AC[ac] = andca(ac, mb);
                    WriteEA(AC[ac]);
                    break; // ANDCAB 
                case OpCodes.SETM:
                    AC[ac] = readEA();
                    break; // SETM 
                case OpCodes.SETMI:
                    AC[ac] = Im;
                    break; // SETMI 
                case OpCodes.SETMM:
                    mb = readMea();
                    WriteEA(mb);
                    break; // SETMM 
                case OpCodes.SETMB:
                    rmac(ac);
                    WriteEA(AC[ac]);
                    break; // SETMB 
                case OpCodes.ANDCM:
                    mb = readEA();
                    AC[ac] = andcm(ac, mb);
                    break; // ANDCM 
                case OpCodes.ANDCMI:
                    AC[ac] = andcm(ac, Im);
                    break; // ANDCMI 
                case OpCodes.ANDCMM:
                    mb = readMea();
                    mb = andcm(ac, mb);
                    WriteEA(mb);
                    break; // ANDCMM 
                case OpCodes.ANDCMB:
                    mb = readMea();
                    AC[ac] = andcm(ac, mb);
                    WriteEA(AC[ac]);
                    break; // ANDCMB 
                case OpCodes.SETA:
                    break; // SETA 
                case OpCodes.SETAI:
                    break; // SETAI 
                case OpCodes.SETAM:
                    WriteEA(AC[ac]);
                    break; // SETAM 
                case OpCodes.SETAB:
                    WriteEA(AC[ac]);
                    break; // SETAB 
                case OpCodes.XOR:
                    mb = readEA();
                    AC[ac] = xor(ac, mb);
                    break; // XOR 
                case OpCodes.XORI:
                    AC[ac] = xor(ac, Im);
                    break; // XORI 
                case OpCodes.XORM:
                    mb = readMea();
                    mb = xor(ac, mb);
                    WriteEA(mb);
                    break; // XORM 
                case OpCodes.XORB:
                    mb = readMea();
                    AC[ac] = xor(ac, mb);
                    WriteEA(AC[ac]);
                    break; // XORB 
                case OpCodes.IOR:
                    mb = readEA();
                    AC[ac] = ior(ac, mb);
                    break; // IOR 
                case OpCodes.IORI:
                    AC[ac] = ior(ac, Im);
                    break; // IORI 
                case OpCodes.IORM:
                    mb = readMea();
                    mb = ior(ac, mb);
                    WriteEA(mb);
                    break; // IORM 
                case OpCodes.IORB:
                    mb = readMea();
                    AC[ac] = ior(ac, mb);
                    WriteEA(AC[ac]);
                    break; // IORB 
                case OpCodes.ANDCB:
                    mb = readEA();
                    AC[ac] = andcb(ac, mb);
                    break; // ANDCB 
                case OpCodes.ANDCBI:
                    AC[ac] = andcb(ac, Im);
                    break; // ANDCBI 
                case OpCodes.ANDCBM:
                    mb = readMea();
                    mb = andcb(ac, mb);
                    WriteEA(mb);
                    break; // ANDCBM 
                case OpCodes.ANDCBB:
                    mb = readMea();
                    AC[ac] = andcb(ac, mb);
                    WriteEA(AC[ac]);
                    break; // ANDCBB 
                case OpCodes.EQV:
                    mb = readEA();
                    AC[ac] = eqv(ac, mb);
                    break; // EQV 
                case OpCodes.EQVI:
                    AC[ac] = eqv(ac, Im);
                    break; // EQVI 
                case OpCodes.EQVM:
                    mb = readMea();
                    mb = eqv(ac, mb);
                    WriteEA(mb);
                    break; // EQVM 
                case OpCodes.EQVB:
                    mb = readMea();
                    AC[ac] = eqv(ac, mb);
                    WriteEA(AC[ac]);
                    break; // EQVB 
                case OpCodes.SETCA:
                    mb = readEA();
                    AC[ac] = setca(ac, mb);
                    break; // SETCA 
                case OpCodes.SETCAI:
                    AC[ac] = setca(ac, Im);
                    break; // SETCAI 
                case OpCodes.SETCAM:
                    mb = readMea();
                    mb = setca(ac, mb);
                    WriteEA(mb);
                    break; // SETCAM 
                case OpCodes.SETCAB:
                    mb = readMea();
                    AC[ac] = setca(ac, mb);
                    WriteEA(AC[ac]);
                    break; // SETCAB 
                case OpCodes.ORCA:
                    mb = readEA();
                    AC[ac] = orca(ac, mb);
                    break; // ORCA 
                case OpCodes.ORCAI:
                    AC[ac] = orca(ac, Im);
                    break; // ORCAI 
                case OpCodes.ORCAM:
                    mb = readMea();
                    mb = orca(ac, mb);
                    WriteEA(mb);
                    break; // ORCAM 
                case OpCodes.ORCAB:
                    mb = readMea();
                    AC[ac] = orca(ac, mb);
                    WriteEA(AC[ac]);
                    break; // ORCAB 
                case OpCodes.SETCM:
                    mb = readEA();
                    AC[ac] = setcm(mb);
                    break; // SETCM 
                case OpCodes.SETCMI:
                    AC[ac] = setcm(Im);
                    break; // SETCMI 
                case OpCodes.SETCMM:
                    mb = readMea();
                    mb = setcm(mb);
                    WriteEA(mb);
                    break; // SETCMM 
                case OpCodes.SETCMB:
                    mb = readMea();
                    AC[ac] = setcm(mb);
                    WriteEA(AC[ac]);
                    break; // SETCMB 
                case OpCodes.ORCM:
                    mb = readEA();
                    AC[ac] = orcm(ac, mb);
                    break; // ORCM 
                case OpCodes.ORCMI:
                    AC[ac] = orcm(ac, Im);
                    break; // ORCMI 
                case OpCodes.ORCMM:
                    mb = readMea();
                    mb = orcm(ac, mb);
                    WriteEA(mb);
                    break; // ORCMM 
                case OpCodes.ORCMB:
                    mb = readMea();
                    AC[ac] = orcm(ac, mb);
                    WriteEA(AC[ac]);
                    break; // ORCMB 
                case OpCodes.ORCB:
                    mb = readEA();
                    AC[ac] = orcb(ac, mb);
                    break; // ORCB 
                case OpCodes.ORCBI:
                    AC[ac] = orcb(ac, Im);
                    break; // ORCBI 
                case OpCodes.ORCBM:
                    mb = readMea();
                    mb = orcb(ac, mb);
                    WriteEA(mb);
                    break; // ORCBM 
                case OpCodes.ORCBB:
                    mb = readMea();
                    AC[ac] = orcb(ac, mb);
                    WriteEA(AC[ac]);
                    break; // ORCBB 
                case OpCodes.SETO:
                    AC[ac] = B36.ONES;
                    break; // SETO 
                case OpCodes.SETOI:
                    AC[ac] = B36.ONES;
                    break; // SETOI 
                case OpCodes.SETOM:
                    mb = B36.ONES;
                    WriteEA(mb);
                    break; // SETOM 
                case OpCodes.SETOB:
                    mb = B36.ONES;
                    WriteEA(mb);
                    AC[ac] = B36.ONES;
                    break; // SETOB 

                    #endregion

                    #region Halfword instructions (0500 - 0577) - checked against KS10 ucode

                case OpCodes.HLL:
                    mb = readEA();
                    AC[ac] = ll(mb, AC[ac]);
                    break; // HLL 
                case OpCodes.HLLI:
                    AC[ac] = ll(Im, AC[ac]);
                    break; // HLLI 
                case OpCodes.HLLM:
                    mb = readMea();
                    mb = ll(AC[ac], mb);
                    WriteEA(mb);
                    break; // HLLM 
                case OpCodes.HLLS:
                    mb = readMea();
                    mb = ll(mb, mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HLLS 
                case OpCodes.HRL:
                    mb = readEA();
                    AC[ac] = rl(mb, AC[ac]);
                    break; // HRL 
                case OpCodes.HRLI:
                    AC[ac] = rl(Im, AC[ac]);
                    break; // HRLI 
                case OpCodes.HRLM:
                    mb = readMea();
                    mb = rl(AC[ac], mb);
                    WriteEA(mb);
                    break; // HRLM 
                case OpCodes.HRLS:
                    mb = readMea();
                    mb = rl(mb, mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HRLS 
                case OpCodes.HLLZ:
                    mb = readEA();
                    AC[ac] = llz(mb);
                    break; // HLLZ 
                case OpCodes.HLLZI:
                    AC[ac] = llz(Im);
                    break; // HLLZI 
                case OpCodes.HLLZM:
                    mb = llz(AC[ac]);
                    WriteEA(mb);
                    break; // HLLZM 
                case OpCodes.HLLZS:
                    mb = readMea();
                    mb = llz(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HLLZS 
                case OpCodes.HRLZ:
                    mb = readEA();
                    AC[ac] = rlz(mb);
                    break; // HRLZ 
                case OpCodes.HRLZI:
                    AC[ac] = rlz(Im);
                    break; // HRLZI 
                case OpCodes.HRLZM:
                    mb = rlz(AC[ac]);
                    WriteEA(mb);
                    break; // HRLZM 
                case OpCodes.HRLZS:
                    mb = readMea();
                    mb = rlz(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HRLZS 
                case OpCodes.HLLO:
                    mb = readEA();
                    AC[ac] = llo(mb);
                    break; // HLLO 
                case OpCodes.HLLOI:
                    AC[ac] = llo(Im);
                    break; // HLLOI 
                case OpCodes.HLLOM:
                    mb = llo(AC[ac]);
                    WriteEA(mb);
                    break; // HLLOM 
                case OpCodes.HLLOS:
                    mb = readMea();
                    mb = llo(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HLLOS 
                case OpCodes.HRLO:
                    mb = readEA();
                    AC[ac] = rlo(mb);
                    break; // HRLO 
                case OpCodes.HRLOI:
                    AC[ac] = rlo(Im);
                    break; // HRLOI 
                case OpCodes.HRLOM:
                    mb = rlo(AC[ac]);
                    WriteEA(mb);
                    break; // HRLOM 
                case OpCodes.HRLOS:
                    mb = readMea();
                    mb = rlo(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HRLOS 
                case OpCodes.HLLE:
                    mb = readEA();
                    AC[ac] = lle(mb);
                    break; // HLLE 
                case OpCodes.HLLEI:
                    AC[ac] = lle(Im);
                    break; // HLLEI 
                case OpCodes.HLLEM:
                    mb = lle(AC[ac]);
                    WriteEA(mb);
                    break; // HLLEM 
                case OpCodes.HLLES:
                    mb = readMea();
                    mb = lle(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HLLES 
                case OpCodes.HRLE:
                    mb = readEA();
                    AC[ac] = rle(mb);
                    break; // HRLE 
                case OpCodes.HRLEI:
                    AC[ac] = rle(Im);
                    break; // HRLEI 
                case OpCodes.HRLEM:
                    mb = rle(AC[ac]);
                    WriteEA(mb);
                    break; // HRLEM 
                case OpCodes.HRLES:
                    mb = readMea();
                    mb = rle(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HRLES 
                case OpCodes.HRR:
                    mb = readEA();
                    AC[ac] = rr(mb, AC[ac]);
                    break; // HRR 
                case OpCodes.HRRI:
                    AC[ac] = rr(Im, AC[ac]);
                    break; // HRRI 
                case OpCodes.HRRM:
                    mb = readMea();
                    mb = rr(AC[ac], mb);
                    WriteEA(mb);
                    break; // HRRM 
                case OpCodes.HRRS:
                    mb = readMea();
                    mb = rr(mb, mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HRRS 
                case OpCodes.HLR:
                    mb = readEA();
                    AC[ac] = lr(mb, AC[ac]);
                    break; // HLR 
                case OpCodes.HLRI:
                    AC[ac] = lr(Im, AC[ac]);
                    break; // HLRI 
                case OpCodes.HLRM:
                    mb = readMea();
                    mb = lr(AC[ac], mb);
                    WriteEA(mb);
                    break; // HLRM 
                case OpCodes.HLRS:
                    mb = readMea();
                    mb = lr(mb, mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HLRS 
                case OpCodes.HRRZ:
                    mb = readEA();
                    AC[ac] = rrz(mb);
                    break; // HRRZ 
                case OpCodes.HRRZI:
                    AC[ac] = rrz(Im);
                    break; // HRRZI 
                case OpCodes.HRRZM:
                    mb = rrz(AC[ac]);
                    WriteEA(mb);
                    break; // HRRZM 
                case OpCodes.HRRZS:
                    mb = readMea();
                    mb = rrz(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HRRZS 
                case OpCodes.HLRZ:
                    mb = readEA();
                    AC[ac] = lrz(mb);
                    break; // HLRZ 
                case OpCodes.HLRZI:
                    AC[ac] = lrz(Im);
                    break; // HLRZI 
                case OpCodes.HLRZM:
                    mb = lrz(AC[ac]);
                    WriteEA(mb);
                    break; // HLRZM 
                case OpCodes.HLRZS:
                    mb = readMea();
                    mb = lrz(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HLRZS 
                case OpCodes.HRRO:
                    mb = readEA();
                    AC[ac] = rro(mb);
                    break; // HRRO 
                case OpCodes.HRROI:
                    AC[ac] = rro(Im);
                    break; // HRROI 
                case OpCodes.HRROM:
                    mb = rro(AC[ac]);
                    WriteEA(mb);
                    break; // HRROM 
                case OpCodes.HRROS:
                    mb = readMea();
                    mb = rro(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HRROS 
                case OpCodes.HLRO:
                    mb = readEA();
                    AC[ac] = lro(mb);
                    break; // HLRO 
                case OpCodes.HLROI:
                    AC[ac] = lro(Im);
                    break; // HLROI 
                case OpCodes.HLROM:
                    mb = lro(AC[ac]);
                    WriteEA(mb);
                    break; // HLROM 
                case OpCodes.HLROS:
                    mb = readMea();
                    mb = lro(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HLROS 
                case OpCodes.HRRE:
                    mb = readEA();
                    AC[ac] = rre(mb);
                    break; // HRRE 
                case OpCodes.HRREI:
                    AC[ac] = rre(Im);
                    break; // HRREI 
                case OpCodes.HRREM:
                    mb = rre(AC[ac]);
                    WriteEA(mb);
                    break; // HRREM 
                case OpCodes.HRRES:
                    mb = readMea();
                    mb = rre(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HRRES 
                case OpCodes.HLRE:
                    mb = readEA();
                    AC[ac] = lre(mb);
                    break; // HLRE 
                case OpCodes.HLREI:
                    AC[ac] = lre(Im);
                    break; // HLREI 
                case OpCodes.HLREM:
                    mb = lre(AC[ac]);
                    WriteEA(mb);
                    break; // HLREM 
                case OpCodes.HLRES:
                    mb = readMea();
                    mb = lre(mb);
                    WriteEA(mb);
                    lac(ac);
                    break; // HLRES 

                    #endregion

                    #region Test instructions (0600 - 0677) - checked against KS10 ucode

                    // In the KS10 ucode, TDN and TSN do not fetch an operand; the Processor
                    // Reference Manual describes them as NOPs that reference memory.

                case OpCodes.TRN:
                    break; // TRN 
                case OpCodes.TLN:
                    break; // TLN 
                case OpCodes.TRNE:
                    tr();
                    T__E(ac);
                    break; // TRNE 
                case OpCodes.TLNE:
                    TL_();
                    T__E(ac);
                    break; // TLNE 
                case OpCodes.TRNA:
                    T__A();
                    break; // TRNA 
                case OpCodes.TLNA:
                    T__A();
                    break; // TLNA 
                case OpCodes.TRNN:
                    tr();
                    T__N(ac);
                    break; // TRNN 
                case OpCodes.TLNN:
                    TL_();
                    T__N(ac);
                    break; // TLNN 
                case OpCodes.TDN:
                    td();
                    break; // TDN 
                case OpCodes.TSN:
                    ts();
                    break; // TSN 
                case OpCodes.TDNE:
                    td();
                    T__E(ac);
                    break; // TDNE 
                case OpCodes.TSNE:
                    ts();
                    T__E(ac);
                    break; // TSNE 
                case OpCodes.TDNA:
                    td();
                    T__A();
                    break; // TDNA 
                case OpCodes.TSNA:
                    ts();
                    T__A();
                    break; // TSNA 
                case OpCodes.TDNN:
                    td();
                    T__N(ac);
                    break; // TDNN 
                case OpCodes.TSNN:
                    ts();
                    T__N(ac);
                    break; // TSNN 
                case OpCodes.TRZ:
                    tr();
                    T_Z(ac);
                    break; // TRZ 
                case OpCodes.TLZ:
                    TL_();
                    T_Z(ac);
                    break; // TLZ 
                case OpCodes.TRZE:
                    tr();
                    T__E(ac);
                    T_Z(ac);
                    break; // TRZE 
                case OpCodes.TLZE:
                    TL_();
                    T__E(ac);
                    T_Z(ac);
                    break; // TLZE 
                case OpCodes.TRZA:
                    tr();
                    T__A();
                    T_Z(ac);
                    break; // TRZA 
                case OpCodes.TLZA:
                    TL_();
                    T__A();
                    T_Z(ac);
                    break; // TLZA 
                case OpCodes.TRZN:
                    tr();
                    T__N(ac);
                    T_Z(ac);
                    break; // TRZN 
                case OpCodes.TLZN:
                    TL_();
                    T__N(ac);
                    T_Z(ac);
                    break; // TLZN 
                case OpCodes.TDZ:
                    td();
                    T_Z(ac);
                    break; // TDZ 
                case OpCodes.TSZ:
                    ts();
                    T_Z(ac);
                    break; // TSZ 
                case OpCodes.TDZE:
                    td();
                    T__E(ac);
                    T_Z(ac);
                    break; // TDZE 
                case OpCodes.TSZE:
                    ts();
                    T__E(ac);
                    T_Z(ac);
                    break; // TSZE 
                case OpCodes.TDZA:
                    td();
                    T__A();
                    T_Z(ac);
                    break; // TDZA 
                case OpCodes.TSZA:
                    ts();
                    T__A();
                    T_Z(ac);
                    break; // TSZA 
                case OpCodes.TDZN:
                    td();
                    T__N(ac);
                    T_Z(ac);
                    break; // TDZN 
                case OpCodes.TSZN:
                    ts();
                    T__N(ac);
                    T_Z(ac);
                    break; // TSZN 
                case OpCodes.TRC:
                    tr();
                    T_C(ac);
                    break; // TRC 
                case OpCodes.TLC:
                    TL_();
                    T_C(ac);
                    break; // TLC 
                case OpCodes.TRCE:
                    tr();
                    T__E(ac);
                    T_C(ac);
                    break; // TRCE 
                case OpCodes.TLCE:
                    TL_();
                    T__E(ac);
                    T_C(ac);
                    break; // TLCE 
                case OpCodes.TRCA:
                    tr();
                    T__A();
                    T_C(ac);
                    break; // TRCA 
                case OpCodes.TLCA:
                    TL_();
                    T__A();
                    T_C(ac);
                    break; // TLCA 
                case OpCodes.TRCN:
                    tr();
                    T__N(ac);
                    T_C(ac);
                    break; // TRCN 
                case OpCodes.TLCN:
                    TL_();
                    T__N(ac);
                    T_C(ac);
                    break; // TLCN 
                case OpCodes.TDC:
                    td();
                    T_C(ac);
                    break; // TDC 
                case OpCodes.TSC:
                    ts();
                    T_C(ac);
                    break; // TSC 
                case OpCodes.TDCE:
                    td();
                    T__E(ac);
                    T_C(ac);
                    break; // TDCE 
                case OpCodes.TSCE:
                    ts();
                    T__E(ac);
                    T_C(ac);
                    break; // TSCE 
                case OpCodes.TDCA:
                    td();
                    T__A();
                    T_C(ac);
                    break; // TDCA 
                case OpCodes.TSCA:
                    ts();
                    T__A();
                    T_C(ac);
                    break; // TSCA 
                case OpCodes.TDCN:
                    td();
                    T__N(ac);
                    T_C(ac);
                    break; // TDCN 
                case OpCodes.TSCN:
                    ts();
                    T__N(ac);
                    T_C(ac);
                    break; // TSCN 
                case OpCodes.TRO:
                    tr();
                    T_O(ac);
                    break; // TRO 
                case OpCodes.TLO:
                    TL_();
                    T_O(ac);
                    break; // TLO 
                case OpCodes.TROE:
                    tr();
                    T__E(ac);
                    T_O(ac);
                    break; // TROE 
                case OpCodes.TLOE:
                    TL_();
                    T__E(ac);
                    T_O(ac);
                    break; // TLOE 
                case OpCodes.TROA:
                    tr();
                    T__A();
                    T_O(ac);
                    break; // TROA 
                case OpCodes.TLOA:
                    TL_();
                    T__A();
                    T_O(ac);
                    break; // TLOA 
                case OpCodes.TRON:
                    tr();
                    T__N(ac);
                    T_O(ac);
                    break; // TRON 
                case OpCodes.TLON:
                    TL_();
                    T__N(ac);
                    T_O(ac);
                    break; // TLON 
                case OpCodes.TDO:
                    td();
                    T_O(ac);
                    break; // TDO 
                case OpCodes.TSO:
                    ts();
                    T_O(ac);
                    break; // TSO 
                case OpCodes.TDOE:
                    td();
                    T__E(ac);
                    T_O(ac);
                    break; // TDOE 
                case OpCodes.TSOE:
                    ts();
                    T__E(ac);
                    T_O(ac);
                    break; // TSOE 
                case OpCodes.TDOA:
                    td();
                    T__A();
                    T_O(ac);
                    break; // TDOA 
                case OpCodes.TSOA:
                    ts();
                    T__A();
                    T_O(ac);
                    break; // TSOA 
                case OpCodes.TDON:
                    td();
                    T__N(ac);
                    T_O(ac);
                    break; // TDON 
                case OpCodes.TSON:
                    ts();
                    T__N(ac);
                    T_O(ac);
                    break; // TSON 

                    #endregion

                    #region  I/O instructions (0700 - 0777)

                    // Only the defined I/O instructions have explicit case labels;
                    // /the rest default to unimplemented (monitor UUO).  Note that   
                    // 710-715 and 720-725 have different definitions under ITS and
                    // use normal effective addresses instead of the special address
                    // calculation required by TOPS-10 and TOPS-20.

                    //case X0700:
                    //    IO7(io700i, io700d);
                    //    break; // I/O 0 
                    //case X0701:
                    //    IO7(io701i, io701d);
                    //    break; // I/O 1 
                    //case X0702:
                    //    IO7(io702i, io702d);
                    //    break; // I/O 2 
                    //case X0704:
                    //    IOC;
                    //    AC[ac] = Read(ea, OPND_PXCT);
                    //    break; // UMOVE 
                    //case X0705:
                    //    IOC;
                    //    Write(ea, AC[ac], OPND_PXCT);
                    //    break; // UMOVEM 
                    //case X0710:
                    //    IOA;
                    //    if (io710(ac, ea)) INCPC();
                    //    break; // TIOE, IORDI 
                    //case X0711:
                    //    IOA;
                    //    if (io711(ac, ea)) INCPC();
                    //    break; // TION, IORDQ 
                    //case X0712:
                    //    IOAM;
                    //    AC[ac] = io712(ea);
                    //    break; // RDIO, IORD 
                    //case X0713:
                    //    IOAM;
                    //    io713(AC[ac], ea);
                    //    break; // WRIO, IOWR 
                    //case X0714:
                    //    IOA;
                    //    io714(AC[ac], ea);
                    //    break; // BSIO, IOWRI 
                    //case X0715:
                    //    IOA;
                    //    io715(AC[ac], ea);
                    //    break; // BCIO, IOWRQ 
                    //case X0716:
                    //    IOC;
                    //    bltu(ac, ea, pflgs, 0);
                    //    break; // BLTBU 
                    //case X0717:
                    //    IOC;
                    //    bltu(ac, ea, pflgs, 1);
                    //    break; // BLTUB 
                    //case X0720:
                    //    IOA;
                    //    if (io720(ac, ea)) INCPC();
                    //    break; // TIOEB, IORDBI 
                    //case X0721:
                    //    IOA;
                    //    if (io721(ac, ea)) INCPC();
                    //    break; // TIONB, IORDBQ 
                    //case X0722:
                    //    IOAM;
                    //    AC[ac] = io722(ea);
                    //    break; // RDIOB, IORDB 
                    //case X0723:
                    //    IOAM;
                    //    io723(AC[ac], ea);
                    //    break; // WRIOB, IOWRB 
                    //case X0724:
                    //    IOA;
                    //    io724(AC[ac], ea);
                    //    break; // BSIOB, IOWRBI 
                    //case X0725:
                    //    IOA;
                    //    io725(AC[ac], ea);
                    //    break; // BCIOB, IOWRBQ 

                    #endregion

                    #region JRST - checked against KS10 ucode

                    // Differences from the KS10: the KS10
                    // - (JRSTF, JEN) refetches the base instruction from PC - 1
                    // - (XJEN) dismisses interrupt before reading the new flags and PC
                    // - (XPCW) writes the old flags and PC before reading the new
                    // ITS microcode includes extended JRST's, although they are not used

                case OpCodes.JRST: // JRST 
                    return handleJRST(ac);

                    #endregion

                default:
                    return InstructionExit.MUUO;
            } // end instruction case op 

            return InstructionExit.Normal;
        }

        private InstructionExit handleJRST(int ac)
        {
            var m = _jrstTab[ac];

            if ((m == jrst_modes.None) ||
                ((m == jrst_modes.JRSTE) && IsUsr) ||
                ((m == jrst_modes.JRSTUio) && IsUsr && !IsUio))
                return InstructionExit.MUUO; // not legal 

            switch ((JRST) ac)
            {
                    // case on subopcode 

                case JRST.JRST: // JRST 0 = jump 
                case JRST.PORTAL: // JRST 1 = portal 
                    jump(Im);
                    break;

                case JRST.JRSTF: // JRST 2 = JRSTF 
                    mb = calcJrstfEA(inst, pxctFlags); // recalc addr w flgs 
                    jump(Im); // set new PC 
                    setNewflags(mb, true); // set new flags 
                    break;

                case JRST.HALT: // JRST 4 = halt 
                    jump(Im); // old_PC = halt + 1 
                    pagerPC = PC; // force right PC 

                    return InstructionExit.Halt;

                case JRST.XJRSTF: // JRST 5 = XJRSTF 
                    dwrs[0] = readEA(); // read doubleword 
                    jump(dwrs[1]); // set new PC 
                    setNewflags(dwrs[0], true); // set new flags 
                    break;

                case JRST.XJEN: // JRST 6 = XJEN 
                    dwrs[0] = readEA(); // read doubleword 
                    piDismiss(); // page ok, dismiss 
                    jump(dwrs[1]); // set new PC 
                    setNewflags(dwrs[0], false); // known to be exec 
                    break;

                case JRST.XPCW: // JRST 7 = XPCW 
                    ulong i;
                    Im = adda(i = Im, 2); // new flags, PC 
                    dwrs[0] = readEA(); // read, test page fail 
                    readM(inca(i)); // test PC write 
                    write(i, xwd(ProcFlags, 0), MmOpnd); // write flags 
                    write(inca(i), PC, MmOpnd); // write PC 
                    jump(dwrs[1]); // set new PC 
                    setNewflags(dwrs[0], false); // known to be exec 
                    break;

                case JRST.DISMISS: // JRST 10 = dismiss 
                    piDismiss(); // dismiss int 
                    jump(Im); // set new PC 
                    break;

                case JRST.JEN: // JRST 12 = JEN 
                    mb = calcJrstfEA(inst, pxctFlags); // recalc addr w flgs 
                    jump(Im); // set new PC 
                    setNewflags(mb, true); // set new flags 
                    piDismiss(); // dismiss int 
                    break;

                case JRST.SFM: // JRST 14 = SFM 
                    write(Im, xwd(ProcFlags, 0), MmOpnd);
                    break;

                case JRST.XJRST: // JRST 15 = XJRST 
                    if (!T20PAG) return InstructionExit.MUUO; // only in TOPS20 paging 
                    jump(readEA()); // jump to M[ea] 
                    break;
            } // end JRST case subop 

            return InstructionExit.Normal;
        }
    }
}