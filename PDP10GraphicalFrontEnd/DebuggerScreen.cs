using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using FileFormats;
using Infragistics.Win.UltraWinDataSource;
using Infragistics.Win.UltraWinGrid;
using Monitor;
using Monitor.Subsystems.Console;
using PDP10CPU.BreakPoints;
using PDP10CPU.CPU;
using PDP10CPU.Enums;
using PDP10CPU.Events;
using PDP10CPU.Exceptions;
using PDP10CPU.Memory;
using Symbols;
using ThirtySixBits;

namespace PDP10GraphicalFrontEnd
{
    public partial class DebuggerScreen : UserControl
    {
        private const string KLADEXE = @"..\..\..\klad\diamon.sav.asc";
        private UserModeCore Core;
        private SimhPDP10CPU CPU;
        private MonitorContext TOPS10;
        private Tops10SAVLoader loader;

        private readonly string[] ACNames = {
                                                "F", "T1", "T2", "T3", "T4", "Q1", "Q2", "Q3",
                                                "P1", "P2", "P3", "P4", "P5", "P6", "CX", "P"
                                            };

        private readonly SortedList<int, string> symbolTable = new SortedList<int, string>();

        private readonly Dictionary<KeyValuePair<int, int>, UltraDataRow> blockMap =
            new Dictionary<KeyValuePair<int, int>, UltraDataRow>();

        private readonly Dictionary<KeyValuePair<int, int>, RowsCollection> GridRows =
            new Dictionary<KeyValuePair<int, int>, RowsCollection>();

        private readonly Dictionary<string, Label> psrIndicators = new Dictionary<string, Label>();
        private Label PSRStat;

        private ulong CycleStart;

        private bool setUpYet;

        public DebuggerScreen()
        {
            InitializeComponent();
        }

        public void cpuSetup()
        {
            Core = new UserModeCore(true);

            CPU = new SimhPDP10CPU(Core, OSTypes.Tops10)
                      {
                          ProcessorType = ProcessorTypes.KA10
                      };

            CPU.PCChanged += CPU_PCChanged;
            CPU.LightsChanged += CPU_LightsChanged;
            CPU.ProcFlagChanged += CPU_ProcFlagChanged;
            CPU.EffectiveAddressCalculated += CPU_EffectiveAddressCalculated;

            TOPS10 = new MonitorContext(CPU);

            TOPS10.TTCALL.ConsoleOutput += TTCALL_ConsoleOutput;
            TOPS10.TTCALL.AttachToConsole();

            loader = new Tops10SAVLoader(Core, KLADEXE);

            addACs();
            addSymbols<int>(typeof (JOBDAT));

            MapCore();

            MapProperties();

            foreach (var seg in Core)
                seg.MemberPageChanged += seg_MemberPageChanged;

            CoreBrowser.Rows[0].Expanded = true;
            var gr = FindGridRow(loader.Transaddr.UL);
            SelectGridRow(gr);
            DecorateRow(gr, Color.Red, "Transfer Address: " + loader.Transaddr);

            CPU.PC = loader.Transaddr.UI;

            CoreBrowser.AfterRowUpdate += CoreBrowser_AfterRowUpdate;

            setUpYet = true;

            CPU.ProcFlags = 0;
            CPU.SetUserMode();
        }

        private void TTCALL_ConsoleOutput(object sender, ConsoleOutputEvent e)
        {
            Invoke(new MethodInvoker(delegate { ConsoleTextBox.Text += e.Output; }));
        }

        private readonly UltraGridRow[] acGridRow = new UltraGridRow[16];
        private readonly UltraDataRow[] acDataRow = new UltraDataRow[16];

