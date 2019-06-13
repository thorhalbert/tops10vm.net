using ThirtySixBits;

namespace PDP10CPU.CPU
{
    public partial class SimhPDP10CPU
    {
        //	pdp10_mdfp.c: PDP-10 multiply/divide and floating point simulator

        //   Copyright (c) 1993-2005, Robert M Supnik

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

        //   2-Apr-04     RMS     Fixed bug in floating point unpack
        //                        Fixed bug in FIXR (found by Phil Stone, fixed by
        //                        Chris Smith)
        //   31-Aug-01    RMS     Changed int64 to long for Windoze
        //   10-Aug-01    RMS     Removed register in declarations

        //   Instructions handled in this module:
        //        imul            integer multiply
        //        idiv            integer divide
        //        mul             multiply
        //        div             divide
        //        dmul            double precision multiply
        //        ddiv            double precision divide
        //        fad(r)          floating add (and round)
        //        fsb(r)          floating subtract (and round)
        //        fmp(r)          floating multiply (and round)
        //        fdv(r)          floating divide and round
        //        fsc             floating scale
        //        fix(r)          floating to fixed (and round)
        //        fltr            fixed to floating and round
        //        dfad            double precision floating add/subtract
        //        dfmp            double precision floating multiply
        //        dfdv            double precision floating divide

        //   The PDP-10 stores double (quad) precision integers in sequential
        //   AC's or memory locations.  Integers are stored in 2's complement
        //   form.  Only the sign of the high order word matters; the signs
        //   in low order words are ignored on input and set to the sign of
        //   the result on output.  Quad precision integers exist only in the
        //   AC's as the result of a DMUL or the dividend of a DDIV.

        //    0 00000000011111111112222222222333333
        //    0 12345678901234567890123456789012345
        //   +-+-----------------------------------+
        //   |S|      high order integer           | AC[n), A
        //   +-+-----------------------------------+
        //   |S|      low order integer            | AC[n + 1), A + 1
        //   +-+-----------------------------------+
        //   |S|      low order integer            | AC[n + 2)
        //   +-+-----------------------------------+
        //   |S|      low order integer            | AC[n + 3)
        //   +-+-----------------------------------+

        //   The PDP-10 supports two floating point formats: single and double
        //   precision.  In both, the exponent is 8 bits, stored in excess
        //   128 notation.  The fraction is expected to be normalized.  A
        //   single precision floating point number has 27 bits of fraction;
        //   a double precision number has 62 bits of fraction (the sign
        //   bit of the second word is ignored and is set to zero).

        //   In a negative floating point number, the exponent is stored in
        //   one's complement form, the fraction in two's complement form.

        //    0 00000000 011111111112222222222333333
        //    0 12345678 901234567890123456789012345
        //   +-+--------+---------------------------+
        //   |S|exponent|      high order fraction  | AC[n), A
        //   +-+--------+---------------------------+
        //   |0|      low order fraction            | AC[n + 1), A + 1
        //   +-+------------------------------------+

        //   Note that treatment of the sign is different for double precision
        //   integers and double precision floating point.  DMOVN (implemented
        //   as an inline macro) follows floating point conventions.

        //   The original PDP-10 CPU (KA10) used a different format for double
        //   precision numbers and included certain instructions to make
        //   software support easier.  These instructions were phased out in
        //   the KL10 and KS10 and are treated as MUUO's.

        //   The KL10 added extended precision (11-bit exponent) floating point
        //   format (so-called G floating).  These instructions were not
        //   implemented in the KS10 and are treated as MUUO's.
        //

        private struct UFP
        {
            // unpacked fp number 
            internal int Sign; // sign 
            internal int Exp; // exponent 
            internal ulong Fhi; // fraction high 
            internal ulong Flo; // for double prec 
        }

        private const uint Msk32 = 0xFFFFFFFF;
        private const ulong Fit27 = (B36.DMASK - 0x07FFFFFF);
        private const ulong Fit32 = (B36.DMASK - Msk32);
        private const bool Sfrc = true;
        private const bool Afrc = false;

// In packed floating point number 

        private static readonly int _fpBias = 0200.Oct(); // exponent bias 
        private const int FpNFhi = 27;
        private const int FpVFhi = 0;
        private static readonly ulong _fpMFhi = B36.OctUL(0000777777777);
        private const int FpNExp = 8;
        private const int FpVExp = (FpVFhi + FpNFhi);
        private static readonly ulong _fpMExp = B36.OctUL(0377);
        private const int FpVSign = (FpVExp + FpNExp);
        private const int FpNFlo = 35;
        //private static int FP_V_FLO; // must be zero 
        private static readonly ulong _fpMFlo = B36.OctUL(0377777777777);

