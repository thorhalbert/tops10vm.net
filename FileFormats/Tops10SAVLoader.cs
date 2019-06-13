using System;
using System.IO;
using PDP10CPU.Memory;
using ThirtySixBits;

namespace FileFormats
{
    public class Tops10SAVLoader
    {
        public UserModeCore Core { get; set; }

        public Word18 Transaddr { get; private set; }

        public Word18 Hiseg_floor { get; private set; }

        public Word18 High_Water { get; private set; }

        public Tops10SAVLoader(UserModeCore core, string infile)
        {
            Core = core;
            //ulong pop, fmt;
            //ulong lw = 0, hwm = 0;

            var hwmPage = 0;

            var hw = 0;

            var lw = 0;

            if (!Core.PageExists(0))
                Core.NewPage(0);

            // Eventually call our raw file reader for 36 bit files
            var inexe = new StreamReader(infile);

            // Read in first word of file

            while (!inexe.EndOfStream)
            {
                var rw = ReadWord(inexe);

                var lh = rw.LHW;

                var leb = rw.LHW.UL;

                if (leb == 0254000.OctU())
                {
                    /*printf("Transfer Address = %o\n",re);*/
                    Transaddr = rw.RHW;
                    break;
                }

                if (((lh & 0400000.Oct18())).NZ)
                    leb |= 37777000000u.OctU();

                var wordsToRead = - leb.SignE();
                var re = Convert.ToInt32(rw.RHW.UL);

                if (wordsToRead <= 0)
                    throw new Exception("?T10_LOADER - SAV Not a count! " + rw);

                //Console.WriteLine("Load %d words at %o\n",-le,re+1);
                lw += wordsToRead;

                for (var i = 0; i < (wordsToRead); i++)
                {
                    var bodyw = ReadWord(inexe);

                    var adrs = re + i + 1;
                    var page = Core.Page(adrs);

                    assureCoreHwm(page, ref hwmPage);

                    Core[re + i + 1] = bodyw;

                    // printf("%o/ %o,,%o  %llo\n",re+i+1,lh,rh,blv);
                    if (re + i + 1 > hw)
                        hw = re + i + 1;
                }
            }

            Console.WriteLine("[{0} Words Loaded From {1}]", lw, infile);

            inexe.Close();

            Console.WriteLine("[End loading {0}]", infile);

            if (Transaddr.NZ)
                Core[0, 0120.Oct18().UI] = new Word36((Word18) 0, Transaddr);

            Transaddr = Core[0, 0120.Oct18().UI].RHW;

            Hiseg_floor = 0400000.Oct18();
            High_Water = (Word18) hw;

            Console.WriteLine("[Loader High-Water-Mark is {0}]", High_Water);
            Console.WriteLine("[Transfer address is {0} Hiseg is {1}]", Transaddr,
                              Hiseg_floor);
        }

        // Fill in the gaps from the bottom of our segment
        private void assureCoreHwm(int page, ref int hwmPage)
        {
            for (var i = hwmPage; i <= page; i++)
                if (!Core.PageExists(i))
                    Core.NewPage(i);
        }

        private static Word36 ReadWord(TextReader inexe)
        {
            var s = inexe.ReadLine();

            while (s.StartsWith("#"))
                s = inexe.ReadLine();

            var p = s.Split(','); // lh,,rh

            var lh = Convert.ToInt32(p[0]).OctU();
            var rh = Convert.ToInt32(p[2]).OctU();

            return (new Word36(lh, rh));
        }

        //private void testOpen()
        //{
        //    GZipStream test = new GZipStream();
        //}
    }
}