using System.Collections.Generic;

namespace EncDecTool
{
    public class Entities
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public List<Field> FieldList
        {
            get { return fieldList; }
        }

        private readonly List<Field> fieldList = new List<Field>();
    }
}