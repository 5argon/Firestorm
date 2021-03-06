using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using E7.Firebase;
using NUnit.Framework;
using UnityEngine;
using E7.Firebase.LitJson;
using System.Collections;

using Yo = System.Collections.Generic.Dictionary<string, object>;

namespace FirestormTesto
{
    public class JsonTest : FirestormTestDataStructure
    {

        private class TooManyFields
        {
            public string uid;
            public string shortId;
            public bool searchable;
            public bool openAsRival;
            public bool globalScore;
            public bool publicProfile;
            public DateTime updateTime;
            public List<object> addedAsRival;
        }

        [Test]
        public void TooManyFieldsLeftDefault()
        {

        string lessValues = @"
    {
      ""name"": ""projects/firestrike5555/databases/(default)/documents/my-collection/my-doc"",
      ""fields"": {
        ""typeNumber"": {
          ""doubleValue"": 12.34
        },
        ""typeBoolean"": {
          ""booleanValue"": true
        }
      },
      ""createTime"": ""2019-01-26T10:18:04.978706Z"",
      ""updateTime"": ""2019-01-28T12:45:25.496931Z""
    }
        ";
            var doc = new FirestormDocumentSnapshot(lessValues);
            var getBack = doc.ConvertTo<TestStruct>();
            Assert.That(getBack.typeString, Is.Null);
            Assert.That(getBack.typeArray, Is.Null);
            Assert.That(getBack.typeMap, Is.Null);
        }

        [Test]
        public void LitJsonHandlesSupportedType()
        {
            JsonReader specialReader = new JsonReader(sampleDocumentJson)
            {
                ObjectAsDictString = true
            };

            var doc = JsonMapper.ToObject<FirestormDocument>(specialReader);

            Assert.That(doc.createTime.Year, Is.EqualTo(2019));
            Assert.That(doc.createTime.Month, Is.EqualTo(1));
            Assert.That(doc.createTime.Day, Is.EqualTo(26));
            Assert.That(doc.createTime.TimeOfDay.Hours, Is.EqualTo(10));

            Assert.That(doc.updateTime.Year, Is.EqualTo(2019));
            Assert.That(doc.updateTime.Month, Is.EqualTo(1));
            Assert.That(doc.updateTime.Day, Is.EqualTo(28));
            Assert.That(doc.updateTime.TimeOfDay.Hours, Is.EqualTo(12));

            Assert.That(doc.fields["typeTimestamp"]["timestampValue"], Is.EqualTo("2019-01-27T17:00:00Z"), "Literally the time text because the other side is an object");
            Assert.That(doc.fields["typeString"]["stringValue"], Is.EqualTo("hey"));
            Assert.That(doc.fields["typeBytes"]["bytesValue"], Is.EqualTo("QUJD"));
            Assert.That(doc.fields["typeNumber"]["doubleValue"], Is.EqualTo(23.44));
            Assert.That(doc.fields["typeNumberInt"]["integerValue"], Is.EqualTo("1234"));
            Assert.That(doc.fields["typeBoolean"]["booleanValue"], Is.EqualTo(true));

            // Test nested fields (lol what a mess)
            Assert.That((string)((JsonData)((JsonData)((Yo)(doc.fields["typeMap"]["mapValue"]))["fields"])["typeTimestampMap"])["timestampValue"], Is.EqualTo("2016-02-13T19:00:00Z"));
            Assert.That((string)((JsonData)((JsonData)((Yo)(doc.fields["typeMap"]["mapValue"]))["fields"])["typeStringMap"])["stringValue"], Is.EqualTo("omgmapmap"));
            Assert.That((double)((JsonData)((JsonData)((Yo)(doc.fields["typeMap"]["mapValue"]))["fields"])["typeNumberMap"])["doubleValue"], Is.EqualTo(98.76));
            Assert.That((bool)((JsonData)((JsonData)((Yo)(doc.fields["typeMap"]["mapValue"]))["fields"])["typeBooleanMap"])["booleanValue"], Is.EqualTo(true));

            var arrayContent = (Dictionary<string,object>)doc.fields["typeArray"]["arrayValue"];
            Assert.That(arrayContent, Is.Not.Null);

            //In LitJSON [ ] would become JsonData if the matching type is an "object"
            JsonData al = (JsonData)arrayContent["values"];
            //Debug.Log( al[0]["stringValue"]);
            Assert.That(al.Count, Is.EqualTo(4), "This passed ensure that it is either key-value or array");

            Assert.That((string)al[0]["stringValue"], Is.EqualTo("in the array!"));
            Assert.That((string)al[1]["timestampValue"], Is.EqualTo("2018-02-16T14:09:04.978706Z"));
            Assert.That((string)al[2]["integerValue"], Is.EqualTo("5678"));
            Assert.That((bool)al[3]["booleanValue"], Is.EqualTo(false));
        }