        private void MapProperties()
        {
            var acBand = PropertyDataSource.Rows;

            for (var i = 0; i < 16; i++)
            {
                var m = new object[4];

                m[0] = i.ToOctal(1) + "/" + ACNames[i];

                var memval = Core[0, i];
                m[1] = memval.ToString();
                m[2] = memval.ASCII();
                m[3] = memval.SIXBIT();

                var acr = acBand.Add(m);
                acr.Tag = i;

                acDataRow[i] = acr;
            }

            foreach (var r in CPUProperties.Rows)
            {
                var dr = (UltraDataRow) r.ListObject;
                var ac = (int) dr.Tag;

                acGridRow[ac] = r;
            }
        }

        private void updateAC(int ac, Word36 newv)
        {
            var dr = acDataRow[ac];

            var memval = newv;

            dr.SetCellValue(1, memval.ToString());
            dr.SetCellValue(2, memval.ASCII());
            dr.SetCellValue(3, memval.SIXBIT());
        }

        private void updateMem(int memloc, Word36 newv)
        {
            var memvalue = newv;

            var m = new object[6];
            //m[0] = false;
            //m[1] = "[" + memloc.ToOctal(6) + "]";
            //if (symbolTable.ContainsKey(memloc))
            //    m[1] += " " + symbolTable[memloc];
            m[2] = memvalue.ToString();
            m[3] = memvalue.Instruction(memloc);
            m[4] = memvalue.ASCII();
            m[5] = memvalue.SIXBIT();

            var dr = GetRowFromAddress(0, (ulong) memloc);

            for (var i = 2; i < 6; i++)
                dr.SetCellValue(i, m[i]);
        }

        private void CoreBrowser_AfterRowUpdate(object sender, RowEventArgs e)
        {
            var m = (int) ((UltraDataRow) e.Row.ListObject).Tag;
            var mem = (ulong) m;
            var breakpoint = (bool) e.Row.Cells[0].Value;

            if (breakpoint)
            {
                if (!CPU.BreakPoints.ContainsKey(mem))
                    CPU.BreakPoints.Add(mem, new BreakContext {BreakAction = BreakPointActions.Pause}
                        );
            }
            else if (CPU.BreakPoints.ContainsKey(mem))
                CPU.BreakPoints.Remove(mem);
        }

        private void MapCore()
        {
            blockMap.Clear();
            for (var seg = 0; seg < Core.InitialSegments; seg++)
            {
                var o = new object[1];
                o[0] = "Segment [" + seg.ToOctal(3) + "]";
                var sr = CoreDataSource.Rows.Add(o);
                sr.Tag = seg;
                for (var page = 0; page < Core.MaxPagesPerSegment; page++)
                    if (Core.PageExists(seg, page))
                    {
                        var id = new KeyValuePair<int, int>(seg, page);

                        var pr = sr.GetChildRows(0);
                        var p = new object[1];
                        p[0] = "Page [" + page.ToOctal(3) + "]";

                        var pc = pr.Add(p);
                        blockMap.Add(id, pc);
                        pc.Tag = id;

                        var mem = pc.GetChildRows(0);
                        for (var memv = 0; memv < Core.PageSize; memv++)
                        {
                            var memloc = page*Core.PageSize + memv;

                            var m = new object[6];
                            m[0] = false;
                            m[1] = "[" + memloc.ToOctal(6) + "]";
                            if (symbolTable.ContainsKey(memloc))
                                m[1] += " " + symbolTable[memloc];
                            var memvalue = Core[seg, (page*Core.PageSize) + memv];
                            m[2] = memvalue.ToString();
                            m[3] = memvalue.Instruction(memloc);
                            m[4] = memvalue.ASCII();
                            m[5] = memvalue.SIXBIT();

                            var memrow = mem.Add(m);
                            memrow.Tag = memloc;
                        }
                    }
            }
            MapGrid();
        }

        private void MapGrid()
        {
            GridRows.Clear();
            foreach (var ans in CoreBrowser.Rows)
            {
                var chil = ans.ChildBands[0];

                foreach (var r in chil.Rows)
                {
                    var ds = (UltraDataRow) r.ListObject;
                    var kvp = (KeyValuePair<int, int>) ds.Tag;

                    var crow = r.ChildBands[0].Rows;

                    GridRows.Add(kvp, crow);
                }
            }
        }

