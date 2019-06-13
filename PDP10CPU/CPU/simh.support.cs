using System;
using System.Text;
using PDP10CPU.Enums;
using PDP10CPU.Exceptions;
using ThirtySixBits;

namespace PDP10CPU.CPU
{
    public partial class SimhPDP10CPU
    {
        //   Permission is hereby granted, free of charge, to any person obtaining a
        //   copy of this software and associated documentation files (the "Software"),
        //   to deal in the Software without restriction, including without limitation
        //   the rights to use, copy, modify, merge, publish, distribute, sublicense,
        //   and/or sell copies of the Software, and to permit persons to whom the
        //   Software is furnished to do so, subject to the following conditions:

        //   The above copyright notice and this permission notice shall be included in
        //   all copies or substantial portions of the Software.

        //   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        //   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        //   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL
        //   ROBERT M SUPNIK BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
        //   IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
        //   CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

        //   Except as contained in this notice, the name of Robert M Supnik shall not be
        //   used in advertising or otherwise to promote the sale, use or other dealings
        //   in this Software without prior written authorization from Robert M Supnik.

        // Architectural constants 

        //private const int PASIZE = 20; // phys addr width 
        //private const int MAXMEMSIZE = (1 << PASIZE); // maximum memory 
        //private const ulong P P36.AMASK = ((1 << PASIZE) - 1);
        //private const int MEMSIZE = MAXMEMSIZE; // fixed, KISS 

        //private static bool MEM_ADDR_NXM(int x)
        //{
        //    return ((x) >= MEMSIZE);
        //}

        private const int EAPxct = 7; // // eff addr calc 
        private const int OpndPxct = 004; // operand, bdst 
        private const int EabpPxct = 002; // bp eff addr calc 
        private const int BstkPxct = 001; // stk, bp op, bsrc 
        //private const int XSRC_PXCT = 002; // extend source 
        //private const int XDST_PXCT = 001; // extend destination 
        private const int MmCur = 000; // current context 

        private ulong MmEA
        {
            get { return (pxctFlags & EAPxct); }
        }

        private ulong MmOpnd
        {
            get { return (pxctFlags & OpndPxct); }
        }

        private ulong MmEabp
        {
            get { return (pxctFlags & EabpPxct); }
        }

        private ulong MmBstk
        {
            get { return (pxctFlags & BstkPxct); }
        }

        // Instruction format 

        private const int InstVOp = 27; // opcode 
        private static readonly ulong _instMOp = B36.OctUL(0777);
        //private const int INST_V_DEV = 26;
        //private static readonly ulong INST_M_DEV = B36.OctUL(0177); // device 
        private const int InstVAC = 23; // AC 
        private static readonly ulong _instMAC = B36.OctUL(017);
        private const int InstVInd = 22; // indirect 
        private static readonly ulong _instInd = InstVInd.Bit();
        private const int InstVXr = 18; // index 
        private static readonly ulong _instMXr = B36.OctUL(017);
        //private static readonly ulong OP_JRST = B36.OctUL(0254); // JRST 
        //private static readonly ulong AC_XPCW = B36.OctUL(07); // XPCW 
        //private static readonly ulong OP_JSR = B36.OctUL(0264); // JSR 

        private static int getOp(ulong x)
        {
            return ((int) (((x) >> InstVOp) & _instMOp));
        }

        //private int GET_DEV(ulong x)
        //{
        //    return ((int) (((x) >> INST_V_DEV) & INST_M_DEV));
        //}

        private static int getAC(ulong x)
        {
            return ((int) (((x) >> InstVAC) & _instMAC));
        }

        private static bool tstInd(ulong x)
        {
            return ((x) & _instInd).NZ();
        }

        private static int getXr(ulong x)
        {
            return ((int) (((x) >> InstVXr) & _instMXr));
        }

        // Byte pointer format 

        private const int BpVP = 30; // position 
        private static readonly ulong _bpMP = B36.OctUL(077);
        private static readonly ulong _bpP = B36.OctUL(0770000000000);
        private const int BpVS = 24; // size 
        private static readonly ulong _bpMS = B36.OctUL(077);
        private static readonly ulong _bpS = B36.OctUL(0007700000000);

        private static int getP(ulong x)
        {
            return ((int) (((x) >> BpVP) & _bpMP));
        }

        private static int getS(ulong x)
        {
            return ((int) (((x) >> BpVS) & _bpMS));
        }

        private static ulong putP(ulong b, ulong x)
        {
            return (((b) & ~_bpP) | ((((x)) & _bpMP) << BpVP));
        }

        private const int FVAov = 17; // arithmetic ovflo 
        private const int FVC0 = 16; // carry 0 
        private const int FVC1 = 15; // carry 1 
        private const int FVFov = 14; // floating ovflo 
        private const int FVFpd = 13; // first part done 
        private const int FVUsr = 12; // user mode 
        private const int FVUio = 11; // user I/O mode 
        private const int FVPub = 10; // private mode 
        private const int FVAfi = 9; // addr fail inhibit 
        private const int FVT2 = 8; // trap 2 
        private const int FVT1 = 7; // trap 1 
        private const int FVFxu = 6; // floating exp unflo 
        private const int FVDck = 5; // divide check 

        // Flags (stored in their own halfword) 

        private static readonly ulong _fAov = FVAov.Bit();
        private static readonly ulong _fC0 = FVC0.Bit();
        private static readonly ulong _fC1 = FVC1.Bit();
        private static readonly ulong _fFov = FVFov.Bit();
        private static readonly ulong _fFpd = FVFpd.Bit();
        private static readonly ulong _fUsr = FVUsr.Bit();
        private static readonly ulong _fUio = FVUio.Bit();
        private static readonly ulong _fPub = FVPub.Bit();
        private static readonly ulong _fAfi = FVAfi.Bit();
        private static readonly ulong _fT2 = FVT2.Bit();
        private static readonly ulong _fT1 = FVT1.Bit();
        private static readonly ulong _fTr = (_fT1 | _fT2);
        private static readonly ulong _fFxu = FVFxu.Bit();
        private static readonly ulong _fDck = FVDck.Bit();
        private static readonly ulong _f_1Pr = (_fAfi); // ITS: 1-proceed 
        private static readonly ulong _fMask = B36.OctUL(0777740); // all flags 

        // Arithmetic processor flags 

        //private static readonly ulong APR_SENB = P36.OctUL(0100000); // set enable 
        //private static readonly ulong APR_CENB = P36.OctUL(0040000); // clear enable 
        //private static readonly ulong APR_CFLG = P36.OctUL(0020000); // clear flag 
        //private static readonly ulong APR_SFLG = P36.OctUL(0010000); // set flag 
        //private static readonly ulong APR_IRQ = P36.OctUL(0000010); // int request 
        //private static readonly ulong APR_M_LVL = P36.OctUL(0000007); // pi level 
        //private const int APR_V_FLG = 4;
        //private static readonly ulong APR_M_FLG = P36.OctUL(0377);
        //private static readonly ulong APRF_ITC = (P36.OctUL(002000) >> APR_V_FLG); // int console flag 
        //private static readonly ulong APRF_NXM = (P36.OctUL(000400) >> APR_V_FLG); // nxm flag 
        //private static readonly ulong APRF_TIM = (P36.OctUL(000040) >> APR_V_FLG); // timer request 
        //private static readonly ulong APRF_CON = (P36.OctUL(000020) >> APR_V_FLG); // console int 

        //private static ulong APR_GETF(ulong x)
        //{
        //    return (((x) >> APR_V_FLG) & APR_M_FLG);
        //}

        // Priority interrupt system 

        //private static readonly ulong PI_CPRQ = P36.OctUL(020000); // drop prog req 
        //private static readonly ulong PI_INIT = P36.OctUL(010000); // clear pi system 
        //private static readonly ulong PI_SPRQ = P36.OctUL(004000); // set prog req 
        //private static readonly ulong PI_SENB = P36.OctUL(002000); // set enables 
        //private static readonly ulong PI_CENB = P36.OctUL(001000); // clear enables 
        //private static readonly ulong PI_CON = P36.OctUL(000400); // turn off pi system 
        //private static readonly ulong PI_SON = P36.OctUL(000200); // turn on pi system 
        //private static readonly ulong PI_M_LVL = P36.OctUL(000177); // level mask 
        //private const int PI_V_PRQ = 18;
        //private const int PI_V_ACT = 8;
        //private const int PI_V_ON = 7;
        //private const int PI_V_ENB = 0;

        // User base register 

        //private static readonly ulong UBR_SETACB = B36.OctUL(0400000000000); // set AC blocks 
        //private static readonly ulong UBR_SETUBR = B36.OctUL(0100000000000); // set UBR 
        //#define UBR_V_CURAC     27                              // current AC block 
        //#define UBR_V_PRVAC     24                              // previous AC block 
        //#define UBR_M_AC        07
        //#define UBR_ACBMASK     0007700000000
        //#define UBR_V_UBR       0                               // user base register 
        //#define UBR_N_UBR       11
        //#define UBR_M_UBR       03777
        //#define UBR_UB P36.RMASK     0000000003777
        //#define UBR_GETCURAC(x) ((int32) (((x) >> UBR_V_CURAC) & UBR_M_AC))
        //#define UBR_GETPRVAC(x) ((int32) (((x) >> UBR_V_PRVAC) & UBR_M_AC))
        //#define UBR_GETUBR(x)   ((int32) (((x) >> UBR_V_UBR) & PAG_M_PPN))
        //private ulong UBRWORD
        //{
        //    get { return (ubr | UBR_SETACB | UBR_SETUBR); }
        //}

