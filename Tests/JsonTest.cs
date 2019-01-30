using System;
using System.Collections.Generic;
using System.IO;
using E7.Firestorm;
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
            ts.typeArray.Add(DateTime.MaxValue);
            ts.typeArray.Add(true);
            ts.typeArray.Add(11.111);

            var jsonString = JsonConvert.SerializeObject(ts, Formatting.Indented, new DocumentConverter<TestStruct>("dummy/path"));

            //File.WriteAllText($"{Application.dataPath}/yay.txt", jsonString);
            //Debug.Log($"{jsonString}");

            var doc = new FirestormDocumentSnapshot(jsonString);
            var getBack = doc.ConvertTo<TestStruct>();

            Assert.That(getBack.typeTimestamp.Year, Is.EqualTo(DateTime.MinValue.Year));
            Assert.That(getBack.typeTimestamp.Month, Is.EqualTo(DateTime.MinValue.Month));
            Assert.That(getBack.typeTimestamp.Day, Is.EqualTo(DateTime.MinValue.Day));
            Assert.That(getBack.typeTimestamp.TimeOfDay.TotalHours, Is.EqualTo(DateTime.MinValue.TimeOfDay.TotalHours));

            Assert.That(getBack.typeString, Is.EqualTo("CYCLONEMAGNUM"));
            Assert.That(getBack.typeNumber, Is.EqualTo(55.55));
            Assert.That(getBack.typeNumberInt, Is.EqualTo(555));
            Assert.That(getBack.typeBoolean, Is.EqualTo(false));
            Assert.That((string)getBack.typeArray[0], Is.EqualTo("5argonTheGod"));
            Assert.That(getBack.typeArray[1], Is.EqualTo(6789));
            var timeInArray = (DateTime)getBack.typeArray[2];

            Assert.That(timeInArray.Year, Is.EqualTo(DateTime.MaxValue.Year));
            Assert.That(timeInArray.Month, Is.EqualTo(DateTime.MaxValue.Month));
            Assert.That(timeInArray.Day, Is.EqualTo(DateTime.MaxValue.Day));
            Assert.That(timeInArray.TimeOfDay.TotalHours, Is.EqualTo(DateTime.MaxValue.TimeOfDay.TotalHours));

            Assert.That((bool)getBack.typeArray[3], Is.EqualTo(true));
            Assert.That((double)getBack.typeArray[4], Is.EqualTo(11.111));

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
