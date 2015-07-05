using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace WitcherBIFTool
{
    public class BIF_DATA
    {
       
        public static MemoryStream getFileDataFromBIF(String filename, long idx)
        {
            MemoryStream ms = new MemoryStream();
            BinaryReader br = new BinaryReader(new FileStream (filename, FileMode.Open));

            String magic, version;
            UInt32 resource_count, resource_table_offset;

            magic = new String(ASCIIEncoding.ASCII.GetChars(br.ReadBytes(4)));
            version = new String(ASCIIEncoding.ASCII.GetChars(br.ReadBytes(4)));

            resource_count = br.ReadUInt32();
            br.ReadUInt32();
            resource_table_offset = br.ReadUInt32();

            br.BaseStream.Seek(resource_table_offset, SeekOrigin.Begin);

            for (int i = 0; i < resource_count; i++)
            {
                UInt32 resid, flags, offset, size, restype;

                resid = br.ReadUInt32();
                flags = br.ReadUInt32();
                offset = br.ReadUInt32();
                size = br.ReadUInt32();
                restype = br.ReadUInt32();

                if (resid == idx)
                {
                    br.BaseStream.Seek(offset, SeekOrigin.Begin);
                    byte[] data = br.ReadBytes(Convert.ToInt32(size));
                    ms.Write(data, 0, Convert.ToInt32(size));
                    break;
                }

            }

            br.Close();

            return ms;
        }
    }
}
