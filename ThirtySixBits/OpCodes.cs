﻿namespace PDP10CPU
{
    public enum OpCodes
    {
        MUUO0 = 0, // 00 
        LUUO1 = 1, // 01 
        LUUO2 = 2, // 02 
        LUUO3 = 3, // 03 
        LUUO4 = 4, // 04 
        LUUO5 = 5, // 05 
        LUUO6 = 6, // 06 
        LUUO7 = 7, // 07 
        LUUO10 = 8, // 010 
        LUUO11 = 9, // 011 
        LUUO12 = 10, // 012 
        LUUO13 = 11, // 013 
        LUUO14 = 12, // 014 
        LUUO15 = 13, // 015 
        LUUO16 = 14, // 016 
        LUUO17 = 15, // 017 
        LUUO20 = 16, // 020 
        LUUO21 = 17, // 021 
        LUUO22 = 18, // 022 
        LUUO23 = 19, // 023 
        LUUO24 = 20, // 024 
        LUUO25 = 21, // 025 
        LUUO26 = 22, // 026 
        LUUO27 = 23, // 027 
        LUUO30 = 24, // 030 
        LUUO31 = 25, // 031 
        LUUO32 = 26, // 032 
        LUUO33 = 27, // 033 
        LUUO34 = 28, // 034 
        LUUO35 = 29, // 035 
        LUUO36 = 30, // 036 
        LUUO37 = 31, // 037 
        CALL = 32, // 040 
        INIT = 33, // 041 
        UUO42 = 34, // 042 
        UUO43 = 35, // 043 
        UUO44 = 36, // 044 
        UUO45 = 37, // 045 
        UUO46 = 38, // 046 
        CALLI = 39, // 047 
        OPEN = 40, // 050 
        TTCALL = 41, // 051 
        RDCLK = 42, // 052 
        UUO53 = 43, // 053 
        UUO54 = 44, // 054 
        RENAME = 45, // 055 
        IN = 46, // 056 
        OUT = 47, // 057 
        SETSTS = 48, // 060 
        STATO = 49, // 061 
        GETSTS = 50, // 062 
        STATUS = 50, // 062 
        STATZ = 51, // 063 
        INBUF = 52, // 064 
        OUTBUF = 53, // 065 
        INPUT = 54, // 066 
        OUTPUT = 55, // 067 
        CLOSE = 56, // 070 
        RELEAS = 57, // 071 
        MTAPE = 58, // 072 
        UGETF = 59, // 073 
        USETI = 60, // 074 
        USETO = 61, // 075 
        LOOKUP = 62, // 076 
        ENTER = 63, // 077 
        UJEN = 64, // 0100 
        X0102 = 66, // 0101 ?
        X0103 = 67, // 0102 ?
        JSYS = 68, // 0104 
        ADJSP = 69, // 0105 
        UUO106 = 70, // 0106 
        UUO107 = 71, // 0107 
        DFAD = 72, // 0110 
        DFSB = 73, // 0111 
        DFMP = 74, // 0112 
        DFDV = 75, // 0113 
        DADD = 76, // 0114 
        DSUB = 77, // 0115 
        DMUL = 78, // 0116 
        DDIV = 79, // 0117 
        DMOVE = 80, // 0120 
        DMOVN = 81, // 0121 
        EXTEND = 83, // 0123 
        DMOVEM = 84, // 0124 
        DMOVNM = 85, // 0125 
        FIXR = 86, // 0126 
        FLTR = 87, // 0127 
        FIX = 88, // 0130 
        UFA = 88, // 0130 
        DFN = 89, // 0131 
        FSC = 90, // 0132 
        IBP = 91, // 0133 
        ADJBP = 91, // 0133 
        ILDB = 92, // 0134 
        ILBP = 92, // 0134	 ??
        LDB = 93, // 0135 
        IDPB = 94, // 0136 
        IDBP = 94, // 0136	  ??
        DPB = 95, // 0137 
        FAD = 96, // 0140 
        FADL = 97, // 0141 
        FADM = 98, // 0142 
        FADB = 99, // 0143 
        FADR = 100, // 0144 
        FADRI = 101, // 0145 
        FADRM = 102, // 0146 
        FADRB = 103, // 0147 
        FSB = 104, // 0150 
        FSBL = 105, // 0151 
        FSBM = 106, // 0152 
        FSBB = 107, // 0153 
        FSBR = 108, // 0154 
        FSBRI = 109, // 0155 
        FSBRM = 110, // 0156 
        FSBRB = 111, // 0157 
        FMP = 112, // 0160 
        FMPL = 113, // 0161 
        FMPM = 114, // 0162 
        FMPB = 115, // 0163 
        FMPR = 116, // 0164 
        FMPRI = 117, // 0165 
        FMPRM = 118, // 0166 
        FMPRB = 119, // 0167 
        FDV = 120, // 0170 
        FDVL = 121, // 0171 
        FDVM = 122, // 0172 
        FDVB = 123, // 0173 
        FDVR = 124, // 0174 
        FDVRI = 125, // 0175 
        FDVRM = 126, // 0176 
        FDVRB = 127, // 0177 
        MOVE = 128, // 0200 
        MOVEI = 129, // 0201 
        MOVEM = 130, // 0202 
        MOVES = 131, // 0203 
        MOVS = 132, // 0204 
        MOVSI = 133, // 0205 
        MOVSM = 134, // 0206 
        MOVSS = 135, // 0207 
        MOVN = 136, // 0210 
        MOVNI = 137, // 0211 
        MOVNM = 138, // 0212 
        MOVNS = 139, // 0213 
        MOVM = 140, // 0214 
        MOVMI = 141, // 0215 
        MOVMM = 142, // 0216 
        MOVMS = 143, // 0217 
        IMUL = 144, // 0220 
        IMULI = 145, // 0221 
        IMULM = 146, // 0222 
        IMULB = 147, // 0223 
        MUL = 148, // 0224 
        MULI = 149, // 0225 
        MULM = 150, // 0226 
        MULB = 151, // 0227 
        IDIV = 152, // 0230 
        IDIVI = 153, // 0231 
        IDIVM = 154, // 0232 
        IDIVB = 155, // 0233 
        DIV = 156, // 0234 
        DIVI = 157, // 0235 
        DIVM = 158, // 0236 
        DIVB = 159, // 0237 
        ASH = 160, // 0240 
        ROT = 161, // 0241 
        LSH = 162, // 0242 
        JFFO = 163, // 0243 
        ASHC = 164, // 0244 
        ROTC = 165, // 0245 
        LSHC = 166, // 0246 
        CIRC = 167, // 247 (ITS)
        EXCH = 168, // 0250 
        BLT = 169, // 0251 
        AOBJP = 170, // 0252 
        AOBJN = 171, // 0253 
        JRST = 172, // 0254 
        JFCL = 173, // 0255 
        XCT = 174, // 0256 
        MAP = 175, // 0257 
        PUSHJ = 176, // 0260 
        PUSH = 177, // 0261 
        POP = 178, // 0262 
        POPJ = 179, // 0263 
        JSR = 180, // 0264 
        JSP = 181, // 0265 
        JSA = 182, // 0266 
        JRA = 183, // 0267 
        ADD = 184, // 0270 
        ADDI = 185, // 0271 
        ADDM = 186, // 0272 
        ADDB = 187, // 0273 
        SUB = 188, // 0274 
        SUBI = 189, // 0275 
        SUBM = 190, // 0276 
        SUBB = 191, // 0277 
        CAI = 192, // 0300 
        CAIL = 193, // 0301 
        CAIE = 194, // 0302 
        CAILE = 195, // 0303 
        CAIA = 196, // 0304 
        CAIGE = 197, // 0305 
        CAIN = 198, // 0306 
        CAIG = 199, // 0307 
        CAM = 200, // 0310 
        CAML = 201, // 0311 
        CAME = 202, // 0312 
        CAMLE = 203, // 0313 
        CAMA = 204, // 0314 
        CAMGE = 205, // 0315 
        CAMN = 206, // 0316 
        CAMG = 207, // 0317 
        ARG = 208, // 0320 
        JUMP = 208, // 0320 
        JUMPL = 209, // 0321 
        JUMPE = 210, // 0322 
        JUMPLE = 211, // 0323 
        JUMPA = 212, // 0324 
        JUMPGE = 213, // 0325 
        JUMPN = 214, // 0326 
        JUMPG = 215, // 0327 
        SKIP = 216, // 0330 
        SKIPL = 217, // 0331 
        SKIPE = 218, // 0332 
        SKIPLE = 219, // 0333 
        SKIPA = 220, // 0334 
        SKIPGE = 221, // 0335 
        SKIPN = 222, // 0336 
        SKIPG = 223, // 0337 
        AOJ = 224, // 0340 
        AOJL = 225, // 0341 
        AOJE = 226, // 0342 
        AOJLE = 227, // 0343 
        AOJA = 228, // 0344 
        AOJGE = 229, // 0345 
        AOJN = 230, // 0346 
        AOJG = 231, // 0347 
        AOS = 232, // 0350 
        AOSL = 233, // 0351 
        AOSE = 234, // 0352 
        AOSLE = 235, // 0353 
        AOSA = 236, // 0354 
        AOSGE = 237, // 0355 
        AOSN = 238, // 0356 
        AOSG = 239, // 0357 
        SOJ = 240, // 0360 
        SOJL = 241, // 0361 
        SOJE = 242, // 0362 
        SOJLE = 243, // 0363 
        SOJA = 244, // 0364 
        SOJGE = 245, // 0365 
        SOJN = 246, // 0366 
        SOJG = 247, // 0367 
        SOS = 248, // 0370 
        SOSL = 249, // 0371 
        SOSE = 250, // 0372 
        SOSLE = 251, // 0373 
        SOSA = 252, // 0374 
        SOSGE = 253, // 0375 
        SOSN = 254, // 0376 
        SOSG = 255, // 0377 
        SETZ = 256, // 0400 
        CLEAR = 256, // 0400 
        SETZI = 257, // 0401 
        CLEARI = 257, // 0401 
        SETZM = 258, // 0402 
        CLEARM = 258, // 0402 
        SETZB = 259, // 0403 
        CLEARB = 259, // 0403 
        AND = 260, // 0404 
        ANDI = 261, // 0405 
        ANDM = 262, // 0406 
        ANDB = 263, // 0407 
        ANDCA = 264, // 0410 
        ANDCAI = 265, // 0411 
        ANDCAM = 266, // 0412 
        ANDCAB = 267, // 0413 
        SETM = 268, // 0414 
        SETMI = 269, // 0415 
        SETMM = 270, // 0416 
        SETMB = 271, // 0417 
        ANDCM = 272, // 0420 
        ANDCMI = 273, // 0421 
        ANDCMM = 274, // 0422 
        ANDCMB = 275, // 0423 
        SETA = 276, // 0424 
        SETAI = 277, // 0425 
        SETAM = 278, // 0426 
        SETAB = 279, // 0427 
        XOR = 280, // 0430 
        XORI = 281, // 0431 
        XORM = 282, // 0432 
        XORB = 283, // 0433 
        IOR = 284, // 0434 
        OR = 284, // 0434 
        IORI = 285, // 0435 
        ORI = 285, // 0435 
        IORM = 286, // 0436 
        ORM = 286, // 0436 
        IORB = 287, // 0437 
        ORB = 287, // 0437 
        ANDCB = 288, // 0440 
        ANDCBI = 289, // 0441 
        ANDCBM = 290, // 0442 
        ANDCBB = 291, // 0443 
        EQV = 292, // 0444 
        EQVI = 293, // 0445 
        EQVM = 294, // 0446 
        EQVB = 295, // 0447 
        SETCA = 296, // 0450 
        SETCAI = 297, // 0451 
        SETCAM = 298, // 0452 
        SETCAB = 299, // 0453 
        ORCA = 300, // 0454 
        ORCAI = 301, // 0455 
        ORCAM = 302, // 0456 
        ORCAB = 303, // 0457 
        SETCM = 304, // 0460 
        SETCMI = 305, // 0461 
        SETCMM = 306, // 0462 
        SETCMB = 307, // 0463 
        ORCM = 308, // 0464 
        ORCMI = 309, // 0465 
        ORCMM = 310, // 0466 
        ORCMB = 311, // 0467 
        ORCB = 312, // 0470 
        ORCBI = 313, // 0471 
        ORCBM = 314, // 0472 
        ORCBB = 315, // 0473 
        SETO = 316, // 0474 
        SETOI = 317, // 0475 
        SETOM = 318, // 0476 
        SETOB = 319, // 0477 
        HLL = 320, // 0500 
        HLLI = 321, // 0501 
        HLLM = 322, // 0502 
        HLLS = 323, // 0503 
        HRL = 324, // 0504 
        HRLI = 325, // 0505 
        HRLM = 326, // 0506 
        HRLS = 327, // 0507 
        HLLZ = 328, // 0510 
        HLLZI = 329, // 0511 
        HLLZM = 330, // 0512 
        HLLZS = 331, // 0513 
        HRLZ = 332, // 0514 
        HRLZI = 333, // 0515 
        HRLZM = 334, // 0516 
        HRLZS = 335, // 0517 
        HLLO = 336, // 0520 
        HLLOI = 337, // 0521 
        HLLOM = 338, // 0522 
        HLLOS = 339, // 0523 
        HRLO = 340, // 0524 
        HRLOI = 341, // 0525 
        HRLOM = 342, // 0526 
        HRLOS = 343, // 0527 
        HLLE = 344, // 0530 
        HLLEI = 345, // 0531 
        HLLEM = 346, // 0532 
        HLLES = 347, // 0533 
        HRLE = 348, // 0534 
        HRLEI = 349, // 0535 
        HRLEM = 350, // 0536 
        HRLES = 351, // 0537 
        HRR = 352, // 0540 
        HRRI = 353, // 0541 
        HRRM = 354, // 0542 
        HRRS = 355, // 0543 
        HLR = 356, // 0544 
        HLRI = 357, // 0545 
        HLRM = 358, // 0546 
        HLRS = 359, // 0547 
        HRRZ = 360, // 0550 
        HRRZI = 361, // 0551 
        HRRZM = 362, // 0552 
        HRRZS = 363, // 0553 
        HLRZ = 364, // 0554 
        HLRZI = 365, // 0555 
        HLRZM = 366, // 0556 
        HLRZS = 367, // 0557 
        HRRO = 368, // 0560 
        HRROI = 369, // 0561 
        HRROM = 370, // 0562 
        HRROS = 371, // 0563 
        HLRO = 372, // 0564 
        HLROI = 373, // 0565 
        HLROM = 374, // 0566 
        HLROS = 375, // 0567 
        HRRE = 376, // 0570 
        HRREI = 377, // 0571 
        HRREM = 378, // 0572 
        HRRES = 379, // 0573 
        HLRE = 380, // 0574 
        HLREI = 381, // 0575 
        HLREM = 382, // 0576 
        HLRES = 383, // 0577 
        TRN = 384, // 0600 
        TLN = 385, // 0601 
        TRNE = 386, // 0602 
        TLNE = 387, // 0603 
        TRNA = 388, // 0604 
        TLNA = 389, // 0605 
        TRNN = 390, // 0606 
        TLNN = 391, // 0607 
        TDN = 392, // 0610 
        TSN = 393, // 0611 
        TDNE = 394, // 0612 
        TSNE = 395, // 0613 
        TDNA = 396, // 0614 
        TSNA = 397, // 0615 
        TDNN = 398, // 0616 
        TSNN = 399, // 0617 
        TRZ = 400, // 0620 
        TLZ = 401, // 0621 
        TRZE = 402, // 0622 
        TLZE = 403, // 0623 
        TRZA = 404, // 0624 
        TLZA = 405, // 0625 
        TRZN = 406, // 0626 
        TLZN = 407, // 0627 
        TDZ = 408, // 0630 
        TSZ = 409, // 0631 
        TDZE = 410, // 0632 
        TSZE = 411, // 0633 
        TDZA = 412, // 0634 
        TSZA = 413, // 0635 
        TDZN = 414, // 0636 
        TSZN = 415, // 0637 
        TRC = 416, // 0640 
        TLC = 417, // 0641 
        TRCE = 418, // 0642 
        TLCE = 419, // 0643 
        TRCA = 420, // 0644 
        TLCA = 421, // 0645 
        TRCN = 422, // 0646 
        TLCN = 423, // 0647 
        TDC = 424, // 0650 
        TSC = 425, // 0651 
        TDCE = 426, // 0652 
        TSCE = 427, // 0653 
        TDCA = 428, // 0654 
        TSCA = 429, // 0655 
        TDCN = 430, // 0656 
        TSCN = 431, // 0657 
        TRO = 432, // 0660 
        TLO = 433, // 0661 
        TROE = 434, // 0662 
        TLOE = 435, // 0663 
        TROA = 436, // 0664 
        TLOA = 437, // 0665 
        TRON = 438, // 0666 
        TLON = 439, // 0667 
        TDO = 440, // 0670 
        TSO = 441, // 0671 
        TDOE = 442, // 0672 
        TSOE = 443, // 0673 
        TDOA = 444, // 0674 
        TSOA = 445, // 0675 
        TDON = 446, // 0676 
        TSON = 447, // 0677 
    }
}