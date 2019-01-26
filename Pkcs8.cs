using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

internal class Pkcs8 
{
    /// <summary>
    /// PITA JWT [Kungfu](https://developers.google.com/identity/protocols/OAuth2ServiceAccount)
    /// </summary>
    public async Task<string> GetServiceAccountAccessTokenAsync(string serviceEmail, string privateKey)
    {
        string jwtHeader = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9";
        var time = DateTimeOffset.Now.ToUnixTimeSeconds();
        int expiresInSecond = 5;

        JwtObject jsonObject = new JwtObject
        {
            iss = serviceEmail,
            scope = "https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/cloud-platform https://www.googleapis.com/auth/datastore",
            aud = "https://www.googleapis.com/oauth2/v4/token",
            exp = time + expiresInSecond,
            iat = time,
        };

        var jsonString = JsonUtility.ToJson(jsonObject);
        string jwtClaimSet = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
        var rsaParameters = DecodeRsaParameters(privateKey);

        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(rsaParameters);
        var signatureBytes = rsa.SignData(Encoding.UTF8.GetBytes($"{jwtHeader}.{jwtClaimSet}"), "SHA256");
        var jwtSignature = Convert.ToBase64String(signatureBytes);

        string completeJwt = $"{jwtHeader}.{jwtClaimSet}.{jwtSignature}";

        //Debug.Log($"Sending JWT : {completeJwt}");

        var req = UnityWebRequest.Post("https://www.googleapis.com/oauth2/v4/token", new Dictionary<string, string>
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
            ["assertion"] = completeJwt
        });
        req.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        //Debug.Log($"{req.uri} {req.url}");
        var ao = req.SendWebRequest();
        while (ao.isDone == false)
        {
            await Task.Yield();
        }

        //Debug.Log($"{ao.webRequest.downloadHandler.text}");
        var res = JsonUtility.FromJson<JwtResponse>(ao.webRequest.downloadHandler.text);
        if (ao.webRequest.isHttpError || ao.webRequest.isNetworkError)
        {
            throw new FirestormException($"Getting service account access token error! {ao.webRequest.error} {ao.webRequest.downloadHandler.text}");
        }

        return res.access_token;
    }

#pragma warning disable 0649
    private struct JwtResponse
    {
        public string access_token;
        public int expires_in;
        public string token_type;
    }

    private struct JwtObject
    {
        public string iss;
        public string scope;
        public string aud;
        public long exp;
        public long iat;
    }