        // User process table entries 

        //private static readonly ulong UPT_T10_UMAP = B36.OctUL(0000); // T10: user map 
        //private static readonly ulong UPT_T10_X340 = B36.OctUL(0400); // T10: exec 340-377 
        //private static readonly ulong UPT_TRBASE = B36.OctUL(0420); // trap base 
        //private static readonly ulong UPT_MUUO = B36.OctUL(0424); // MUUO block 
        //private static readonly ulong UPT_MUPC = B36.OctUL(0425); // caller's PC 
        //private static readonly ulong UPT_T10_CTX = B36.OctUL(0426); // T10: context 
        //private static readonly ulong UPT_T20_UEA = B36.OctUL(0426); // T20: address 
        //private static readonly ulong UPT_T20_CTX = B36.OctUL(0427); // T20: context 
        //private static readonly ulong UPT_ENPC = B36.OctUL(0430); // MUUO new PC, exec 
        //private static readonly ulong UPT_1PO = B36.OctUL(0432); // ITS 1-proc: old PC 
        //private static readonly ulong UPT_1PN = B36.OctUL(0433); // ITS 1-proc: new PC 
        //private static readonly ulong UPT_UNPC = B36.OctUL(0434); // MUUO new PC, user 
        //private const ulong UPT_NPCT = 1;
        //private static readonly ulong UPT_T10_PAG = B36.OctUL(0500); // T10: page fail blk 
        //private static readonly ulong UPT_T20_PFL = B36.OctUL(0500); // T20: page fail wd 
        //private static readonly ulong UPT_T20_OFL = B36.OctUL(0501); // T20: flags 
        //private static readonly ulong UPT_T20_OPC = B36.OctUL(0502); // T20: old PC 
        //private static readonly ulong UPT_T20_NPC = B36.OctUL(0503); // T20: new PC 
        //private static readonly ulong UPT_T20_SCTN = B36.OctUL(0540); // T20: section 0 ptr 

        // Exec process table entries 

        //private static readonly ulong EPT_PIIT = P36.OctUL(0040); // PI interrupt table 
        //private static readonly ulong EPT_UBIT = P36.OctUL(0100); // Unibus intr table 
        //private static readonly ulong EPT_T10_X400 = P36.OctUL(0200); // T10: exec 400-777 
        //private static readonly ulong EPT_TRBASE = P36.OctUL(0420); // trap base 
        //private static readonly ulong EPT_ITS_PAG = P36.OctUL(0440); // ITS: page fail blk 
        //private static readonly ulong EPT_T20_SCTN = P36.OctUL(0540); // T20: section 0 ptr 
        //private static readonly ulong EPT_T10_X000 = P36.OctUL(0600); // T10: exec 0 - 337 

        // Microcode constants 

        //private static readonly ulong UC_INHCST = P36.OctUL(0400000000000); // inhibit CST update 
        //private static readonly ulong UC_UBABLT = P36.OctUL(0040000000000); // BLTBU and BLTUB 
        //private static readonly ulong UC_KIPAGE = P36.OctUL(0020000000000); // "KI" paging 
        //private static readonly ulong UC_KLPAGE = P36.OctUL(0010000000000); // "KL" paging 
        //private static readonly ulong UC_VERDEC = (P36.OctUL(0130) << 18); // ucode version 
        //private const ulong UC_VERITS = (262u << 18);
        //private const ulong UC_SERDEC = 4097;
        //private const ulong UC_SERITS = 1729;

        //private static readonly ulong UC_AIDDEC = (UC_INHCST | UC_UBABLT | UC_KIPAGE | UC_KLPAGE |
        //                                           UC_VERDEC | UC_SERDEC);

        //private static readonly ulong UC_AIDITS = (UC_KIPAGE | UC_VERITS | UC_SERITS);
        //private static readonly ulong UC_HSBDEC = P36.OctUL(0376000); // DEC initial HSB 
        //private static readonly ulong UC_HSBITS = P36.OctUL(0000500); // ITS initial HSB 

        // Front end communications region 

        //private static readonly ulong FE_SWITCH = P36.OctUL(030); // halt switch 
        //private static readonly ulong FE_KEEPA = P36.OctUL(031); // keep alive 
        //private static readonly ulong FE_CTYIN = P36.OctUL(032); // console in 
        //private static readonly ulong FE_CTYOUT = P36.OctUL(033); // console out 
        //private static readonly ulong FE_KLININ = P36.OctUL(034); // KLINIK in 
        //private static readonly ulong FE_KLINOUT = P36.OctUL(035); // KLINIK out 
        //private static readonly ulong FE_RHBASE = P36.OctUL(036); // boot: RH11 addr 
        //private static readonly ulong FE_UNIT = P36.OctUL(037); // boot: unit num 
        //private static readonly ulong FE_MTFMT = P36.OctUL(040); // boot: magtape params 
        //private static readonly ulong FE_CVALID = P36.OctUL(0400); // char valid flag 

        private readonly ulong[] bytemask =
            new[]
                {
                    B36.OctUL(0),
                    B36.OctUL(01), B36.OctUL(03), B36.OctUL(07), B36.OctUL(017), B36.OctUL(037), B36.OctUL(077),
                    B36.OctUL(0177), B36.OctUL(0377), B36.OctUL(0777), B36.OctUL(01777), B36.OctUL(03777),
                    B36.OctUL(07777),
                    B36.OctUL(017777), B36.OctUL(037777), B36.OctUL(077777),
                    B36.OctUL(0177777), B36.OctUL(0377777), B36.OctUL(0777777),
                    B36.OctUL(01777777), B36.OctUL(03777777), B36.OctUL(07777777),
                    B36.OctUL(017777777), B36.OctUL(037777777), B36.OctUL(077777777),
                    B36.OctUL(0177777777), B36.OctUL(0377777777), B36.OctUL(0777777777),
                    B36.OctUL(01777777777), B36.OctUL(03777777777), B36.OctUL(07777777777),
                    B36.OctUL(017777777777), B36.OctUL(037777777777), B36.OctUL(077777777777),
                    B36.OctUL(0177777777777), B36.OctUL(0377777777777), B36.OctUL(0777777777777),
                    B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES,
                    B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES,
                    B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES, B36.ONES
                };

        private readonly int[] piM2Lvl = new[]
                                              {
                                                  0, 7, 6, 6, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 4,
                                                  3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
                                                  2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
                                                  2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
                                                  1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                                                  1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                                                  1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                                                  1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
                                              };

        // Page table fill operations 

        private enum PageTableFillOperations
        {
            PtfRd = 0, // read check 
            PtfWr = 1, // write check 
            PtfMap = 2, // map instruction 
            PtfCon = 4, // console access 
        }

        // Word assemblies 

        private ulong Flpc
        {
            get { return xwd(ProcFlags, PC); }
        }

        private ulong uuoword(int op, int ac)
        {
            return (((ulong) op) << InstVOp) | (((ulong) ac) << InstVAC) | Im;
        }

        //private ulong APRHWORD
        //{
        //    get { return ((apr_flg << APR_V_FLG) | (apr_lvl & APR_M_LVL) | (P36.NZ(apr_flg & apr_enb) ? APR_IRQ : 0)); }
        //}

        //private ulong APRWORD
        //{
        //    get { return ((apr_enb << (APR_V_FLG + 18)) | APRHWORD); }
        //}

        //private ulong PIHWORD
        //{
        //    get { return ((pi_act << PI_V_ACT) | (pi_on << PI_V_ON) | (pi_enb << PI_V_ENB)); }
        //}

        //private ulong PIWORD
        //{
        //    get { return ((pi_prq << PI_V_PRQ) | PIHWORD); }
        //}

        private static int addac(int x, int i)
        {
            //#define ADDAC(x,i)      (((x) + (i)) & INST_M_AC)
            return (((x) + (i)) & (int) _instMAC);
        }

        private bool IsUsr
        {
            get { return tstf(_fUsr); }
        }

        private bool IsUio
        {
            get { return tstf(_fUio); }
        }

        private bool tstf(ulong flg)
        {
            return (ProcFlags & flg).NZ();
        }

        private enum jrst_modes
        {
            None,
            JRSTU,
            JRSTE,
            JRSTUio,
        }

        private static readonly jrst_modes[] _jrstTab =
            {
                jrst_modes.JRSTU, jrst_modes.JRSTU, jrst_modes.JRSTU,
                jrst_modes.None, jrst_modes.JRSTE, jrst_modes.JRSTU,
                jrst_modes.JRSTE, jrst_modes.JRSTE,
                jrst_modes.JRSTUio, jrst_modes.None, jrst_modes.JRSTUio,
                jrst_modes.None, jrst_modes.JRSTE, jrst_modes.JRSTU,
                jrst_modes.None, jrst_modes.None
            };

        private enum EXtendCodes
        {
            XtMuuo = 0,
            XtSkip = 1,
            XtNosk = 2,
        }

