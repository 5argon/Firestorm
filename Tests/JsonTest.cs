using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace FirestormTests
{
    public class JsonTest : FirestormTestBase
    {
        [Test]
        public void Deserialize()
        {
            var doc = new FirestormDocumentSnapshot(sampleDocumentJson);
            Assert.That(doc.createTime.Year, Is.EqualTo(2019));
            Assert.That(doc.createTime.Month, Is.EqualTo(1));
            Assert.That(doc.createTime.Day, Is.EqualTo(26));
            Assert.That(doc.createTime.TimeOfDay.Hours, Is.EqualTo(10));

            Assert.That(doc.updateTime.Year, Is.EqualTo(2019));
            Assert.That(doc.updateTime.Month, Is.EqualTo(1));
            Assert.That(doc.updateTime.Day, Is.EqualTo(28));
            Assert.That(doc.updateTime.TimeOfDay.Hours, Is.EqualTo(12));

            var ts = doc.ConvertTo<TestStruct>();

            Assert.That(ts.typeTimestamp.Year, Is.EqualTo(2019));
            Assert.That(ts.typeTimestamp.Month, Is.EqualTo(1));
            Assert.That(ts.typeTimestamp.Day, Is.EqualTo(27));
            Assert.That(ts.typeTimestamp.TimeOfDay.TotalHours, Is.EqualTo(17));

            Assert.That(ts.typeString, Is.EqualTo("hey"));
            Assert.That(ts.typeNumber, Is.EqualTo(23.44));
            Assert.That(ts.typeNumberInt, Is.EqualTo(1234));
            Assert.That(ts.typeBoolean, Is.EqualTo(true));
            Assert.That((string)ts.typeArray[0], Is.EqualTo("in the array!"));
            Assert.That(ts.typeArray[1], Is.EqualTo(5678));
            Assert.That((bool)ts.typeArray[2], Is.EqualTo(false));
        }

        [Test]
        public void SerializeThenDeserialize()
        {
            var ts = new TestStruct();
            ts.typeTimestamp = DateTime.MinValue;
            ts.typeString = "CYCLONEMAGNUM";
            ts.typeNumber = 55.55;
            ts.typeNumberInt = 555;
            ts.typeBoolean = false;
            ts.typeArray = new List<object>();
            ts.typeArray.Add("5argonTheGod");
            ts.typeArray.Add(6789);
            ts.typeArray.Add(true);
            ts.typeArray.Add(11.111);

            var jsonString = JsonConvert.SerializeObject(ts, Formatting.Indented, new DocumentConverter<TestStruct>("dummy/path"));

            //File.WriteAllText($"{Application.dataPath}/yay.txt", jsonString);
            //Debug.Log($"{jsonString}");

            var doc = new FirestormDocumentSnapshot(jsonString);
            var convertBack = doc.ConvertTo<TestStruct>();

            Assert.That(convertBack.typeTimestamp.Year, Is.EqualTo(1));
            Assert.That(convertBack.typeTimestamp.Month, Is.EqualTo(1));
            Assert.That(convertBack.typeTimestamp.Day, Is.EqualTo(1));
            Assert.That(convertBack.typeTimestamp.TimeOfDay.TotalHours, Is.EqualTo(0));

            Assert.That(convertBack.typeString, Is.EqualTo("CYCLONEMAGNUM"));
            Assert.That(convertBack.typeNumber, Is.EqualTo(55.55));
            Assert.That(convertBack.typeNumberInt, Is.EqualTo(555));
            Assert.That(convertBack.typeBoolean, Is.EqualTo(false));
            Assert.That((string)convertBack.typeArray[0], Is.EqualTo("5argonTheGod"));
            Assert.That(convertBack.typeArray[1], Is.EqualTo(6789));
            Assert.That((bool)convertBack.typeArray[2], Is.EqualTo(true));
            Assert.That((double)convertBack.typeArray[3], Is.EqualTo(11.111));
        }

        [Test]
        public void DocumentMask()
        {
            var mask = new DocumentMask { fieldPaths = new string[] { "a", "b", "c" } };
            var fields = JsonConvert.SerializeObject(mask);
            Assert.That(fields, Is.EqualTo(@"{""fieldPaths"":[""a"",""b"",""c""]}"));
        }
    }
}
