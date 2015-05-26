using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CustomXmlSerializationExample
{
    class Console
    {
        static void Main(string[] args)
        {
            var g = new[]
            {
                new Group {IsDefault = true, MgsProfileId = 500, Name = "P0", Rank = 0},
                new Group {IsDefault = true, MgsProfileId = 501, Name = "P1", Rank = 1},
                new Group {IsDefault = false, MgsProfileId = 502, Name = "P2", Rank = 2}
            };

            var c = new Category
            {
                Currency = "CNY",
                Groups = g,
                Id = 1,
                Name = "Exposure"
            };




            using(var stream = new MemoryStream())
            using (var textWriter = new StreamWriter(stream))
            {
                var serializer = new XmlSerializer(typeof (Group));

                serializer.Serialize(textWriter, g[1]);

                var buf = stream.GetBuffer();
                var doc = Encoding.UTF8.GetString(buf);
                System.Console.Out.WriteLine("doc = {0}", doc);

                stream.Position = 0;

                var obj = serializer.Deserialize(stream);

            }
            using(var stream = new MemoryStream())
            using (var textWriter = new StreamWriter(stream))
            {
                var serializer = new XmlSerializer(typeof (Category));

                serializer.Serialize(textWriter, c);

                var buf = stream.GetBuffer();
                var doc = Encoding.UTF8.GetString(buf);

                System.Console.Out.WriteLine("doc = {0}", doc);

                stream.Position = 0;

                var obj = serializer.Deserialize(stream);
            }
        }
    }

    [XmlRoot("Groups")]
    public class Group : IXmlSerializable
    {
        private const string ElementName = "Group";
        private const string NameAttrName = "Name";
        private const string ProfileIdAttrName = "MgsProfileID";
        private const string IsDefaultAttrName = "IsDefault";
        private const string RankAttrName = "Rank";
        public string Name { get; set; }
        public int MgsProfileId { get; set; }
        public bool IsDefault { get; set; }
        public int Rank { get; set; }
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.Name != ElementName)
                reader.ReadToDescendant(ElementName);
            Name = reader.GetAttribute(NameAttrName);
            MgsProfileId = int.Parse(reader.GetAttribute(ProfileIdAttrName) ?? "0");
            IsDefault = bool.Parse(reader.GetAttribute(IsDefaultAttrName) ?? "false");
            Rank = int.Parse(reader.GetAttribute(RankAttrName) ?? "0");
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(ElementName);
            writer.WriteAttributeString(NameAttrName, Name);
            writer.WriteAttributeString(ProfileIdAttrName, MgsProfileId.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(IsDefaultAttrName, IsDefault.ToString());
            writer.WriteAttributeString(RankAttrName, Rank.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }
    }

    [XmlRoot("CustomerAccountGroups")]
    public class Category : IXmlSerializable
    {
        private const string ElementName = "GroupCategory";
        private const string NameAttrName = "Name";
        private const string IdAttrName = "Id";
        private const string CurrencyAttrName = "Currency";
        public int Id { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public Group[] Groups { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            Name = reader.GetAttribute(NameAttrName);
            Currency = reader.GetAttribute(CurrencyAttrName);
            Id = int.Parse(reader.GetAttribute(IdAttrName) ?? "0");
            var groups = new List<Group>();
            reader.ReadStartElement();
            while (reader.IsStartElement("Group"))
            {
                var g = new Group();
                g.ReadXml(reader);
                groups.Add(g);
                reader.ReadStartElement();
            }
            Groups = groups.ToArray();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(ElementName);
            writer.WriteAttributeString(NameAttrName, Name);
            writer.WriteAttributeString(IdAttrName, Id.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(CurrencyAttrName, Currency);
            if (Groups != null && Groups.Length > 0)
            {
                foreach (var g in Groups)
                {
                    g.WriteXml(writer);
                }
            }
            writer.WriteEndElement();
        }
    }
}