        private void addACs()
        {
            for (var i = 0; i < 16; i++)
                symbolTable.Add(i, "AC" + i.ToOctal(1) + "/" + ACNames[i]);
        }

        private void addSymbols<T>(Type type)
        {
            var symList = DECSyms.MungeEnum<T>(type);
            foreach (var v in symList)
                if (!symbolTable.ContainsKey(Convert.ToInt32(v.Value)))
                    symbolTable.Add(Convert.ToInt32(v.Value), v.Key);
        }

        // EVENTUALLY NEED TO REDO AS CROSS THREAD EVENTS AS WE WILL WANT TO RUN CPU IN SEPARATE THREAD

        private void CPU_EffectiveAddressCalculated(object sender, EffectiveAddressCalculatedEvent e)
        {
            if (!setUpYet) return;
        }

        private delegate void EventCaller(object sender, EventArgs e);

        private void InvokeEvent(EventCaller caller, object sender, EventArgs e)
        {
            if (!setUpYet) return;

            Invoke(caller, sender, e);
        }

        private void seg_MemberPageChanged(object sender, MemberPageChangedEvent e)
        {
            if (!setUpYet) return;

            InvokeEvent(UpdateMemoryRow, sender, e);
        }

        private void UpdateMemoryRow(object sender, EventArgs evt)
        {
            var e = (MemberPageChangedEvent) evt;
            var seg = (Segment) sender;
            var page = e.Page;
            var pageNum = e.PageNum;
            var ep = e.PageChange;

            var memloc = pageNum*Core.PageSize + ep.Index;

            if (memloc < 16)
                updateAC(memloc, ep.NewValue);
            else
                updateMem(memloc, ep.NewValue);
        }

        private void CPU_ProcFlagChanged(object sender, ProcFlagChangedEvent e)
        {
            InvokeEvent(ProcessProcFlagChange, sender, e);
        }

        private void ProcessProcFlagChange(object sender, EventArgs evt)
        {
            var e = (ProcFlagChangedEvent) evt;
            if (psrIndicators.Count <= 0)
            {
                var flags = e.SerializedFlags.Split('|');
                foreach (var flag in flags)
                {
                    var settings = flag.Split('=');

                    var newLabel = new Label
                                       {
                                           AutoSize = true,
                                           Text = settings[0],
                                           Margin = new Padding(0, 0, 0, 0),
                                       };
                    psrIndicators.Add(settings[0], newLabel);

                    PSRStatusPanel.Controls.Add(newLabel);
                }

                //PSRStat = new Label
                //            {
                //                AutoSize = true,
                //                Text = new Word36(e.NewFlag).ToString(),
                //};
                //PSRStatusPanel.Controls.Add(PSRStat);
            }

            var flaglist = e.SerializedFlags.Split('|');
            foreach (var flag in flaglist)
            {
                var settings = flag.Split('=');

                var v = Convert.ToInt32(settings[1]) != 0;

                var findLabel = psrIndicators[settings[0]];

                var setCol = v ? Color.Red : Color.DarkGray;

                if (findLabel.ForeColor != setCol)
                {
                    findLabel.ForeColor = setCol;
                    findLabel.Invalidate();
                }
            }

            //PSRStat.Text = new Word36(e.NewFlag).ToString();
        }

        private void CPU_LightsChanged(object sender, LightsChangedEvent e)
        {
            if (!setUpYet) return;
        }

        private void CPU_PCChanged(object sender, PCChangedEvent e)
        {
            if (!setUpYet) return;

            InvokeEvent(ProcessPCChanged, sender, e);
        }

        private void ProcessPCChanged(object sender, EventArgs evt)
        {
            var e = (PCChangedEvent) evt;
            var pc = e.NewPC;
            var seg = 0;

            PCLabel.Text = seg.ToOctal(3) + "/" + pc.ToOctal(6);
            //PCLabel.Invalidate();

            var inc = CPU.InstructionsExecuted - CycleStart;

            if (CPU.Runmode == RunModes.FreeRun)
            {
                CycleCount.Text = "#" + inc;
                return;
            }

            CycleCount.Text = "ss";

            var gr = FindGridRow(pc);
            SelectGridRow(gr);
        }