        private static ulong unegate(ulong x)
        {
            var t = (long) x;
            t = -t;
            return (ulong) t;
        }

        //// Memory operations ----------------------------------------------------------------

        //private void RD()
        //{
        //    // #define RD              mb = Read (ea, MM_OPND)

        //    AdrsToMemory = ReadEA();
        //}

        //private void RD2()
        //{
        //    // #define RD2             rs[0] = Read (ea, MM_OPND); 

        //    DWRS[0] = ReadEA();
        //}

        private void rdac(int ac)
        {
            // #define RDAC            AC[ac] = Read (ea, MM_OPND)

            AC[ac] = readEA();
        }

        //private void AdrsToMemory = ReadMEA()
        //{
        //    // #define RM              mb = ReadM (ea, MM_OPND)

        //    AdrsToMemory = ReadMEA();
        //}

        private void rmac(int ac)
        {
            // #define RMAC            AC[ac] = ReadM (ea, MM_OPND)

            AC[ac] = readMea();
        }

        private void rdp(int ac)
        {
            // #define RDP             mb = Read (((a10) AC[ac]) &   P36.AMASK, MM_BSTK)
            //                        rs[1] = Read (INCA (ea), MM_OPND)

            mb = read(AC[ac] & B36.AMASK, MmBstk);
            dwrs[1] = read(inca(Im), MmOpnd);
        }

        //private void WR()
        //{
        //    // #define WR              Write (ea, mb, MM_OPND)
        //    WriteEA(AdrsToMemory);
        //}

        private void wrac(int ac)
        {
            // #define WRAC            Write (ea, AC[ac], MM_OPND)
            write(Im, AC[ac], MmOpnd);
        }

        private void wrp(int ac, ulong x)
        {
            // #define WRP(x)          Write (((a10) INCA (AC[ac])), (x), MM_BSTK)

            write(inca(AC[ac]), x, MmBstk);
        }

        //private void WR1()
        //{
        //    // #define WR1             Write (ea, rs[0], MM_OPND)
        //    WriteEA(DWRS[0]);
        //}

        private void wr2()
        {
            // #define WR2             ReadM (INCA (ea), MM_OPND); \
            //                        Write (ea, rs[0], MM_OPND); \
            //                        Write (INCA (ea), rs[1], MM_OPND)
            readM(inca(Im));
            WriteEA(dwrs[0]);
            write(inca(Im), dwrs[1], MmOpnd);
        }

        //         // Address operations ----------------------------------------------------------------

        private void jump(ulong x)
        {
            // #define JUMP(x)         PCQ_ENTRY, PC = ((a10) (x)) &   P36.AMASK
            PCQ_ENTRY();
            PC = x & B36.AMASK;
        }

        private void incpc()
        {
            // #define INCPC           PC = INCA (PC)
            PC = inca(PC);
        }

        private void subj(ulong x)
        {
            // #define SUBJ(x)         CLRF (F_AFI | F_FPD | F_TR); JUMP (x)
            clrf(_fAfi | _fFpd | _fTr);
            jump(x);
        }

        //// AC operations ----------------------------------------------------------------

        private void aobac(int x)
        {
            // #define AOBAC           AC[ac] = AOB (AC[ac])
            AC[x] = aob(AC[x]);
        }

        private void sobac(int x)
        {
            // #define SOBAC           AC[ac] = SOB (AC[ac])
            AC[x] = sob(AC[x]);
        }

        private void g2AC(int x)
        {
            // #define G2AC            rs[0] = AC[ac], rs[1] = AC[p1]
            dwrs[0] = AC[x];
            dwrs[1] = AC[addac(x, 1)];
        }

        private void s1AC(int x)
        {
            // #define S1AC            AC[ac] = rs[0]
            AC[x] = dwrs[0];
        }

        private void s2AC(int x)
        {
            // #define S2AC            S1AC, AC[p1] = rs[1]
            s1AC(x);
            AC[addac(x, 1)] = dwrs[1];
        }

        private void lac(int ac)
        {
            // #define LAC             if (ac) AC[ac] = mb
            if (ac != 0)
                AC[ac] = mb;
        }

        //// Tests and compares ----------------------------------------------------------------

        private static bool tl(ulong x)
        {
            // #define TL(a)           (TSTS (a) != 0)
            return tstsf(x);
        }

        private static bool te(ulong x)
        {
            // #define TE(a)           ((a) == 0)
            return x.Z();
        }

        private static bool tle(ulong x)
        {
            // #define TLE(a)          (TL (a) || TE (a))
            return tl(x) || te(x);
        }

        private static bool tg(ulong x)
        {
            // #define TG(a)           (TGE (a) && TN (a))
            return tge(x) && tn(x);
        }

        private static bool tn(ulong x)
        {
            // #define TN(a)           ((a) != 0)
            return x.NZ();
        }

        private static bool tge(ulong x)
        {
            // #define TGE(a)          (TSTS (a) == 0)
            return tsts(x).Z();
        }

        private bool cl(int ac, ulong a)
        {
            // #define CL(a)           ((TSTS (AC[ac] ^ a))? (a < AC[ac]): (AC[ac] < a))
            return (tsts(AC[ac] ^ a).NZ() ? (a < AC[ac]) : (AC[ac] < a));
        }

        private bool ce(int ac, ulong a)
        {
            // #define CE(a)           (AC[ac] == a)
            return AC[ac] == a;
        }

        private bool cle(int ac, ulong a)
        {
            // #define CLE(a)          (CL (a) || CE (a))
            return (cl(ac, a) || ce(ac, a));
        }

        private bool cge(int ac, ulong a)
        {
            // #define CGE(a)          (!CL (a))
            return (!cl(ac, a));
        }

        private bool cn(int ac, ulong a)
        {
            // #define CN(a)           (AC[ac] != a)
            return (AC[ac] != a);
        }

        private bool cg(int ac, ulong a)
        {
            // #define CG(a)           (CGE (a) && CN (a))
            return (cge(ac, a) && cn(ac, a));
        }

        //// Word assemblies ----------------------------------------------------------------

        // #define FLPC            XWD (flags, PC)
        // #define UUOWORD         (((d10) op) << INST_V_OP) | (((d10) ac) << INST_V_AC) | ea
        // #define APRHWORD        ((apr_flg << APR_V_FLG) | (apr_lvl & APR_M_LVL) | \
        //                        ((apr_flg & apr_enb)? APR_IRQ: 0))
        // #define APRWORD         ((apr_enb << (APR_V_FLG + 18)) | APRHWORD)
        // #define PIHWORD         ((pi_act << PI_V_ACT) | (pi_on << PI_V_ON) | \
        //                        (pi_enb << PI_V_ENB))
        // #define PIWORD          ((pi_prq << PI_V_PRQ) | PIHWORD)

        //// Instruction operations 
        /// 
        private void cibp()
        {
            // #define CIBP            if (!TSTF (F_FPD)) { ibp (ea, pflgs); SETF (F_FPD); }
            if (tstf(_fFpd)) return;
            ibp(Im, pxctFlags);
            setf(_fFpd);
        }

        private void LDB(int ac)
        {
            // #define LDB             AC[ac] = ldb (ea, pflgs)
            AC[ac] = ldb(Im, pxctFlags);
        }

        private void DPB(int ac)
        {
            // #define DPB             dpb (AC[ac], ea, pflgs)
            dpb(AC[ac], Im, pxctFlags);
        }

        private ulong FAD(int ac, ulong s)
        {
            // #define FAD(s)          fad (AC[ac], s, FALSE, 0)
            return fad(AC[ac], s, false, 0);
        }

        private ulong fadr(int ac, ulong s)
        {
            // #define FADR(s)         fad (AC[ac], s, TRUE, 0)
            return fad(AC[ac], s, true, 0);
        }

        private ulong fsbr(int ac, ulong s)
        {
            // #define FSBR(s)         fad (AC[ac], s, TRUE, 1)
            return fad(AC[ac], s, true, 1);
        }

        private ulong fsb(int ac, ulong s)
        {
            // #define FSB(s)          fad (AC[ac], s, FALSE, 1)
            return fad(AC[ac], s, false, 1);
        }

        private ulong FMP(int ac, ulong s)
        {
            // #define FMP(s)          fmp (AC[ac], s, FALSE)
            return fmp(AC[ac], s, false);
        }

        private ulong fmpr(int ac, ulong s)
        {
            // #define FMPR(s)         fmp (AC[ac], s, TRUE)
            return fmp(AC[ac], s, true);
        }

        private bool FDV(int ac, ulong s)
        {
            // #define FDV(s)          fdv (AC[ac], s, rs, FALSE)
            return fdv(AC[ac], s, out dwrs[0], false);
        }

        private bool fdvr(int ac, ulong s)
        {
            // #define FDVR(s)         fdv (AC[ac], s, rs, TRUE)
            return fdv(AC[ac], s, out dwrs[0], true);
        }

        private ulong movn(ulong s)
        {
            // #define MOVN(s)         NEG (s); MOVNF(s)
            var r = neg(s);
            movnf(s);

            return r;
        }

        private ulong movm(ulong s)
        {
            // #define MOVM(s)         ABS (s); MOVMF(s)
            var r = abs(s);
            movmf(s);

            return r;
        }

