using System.Xml;

namespace Thorsbrain.Denc.Language
{
    public struct YyErrorInstance
    {
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        public string Error { get; set; }

        public void XmlSerialize(XmlWriter xtw)
        {
            xtw.WriteStartElement("CodeError");

            var bufPos = StartLine + ":" + StartColumn + "/" + EndLine + ":" + EndColumn;

            xtw.WriteAttributeString("Pos", bufPos);

            xtw.WriteString(Error);

            xtw.WriteEndElement();
        }
    }
}