using System;
using System.Collections.Generic;
using System.Text;
using Thorsbrain.Denc.Language.Generator;

namespace Thorsbrain.Denc.Language.Element.Programs
{
    public class NameSpace : IStatement
    {
        public string ProgNameSpace { get; private set; }

        public NameSpace(IEnumerable<string> stringlist)
        {
            StringBuilder sb = null;

            foreach (var s in stringlist)
            {
                if (sb == null)
                    sb = new StringBuilder();
                else
                    sb.Append('.');

                sb.Append(s);
            }

            ProgNameSpace = sb != null ? sb.ToString() : "Undefined";
        }

        #region Implementation of IStatement

        public void Emit(CodeWriter writer, DencProgram program)
        {
           
        }

        public void Setup(DencProgram program, ClassHeir classHeir, IStatement parent, ref StatementFlowInitializer flow)
        {
            
        }

        #endregion
    }
}