        private ulong ADD(int ac, ulong s)
        {
            // #define ADD(s)          add (AC[ac], s)
            return add(AC[ac], s);
        }

        private ulong SUB(int ac, ulong s)
        {
            // #define SUB(s)          sub (AC[ac], s)
            return sub(AC[ac], s);
        }

        private ulong IMUL(int ac, ulong s)
        {
            // #define IMUL(s)         imul (AC[ac], s)
            return imul(AC[ac], s);
        }

        private void MUL(int ac, ulong s)
        {
            // #define MUL(s)          mul (AC[ac], s, rs)
            mul(AC[ac], s, dwrs);
        }

        private bool IDIV(int ac, ulong s)
        {
            // #define IDIV(s)         idiv (AC[ac], s, rs)
            return idiv(AC[ac], s, dwrs);
        }

        private bool div(int ac, ulong s)
        {
            // #define DIV(s)          divi (ac, s, rs)
            return divi(ac, s, dwrs);
        }

        private void aoj(int ac)
        {
            // #define AOJ             AC[ac] = INC (AC[ac]); INCF (AC[ac])
            AC[ac] = inc(AC[ac]);
            incf(AC[ac]);
        }

        private void aos(int ac)
        {
            // #define AOS             RM; mb = INC (mb); WR; INCF (mb); LAC
            mb = readMea();
            mb = inc(mb);
            WriteEA(mb);
            incf(mb);
            lac(ac);
        }

        private void soj(int ac)
        {
            // #define SOJ             AC[ac] = DEC (AC[ac]); DECF (AC[ac])
            AC[ac] = dec(AC[ac]);
            decf(AC[ac]);
        }

        private void sos(int ac)
        {
            // #define SOS             RM; mb = DEC (mb); WR; DECF (mb); LAC
            mb = readMea();
            mb = dec(mb);
            WriteEA(mb);
            decf(mb);
            lac(ac);
        }

        private ulong setca(int ac, ulong s)
        {
            // #define SETCA(s)        ~AC[ac] &  P36.DMASK
            return ~AC[ac] & B36.DMASK;
        }

        private static ulong setcm(ulong s)
        {
            // #define SETCM(s)        ~(s) &  P36.DMASK;
            return ~(s) & B36.DMASK;
        }

        private ulong and(int ac, ulong s)
        {
            // #define AND(s)          AC[ac] & (s)
            return AC[ac] & (s);
        }

        private ulong andca(int ac, ulong s)
        {
            // #define ANDCA(s)        ~AC[ac] & (s)
            return ~AC[ac] & (s);
        }

        private ulong andcm(int ac, ulong s)
        {
            // #define ANDCM(s)        AC[ac] & ~(s)
            return AC[ac] & ~(s);
        }

        private ulong andcb(int ac, ulong s)
        {
            // #define ANDCB(s)        (~AC[ac] & ~(s)) &  P36.DMASK
            return (~AC[ac] & ~(s)) & B36.DMASK;
        }

        private ulong ior(int ac, ulong s)
        {
            // #define IOR(s)          AC[ac] | (s)
            return AC[ac] | (s);
        }

        private ulong orca(int ac, ulong s)
        {
            // #define ORCA(s)         (~AC[ac] | (s)) &  P36.DMASK
            return (~AC[ac] | (s)) & B36.DMASK;
        }

        private ulong orcm(int ac, ulong s)
        {
            // #define ORCM(s)         (AC[ac] | ~(s)) &  P36.DMASK
            return (AC[ac] | ~(s)) & B36.DMASK;
        }

        private ulong orcb(int ac, ulong s)
        {
            // #define ORCB(s)         (~AC[ac] | ~(s)) &  P36.DMASK
            return (~AC[ac] | ~(s)) & B36.DMASK;
        }

        private ulong xor(int ac, ulong s)
        {
            // #define XOR(s)          AC[ac] ^ (s)
            return AC[ac] ^ (s);
        }

        private ulong eqv(int ac, ulong s)
        {
            // #define EQV(s)          (~(AC[ac] ^ (s))) &  P36.DMASK
            return (~(AC[ac] ^ (s))) & B36.DMASK;
        }

        private static ulong ll(ulong s, ulong d)
        {
            // #define LL(s,d)         ((s) &  P36.LMASK) | ((d) &  P36.RMASK)
            return ((s) & B36.LMASK) | ((d) & B36.RMASK);
        }

        private static ulong rl(ulong s, ulong d)
        {
            // #define RL(s,d)         (((s) << 18) &  P36.LMASK) | ((d) &  P36.RMASK)
            return (((s) << 18) & B36.LMASK) | ((d) & B36.RMASK);
        }

        private static ulong rr(ulong s, ulong d)
        {
            // #define RR(s,d)         ((s) &  P36.RMASK) | ((d) &  P36.LMASK)
            return ((s) & B36.RMASK) | ((d) & B36.LMASK);
        }

        private static ulong lr(ulong s, ulong d)
        {
            // #define LR(s,d)         (((s) >> 18) &  P36.RMASK) | ((d) &  P36.LMASK)
            return (((s) >> 18) & B36.RMASK) | ((d) & B36.LMASK);
        }

        private static ulong llo(ulong s)
        {
            // #define LLO(s)          ((s) &  P36.LMASK) |  P36.RMASK
            return ((s) & B36.LMASK) | B36.RMASK;
        }

        private static ulong rlo(ulong s)
        {
            // #define RLO(s)          (((s) << 18) &  P36.LMASK) |  P36.RMASK
            return (((s) << 18) & B36.LMASK) | B36.RMASK;
        }

        private static ulong rro(ulong s)
        {
            // #define RRO(s)          ((s) &  P36.RMASK) |  P36.LMASK
            return ((s) & B36.RMASK) | B36.LMASK;
        }

        private static ulong lro(ulong s)
        {
            // #define LRO(s)          (((s) >> 18) &  P36.RMASK) |  P36.LMASK
            return (((s) >> 18) & B36.RMASK) | B36.LMASK;
        }

        private static ulong lle(ulong s)
        {
            // #define LLE(s)          ((s) &  P36.LMASK) | (((s) &  P36.LSIGN)?  P36.RMASK: 0)
            return ((s) & B36.LMASK) | (((s) & B36.LSIGN).NZ() ? B36.RMASK : 0);
        }

        private static ulong rle(ulong s)
        {
            // #define RLE(s)          (((s) << 18) &  P36.LMASK) | (((s) &  P36.RSIGN)?  P36.RMASK: 0)
            return (((s) << 18) & B36.LMASK) | (((s) & B36.RSIGN).NZ() ? B36.RMASK : 0);
        }

        private static ulong rre(ulong s)
        {
            // #define RRE(s)          ((s) &  P36.RMASK) | (((s) &  P36.RSIGN)?  P36.LMASK: 0)
            return ((s) & B36.RMASK) | (((s) & B36.RSIGN).NZ() ? B36.LMASK : 0);
        }

        private static ulong lre(ulong s)
        {
            // #define LRE(s)          (((s) >> 18) &  P36.RMASK) | (((s) &  P36.LSIGN)?  P36.LMASK: 0)
            return (((s) >> 18) & B36.RMASK) | (((s) & B36.LSIGN).NZ() ? B36.LMASK : 0);
        }

        private void tr()
        {
            // #define TR_             mb = IM
            mb = Im;
        }

        private void TL_()
        {
            // #define TL_             mb = IMS
            mb = Ims;
        }

        private void ts()
        {
            // #define TS_             RD; mb = SWP (mb)
            mb = readEA();
            mb = swp(mb);
        }

        private void td()
        {
            // #define TD_             RD
            mb = readEA();
        }

        private void T__E(int ac)
        {
            // #define T__E            if ((AC[ac] & mb) == 0) INCPC
            if ((AC[ac] & mb) == 0) incpc();
        }

        private void T__A()
        {
            // #define T__A            INCPC
            incpc();
        }

        private void T__N(int ac)
        {
            // #define T__N            if ((AC[ac] & mb) != 0) INCPC
            if ((AC[ac] & mb) != 0) incpc();
        }

        private void T_Z(int ac)
        {
            // #define T_Z             AC[ac] = AC[ac] & ~mb
            AC[ac] = AC[ac] & ~mb;
        }

        private void T_C(int ac)
        {
            // #define T_C             AC[ac] = AC[ac] ^ mb
            AC[ac] = AC[ac] ^ mb;
        }

        private void T_O(int ac)
        {
            // #define T_O             AC[ac] = AC[ac] | mb
            AC[ac] = AC[ac] | mb;
        }

        // #define IOC             if (TSTF (F_USR) && !TSTF (F_UIO)) goto MUUO;
        // #define IO7(x,y)        IOC; fptr = ((Q_ITS)? x[ac]: y[ac]); \
        //                        if (fptr == NULL) goto MUUO; \
        //                        if (fptr (ea, MM_OPND)) INCPC; break;
        // #define IOA             IOC; if (!Q_ITS) ea = calc_ioea (inst, pflgs)
        // #define IOAM            IOC; ea = ((Q_ITS)? ((a10) Read (ea, MM_OPND)): \
        //                            calc_ioea (inst, pflgs))

        //// Flag tests 

