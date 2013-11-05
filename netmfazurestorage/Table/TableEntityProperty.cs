using NetMf.CommonExtensions;

namespace netmfazurestorage.Table
{
    public class TableEntityProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }

        public override string ToString()
        {
            return StringUtility.Format("<d:{0} m:type=\"{2}\">{1}</d:{0}>", Name, Value, Type);
        }
    }
}