        private static int getFpsign(ulong x)
        {
            //#define GET_FPSIGN(x)   ((int) (((x) >> FP_V_SIGN) & 1))
            return ((int) (((x) >> FpVSign) & 1));
        }

        private static int getFpexp(ulong x)
        {
            //#define GET_FPEXP(x)    ((int) (((x) >> FP_V_EXP) & FP_M_EXP))
            return ((int) (((x) >> FpVExp) & _fpMExp));
        }

        private static ulong getFphi(ulong x)
        {
            //#define GET_FPHI(x)     ((x) & FP_M_FHI)
            return ((x) & _fpMFhi);
        }

        private static ulong getFplo(ulong x)
        {
            //#define GET_FPLO(x)     ((x) & FP_M_FLO)
            return ((x) & _fpMFlo);
        }

        // In unpacked floating point number 

        private const int FpNGuard = 1;
        private const int FpVUflo = FpNGuard;
        //private static int FP_V_URNDD = (FP_V_UFLO - 1); // dp round bit 
        private const int FpVUfhi = (FpVUflo + FpNFlo);
        //private static int FP_V_URNDS = (FP_V_UFHI - 1); // sp round bit 
        private const int FpVUcry = (FpVUfhi + FpNFhi);
        private const int FpVUnorm = (FpVUcry - 1);
        private const ulong FpUfhi = 0x7FFFFFF000000000ul;
        private const ulong FpUflo = 0x0000000FFFFFFFFEul;
        private const ulong FpUfrac = 0x7FFFFFFFFFFFFFFEul;
        private const ulong FpUrndd = 0x0000000000000001ul;
        private const ulong FpUrnds = 0x0000000800000000ul;
        private const ulong FpUnorm = 0x4000000000000000ul;
        private const ulong FpUcry = 0x8000000000000000ul;
        private const ulong FpOnes = 0xFFFFFFFFFFFFFFFFul;

        private static ulong uneg(ulong x)
        {
            //#define UNEG(x)         ((~x) + 1)
            return ((~x) + 1);
        }

        private static void duneg(ref UFP x)
        {
            //#define DUNEG(x)        x.flo = UNEG (x.flo); x.fhi = ~x.fhi + (x.flo == 0)
            x.Flo = uneg(x.Flo);
            x.Fhi = ~x.Fhi + (x.Flo == 0).B();
        }

        // Integer multiply - checked against KS-10 ucode 

        private ulong imul(ulong a, ulong b)
        {
            var rsv = new ulong[2];

            if ((a == B36.SIGN) &&
                (b == B36.SIGN))
            {
                // KS10 hack 
                setf(_fAov | _fT1); // -2**35 squared 
                return B36.SIGN;
            }
            mul(a, b, rsv); // mpy, dprec result 
            if (rsv[0].NZ() &&
                (rsv[0] != B36.ONES))
            {
                // high not all sign? 
                rsv[1] = tstsf(a ^ b) ? sets(rsv[1]) : clrs(rsv[1]); // set sign 
                setf(_fAov | _fT1); // overflow 
            }
            return rsv[1];
        }

        // Integer divide, return quotient, remainder  - checked against KS10 ucode
        //   The KS10 does not recognize -2^35/-1 as an error.  Instead, it produces
        //   2^35 (that is, -2^35) as the incorrect result.

        private bool idiv(ulong a, ulong b, ulong[] rsa)
        {
            var dvd = abs(a); // make ops positive 
            var dvr = abs(b);

            if (dvr == 0)
            {
                // divide by 0? 
                setf(_fDck | _fAov | _fT1); // set flags, return 
                return false;
            }
            rsa[0] = dvd/dvr; // get quotient 
            rsa[1] = dvd%dvr; // get remainder 
            if (tstsf(a ^ b))
                rsa[0] = neg(rsa[0]); // sign of result 
            if (tstsf(a))
                rsa[1] = neg(rsa[1]); // sign of remainder 
            return true;
        }

        // Multiply, return double precision result - checked against KS10 ucode 

