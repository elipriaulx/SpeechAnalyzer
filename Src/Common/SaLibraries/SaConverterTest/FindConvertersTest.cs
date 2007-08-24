using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sil.Sa.Converter;
using NUnit.Framework;
using SilEncConverters;


namespace Sil.Sa.Converter.Test
{
    [TestFixture]
    public class FindConvertersTest
    {
        [Test]
        public void FindTest()
        {
            string fontIn = "ASAP SILDoulos";

            FontConverterMap[] mappings = FindConverters.GetFontConverterMappings(fontIn);

            Assert.IsNotNull(mappings);

            if(mappings != null)
            {
                Assert.IsNotEmpty(mappings);
                Assert.AreEqual(1, mappings.GetLength(0));
                foreach (FontConverterMap map in mappings)
                {
                    Assert.AreEqual(fontIn, map.FontFaceIn);
                    Assert.IsNotNull(map.FontFaceOut);
                    Assert.IsNotNull(map.Mapping);
                    Assert.IsNotNull(map.Converter);
                    Assert.AreEqual("Doulos SIL", map.FontFaceOut);
                    Assert.AreEqual("ASAP SIL IPA93<>UNICODE", map.Mapping);
                    Assert.AreEqual(ConvType.Legacy_to_from_Unicode, map.Converter.ConversionType);
                    Assert.AreEqual((int)ProcessTypeFlags.UnicodeEncodingConversion, map.Converter.ProcessType);
                }
            }
        }

        [Test]
        public void FindReverseTest()
        {
            string fontIn = "Doulos SIL";

            FontConverterMap[] mappings = FindConverters.GetFontConverterMappings(fontIn);

            Assert.IsNotNull(mappings);

            if (mappings != null)
            {
                Assert.IsNotEmpty(mappings);
                foreach (FontConverterMap map in mappings)
                {
                    Assert.AreEqual(fontIn, map.FontFaceIn);
                    Assert.IsNotNull(map.FontFaceOut);
                    Assert.IsNotNull(map.Mapping);
                    Assert.IsNotNull(map.Converter);
                    //Assert.AreEqual("SILDoulos IPA93", map.FontFaceOut);
                    //Assert.AreEqual("ASAP SIL IPA93<>UNICODE", map.Mapping);
                    Assert.AreEqual(ConvType.Legacy_to_from_Unicode, map.Converter.ConversionType);
                    Assert.AreEqual((int)ProcessTypeFlags.UnicodeEncodingConversion, map.Converter.ProcessType);
                }
            }
        }

        [Test]
        public void ConvertTest()
        {
            string fontIn = "ASAP SILDoulos";

            FontConverterMap[] mappings = FindConverters.GetFontConverterMappings(fontIn);

            string phResult = mappings[0].Convert("mEsI. bUkHuÎ . tHu lEz Qme. kHI s)tH IsI oWZdWi.");

            Assert.AreEqual("mɛsɪ. bʊkʰuɜ . tʰu lɛz æme. kʰɪ sɒ̃tʰ ɪsɪ oʷʒɔɒdʷi.", phResult);
        }
        [Test]
        public void ReverseConvertTest()
        {
            string fontIn = "Doulos SIL";
            string phResult = null;

            FontConverterMap[] mappings = FindConverters.GetFontConverterMappings(fontIn);

            foreach (FontConverterMap map in mappings)
            {
                if (map.FontFaceOut == "ASAP SILDoulos")
                {
                    string expected = "mEsI. bUkHuÎ . tHu lEz Qme. kHI s)tH IsI oWZdWi.";
                    
                    phResult = map.Convert("mɛsɪ. bʊkʰuɜ . tʰu lɛz æme. kʰɪ sɒ̃tʰ ɪsɪ oʷʒɔɒdʷi.");

                    Assert.AreEqual(expected.Length, phResult.Length);
                    for (int i = 0; i < phResult.Length; i++)
                    {
                        Assert.AreEqual(expected[i], phResult[i] & 0xff);
                        Assert.AreEqual(expected[i] + 0xf000, phResult[i]);
                    }
                }
            }
            Assert.IsNotNull(phResult);
        }
    }

    public class ConsoleTest
    {
        public static void Main(string[] args)
        {
            string fontIn = "ASAP SILDoulos";

            FontConverterMap[] mappings = FindConverters.GetFontConverterMappings(fontIn);

            foreach (FontConverterMap map in mappings)
            {
                Console.WriteLine("Font In: {0} Mapping: {1} Font Out: {2}", map.FontFaceIn, map.Mapping, map.FontFaceOut);
                Console.WriteLine("ProcessType {0}", map.Converter.ProcessType);
                Console.WriteLine("ConvType {0}", map.Converter.ConversionType);

                Console.WriteLine(map.Convert("mEsI. bUkHuÎ . tHu lEz Qme. kHI s)tH IsI oWZdWi."));
            }

            Console.ReadKey(false);            
        }

    }
}
