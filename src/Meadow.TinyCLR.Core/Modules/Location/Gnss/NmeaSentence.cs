// Decompiled with JetBrains decompiler
// Type: Meadow.Peripherals.Sensors.Location.Gnss.NmeaSentence
// Assembly: Meadow, Version=0.15.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D9B35635-6FC1-451D-8AC3-289CFF008676
// Assembly location: D:\experiment\TINYCLR\Meadow.Foundation\Source\Meadow.Foundation.Libraries_and_Frameworks\Sensors.GPS.NMEA\bin\Debug\net472\Meadow.dll

//using Meadow.Utilities;
using Meadow.Utilities;
using System;
using System.Collections;
//using System.Collections.Generic;



namespace Meadow.Peripherals.Sensors.Location.Gnss
{
  public class NmeaSentence
  {
    public string StartingDelimiter { get; set; } = "$";

    public string TalkerID { get; set; } = "GP";

    public string TalkerSystemName => Lookups.KnownTalkerIDs.Contains(this.TalkerID) ? Lookups.KnownTalkerIDs[this.TalkerID].ToString():string.Empty;

    public string Prefix { get; set; }

    public ArrayList DataElements { get; set; } = new  ArrayList();

    public byte Checksum => ChecksumCalculator.XOR(this.GetDataString());

    protected string GetDataString() => this.StartingDelimiter + this.TalkerID + this.Prefix + "," +  JoinString(",", (string[]) this.DataElements.ToArray());
    string JoinString(string separator, params string[] data)
        {
            var counter = 0;
            var combined = "";
            foreach(var item in data)
            {
                counter++;
                if (counter > 1)
                {
                    combined += separator;
                }
                combined += item;
            }
            return combined;
        }
    public override string ToString()
    {
      string dataString = this.GetDataString();
      return dataString + "*" + ChecksumCalculator.XOR(dataString).ToString();
    }

        /*
    public static NmeaSentence From(string sentence)
    {
      NmeaSentence nmeaSentence = new NmeaSentence();
      int length1 = !string.IsNullOrEmpty(sentence) ? sentence.LastIndexOf('*') : throw new ArgumentException("Empty sentence. Nothing to parse.");
      string str1 = length1 > 0 ? sentence.Substring(length1 + 1) : throw new ArgumentException("No checksum found. Invalid data.");
      string str2 = sentence.Substring(0, length1);
      byte num1 = ChecksumCalculator.XOR(str2.Substring(1));
      byte num2;
      try
      {
        num2 = Convert.ToByte(str1.Trim(), 16);
      }
      catch (Exception ex)
      {
        throw new ArgumentException("Checksum failed to parse, error: " + ex.Message);
      }
      if ((int) num1 != (int) num2)
        throw new ArgumentException("Checksum does not match data. Invalid data.");
      int length2 = str2.IndexOf(',') - 1 == 5 ? 2 : 1;
      int startIndex = length2 == 2 ? 3 : 2;
      nmeaSentence.StartingDelimiter = str2.Substring(0, 1);
      nmeaSentence.TalkerID = str2.Substring(1, length2);
      nmeaSentence.Prefix = str2.Substring(startIndex, 3);
      Span<string> span1 = (Span<string>) MemoryExtensions.AsSpan<string>((M0[]) str2.Split(',', StringSplitOptions.None));
      if (((Span<string>) ref span1).get_Length() <= 0)
        throw new ArgumentException("No data in sentence.");
      nmeaSentence.DataElements.Clear();
      List<string> dataElements = nmeaSentence.DataElements;
      Span<string> span2 = ((Span<string>) ref span1).Slice(1);
      string[] array = ((Span<string>) ref span2).ToArray();
      dataElements.AddRange((IEnumerable<string>) array);
      return nmeaSentence;
    }*/
  }
}