        private void mul(ulong s1, ulong s2, ulong[] rsa)
        {
            var a = abs(s1);
            var b = abs(s2);
            ulong r;

            if ((a == 0) || (b == 0))
            {
                // operand = 0? 
                rsa[0] = rsa[1] = 0; // result 0 
                return;
            }
            if ((a & Fit32).NZ() ||
                (b & Fit32).NZ())
            {
                // fit in 64b? 
                var t = a >> 18;
                a = a & B36.RMASK; // "dp" multiply 
                var u = b >> 18;
                b = b & B36.RMASK;
                r = (a*b) + (((a*u) + (b*t)) << 18); // low is only 35b 
                rsa[0] = ((t*u) << 1) + (r >> 35); // so lsh hi 1 
                rsa[1] = r & B36.MMASK;
            }
            else
            {
                r = a*b; // fits, native mpy 
                rsa[0] = r >> 35; // split at bit 35 
                rsa[1] = r & B36.MMASK;
            }

            if (tstsf(s1 ^ s2))
                mkdneg(rsa);
            else if (tstsf(rsa[0]))
            {
                // result +, 2**70? 
                setf(_fAov | _fT1); // overflow 
                rsa[1] = sets(rsa[1]); // consistent - 
            }
            return;
        }

        // Divide, return quotient and remainder - checked against KS10 ucode
        //   Note that the initial divide check catches the case -2^70/-2^35;
        //   thus, the quotient can have at most 35 bits.

        private bool divi(int ac, ulong b, ulong[] rsa)
        {
            var p1 = addac(ac, 1);
            var dvr = abs(b); // make divr positive 
            var dvd = new ulong[2];

            dvd[0] = AC[ac]; // divd high 
            dvd[1] = clrs(AC[p1]); // divd lo, clr sgn 
            if (tstsf(AC[ac]))
                dmovn(dvd);
            if (dvd[0] >= dvr)
            {
                // divide fail? 
                setf(_fAov | _fDck | _fT1); // set flags, return 
                return false;
            }
            if ((dvd[0] & Fit27).NZ())
            {
                // fit in 63b? 
                int i;
                for (i = 0, rsa[0] = 0; i < 35; i++)
                {
                    // 35 quotient bits 
                    dvd[0] = (dvd[0] << 1) | ((dvd[1] >> 34) & 1);
                    dvd[1] = (dvd[1] << 1) & B36.MMASK; // shift dividend 
                    rsa[0] = rsa[0] << 1; // shift quotient 
                    if (dvd[0] < dvr) continue;
                    // subtract work? 
                    dvd[0] = dvd[0] - dvr; // quo bit is 1 
                    rsa[0] = rsa[0] + 1;
                }
                rsa[1] = dvd[0]; // store remainder 
            }
            else
            {
                var t = (dvd[0] << 35) | dvd[1];
                rsa[0] = t/dvr; // quotient 
                rsa[1] = t%dvr; // remainder 
            }
            if (tstsf(AC[ac] ^ b))
                rsa[0] = neg(rsa[0]); // sign of result 
            if (tstsf(AC[ac]))
                rsa[1] = neg(rsa[1]); // sign of remainder 
            return true;
        }

        // Double precision multiply.  This is done the old fashioned way.  Cross
        //   product multiplies would be a lot faster but would require more code.

