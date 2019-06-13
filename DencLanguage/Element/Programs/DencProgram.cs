using System;
using System.Collections.Generic;
using Thorsbrain.Denc.Language.Architectures;

namespace Thorsbrain.Denc.Language.Element.Programs
{
    public class DencProgram
    {
        public Architecture ArchitectureElement { get; private set; }

        public ArchitectureBase Architecture { get; private set; }

        public NameSpace ProgNameSpace { get; private set; }

        public List<ClassHeir> ClassList
        {
            get { return classList; }
        }

        private readonly List<ClassHeir> classList = new List<ClassHeir>();

        public DencProgram(Architecture architecture, IEnumerable<IStatement> statementlist)
        {
            ArchitectureElement = architecture;

            switch (ArchitectureElement.ArchType.ToLowerInvariant().Trim())
            {
                case "pdp10":
                    Architecture = new PDP10(this);
                    break;
                case "modern":
                    Architecture = new Modern(this);
                    break;
                default:
                    throw new ApplicationException("Unknown architecture specified");
            }

            foreach (var s in statementlist)
            {
                if (s == null) continue;

                if (s is NameSpace)
                {
                    ProgNameSpace = (NameSpace) s;
                    continue;
                }
                if (s is ClassHeir)
                {
                    var c = (ClassHeir) s;
                    ClassList.Add(c);
                    continue;
                }

                throw new ApplicationException("Unknown Root Statement: " + s.GetType());
            }
        }
    }
}