        private void movnf(ulong x)
        {
            // #define MOVNF(x)        if ((x) ==  P36.MAXNEG) SETF (F_C1 | F_AOV | F_T1); \
            //                        else if ((x) == 0) SETF (F_C0 | F_C1)
            if ((x) == B36.MAXNEG)
                setf(_fC1 | _fAov | _fT1);
            else if ((x) == 0)
                setf(_fC0 | _fC1);
        }

        private void movmf(ulong x)
        {
            // #define MOVMF(x)    
            if ((x) == B36.MAXNEG)
                setf(_fC1 | _fAov | _fT1);
            if ((x) == B36.MAXNEG)
                setf(_fC1 | _fAov | _fT1);
        }

        private void incf(ulong x)
        {
            // #define INCF(x)         if ((x) == 0) SETF (F_C0 | F_C1); \
            //                        else if ((x) ==  P36.MAXNEG) SETF (F_C1 | F_AOV | F_T1)
            switch ((x))
            {
                case 0:
                    setf(_fC0 | _fC1);
                    break;
                case B36.MAXNEG:
                    setf(_fC1 | _fAov | _fT1);
                    break;
            }
        }

        private void decf(ulong x)
        {
            // #define DECF(x)         if ((x) == MAXPOS) SETF (F_C0 | F_AOV | F_T1); \
            //                        else if ((x) !=  P36.ONES) SETF (F_C0 | F_C1)
            if ((x) == B36.MAXPOS)
                setf(_fC0 | _fAov | _fT1);
            else if ((x) != B36.ONES)
                setf(_fC0 | _fC1);
        }

        private void pushf(int ac)
        {
            // #define PUSHF           if (LRZ (AC[ac]) == 0) SETF (F_T2)
            if (lrz(AC[ac]) == 0) setf(_fT2);
        }

        private void popf(int ac)
        {
            // #define POPF            if (LRZ (AC[ac]) ==  P36.RMASK) SETF (F_T2)
            if (lrz(AC[ac]) == B36.RMASK) setf(_fT2);
        }

        private void dmovnf()
        {
            // #define DMOVNF          if (rs[1] == 0) { MOVNF (rs[0]); }
            if (dwrs[1] == 0) movnf(dwrs[0]);
        }

        //        // Halfword operations 

        private static ulong incr(ulong x)
        {
            //#define INCR(x)         ADDR (x, 1)
            return addr(x, 1);
        }

        private static ulong addr(ulong x, ulong y)
        {
            //#define ADDR(x,y)       (((x) + (y)) &  P36.RMASK)
            return (((x) + (y)) & B36.RMASK);
        }

        private ulong aob(ulong x)
        {
            // There's a KA only AOB behavior/bug that I'd like to emulator
            // On AOBJN (at least), if you AOB -1,,-1 you get -1,0 as you get a
            // carry over to bit 18.  None of the other processors do this. KI,KL,KS AFAIK
            // My only question is which AOB instructions do this -- It's probably all of them.

            // Not quite right -- all incrments of the right generate a carry, not just -1,-1

            var kaCary = false;

            if (ProcessorType == ProcessorTypes.KA10 &&
                x.RWD() == B36.RMASK)
                kaCary = true;

            var lef = incl(x);
            var rig = incr(x);

            if (kaCary)
                lef = incl(lef);

            //#define AOB(x)          (INCL (x) | INCR(x))
            return (lef | rig);
        }

        private static ulong incl(ulong x)
        {
            //#define INCL(x)         ADDL (x, 1)
            return addl(x, 1);
        }

        private static ulong addl(ulong x, ulong y)
        {
            //#define ADDL(x,y)       (((x) + ((y) << 18)) &  P36.LMASK)
            return (((x) + ((y) << 18)) & B36.LMASK);
        }

        private static ulong sob(ulong x)
        {
            //#define SOB(x)          (DECL (x) | DECR(x))
            return (decl(x) | decr(x));
        }

        private static ulong decr(ulong x)
        {
            //#define DECR(x)         SUBR (x, 1)
            return subr(x, 1);
        }

        private static ulong subr(ulong x, ulong y)
        {
            //#define SUBR(x,y)       (((x) - (y)) &  P36.RMASK)	
            return (((x) - (y)) & B36.RMASK);
        }

        private static ulong decl(ulong x)
        {
            //#define DECL(x)         SUBL (x, 1)
            return subl(x, 1);
        }

        private static ulong subl(ulong x, ulong y)
        {
            //#define SUBL(x,y)       (((x) - ((y) << 18)) &  P36.LMASK)	
            return (((x) - ((y) << 18)) & B36.LMASK);
        }

        private static ulong llz(ulong x)
        {
            //#define LLZ(x)          ((x) &  P36.LMASK)
            return ((x) & B36.LMASK);
        }

        private static ulong rlz(ulong x)
        {
            //#define RLZ(x)          (((x) << 18) &  P36.LMASK)
            return (((x) << 18) & B36.LMASK);
        }

        private static ulong rrz(ulong x)
        {
            //#define RRZ(x)          ((x) &  P36.RMASK)
            return ((x) & B36.RMASK);
        }

        private static ulong lrz(ulong x)
        {
            //#define LRZ(x)          (((x) >> 18) &  P36.RMASK)
            return (((x) >> 18) & B36.RMASK);
        }

        private static int lit8(int x)
        {
            //#define LIT8(x)         (((x) &  P36.RSIGN)? \
            //                        (((x) & 0377)? (-(x) & 0377): 0400): ((x) & 0377))
            return ((x) & (int) B36.RSIGN).NZ()
                       ? (((x) & 0377.Oct()).NZ() ? (-(x) & 0377.Oct()) : 0400.Oct())
                       : ((x) & 0377.Oct());
        }

        //// Fullword operations 

        private static ulong inc(ulong x)
        {
            //#define INC(x)          (((x) + 1) &  P36.DMASK)
            return (((x) + 1) & B36.DMASK);
        }

        private static ulong dec(ulong x)
        {
            //#define DEC(x)          (((x) - 1) &  P36.DMASK)	
            return (((x) - 1) & B36.DMASK);
        }

        private static ulong swp(ulong x)
        {
            //#define SWP(x)          ((((x) << 18) &  P36.LMASK) | (((x) >> 18) &  P36.RMASK))
            return ((((x) << 18) & B36.LMASK) | (((x) >> 18) & B36.RMASK));
        }

        private static ulong xwd(ulong x, ulong y)
        {
            //#define XWD(x,y)        (((((d10) (x)) << 18) &  P36.LMASK) | (((d10) (y)) &  P36.RMASK))
            return (((x << 18) & B36.LMASK) | (y & B36.RMASK));
        }

        private static ulong sets(ulong x)
        {
            //#define SETS(x)         ((x) | SIGN)
            return ((x) | B36.SIGN);
        }

        private static ulong clrs(ulong x)
        {
            //#define CLRS(x)         ((x) & ~ P36.SIGN)
            return ((x) & ~ B36.SIGN);
        }

        private static ulong tsts(ulong x)
        {
            //#define TSTS(x)         ((x) &  P36.SIGN)
            return ((x) & B36.SIGN);
        }

        private static bool tstsf(ulong x)
        {
            return tsts(x).NZ();
        }

        private static ulong neg(ulong x)
        {
            //#define NEG(x)          (-(x) &  P36.DMASK)
            var y = (long) x;
            return (((ulong) (-y)) & B36.DMASK);
        }

        private static ulong abs(ulong x)
        {
            //#define ABS(x)          (TSTS (x)? NEG(x): (x))
            return (tstsf(x) ? neg(x) : (x));
        }

        private static ulong sxt(ulong x)
        {
            //#define SXT(x)          (TSTS (x)? (x) | ~ P36.DMASK: (x))
            return (tstsf(x) ? (x) | ~ B36.DMASK : (x));
        }

        //// Doubleword operations (on 2-word arrays) 
        private static void dmovn(ulong[] regs)
        {
            //#define DMOVN(rs)       rs[1] = (-rs[1]) &  P36.MMASK; \
            //                        rs[0] = (~rs[0] + (rs[1] == 0)) &  P36.DMASK

            var t = (long) regs[1];
            t = -t;
            var w = (ulong) t;
            w &= B36.MMASK;

            regs[1] = w;

            regs[0] = ~regs[0] + (regs[1].Z() ? 1UL : 0);
            regs[0] &= B36.DMASK;
        }

        private static void mkdneg(ulong[] rs)
        {
            //#define MKDNEG(rs)      rs[1] = SETS (-rs[1]) &  P36.DMASK; \
            //                        rs[0] = (~rs[0] + (rs[1] ==  P36.MAXNEG)) &  P36.DMASK	

            var t = (long) rs[1];
            t = -t;
            var t1 = (ulong) t;
            rs[1] = sets(t1) & B36.DMASK;
            rs[0] = (~rs[0] + (rs[1] == B36.MAXNEG).B()) & B36.DMASK;
        }

        private static bool dcmpge(ulong[] a, ulong[] b)
        {
            //#define DCMPGE(a,b)     ((a[0] > b[0]) || ((a[0] == b[0]) && (a[1] >= b[1])))

            return ((a[0] > b[0]) || ((a[0] == b[0]) && (a[1] >= b[1])));
        }