        private void dmul(int ac, ulong[] mpy)
        {
            var p1 = addac(ac, 1);
            var p2 = addac(ac, 2);
            var p3 = addac(ac, 3);
            int i;
            var mpc = new ulong[2];

            mpc[0] = AC[ac]; // mplcnd hi 
            mpc[1] = clrs(AC[p1]); // mplcnd lo, clr sgn 
            var sign = mpc[0] ^ mpy[0]; // sign of result 
            if (tstsf(mpc[0]))
                dmovn(mpc);
            if (tstsf(mpy[0]))
                dmovn(mpy);
            else
                mpy[1] = clrs(mpy[1]); // clear mpy lo sign 
            AC[ac] = AC[p1] = AC[p2] = AC[p3] = 0; // clear AC's 
            if (((mpy[0] | mpy[1]) == 0) ||
                ((mpc[0] | mpc[1]) == 0))
                return;
            for (i = 0; i < 71; i++)
            {
                // 71 mpyer bits 
                if (i.NZ())
                {
                    // shift res, mpy 
                    AC[p3] = (AC[p3] >> 1) | ((AC[p2] & 1) << 34);
                    AC[p2] = (AC[p2] >> 1) | ((AC[p1] & 1) << 34);
                    AC[p1] = (AC[p1] >> 1) | ((AC[ac] & 1) << 34);
                    AC[ac] = AC[ac] >> 1;
                    mpy[1] = (mpy[1] >> 1) | ((mpy[0] & 1) << 34);
                    mpy[0] = mpy[0] >> 1;
                }
                if ((mpy[1] & 1).NZ())
                {
                    // if mpy lo bit = 1 
                    AC[p1] = AC[p1] + mpc[1];
                    AC[ac] = AC[ac] + mpc[0] + (tsts((AC[p1] != 0).B())); // Huh?
                    AC[p1] = clrs(AC[p1]);
                }
            }
            if (tstsf(sign))
            {
                // result minus? 
                AC[p3] = (unegate(AC[p3])) & B36.MMASK; // quad negate 
                AC[p2] = (~AC[p2] + (AC[p3] == 0).B()) & B36.MMASK;
                AC[p1] = (~AC[p1] + (AC[p2] == 0).B()) & B36.MMASK;
                AC[ac] = (~AC[ac] + (AC[p1] == 0).B()) & B36.DMASK;
            }
            else if (tstsf(AC[ac]))
                setf(_fAov | _fT1); // wrong sign 
            if (tstsf(AC[ac]))
            {
                // if result - 
                AC[p1] = sets(AC[p1]); // make signs consistent 
                AC[p2] = sets(AC[p2]);
                AC[p3] = sets(AC[p3]);
            }
            return;
        }

        // Double precision divide - checked against KS10 ucode 

        private void ddiv(int ac, ulong[] dvr)
        {
            int i;
            var qu = new ulong[2];
            var dvd = new ulong[4];

            dvd[0] = AC[ac]; // save dividend 
            for (i = 1; i < 4; i++)
                dvd[i] = clrs(AC[addac(ac, i)]);
            var sign = AC[ac] ^ dvr[0];
            if (tstsf(AC[ac]))
            {
                // get abs (dividend) 
                int cryin;
                for (i = 3, cryin = 1; i > 0; i--)
                {
                    // negate quad 
                    dvd[i] = (~dvd[i] + (ulong) cryin) & B36.MMASK; // comp + carry in 
                    if (dvd[i].NZ())
                        cryin = 0; // next carry in 
                }
                dvd[0] = (~dvd[0] + (ulong) cryin) & B36.DMASK;
            }
            if (tstsf(dvr[0]))
                dmovn(dvr);
            else
                dvr[1] = clrs(dvr[1]);
            if (dcmpge(dvd, dvr))
            {
                // will divide work? 
                setf(_fAov | _fDck | _fT1); // no, set flags 
                return;
            }
            qu[0] = qu[1] = 0; // clear quotient 
            for (i = 0; i < 70; i++)
            {
                // 70 quotient bits 
                dvd[0] = ((dvd[0] << 1) | ((dvd[1] >> 34) & 1)) & B36.DMASK;

                dvd[1] = ((dvd[1] << 1) | ((dvd[2] >> 34) & 1)) & B36.MMASK;
                dvd[2] = ((dvd[2] << 1) | ((dvd[3] >> 34) & 1)) & B36.MMASK;
                dvd[3] = (dvd[3] << 1) & B36.MMASK; // shift dividend 
                qu[0] = (qu[0] << 1) | ((qu[1] >> 34) & 1); // shift quotient 
                qu[1] = (qu[1] << 1) & B36.MMASK;
                if (dcmpge(dvd, dvr))
                {
                    // subtract work? 
                    dvd[0] = dvd[0] - dvr[0] - (dvd[1] < dvr[1]).B();
                    dvd[1] = (dvd[1] - dvr[1]) & B36.MMASK; // do subtract 
                    qu[1] = qu[1] + 1; // set quotient bit 
                }
            }
            if (tstsf(sign) && (qu[0] | qu[1]).NZ())
                mkdneg(qu);
            if (tstsf(AC[ac]) && (dvd[0] | dvd[1]).NZ())
                mkdneg(dvd);
            AC[ac] = qu[0]; // quotient 
            AC[addac(ac, 1)] = qu[1];
            AC[addac(ac, 2)] = dvd[0]; // remainder 
            AC[addac(ac, 3)] = dvd[1];
            return;
        }

