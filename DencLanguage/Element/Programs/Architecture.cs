namespace Thorsbrain.Denc.Language.Element.Programs
{
    public class Architecture
    {
        private readonly string archType;

        public Architecture(string val)
        {
            archType = val;
        }

        public string ArchType
        {
            get { return archType; }
        }
    }
}