#pragma warning restore 0649

    // PKCS#8 specification: https://www.ietf.org/rfc/rfc5208.txt
    // ASN.1 specification: https://www.itu.int/ITU-T/studygroups/com17/languages/X.690-0207.pdf

    /// <summary>
    /// An incomplete ASN.1 decoder, only implements what's required
    /// to decode a Service Credential.
    /// </summary>
    internal class Asn1
    {
        internal enum Tag
        {
            Integer = 2,
            OctetString = 4,
            Null = 5,
            ObjectIdentifier = 6,
            Sequence = 16,
        }

        internal class Decoder
        {
            public Decoder(byte[] bytes)
            {
                _bytes = bytes;
                _index = 0;
            }

            private byte[] _bytes;
            private int _index;

            public object Decode()
            {
                Tag tag = ReadTag();
                switch (tag)
                {
                    case Tag.Integer:
                        return ReadInteger();
                    case Tag.OctetString:
                        return ReadOctetString();
                    case Tag.Null:
                        return ReadNull();
                    case Tag.ObjectIdentifier:
                        return ReadOid();
                    case Tag.Sequence:
                        return ReadSequence();
                    default:
                        throw new NotSupportedException($"Tag '{tag}' not supported.");
                }
            }

            private byte NextByte() => _bytes[_index++];

            private byte[] ReadLengthPrefixedBytes()
            {
                int length = ReadLength();
                return ReadBytes(length);
            }

            private byte[] ReadInteger() => ReadLengthPrefixedBytes();

            private object ReadOctetString()
            {
                byte[] bytes = ReadLengthPrefixedBytes();
                return new Decoder(bytes).Decode();
            }

            private object ReadNull()
            {
                int length = ReadLength();
                if (length != 0)
                {
                    throw new InvalidDataException("Invalid data, Null length must be 0.");
                }
                return null;
            }

            private int[] ReadOid()
            {
                byte[] oidBytes = ReadLengthPrefixedBytes();
                List<int> result = new List<int>();
                bool first = true;
                int index = 0;
                while (index < oidBytes.Length)
                {
                    int subId = 0;
                    byte b;
                    do
                    {
                        b = oidBytes[index++];
                        if ((subId & 0xff000000) != 0)
                        {
                            throw new NotSupportedException("Oid subId > 2^31 not supported.");
                        }
                        subId = (subId << 7) | (b & 0x7f);
                    } while ((b & 0x80) != 0);
                    if (first)
                    {
                        first = false;
                        result.Add(subId / 40);
                        result.Add(subId % 40);
                    }
                    else
                    {
                        result.Add(subId);
                    }
                }
                return result.ToArray();
            }

            private object[] ReadSequence()
            {
                int length = ReadLength();
                int endOffset = _index + length;
                if (endOffset < 0 || endOffset > _bytes.Length)
                {
                    throw new InvalidDataException("Invalid sequence, too long.");
                }
                List<object> sequence = new List<object>();
                while (_index < endOffset)
                {
                    sequence.Add(Decode());
                }
                return sequence.ToArray();
            }

            private byte[] ReadBytes(int length)
            {
                if (length <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length), "length must be positive.");
                }
                if (_bytes.Length - length < 0)
                {
                    throw new ArgumentException("Cannot read past end of buffer.");
                }
                byte[] result = new byte[length];
                Array.Copy(_bytes, _index, result, 0, length);
                _index += length;
                return result;
            }

            private Tag ReadTag()
            {
                byte b = NextByte();
                int tag = b & 0x1f;
                if (tag == 0x1f)
                {
                    // A tag value of 0x1f (31) indicates a tag value of >30 (spec section 8.1.2.4)
                    throw new NotSupportedException("Tags of value > 30 not supported.");
                }
                else
                {
                    return (Tag)tag;
                }
            }

            private int ReadLength()
            {
                byte b0 = NextByte();
                if ((b0 & 0x80) == 0)
                {
                    return b0;
                }
                else
                {
                    if (b0 == 0xff)
                    {
                        throw new InvalidDataException("Invalid length byte: 0xff");
                    }
                    int byteCount = b0 & 0x7f;
                    if (byteCount == 0)
                    {
                        throw new NotSupportedException("Lengths in Indefinite Form not supported.");
                    }
                    int result = 0;
                    for (int i = 0; i < byteCount; i++)
                    {
                        if ((result & 0xff800000) != 0)
                        {
                            throw new NotSupportedException("Lengths > 2^31 not supported.");
                        }
                        result = (result << 8) | NextByte();
                    }
                    return result;
                }
            }

        }

        public static object Decode(byte[] bs) => new Decoder(bs).Decode();

    }

    public static RSAParameters DecodeRsaParameters(string pkcs8PrivateKey)
    {
        const string PrivateKeyPrefix = "-----BEGIN PRIVATE KEY-----";
        const string PrivateKeySuffix = "-----END PRIVATE KEY-----";

        pkcs8PrivateKey = pkcs8PrivateKey.Trim();
        if (!pkcs8PrivateKey.StartsWith(PrivateKeyPrefix) || !pkcs8PrivateKey.EndsWith(PrivateKeySuffix))
        {
            throw new ArgumentException(
                $"PKCS8 data must be contained within '{PrivateKeyPrefix}' and '{PrivateKeySuffix}'.", nameof(pkcs8PrivateKey));
        }
        string base64PrivateKey =
            pkcs8PrivateKey.Substring(PrivateKeyPrefix.Length, pkcs8PrivateKey.Length - PrivateKeyPrefix.Length - PrivateKeySuffix.Length);
        // FromBase64String() ignores whitespace, so further Trim()ing isn't required.
        byte[] pkcs8Bytes = Convert.FromBase64String(base64PrivateKey);

        object ans1 = Asn1.Decode(pkcs8Bytes);
        object[] parameters = (object[])((object[])ans1)[2];

        var rsaParmeters = new RSAParameters
        {
            Modulus = TrimLeadingZeroes((byte[])parameters[1]),
            Exponent = TrimLeadingZeroes((byte[])parameters[2], alignTo8Bytes: false),
            D = TrimLeadingZeroes((byte[])parameters[3]),
            P = TrimLeadingZeroes((byte[])parameters[4]),
            Q = TrimLeadingZeroes((byte[])parameters[5]),
            DP = TrimLeadingZeroes((byte[])parameters[6]),
            DQ = TrimLeadingZeroes((byte[])parameters[7]),
            InverseQ = TrimLeadingZeroes((byte[])parameters[8]),
        };

        return rsaParmeters;
    }

    internal static byte[] TrimLeadingZeroes(byte[] bs, bool alignTo8Bytes = true)
    {
        int zeroCount = 0;
        while (zeroCount < bs.Length && bs[zeroCount] == 0) zeroCount += 1;

        int newLength = bs.Length - zeroCount;
        if (alignTo8Bytes)
        {
            int remainder = newLength & 0x07;
            if (remainder != 0)
            {
                newLength += 8 - remainder;
            }
        }

        if (newLength == bs.Length)
        {
            return bs;
        }

        byte[] result = new byte[newLength];
        if (newLength < bs.Length)
        {
            Buffer.BlockCopy(bs, bs.Length - newLength, result, 0, newLength);
        }
        else
        {
            Buffer.BlockCopy(bs, 0, result, newLength - bs.Length, bs.Length);
        }
        return result;
    }

}