        // Single precision floating add/subtract - checked against KS10 ucode
        //   The KS10 shifts the smaller operand regardless of the exponent diff.
        //   This code will not shift more than 63 places; shifts beyond that
        //   cannot change the value of the smaller operand.

        //   If the signs of the operands are the same, the result sign is the
        //   same as the source sign; the sign of the result fraction is actually
        //   part of the data.  If the signs of the operands are different, the
        //   result sign is determined by the fraction sign.

        private ulong fad(ulong op1, ulong op2, bool rnd, int inv)
        {
            UFP a;
            UFP b;

            if (inv.NZ()) op2 = neg(op2); // subtract? -b 
            if (op1 == 0)
                funpack(op2, 0, out a, Afrc); // a = 0? result is b 
            else if (op2 == 0)
                funpack(op1, 0, out a, Afrc); // b = 0? result is a 
            else
            {
                funpack(op1, 0, out a, Sfrc); // unpack operands 
                funpack(op2, 0, out b, Sfrc); // fracs are 2's comp 
                var ediff = a.Exp - b.Exp;
                if (ediff < 0)
                {
                    // a < b? switch 
                    var t = a;
                    a = b;
                    b = t;
                    ediff = -ediff;
                }
                if (ediff > 63)
                    ediff = 63; // cap diff at 63 
                if (ediff.NZ())
                    b.Fhi = b.Fhi >> ediff; // shift b (signed) 
                a.Fhi = a.Fhi + b.Fhi; // add fractions 
                if ((a.Sign ^ b.Sign).NZ()) // add or subtract? 
                    if ((a.Fhi & FpUcry).NZ())
                    {
                        // subtract, frac -? 
                        a.Fhi = uneg(a.Fhi); // complement result 
                        a.Sign = 1; // result is - 
                    }
                    else a.Sign = 0; // result is + 
                else
                {
                    if (a.Sign.NZ())
                        a.Fhi = uneg(a.Fhi); // add, src -? comp 
                    if ((a.Fhi & FpUcry).NZ())
                    {
                        // check for carry 
                        a.Fhi = a.Fhi >> 1; // flo won't be used 
                        a.Exp = a.Exp + 1;
                    }
                }
            }
            fnorm(ref a, (rnd ? FpUrnds : 0)); // normalize, round 
            ulong dis;
            return fpack(a, out dis, false, false);
        }

        // Single precision floating multiply.  Because the fractions are 27b,
        //   a 64b multiply can be used for the fraction multiply.  The 27b
        //   fractions are positioned 0'frac'0000, resulting in 00'hifrac'0..0.
        //   The extra 0 is accounted for by biasing the result exponent.

        private const int FpVSpm = (FpVUfhi - (32 - FpNFhi - 1));

        private ulong fmp(ulong op1, ulong op2, bool rnd)
        {
            UFP a, b;

            funpack(op1, 0, out a, Afrc); // unpack operands 
            funpack(op2, 0, out b, Afrc); // fracs are abs val 
            if ((a.Fhi == 0) ||
                (b.Fhi == 0))
                return 0; // either 0?  
            a.Sign = a.Sign ^ b.Sign; // result sign 
            a.Exp = a.Exp + b.Exp - _fpBias + 1; // result exponent 
            a.Fhi = (a.Fhi >> FpVSpm)*(b.Fhi >> FpVSpm); // high 27b of result 
            fnorm(ref a, (rnd ? FpUrnds : 0)); // normalize, round 
            ulong dis;
            return fpack(a, out dis, false, false);
        }

        // Single precision floating divide.  Because the fractions are 27b, a
        //   64b divide can be used for the fraction divide.  Note that 28b-29b
        //   of fraction are developed; the code will do one special normalize to
        //   make sure that the 28th bit is not lost.  Also note the special
        //   treatment of negative quotients with non-zero remainders; this
        //   implements the note on p2-23 of the Processor Reference Manual.

