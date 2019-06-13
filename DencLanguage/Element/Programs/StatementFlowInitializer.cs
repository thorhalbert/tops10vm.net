using System;
using System.Collections.Generic;
using Thorsbrain.Denc.Language.Element.ClassParts;

namespace Thorsbrain.Denc.Language.Element.Programs
{
    public class StatementFlowInitializer
    {
        public int Level { get; private set; }

        public int Address { get; private set; }

        /// <summary>
        /// Radix flows down, but doesn't flow up (end of class ends that radix and it reverts)
        /// </summary>
        public int Radix { get; set; }

        public Label LastLabel { get; set; }

        readonly Stack<StatementFlowInitializer> classStack = new Stack<StatementFlowInitializer>();

        public DencProgram Program { get; private set; }

        public StatementFlowInitializer(DencProgram program, ClassHeir classHeir)
        {
            Program = program;

            Level = 0;
            Address = 0;

            Radix = Program.Architecture.DefaultRadix;
            LastLabel = null;
        }

     
        public void Push()
        {
            classStack.Push(this);

            Level++;

            Address = 0; // Start of new class definition
            LastLabel = null;
        }

        public void Pop()
        {
            Level--;

            var old = classStack.Pop();

            Address = old.Address;
            Radix = old.Radix;
            LastLabel = null; // Don't let this survive a new class -- too confusing
        }

      

        public void SetupStorage(IStorageElement element)
        {
           element.StartAddress= Address;

            Address += element.Length;

        

            element.Identifier = "element" + element.StartAddress;
            if (LastLabel != null)
            {
                element.Identifier = Program.Architecture.NameClean(LastLabel.Name);
                element.ExposeProperty = true;
            }

            if (element.Length > 0)
                LastLabel = null; // Used it up

        }
    }
}