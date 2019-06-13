using Monitor.Subsystems;
using Monitor.Subsystems.UUOHandlers;
using PDP10CPU;
using PDP10CPU.CPU;
using PDP10CPU.Memory;

namespace Monitor
{
    public class MonitorContext
    {
        //private static MonitorContextSingleton singleton;

        //public static MonitorContextSingleton Singleton
        //{
        //    get { return singleton; }
        //}

        public bool DiaMonMode { get; set; }

        public SimhPDP10CPU CPU { get; private set; }
        public UserModeCore CORE { get; private set; }
        public Accumulators AC { get; private set; }

        public TTCALL TTCALL { get; private set; }
        public CALLI CALLI { get; private set; }

        // Reserved for when we get around to implementing them
        public IUUOHandler CALL { get; private set; }
        public IUUOHandler INIT { get; private set; }
        public IUUOHandler OPEN { get; private set; }
        public IUUOHandler RDCLK { get; private set; }
        public IUUOHandler RENAME { get; private set; }
        public IUUOHandler uuoIN { get; private set; }
        public IUUOHandler uuoOUT { get; private set; }
        public IUUOHandler SETSTS { get; private set; }
        public IUUOHandler STATO { get; private set; }
        public IUUOHandler GETSTS { get; private set; }
        public IUUOHandler STATUS { get; private set; }
        public IUUOHandler STATZ { get; private set; }
        public IUUOHandler INBUF { get; private set; }
        public IUUOHandler OUTBUF { get; private set; }
        public IUUOHandler INPUT { get; private set; }
        public IUUOHandler OUTPUT { get; private set; }
        public IUUOHandler CLOSE { get; private set; }
        public IUUOHandler RELEAS { get; private set; }
        public IUUOHandler MTAPE { get; private set; }
        public IUUOHandler UGETF { get; private set; }
        public IUUOHandler USETI { get; private set; }
        public IUUOHandler USETO { get; private set; }
        public IUUOHandler LOOKUP { get; private set; }
        public IUUOHandler ENTER { get; private set; }

        public MonitorContext(SimhPDP10CPU cpu)
        {
          
            CPU = cpu;
            CORE = cpu.CORE;
            AC = cpu.AC;

            TTCALL = (TTCALL) registerUUO(OpCodes.TTCALL, new TTCALL());
            CALLI = (CALLI) registerUUO(OpCodes.CALLI, new CALLI());
        }

        private IUUOHandler registerUUO(OpCodes opcode, IUUOHandler handler)
        {
            CPU.RegisterUUOHandler(opcode, handler.HandleUUO);
            handler.Setup(this);

            return handler;
        }
    }
}