        private bool fdv(ulong op1, ulong op2, out ulong rsv, bool rnd)
        {
            UFP a, b;
            ulong savhi;
            var rem = false;

            funpack(op1, 0, out a, Afrc); // unpack operands 
            funpack(op2, 0, out b, Afrc); // fracs are abs val 
            if (a.Fhi >= 2*b.Fhi)
            {
                // will divide work? 
                setf(_fAov | _fDck | _fFov | _fT1);
                rsv = 0;
                return false;
            }
            if ((savhi = a.Fhi).NZ())
            {
                // dvd = 0? quo = 0 
                a.Sign = a.Sign ^ b.Sign; // result sign 
                a.Exp = a.Exp - b.Exp + _fpBias + 1; // result exponent 
                a.Fhi = a.Fhi/(b.Fhi >> (FpNFhi + 1)); // do divide 
                if (a.Sign.NZ() &&
                    (savhi != (a.Fhi*(b.Fhi >> (FpNFhi + 1)))))
                    rem = true; // KL/KS hack 
                a.Fhi = a.Fhi << (FpVUnorm - FpNFhi - 1); // put quo in place 
                if ((a.Fhi & FpUnorm) == 0)
                {
                    // normalize 1b 
                    a.Fhi = a.Fhi << 1; // before masking 
                    a.Exp = a.Exp - 1;
                }
                a.Fhi = a.Fhi & (FpUfhi | FpUrnds); // mask quo to 28b 
            }
            fnorm(ref a, (rnd ? FpUrnds : 0)); // normalize, round 
            ulong dis;
            rsv = fpack(a, out dis, false, rem); // pack result 
            return true;
        }

        // Single precision floating scale. 

        private ulong fsc(ulong val, ulong eav)
        {
            var sc = lit8((int) eav);
            UFP a;

            if (val == 0)
                return 0;

            funpack(val, 0, out a, Afrc); // unpack operand 
            if ((eav & B36.RSIGN).NZ())
                a.Exp = a.Exp - sc; // adjust exponent 
            else a.Exp = a.Exp + sc;
            fnorm(ref a, 0); // renormalize 
            ulong dis;
            return fpack(a, out dis, false, false); // pack result 
        }

        // Float integer operand and round 

        private ulong fltr(ulong mbv)
        {
            UFP a;
            var val = abs(mbv);

            a.Sign = getFpsign(mbv); // get sign 
            a.Exp = _fpBias + 36; // initial exponent 
            a.Fhi = val << (FpVUnorm - 35); // left justify op 
            a.Flo = 0;
            fnorm(ref a, FpUrnds); // normalize, round 
            ulong dis;
            return fpack(a, out dis, false, false); // pack result 
        }

        // Fix and truncate/round floating operand 

        private void fix(int ac, ulong mbv, bool rnd)
        {
            UFP a;

            funpack(mbv, 0, out a, Afrc); // unpack operand 
            if (a.Exp > (_fpBias + FpNFhi + FpNExp))
                setf(_fAov | _fT1);
            else if (a.Exp < _fpBias)
                AC[ac] = 0; // < 1/2? 
            else
            {
                var sc = FpVUnorm - (a.Exp - _fpBias) + 1;
                AC[ac] = a.Fhi >> sc;
                if (rnd)
                {
                    var so = a.Fhi << (64 - sc);
                    if (so >= (0x8000000000000000 + (ulong) a.Sign)) AC[ac] = AC[ac] + 1;
                }
                if (a.Sign.NZ())
                    AC[ac] = neg(AC[ac]);
            }
            return;
        }

        //Double precision floating add/subtract
        //  Since a.flo is 0, adding b.flo is just a copy - this is incorporated into
        //  the denormalization step.  If there's no denormalization, bflo is zero too.

        private void dfad(int ac, ulong[] rsa, int inv)
        {
            var p1 = addac(ac, 1);
            UFP a, b;

            if (inv.NZ()) dmovn(rsa);
            if ((AC[ac] | AC[p1]) == 0)
                funpack(rsa[0], rsa[1], out a, Afrc);
                // a == 0? sum = b 
            else if ((rsa[0] | rsa[1]) == 0)
                funpack(AC[ac], AC[p1], out a, Afrc);
                // b == 0? sum = a 
            else
            {
                funpack(AC[ac], AC[p1], out a, Sfrc); // unpack operands 
                funpack(rsa[0], rsa[1], out b, Sfrc);
                var ediff = a.Exp - b.Exp;
                if (ediff < 0)
                {
                    // a < b? switch 
                    var t = a;
                    a = b;
                    b = t;
                    ediff = -ediff;
                }
                if (ediff > 127)
                    ediff = 127; // cap diff at 127 
                if (ediff > 63)
                {
                    // diff > 63? 
                    a.Flo = (b.Fhi >> (ediff - 64)); // b hi to a lo 
                    b.Fhi = b.Sign.NZ() ? FpOnes : 0; // hi = all sign 
                }
                else if (ediff.NZ())
                {
                    // diff <= 63 
                    a.Flo = (b.Flo >> ediff) | (b.Fhi << (64 - ediff));
                    b.Fhi = b.Fhi >> ediff; // shift b (signed) 
                }
                a.Fhi = a.Fhi + b.Fhi; // do add 
                if ((a.Sign ^ b.Sign).NZ()) // add or subtract? 
                    if ((a.Fhi & FpUcry).NZ())
                    {
                        // subtract, frac -? 
                        duneg(ref a); // complement result 
                        a.Sign = 1; // result is - 
                    }
                    else a.Sign = 0; // result is + 
                else
                {
                    if (a.Sign.NZ())
                        duneg(ref a);
                    // add, src -? comp 
                    if ((a.Fhi & FpUcry).NZ())
                    {
                        // check for carry 
                        a.Fhi = a.Fhi >> 1; // flo won't be used 
                        a.Exp = a.Exp + 1;
                    }
                }
            }
            fnorm(ref a, FpUrndd); // normalize, round 
            ulong res;
            AC[ac] = fpack(a, out res, true, false); // pack result 
            AC[p1] = res;
            return;
        }