        private void CoreBrowser_InitializeLayout(object sender, InitializeLayoutEventArgs e) {}

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) {}

        private void StepButton_Click(object sender, EventArgs e)
        {
            if (!setUpYet) return;

            CPU.Runmode = RunModes.SingleStep;

            RunCPU();
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            if (!setUpYet) return;

            CPU.Runmode = RunModes.FreeRun;

            RunCPU();
        }

        private void RunCPU()
        {
            CycleStart = CPU.InstructionsExecuted;

            var thr = new Thread(RunCPUNewThread)
                          {
                              Name = "PDP-10 CPU[0]",
                              Priority = ThreadPriority.BelowNormal,
                          };
            thr.Start();
        }

        private void RunCPUNewThread()
        {
            try
            {
                HandleHalt(CPU.ProcessorMainloop());
            }
            catch (InstructionFailure ex)
            {
                Invoke(new MethodInvoker(() =>
                                         MessageBox.Show(this,
                                                         "Processor Exited with Error " + ex.ExitType + " / " +
                                                         ex.Message
                                                         + " at PC:" + ex.PC.ToOctal(6),
                                                         "CPU Fault", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
            }
                //catch (Exception ex)
                //{
                //    Invoke(new MethodInvoker(() =>
                //                             MessageBox.Show(this, "Processor Exited with Unknown Error " + ex.Message,    + " at PC:" + ex.PC.ToOctal(6)
                //                                             "CPU Fault", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                //}
            finally
            {
                Invoke(new MethodInvoker(() =>
                                             {
                                                 var gr = FindGridRow(CPU.PC);
                                                 SelectGridRow(gr);
                                             }));
            }
        }

        private void HandleHalt(InstructionExit instructionExit)
        {
            switch (instructionExit)
            {
                case InstructionExit.BreakPoint:
                case InstructionExit.SingleStep:
                    return;
                default:
                    TOPS10.TTCALL.OUTSTRline("?Program Exited: " + instructionExit + " at PC:" + CPU.PC.ToOctal(6));
                    Invoke(
                        new MethodInvoker(() => MessageBox.Show(this, "Processor Exited with Status " + instructionExit,
                                                                "CPU Fault", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                    break;
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            CPU.Runmode = RunModes.SingleStep; // Make it stop after current instruction
        }

        private UltraGridRow FindGridRow(ulong pc)
        {
            var seg = 0;

            var gridrow = GetGridRowFromAddress(seg, pc);

            return gridrow;
        }

        private void DecorateRow(UltraGridRow gridrow, Color bgRow, string tooltip)
        {
            gridrow.Appearance.BackColor = Color.Red;
            if (tooltip != null)
                gridrow.ToolTipText = tooltip;
        }

        private void SelectGridRow(UltraGridRow gridrow)
        {
            gridrow.ParentRow.Expanded = true;
            gridrow.ParentRow.Selected = true;

            gridrow.Selected = true;

            CoreBrowser.ActiveRowScrollRegion.ScrollRowIntoView(gridrow);
        }

        private UltraGridRow GetGridRowFromAddress(int seg, ulong pc)
        {
            var page = (int) pc/Core.PageSize;

            var block = GridRows[new KeyValuePair<int, int>(seg, page)];

            return block[(int) pc%Core.PageSize];
        }

        private UltraDataRow GetRowFromAddress(int seg, ulong pc)
        {
            var page = (int) pc/Core.PageSize;

            var block = blockMap[new KeyValuePair<int, int>(seg, page)];
            var childrn = block.GetChildRows(0);
            return childrn[(int) pc%Core.PageSize];
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            CPU.PC = loader.Transaddr.UI;
            CPU.ProcFlags = 0;
            CPU.SetUserMode();
        }
    }
}