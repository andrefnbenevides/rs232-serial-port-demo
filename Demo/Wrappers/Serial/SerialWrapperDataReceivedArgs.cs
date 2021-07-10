namespace Demo.Wrappers.Serial
{
   public class SerialWrapperDataReceivedArgs
   {
      public byte[] DataBytes { get; set; }
      public string DataHex { get; set; }
      public SerialWrapperDataReceivedArgs(byte[] bytes, string data)
      {
         this.DataBytes = bytes;
         this.DataHex = data;
      }
   }
}