        // Double precision floating multiply
        //   The 62b fractions are multiplied, with cross products, to produce a
        //   124b fraction with two leading and two trailing 0's.  Because the
        //   product has 2 leading 0's, instead of the normal 1, an extra
        //   normalization step is needed.  Accordingly, the exponent calculation
        //   increments the result exponent, to compensate for normalization.

        private void dfmp(int ac, ulong[] rsv)
        {
            var p1 = addac(ac, 1);
            UFP a, b;

            funpack(AC[ac], AC[p1], out a, Afrc); // unpack operands 
            funpack(rsv[0], rsv[1], out b, Afrc);

            if ((a.Fhi == 0) || (b.Fhi == 0))
            {
                // either 0? result 0 
                AC[ac] = AC[p1] = 0;
                return;
            }

            a.Sign = a.Sign ^ b.Sign; // result sign 
            a.Exp = a.Exp + b.Exp - _fpBias + 1; // result exponent 
            var xh = a.Fhi >> 32;
            var xl = a.Fhi & Msk32;
            var yh = b.Fhi >> 32;
            var yl = b.Fhi & Msk32;
            a.Fhi = xh*yh; // hi xproduct 
            a.Flo = xl*yl; // low xproduct 
            var mid = (xh*yl) + (yh*xl);
            a.Flo = a.Flo + (mid << 32); // add mid lo to lo 
            a.Fhi = a.Fhi + ((mid >> 32) & Msk32) + (a.Flo < (mid << 32)).B();
            fnorm(ref a, FpUrndd); // normalize, round 
            ulong res;
            AC[ac] = fpack(a, out res, true, false); // pack result 
            AC[p1] = res;

            return;
        }

        // Double precision floating divide
        //   This algorithm develops a full 62 bits of quotient, plus one rounding
        //   bit, in the low order 63b of a 64b number.  To do this, we must assure
        //   that the initial divide step generates a 1.  If it would fail, shift
        //   the dividend left and decrement the result exponent accordingly.

        private void dfdv(int ac, ulong[] rsv)
        {
            var p1 = addac(ac, 1);
            ulong qu = 0;
            UFP a, b;

            funpack(AC[ac], AC[p1], out a, Afrc); // unpack operands 
            funpack(rsv[0], rsv[1], out b, Afrc);

            if (a.Fhi >= 2*b.Fhi)
            {
                // will divide work? 
                setf(_fAov | _fDck | _fFov | _fT1);
                return;
            }

            if (a.Fhi.NZ())
            {
                // dvd = 0? quo = 0 
                a.Sign = a.Sign ^ b.Sign; // result sign 
                a.Exp = a.Exp - b.Exp + _fpBias + 1; // result exponent 
                if (a.Fhi < b.Fhi)
                {
                    // make sure initial 
                    a.Fhi = a.Fhi << 1; // divide step will work 
                    a.Exp = a.Exp - 1;
                }
                int i;
                for (i = 0; i < 63; i++)
                {
                    // 63b of quotient 
                    qu = qu << 1; // shift quotient 
                    if (a.Fhi >= b.Fhi)
                    {
                        // will div work? 
                        a.Fhi = a.Fhi - b.Fhi; // sub, quo = 1 
                        qu = qu + 1;
                    }
                    a.Fhi = a.Fhi << 1; // shift dividend 
                }
                a.Fhi = qu;
            }

            fnorm(ref a, FpUrndd); // normalize, round 
            ulong res;
            AC[ac] = fpack(a, out res, true, false); // pack result 
            AC[p1] = res;

            return;
        }

