using System.Collections.Generic;

namespace EncDecTool
{
    public class EntityHeader
    {
        public string NameSpace { get; set; }
        public WordTypes WordType { get; set; }
        public string Module { get; set; }
        public NamingStyles NameStyle { get; set; }
        public string Description { get; set; }

        public List<Entities> EntityList
        {
            get { return entityList; }
        }

        private readonly List<Entities> entityList = new List<Entities>();
    }
}