        //// Address operations 
        private static ulong adda(ulong x, ulong i)
        {
            //#define ADDA(x,i)       (((x) + (i)) &   P36.AMASK)
            return (((x) + (i)) & B36.AMASK);
        }

        private static ulong inca(ulong x)
        {
            //#define INCA(x)         ADDA (x, 1)
            return adda(x, 1);
        }

        private void clrf(ulong x)
        {
            //#define CLRF(x)         flags = flags & ~(x)
            ProcFlags &= ~(x);
        }

        private void setf(ulong x)
        {
            //#define SETF(x)         flags = flags | (x)
            ProcFlags |= (x);
        }

        public void SetUserMode()
        {
            setf(_fUsr);
        }

        private string serializeProcFlags(ulong flags)
        {
            var sb = new StringBuilder();

            setproc(sb, flags, _fAov, "AOV");
            setproc(sb, flags, _fC0, "C0");
            setproc(sb, flags, _fC1, "C1");
            setproc(sb, flags, _fFov, "FOV");
            setproc(sb, flags, _fFpd, "FPD");
            setproc(sb, flags, _fUsr, "USR");
            setproc(sb, flags, _fUio, "UIO");
            setproc(sb, flags, _fPub, "PUB");
            setproc(sb, flags, _fT1, "T1");
            setproc(sb, flags, _fT2, "T2");
            setproc(sb, flags, _fFxu, "FXU");
            setproc(sb, flags, _fDck, "DCK");
            if (RunOS == OSTypes.ITS)
                setproc(sb, flags, _f_1Pr, "1PR");
            else
                setproc(sb, flags, _fAfi, "AFI");

            return sb.ToString();
        }

        private static void setproc(StringBuilder sb, ulong flags, ulong mask, string s)
        {
            var st = 0;
            if ((flags & mask) != 0) st = 1;
            if (sb.Length > 0)
                sb.Append('|');
            sb.Append(s);
            sb.Append('=');
            sb.Append(st);
        }

        //#define TSTF(x)         (flags & (x))

        private void PCQ_ENTRY()
        {
            //#define PCQ_ENTRY       pcq[pcq_p = (pcq_p - 1) & PCQ_MASK] = PC
        }

        // Indexing Register 
        // This handles switching between multiple contexts - so, we have just one for now
        private ulong xrIdxReg(int r, ulong prv)
        {
            //#define XR(r,prv)       ((prv)? ac_prv[r]: ac_cur[r])   /* AC select context */
            //throw new NotImplementedException();

            return AC[r];
        }

        //private void WriteP(ulong value, ulong pc)
        //{
        //    throw new NotImplementedException();

        //    CORE[CurrentSegment, pc] = new Word36(value);
        //}

        private void WriteEA(ulong adrs)
        {
            write(Im, adrs, MmOpnd);
        }

        private void write(ulong ea, ulong value, object mm_OPND)
        {
            CORE[CurrentSegment, ea] = new Word36(value);
        }

        private ulong readMea()
        {
            return readM(Im);
        }

        private ulong readM(ulong x)
        {
            // Seems like they're all mm_OPND, so get rid of that operand
            return CORE[CurrentSegment, x].UL;
        }

        private ulong readP(ulong ea)
        {
            return CORE[CurrentSegment, ea].UL;
        }

        private ulong readEA()

        {
            return read(Im, MmOpnd);
        }

        private ulong read(ulong ea, ulong pxctflg)
        {
            return CORE[CurrentSegment, ea].UL;
        }

        private static ulong map(ulong ea, ulong pxctflg)
        {
            throw new NotImplementedException();
        }

        private void setNewflags(ulong fl, bool b)
        {
            fl = lrz(fl); // Get the status bits off the left word

            if (!b)
                throw new NotImplementedException(); // Exec Mode

            if (tstf(_fUsr)) // Can't go exec from user - duh
            {
                fl |= _fUsr;
                if (!tstf(_fUio)) // Can't set userio if not already set
                    fl &= ~_fUio;
            }

            //if (jrst && TSTF(F_USR))
            //{                             /* if in user now */
            //    fl = fl | F_USR;                                    /* can't clear user */
            //    if (!TSTF(F_UIO)) fl = fl & ~F_UIO;                /* if !UIO, can't set */
            //}
            //if (Q_ITS && (fl & F_1PR))
            //{                            /* ITS 1-proceed? */
            //    its_1pr = 1;                                        /* set flag */
            //    fl = fl & ~F_1PR;                                   /* vanish bit */
            //}

            ProcFlags = fl & _fMask; /* set new flags */
        }

        private static void piDismiss()
        {
            throw new NotImplementedException();
        }

        private static EXtendCodes xtend(int ac, ulong ea, ulong pflgs)
        {
            throw new NotImplementedException();
        }

        // Single word integer routines 

        // Integer add

        //Truth table for integer add

        //     case    a       b       r       flags
        //     1       +       +       +       none
        //     2       +       +       -       AOV + C1
        //     3       +       -       +       C0 + C1
        //     4       +       -       -       -
        //     5       -       +       +       C0 + C1
        //     6       -       +       -       -
        //     7       -       -       +       AOV + C0
        //     8       -       -       -       C0 + C1

        private ulong add(ulong a, ulong b)
        {
            var r = (a + b) & B36.DMASK;

            if (tstsf(a & b))
            {
                // cases 7,8 
                if (tstsf(r))
                    setf(_fC0 | _fC1); // case 8 
                else
                    setf(_fC0 | _fAov | _fT1); // case 7 
                return r;
            }
            if (!tstsf(a | b))
            {
                // cases 1,2 
                if (tstsf(r))
                    setf(_fC1 | _fAov | _fT1); // case 2 
                return r; // case 1 
            }
            if (!tstsf(r))
                setf(_fC0 | _fC1); // cases 3,5 
            return r;
        }

        // Integer subtract - actually ac + ~op + 1 

        private ulong sub(ulong a, ulong b)
        {
            var r = (a - b) & B36.DMASK;

            if (tstsf(a & ~b))
            {
                // cases 7,8 
                if (tstsf(r))
                    setf(_fC0 | _fC1); // case 8 
                else
                    setf(_fC0 | _fAov | _fT1); // case 7 
                return r;
            }
            if (!tstsf(a | ~b))
            {
                // cases 1,2 
                if (tstsf(r))
                    setf(_fC1 | _fAov | _fT1); // case 2 
                return r; // case 1 
            }
            if (!tstsf(r))
                setf(_fC0 | _fC1); // cases 3,5 
            return r;
        }

        // Logical shift 

        private static ulong lsh(ulong val, ulong eai)
        {
            var sc = lit8((int) eai);

            if (sc > 35)
                return 0;
            if ((eai & B36.RSIGN).NZ())
                return (val >> sc);
            return ((val << sc) & B36.DMASK);
        }

        // Rotate 

        private static ulong rot(ulong val, ulong eai)
        {
            var sc = lit8((int) eai)%36;

            if (sc == 0)
                return val;
            if ((eai & B36.RSIGN).NZ())
                sc = 36 - sc;

            return (((val << sc) | (val >> (36 - sc))) & B36.DMASK);
        }

        // Double word integer instructions 

        // Double add - see case table for single add 

        private void dadd(int ac, ulong[] rsi)
        {
            var p1 = addac(ac, 1);

            AC[p1] = clrs(AC[p1]) + clrs(rsi[1]); // add lo 
            var r = (AC[ac] + rsi[0] + (tstsf(AC[p1]) ? 1UL : 0)) & B36.DMASK;
            if (tstsf(AC[ac] & rsi[0])) // cases 7,8 
                if (tstsf(r))
                    setf(_fC0 | _fC1); // case 8 
                else
                    setf(_fC0 | _fAov | _fT1); // case 7 
            else if (!tstsf(AC[ac] | rsi[0]))
            {
                // cases 1,2 
                if (tstsf(r))
                    setf(_fC1 | _fAov | _fT1); // case 2 
            }
            else if (!tstsf(r))
                setf(_fC0 | _fC1); // cases 3,5 
            AC[ac] = r;
            AC[p1] = tstsf(r) ? sets(AC[p1]) : clrs(AC[p1]);

            return;
        }

        // Double subtract - see comments for single subtract 

        private void dsub(int ac, ulong[] rsi)
        {
            var p1 = addac(ac, 1);

            AC[p1] = clrs(AC[p1]) - clrs(rsi[1]); // sub lo 
            var r = (AC[ac] - rsi[0] - (tstsf(AC[p1]) ? 1UL : 0)) & B36.DMASK;

            if (tstsf(AC[ac] & ~rsi[0])) // cases 7,8 
                if (tstsf(r))
                    setf(_fC0 | _fC1); // case 8 
                else
                    setf(_fC0 | _fAov | _fT1); // case 7 
            else if (!tstsf(AC[ac] | ~rsi[0]))
            {
                // cases 1,2 
                if (tstsf(r))
                    setf(_fC1 | _fAov | _fT1); // case 2 
            }
            else if (!tstsf(r))
                setf(_fC0 | _fC1); // cases 3,5 

            AC[ac] = r;
            AC[p1] = (tstsf(r) ? sets(AC[p1]) : clrs(AC[p1])) & B36.DMASK;

            return;
        }

        // Logical shift combined 