        [Test]
        public void Deserialize()
        {
            var dsnap = new FirestormDocumentSnapshot(sampleDocumentJson);
            var doc = dsnap.Document;
            Assert.That(doc.createTime.Year, Is.EqualTo(2019));
            Assert.That(doc.createTime.Month, Is.EqualTo(1));
            Assert.That(doc.createTime.Day, Is.EqualTo(26));
            Assert.That(doc.createTime.TimeOfDay.Hours, Is.EqualTo(10));

            Assert.That(doc.updateTime.Year, Is.EqualTo(2019));
            Assert.That(doc.updateTime.Month, Is.EqualTo(1));
            Assert.That(doc.updateTime.Day, Is.EqualTo(28));
            Assert.That(doc.updateTime.TimeOfDay.Hours, Is.EqualTo(12));

            var ts = dsnap.ConvertTo<TestStruct>();

            Assert.That(ts.typeTimestamp.Year, Is.EqualTo(2019));
            Assert.That(ts.typeTimestamp.Month, Is.EqualTo(1));
            Assert.That(ts.typeTimestamp.Day, Is.EqualTo(27));
            Assert.That(ts.typeTimestamp.TimeOfDay.TotalHours, Is.EqualTo(17));

            Assert.That(ts.typeMap.typeTimestampMap.Year, Is.EqualTo(2016));
            Assert.That(ts.typeMap.typeTimestampMap.Month, Is.EqualTo(2));
            Assert.That(ts.typeMap.typeTimestampMap.Day, Is.EqualTo(13));
            Assert.That(ts.typeMap.typeTimestampMap.TimeOfDay.Hours, Is.EqualTo(19));
            Assert.That(ts.typeMap.typeStringMap, Is.EqualTo("omgmapmap"));
            Assert.That(ts.typeMap.typeNumberMap, Is.EqualTo(98.76));
            Assert.That(ts.typeMap.typeBooleanMap, Is.EqualTo(true));

            Assert.That(ts.typeString, Is.EqualTo("hey"));
            Assert.That(ts.typeNumber, Is.EqualTo(23.44));
            Assert.That(ts.typeNumberInt, Is.EqualTo(1234));
            Assert.That(ts.typeBytes, Is.EquivalentTo(new byte[] { 0x41, 0x42, 0x43 }));
            Assert.That(ts.typeBoolean, Is.EqualTo(true));
            Assert.That((string)ts.typeArray[0], Is.EqualTo("in the array!"));
            Assert.That((string)ts.typeArray[1], Is.EqualTo("2018-02-16T14:09:04.978706Z"), "Because a list of object is the receiver, it could not tell LitJSON that this one should be DateTime. You get a string date time instead.");
            Assert.That(ts.typeArray[2], Is.EqualTo(5678));
            Assert.That((bool)ts.typeArray[3], Is.EqualTo(false));
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
            ts.typeEnum = TestEnum.B;
            ts.typeBytes = new byte[] { 0x41, 0x42, 0x43 };

            var minPlus2 = DateTime.MinValue + TimeSpan.FromHours(2);
            ts.typeMap = new TestStructInner
            {
                typeTimestampMap = minPlus2,
                typeBooleanMap = true,
                typeNumberMap = 777.88,
                typeStringMap = "nemii"
            };

            ts.typeArray = new List<object>();
            ts.typeArray.Add("5argonTheGod");
            ts.typeArray.Add(6789);
            DateTime dt = DateTime.MinValue + TimeSpan.FromHours(1);
            ts.typeArray.Add(dt);
            ts.typeArray.Add(true);
            ts.typeArray.Add(11.111);

            //var jsonString = JsonConvert.SerializeObject(ts, Formatting.Indented, new DocumentConverter<TestStruct>("dummy/path"));
            var jsonString = FirestormUtility.ToJsonDocument(ts, "dummy/path");

            //File.WriteAllText($"{Application.dataPath}/yay.txt", jsonString);
            //Debug.Log($"{jsonString}");

            var doc = new FirestormDocumentSnapshot(jsonString);
            var getBack = doc.ConvertTo<TestStruct>();

            Assert.That(getBack.typeTimestamp.Year, Is.EqualTo(DateTime.MinValue.Year));
            Assert.That(getBack.typeTimestamp.Month, Is.EqualTo(DateTime.MinValue.Month));
            Assert.That(getBack.typeTimestamp.Day, Is.EqualTo(DateTime.MinValue.Day));
            Assert.That(getBack.typeTimestamp.TimeOfDay.TotalHours, Is.EqualTo(DateTime.MinValue.TimeOfDay.TotalHours));

            Assert.That(getBack.typeMap.typeTimestampMap.Year, Is.EqualTo(minPlus2.Year));
            Assert.That(getBack.typeMap.typeTimestampMap.Month, Is.EqualTo(minPlus2.Month));
            Assert.That(getBack.typeMap.typeTimestampMap.Day, Is.EqualTo(minPlus2.Day));
            Assert.That(getBack.typeMap.typeTimestampMap.TimeOfDay.TotalHours, Is.EqualTo(minPlus2.TimeOfDay.TotalHours));
            Assert.That(getBack.typeMap.typeBooleanMap, Is.EqualTo(true));
            Assert.That(getBack.typeMap.typeNumberMap, Is.EqualTo(777.88));
            Assert.That(getBack.typeMap.typeStringMap, Is.EqualTo("nemii"));

            Assert.That(getBack.typeString, Is.EqualTo("CYCLONEMAGNUM"));
            Assert.That(getBack.typeNumber, Is.EqualTo(55.55));
            Assert.That(getBack.typeNumberInt, Is.EqualTo(555));
            Assert.That(getBack.typeBoolean, Is.EqualTo(false));
            Assert.That(getBack.typeEnum, Is.EqualTo(TestEnum.B));
            Assert.That(getBack.typeBytes, Is.EquivalentTo(new byte[] { 0x41, 0x42, 0x43 }));

            Assert.That((string)getBack.typeArray[0], Is.EqualTo("5argonTheGod"));
            Assert.That(getBack.typeArray[1], Is.EqualTo(6789));
            var timeInArray = DateTime.Parse((string)getBack.typeArray[2]).ToUniversalTime();

            Assert.That(timeInArray.Year, Is.EqualTo(dt.Year));
            Assert.That(timeInArray.Month, Is.EqualTo(dt.Month));
            Assert.That(timeInArray.Day, Is.EqualTo(dt.Day));
            Assert.That(timeInArray.TimeOfDay.TotalHours, Is.EqualTo(dt.TimeOfDay.TotalHours));

            Assert.That((bool)getBack.typeArray[3], Is.EqualTo(true));
            Assert.That((double)getBack.typeArray[4], Is.EqualTo(11.111));

        }

        [Test]
        public void DocumentMask()
        {
            var mask = new DocumentMask { fieldPaths = new string[] { "a", "b", "c" } };
            var fields = JsonMapper.ToJson(mask);
            Assert.That(fields, Is.EqualTo(@"{""fieldPaths"":[""a"",""b"",""c""]}"));
        }
    }
}