        // Unpack floating point operand 

        private static void funpack(ulong h, ulong l, out UFP r, bool sgn)
        {
            r = new UFP {Sign = getFpsign(h), Exp = getFpexp(h)};

            var fphi = getFphi(h);
            var fplo = getFplo(l);

            r.Fhi = (fphi << FpVUfhi) | (fplo << FpVUflo);
            r.Flo = 0;

            if (r.Sign.NZ())
            {
                r.Exp = r.Exp ^ (int) _fpMExp; // 1s comp exp 
                if (sgn) // signed frac? 
                    if (r.Fhi.NZ())
                        r.Fhi = r.Fhi | FpUcry; // extend sign 
                    else
                    {
                        r.Exp = r.Exp + 1;
                        r.Fhi = FpUcry | FpUnorm;
                    }
                else // abs frac 
                    if (r.Fhi.NZ())
                        r.Fhi = uneg(r.Fhi) & FpUfrac;
                    else
                    {
                        r.Exp = r.Exp + 1;
                        r.Fhi = FpUnorm;
                    }
            }
            return;
        }

        // Normalize and optionally round floating point operand 

        private static void fnorm(ref UFP a, ulong rnd)
        {
            a = new UFP();
            var normmask = new ulong[]
                               {
                                   0x6000000000000000,
                                   0x7800000000000000,
                                   0x7F80000000000000,
                                   0x7FFF800000000000,
                                   0x7FFFFFFF80000000,
                                   0x7FFFFFFFFFFFFFFF
                               };
            var normtab = new[] {1, 2, 4, 8, 16, 32, 63};

            if ((a.Fhi & FpUcry).NZ())
            {
                // carry set? 
                //printf("%%PDP-10 FP: carry bit set at normalization, PC = %o\n", pager_PC);
                a.Flo = (a.Flo >> 1) | ((a.Fhi & 1) << 63); // try to recover 
                a.Fhi = a.Fhi >> 1; // but root cause 
                a.Exp = a.Exp + 1; // should be fixed! 
            }
            if ((a.Fhi | a.Flo) == 0)
            {
                // if fraction = 0 
                a.Sign = a.Exp = 0; // result is 0 
                return;
            }
            while ((a.Fhi & FpUnorm) == 0)
            {
                // normalized? 
                int i;
                for (i = 0; i < 6; i++)
                    if ((a.Fhi & normmask[i]).NZ()) break;
                a.Fhi = (a.Fhi << normtab[i]) | (a.Flo >> (64 - normtab[i]));
                a.Flo = a.Flo << normtab[i];
                a.Exp = a.Exp - normtab[i];
            }
            if (rnd.NZ())
            {
                // rounding? 
                a.Fhi = a.Fhi + rnd; // add round const 
                if ((a.Fhi & FpUcry).NZ())
                {
                    // if carry out, 
                    a.Fhi = a.Fhi >> 1; // renormalize 
                    a.Exp = a.Exp + 1;
                }
            }
            return;
        }

        // Pack floating point result 

        private ulong fpack(UFP r, out ulong lo, bool setlo, bool fdvneg)
        {
            var val = new ulong[2];

            if (r.Exp < 0)
                setf(_fAov | _fFov | _fFxu | _fT1);
            else if ((ulong) r.Exp > _fpMExp)
                setf(_fAov | _fFov | _fT1);
            val[0] = (((((ulong) r.Exp) & _fpMExp) << FpVExp) |
                      ((r.Fhi & FpUfhi) >> FpVUfhi)) & B36.DMASK;
            if (setlo)
                val[1] = ((r.Fhi & FpUflo) >> FpVUflo) & B36.MMASK;
            else
                val[1] = 0;
            if (r.Sign.NZ()) // negate? 
                if (fdvneg)
                {
                    // fdvr special? 
                    val[1] = ~val[1] & B36.MMASK; // 1's comp 
                    val[0] = ~val[0] & B36.DMASK;
                }
                else
                    dmovn(val);
            lo = 0;
            if (setlo)
                lo = val[1];
            return val[0];
        }
    }
}