        private void lshc(int ac, ulong eai)
        {
            var p1 = addac(ac, 1);
            var sc = lit8((int) eai);

            if (sc > 71)
                AC[ac] = AC[p1] = 0;
            else if ((eai & B36.RSIGN).NZ())
                if (sc >= 36)
                {
                    AC[p1] = AC[ac] >> (sc - 36);
                    AC[ac] = 0;
                }
                else
                {
                    AC[p1] = ((AC[p1] >> sc) | (AC[ac] << (36 - sc))) & B36.DMASK;
                    AC[ac] = AC[ac] >> sc;
                }
            else if (sc >= 36)
            {
                AC[ac] = (AC[p1] << (sc - 36)) & B36.DMASK;
                AC[p1] = 0;
            }
            else
            {
                AC[ac] = ((AC[ac] << sc) | (AC[p1] >> (36 - sc))) & B36.DMASK;
                AC[p1] = (AC[p1] << sc) & B36.DMASK;
            }
            return;
        }

        // Rotate combined 

        private void rotc(int ac, ulong eai)
        {
            var p1 = addac(ac, 1);
            var sc = lit8((int) eai)%72;
            var t = AC[ac];

            if (sc == 0)
                return;
            if ((eai & B36.RSIGN).NZ())
                sc = 72 - sc;

            if (sc >= 36)
            {
                AC[ac] = ((AC[p1] << (sc - 36)) | (t >> (72 - sc))) & B36.DMASK;
                AC[p1] = ((t << (sc - 36)) | (AC[p1] >> (72 - sc))) & B36.DMASK;
            }
            else
            {
                AC[ac] = ((t << sc) | (AC[p1] >> (36 - sc))) & B36.DMASK;
                AC[p1] = ((AC[p1] << sc) | (t >> (36 - sc))) & B36.DMASK;
            }
            return;
        }

        // Arithmetic shifts 

        private ulong ash(ulong val, ulong eai)
        {
            var sc = lit8((int) eai);
            var sign = tsts(val);
            var fill = sign.NZ() ? B36.ONES : 0;

            if (sc == 0)
                return val;
            if (sc > 35)
                sc = 35; // cap sc at 35 
            if ((eai & B36.RSIGN).NZ())
                return (((val >> sc) | (fill << (36 - sc))) & B36.DMASK);

            var so = val >> (35 - sc);
            if (so != (sign.NZ() ? bytemask[sc + 1] : 0))
                setf(_fAov | _fT1);

            return (sign | ((val << sc) & B36.MMASK));
        }

        private void ashc(int ac, ulong eai)
        {
            var sc = lit8((int) eai);
            var p1 = addac(ac, 1);
            var sign = tsts(AC[ac]);
            var fill = sign.NZ() ? B36.ONES : 0;
            ulong so;

            if (sc == 0)
                return;
            if (sc > 70)
                sc = 70; // cap sc at 70 
            AC[ac] = clrs(AC[ac]); // clear signs 
            AC[p1] = clrs(AC[p1]);

            if ((eai & B36.RSIGN).NZ())
                if (sc >= 35)
                {
                    // right 36..70 
                    AC[p1] = ((AC[ac] >> (sc - 35)) | (fill << (70 - sc))) & B36.DMASK;
                    AC[ac] = fill;
                }
                else
                {
                    AC[p1] = sign | // right 1..35 
                             (((AC[p1] >> sc) | (AC[ac] << (35 - sc))) & B36.MMASK);
                    AC[ac] = ((AC[ac] >> sc) | (fill << (35 - sc))) & B36.DMASK;
                }
            else if (sc >= 35)
            {
                // left 36..70 
                so = AC[p1] >> (70 - sc); // bits lost left 
                if ((AC[ac] != (sign.NZ() ? B36.MMASK : 0)) ||
                    (so != (sign.NZ() ? bytemask[sc - 35] : 0)))
                    setf(_fAov | _fT1);
                AC[ac] = sign | ((AC[p1] << (sc - 35)) & B36.MMASK);
                AC[p1] = sign;
            }
            else
            {
                so = AC[ac] >> (35 - sc); // bits lost left 
                if (so != (sign.NZ() ? bytemask[sc] : 0))
                    setf(_fAov | _fT1);
                AC[ac] = sign |
                         (((AC[ac] << sc) | (AC[p1] >> (35 - sc))) & B36.MMASK);
                AC[p1] = sign | ((AC[p1] << sc) & B36.MMASK);
            }
            return;
        }

        // Effective address routines 

        //Calculate effective address - used by byte instructions, extended
        //  instructions, and interrupts to get a different mapping context from
        //  the main loop.  prv is either EABP_PXCT or MM_CUR.

        private ulong calcEA(ulong insti, ulong prv)
        {
            int i;
            ulong eai = 0;
            ulong indrct;

            for (indrct = insti, i = 0; i < IndMax; i++)
            {
                eai = indrct.RWD();
                var xr = getXr(indrct);
                if (xr.NZ())
                    eai = (eai + (xrIdxReg(xr, prv))) & B36.AMASK;
                if (tstInd(indrct))
                    indrct = read(eai, prv);
                else break;
            }
            if (i >= IndMax)
                throw new Exception("Indirection max reached");

            return eai;
        }

        //Calculate I/O effective address.  Cases:
        //  - No index or indirect, return addr from instruction
        //  - Index only, index >= 0, return 36b sum of addr + index
        //  - Index only, index <= 0, return 18b sum of addr + index
        //  - Indirect, calculate 18b sum of addr + index, return
        //               entire word fetch (single level)

        private ulong calcIoea(ulong insti, int pflgsi)
        {
            var xr = getXr(insti);
            var iea = insti.RWD();
            if (tstInd(insti))
            {
                // indirect? 
                if (xr.NZ())
                    iea = (iea + (xrIdxReg(xr, MmEA))) & B36.AMASK;
                iea = read(iea, MmEA);
            }
            else if (xr.NZ())
            {
                // direct + idx? 
                iea = iea + (xrIdxReg(xr, MmEA));
                if (tstsf(xrIdxReg(xr, MmEA)))
                    iea = iea & B36.AMASK;
            }
            return iea;
        }

        //Calculate JRSTF effective address.  This routine preserves
        //  the left half of the effective address, to be the new flags.

        private ulong calcJrstfEA(ulong insti, ulong pflgsi)
        {
            int i;
            ulong mbi = 0;

            for (i = 0; i < IndMax; i++)
            {
                mbi = insti;
                var xr = getXr(insti);
                if (xr.NZ())
                {
                    mbi = (mbi & B36.AMASK);
                    mbi += xrIdxReg(xr, MmEA);
                }
                if (tstInd(insti))
                    insti = read((mbi) & B36.AMASK, MmEA);
                else break;
            }
            if (i >= IndMax)
                throw new Exception("Indirection max reached");
            return (mbi & B36.DMASK);
        }

        // Byte pointer routines 

        // Increment byte pointer - checked against KS10 ucode 

        private void ibp(ulong eai, ulong pflgsi)
        {
            var bp = readM(eai);
            var p = getP(bp);
            var s = getS(bp);
            p = p - s; // adv P 
            if (p < 0)
            {
                // end of word? 
                bp = (bp & B36.LMASK) | (incr(bp)); // incr addr 
                p = (36 - s) & 077; // reset P 
            }
            bp = putP(bp, (ulong) p); // store new P 
            write(eai, bp, MmOpnd); // store byte ptr 
            return;
        }

        // Load byte 

        private ulong ldb(ulong ea, ulong pflgs)
        {
            var bp = read(ea, MmOpnd);
            var p = getP(bp);
            var s = getS(bp);
            var ba = calcEA(bp, MmEabp);
            var wd = read(ba, MmBstk);
            wd = (wd >> p); // align byte 
            wd = wd & bytemask[s]; // mask to size 
            return wd;
        }

        // Deposit byte - must use read and write to get page fail correct 

        private void dpb(ulong val, ulong ea, ulong pflgs)
        {
            var bp = read(ea, MmOpnd);
            var p = getP(bp);
            var s = getS(bp);
            var ba = calcEA(bp, MmEabp);
            var wd = read(ba, MmBstk);
            var mask = bytemask[s] << p;
            val = val << p;
            wd = (wd & ~mask) | (val & mask); // insert byte 
            write(ba, wd & B36.DMASK, MmBstk);
            return;
        }

        //Adjust byte pointer - checked against KS10 ucode 
        //  The KS10 divide checks if the bytes per word = 0, which is a simpler
        //  formulation of the processor reference manual check.

        private void adjbp(int ac, ulong ea, ulong pflgs)
        {
            var val = AC[ac];
            var bp = read(ea, MmOpnd);
            var p = getP(bp);
            var s = getS(bp);
            if (s.NZ())
            {
                var left = (ulong) ((36 - p)/s);
                var bywrd = left + (ulong) (p/s);
                if (bywrd == 0)
                {
                    // zero bytes? 
                    setf(_fAov | _fT1 | _fDck); // set flags 
                    return; // abort operation 
                }
                var newby = left + sxt(val);
                var wdadj = newby/bywrd;
                var byadj = (newby >= 0) ? newby%bywrd : unegate((unegate(newby))%bywrd);
                if (byadj <= 0)
                {
                    byadj = byadj + bywrd; // make adj positive 
                    wdadj = wdadj - 1;
                }
                p = (36 - ((int) byadj)*s) - ((36 - p)%s); // new p 
                bp = (putP(bp, (ulong) p) & B36.LMASK) | ((bp + wdadj) & B36.RMASK);
            }
            AC[ac] = bp;
            return;
        }

        //Block transfer - checked against KS10 ucode
        //  The KS10 uses instruction specific recovery code in page fail
        //  to set the AC properly for restart.  Lacking this mechanism,
        //  the simulator must test references in advance.
        //  The clocking test guarantees forward progress under single step.

        private void blt(int ac, ulong ea, ulong pflgs)
        {
            var srca = lrz(AC[ac]);
            var dsta = rrz(AC[ac]);
            var lnt = ea - dsta + 1;
            int flg;

            AC[ac] = xwd(srca + lnt, dsta + lnt);
            for (flg = 0; dsta <= ea; flg++)
            {
                // loop 
                int t;
                if (flg.NZ() && (t = testInt()).NZ())
                {
                    // timer event? 
                    AC[ac] = xwd(srca, dsta); // AC for intr 
                    throw new Exception("BLT Interrupt");
                }
                if (accViol(srca & B36.AMASK, MmBstk, PageTableFillOperations.PtfRd))
                {
                    // src access viol? 
                    AC[ac] = xwd(srca, dsta); // AC for page fail 
                    read(srca & B36.AMASK, MmBstk); // force trap 
                }
                if (accViol(dsta & B36.AMASK, MmOpnd, PageTableFillOperations.PtfWr))
                {
                    // dst access viol? 
                    AC[ac] = xwd(srca, dsta); // AC for page fail 
                    readM(dsta & B36.AMASK); // force trap 
                }
                var srcv = read(srca & B36.AMASK, MmBstk);
                write(dsta & B36.AMASK, srcv, MmOpnd); // write 
                srca = srca + 1; // incr addr 
                dsta = dsta + 1;
            }
            return;
        }

        private bool accViol(ulong x, ulong mmBstk, PageTableFillOperations ptfo)
        {
            var page = CORE.Page((int) x);
            if (!CORE.PageExists(page)) return true; // No Such Table

            switch (ptfo)
            {
                case PageTableFillOperations.PtfRd:
                    return false; // Everything's readable so far
                case PageTableFillOperations.PtfWr:
                    return !CORE.PageProtection(page).Writable;
            }

            return true; // Eh?
        }

        // I/O block transfers - byte to Unibus (0) and Unibus to byte (1) 

        private static readonly ulong _byte1 = B36.OctUL(0776000000000);
        private static readonly ulong _byte2 = B36.OctUL(0001774000000);
        private static readonly ulong _byte3 = B36.OctUL(0000003770000);
        private static readonly ulong _byte4 = B36.OctUL(0000000007760);
        // unused               0000000000017 

        private void bltu(int ac, ulong ea, int pflgs, int dir)
        {
            var srca = lrz(AC[ac]);
            var dsta = rrz(AC[ac]);
            var lnt = ea - dsta + 1;
            int flg;

            AC[ac] = xwd(srca + lnt, dsta + lnt);
            for (flg = 0; dsta <= ea; flg++)
            {
                // loop 
                int t;
                if (flg.NZ() && (t = testInt()).NZ())
                {
                    // timer event? 
                    AC[ac] = xwd(srca, dsta); // AC for intr 
                    throw new Exception("BLTU Interrupt");
                }
                if (accViol(srca & B36.AMASK, MmBstk, PageTableFillOperations.PtfRd))
                {
                    // src access viol? 
                    AC[ac] = xwd(srca, dsta); // AC for page fail 
                    read(srca & B36.AMASK, MmBstk); // force trap 
                }
                if (accViol(dsta & B36.AMASK, MmOpnd, PageTableFillOperations.PtfWr))
                {
                    // dst access viol? 
                    AC[ac] = xwd(srca, dsta); // AC for page fail 
                    readM(dsta & B36.AMASK); // force trap 
                }
                var srcv = read(srca & B36.AMASK, MmBstk);
                ulong dstv;
                if (dir.NZ())
                    dstv = ((srcv << 10) & _byte1) | ((srcv >> 6) & _byte2) |
                           ((srcv << 12) & _byte3) | ((srcv >> 4) & _byte4);
                else
                    dstv = ((srcv & _byte1) >> 10) | ((srcv & _byte2) << 6) |
                           ((srcv & _byte3) >> 12) | ((srcv & _byte4) << 4);
                write(dsta & B36.AMASK, dstv, MmOpnd); // write 
                srca = srca + 1; // incr addr 
                dsta = dsta + 1;
            }
            return;
        }

        // Utility routine to test for I/O event and interrupt 

        private static int testInt()
        {
            //if (sim_interval <= 0)
            //{
            //    // check queue 
            //    int t;
            //    if (t = sim_process_event())
            //        return t; // IO event? 
            //    if (pi_eval())
            //        return (INTERRUPT); // interrupt? 
            //}
            //else
            //    sim_interval--; // count clock 

            return 0;
        }

        //Adjust stack pointer

        //  The reference manual says to trap on:
        //  1) E < 0, left changes from + to -
        //  2) E >= 0, left changes from - to +
        //  This is the same as trap on:
        //  1) E and left result have same signs
        //  2) initial value and left result have different signs

        private ulong adjsp(ulong val, ulong ea)
        {
            var imm = ea;

            var left = addl(val, imm);
            var right = addr(val, imm);
            if (tstsf((val ^ left) & (~left ^ rlz(imm))))
                setf(_fT2);
            return (left | right);
        }

        //Jump if find first  P36.ONES
        //  Takes advantage of 7 bit find first table for priority interrupts.

        private int jffo(ulong val)
        {
            int i;

            if ((val & B36.DMASK) == 0)
                return 0;
            for (i = 0; i <= 28; i = i + 7)
            {
                // scan five bytes 
                var by = (int) ((val >> (29 - i)) & B36.OctUL(0177));
                if (by.NZ())
                    return (piM2Lvl[by] + i - 1);
            }
            return 35; // must be bit 35 
        }

        // Circulate - ITS only instruction

        //Bits rotated out of AC are rotated into the opposite end of AC+1 - why?
        //No attempt is made to optimize this instruction.

        private void circ(int ac, int ea)
        {
            var sc = lit8(ea)%72;
            var p1 = addac(ac, 1);
            int i;

            if (sc == 0)
                return; // any shift? 
            if (((ulong) ea & B36.RSIGN).NZ())
                sc = 72 - sc; // if right, make left 
            for (i = 0; i < sc; i++)
            {
                // one bit at a time 
                var val = tsts(AC[ac]);
                AC[ac] = ((AC[ac] << 1) | (AC[p1] & 1)) & B36.DMASK;
                AC[p1] = (AC[p1] >> 1) | val; // shift in 
            }
            return;
        }

        private InstructionExit procMUUO(int op, int ac)
        {
            // Need to implement user mode UUOs

            var iType = InstructionExit.UnimplimentedUUO;

            throw new InstructionFailure(this,
                                         iType,
                                         PC, inst,
                                         (OpCodes) op,
                                         ac,
                                         Im,
                                         "procMUUO unimplemented",
                                         null);

            // If undefined, monitor UUO - checked against KS10 ucode
            // The KS10 implements a much more limited version of MUUO flag handling.
            // In the KS10, the trap ucode checks for opcodes 000-077.  If the opcode
            // is in that range, the trap flags are not cleared.  Instead, the MUUO
            // microcode stores the flags with traps cleared, and uses the trap flags
            // to determine how to vector.  Thus, MUUO's >= 100 will vector incorrectly.

            //its_2pr = 0; // clear trap 
            //if (T20PAG)
            //{
            //    // TOPS20 paging? 
            //    var tf = (op << (INST_V_OP - 18)) | (ac << (INST_V_AC - 18));
            //    WriteP(upta + UPT_MUUO, XWD(ProcFlags & ~(F_T2 | F_T1), (ulong) tf)); // store flags,,op+ac  // traps clear 
            //    WriteP(upta + UPT_MUPC, PC); // store PC 
            //    WriteP(upta + UPT_T20_UEA, EA); // store eff addr 
            //    WriteP(upta + UPT_T20_CTX, UBRWORD); // store context 
            //}
            //else
            //{
            //    // TOPS10/ITS 
            //    WriteP(upta + UPT_MUUO, UUOWORD(op, ac)); // store instr word 
            //    WriteP(upta + UPT_MUPC, XWD( // store flags,,PC 
            //                                ProcFlags & ~(F_T2 | F_T1), PC)); // traps clear 
            //    WriteP(upta + UPT_T10_CTX, UBRWORD); // store context 
            //}
            //EA = upta + (isUSR ? UPT_UNPC : UPT_ENPC) +
            //     (pager_tc ? UPT_NPCT : 0); // calculate vector 
            //MB = ReadP(EA); // new flags, PC 
            //JUMP(MB); // set new PC 
            //if (isUSR) MB = MB | XWD(F_UIO, 0); // set PCU 
            //set_newflags(MB, false); // set new flags 
        }

        public void RegisterUUOHandler(OpCodes opcode, ProcessInstruction newHandler)
        {
            uuoHandlers[(int) opcode] = newHandler;
        }
    }
}