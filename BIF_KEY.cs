using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace WitcherBIFTool
{
   

    public class BIF_FILETABLE_ENTRY
    {
        public UInt32 BIFSize, NameOffset, NameSize;
        public String BIFName;
        public List<BIF_KEYTABLE_ENTRY> ownedResources = new List<BIF_KEYTABLE_ENTRY>();

        public override string ToString()
        {
            return "BIF_FILETABLE_ENTRY " + BIFSize + " Size " + NameOffset + " NameOff " + NameSize + " NameSize " + BIFName;
        }

        public BIF_FILETABLE_ENTRY(BinaryReader br)
        {

            BIFSize = br.ReadUInt32();
            NameOffset = br.ReadUInt32();
            NameSize = br.ReadUInt32();

            long seekPos = br.BaseStream.Position;

            br.BaseStream.Seek(NameOffset, SeekOrigin.Begin);
            BIFName = new String(ASCIIEncoding.ASCII.GetChars(br.ReadBytes(Convert.ToInt32(NameSize))));
           
            br.BaseStream.Seek(seekPos, SeekOrigin.Begin);
        }

    }

    public class BIF_KEYTABLE_ENTRY
    {
        public String ResourceName = "";
        public UInt32 ResourceID, Flags;
        public UInt16 ResourceType;
        public UInt32 ResourceIdx;

        public override string ToString()
        {
            return "BIF_KEYTABLE_ENTRY " + ResourceName + " RESNAME " + ResourceID + " RESID " + ResourceType + " RESTYPE " + ResourceIdx + " RESIDX ";
        }

        public BIF_KEYTABLE_ENTRY(BinaryReader br)
        {

            ResourceName = new String(br.ReadChars(16)).Replace("\0","");
            ResourceType = br.ReadUInt16();
            ResourceID = br.ReadUInt32();
            Flags = br.ReadUInt32();
            
            ResourceIdx = Flags;
            
            ResourceIdx = (ResourceIdx & 0xFFF00000) >> 20;
            //MessageBox.Show(ResourceIdx+"");
        }

    }

    public class BIF_KEY
    {
        public char[] signature = new char[4];
        public char[] version = new char[4];
        public UInt32 FILETABLE_ENTRIES, KEYTABLE_ENTRIES, FILETABLE_OFFSET, KEYTABLE_OFFSET, BUILD_YEAR = 1900, BUILD_DAY;
        public List<BIF_FILETABLE_ENTRY> FILETABLE = new List<BIF_FILETABLE_ENTRY>();

        public override string ToString()
        {
            return "BIF_KEY " + FILETABLE_ENTRIES + " FILETABS " + KEYTABLE_ENTRIES + " KEYTABS " + FILETABLE_OFFSET + " FILEOFF " + KEYTABLE_OFFSET + " KEYTABLEOFFS " + BUILD_YEAR + "YEAR "+ BUILD_DAY;
        }

        public BIF_KEY(String filename)
        {
            BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open));
            
            signature = br.ReadChars(4);
            version = br.ReadChars(4);
            
            FILETABLE_ENTRIES = br.ReadUInt32();
            KEYTABLE_ENTRIES = br.ReadUInt32();
            br.ReadBytes(4);

            FILETABLE_OFFSET = br.ReadUInt32();
            KEYTABLE_OFFSET = br.ReadUInt32();
            BUILD_YEAR += br.ReadUInt32();
            BUILD_DAY = br.ReadUInt32();

           // MessageBox.Show(this+"");


            br.BaseStream.Seek(FILETABLE_OFFSET, SeekOrigin.Begin);

            for (int i = 0; i < FILETABLE_ENTRIES; i++)
            {
                FILETABLE.Add(new BIF_FILETABLE_ENTRY(br)); 
            }

            br.BaseStream.Seek(KEYTABLE_OFFSET, SeekOrigin.Begin);

            for (int i = 0; i < KEYTABLE_ENTRIES; i++)
            {
                BIF_KEYTABLE_ENTRY key = new BIF_KEYTABLE_ENTRY(br);
                FILETABLE[(int)key.ResourceIdx].ownedResources.Add(key);
            }

            br.Close();

        }
    }

    public class BIF_Utility
    {

        public static String makeNewResName(String filename, UInt16 restype)
        {
            return filename + "." +  resTypeLookup[restype];
        }

        public static void StreamCopyTo(Stream a, Stream b)
        {
            byte[] chunk = new byte[8192]; int cnt;
            while ((cnt = a.Read(chunk, 0, chunk.Length)) != 0)
                b.Write(chunk, 0, cnt);
        }

        public static Dictionary<UInt16, String> resTypeLookup = new Dictionary<ushort, string>()
        {	
	            {0x0000 , "res"},     
	            {0x0001 , "bmp"},
	            {0x0002 , "mve"},
	            {0x0003 , "tga"},
	            {0x0004 , "wav"},
	            {0x0006 , "plt"},
	            {0x0007 , "ini"},
	            {0x0008 , "mp3"},
	            {0x0009 , "mpg"},
	            {0x000A , "txt"},
	            {0x000B , "xml"},
	            {0x07D0 , "plh"},
	            {0x07D1 , "tex"},
	            {0x07D2 , "mdl"},
	            {0x07D3 , "thg"},
	            {0x07D5 , "fnt"},
	            {0x07D7 , "lua" },    
	            {0x07D8 , "slt"},
	            {0x07D9 , "nss"},
	            {0x07DA , "ncs"},
	            {0x07DB , "mod"},
	            {0x07DC , "are" },    
	            {0x07DD , "set"  },   
	            {0x07DE , "ifo"},
	            {0x07DF , "bic" },    
	            {0x07E0 , "wok"  },   
	            {0x07E1 , "2da"   },  
	            {0x07E2 , "tlk"},
	            {0x07E6 , "txi"},
	            {0x07E7 , "git" },    
	            {0x07E8 , "bti"},
	            {0x07E9 , "uti"},
	            {0x07EA , "btc"},
	            {0x07EB , "utc"},
	            {0x07ED , "dlg"},
	            {0x07EE , "itp"},
	            {0x07EF , "btt"},
	            {0x07F0 , "utt"},
	            {0x07F1 , "dds"},
	            {0x07F2 , "bts"},
	            {0x07F3 , "uts"},
	            {0x07F4 , "ltr"},
	            {0x07F5 , "gff"},
	            {0x07F6 , "fac"},
	            {0x07F7 , "bte"},
	            {0x07F8 , "ute"},
	            {0x07F9 , "btd"},
	            {0x07FA , "utd"},
	            {0x07FB , "btp"},
	            {0x07FC , "utp"},
	            {0x07FD , "dft" },    
	            {0x07FE , "gic"},
	            {0x07FF , "gui" },    
	            {0x0800 , "css"},
	            {0x0801 , "ccs"},
	            {0x0802 , "btm"},
	            {0x0803 , "utm"},
	            {0x0804 , "dwk"},
	            {0x0805 , "pwk"},
	            {0x0806 , "btg"},
	            {0x0808 , "jrl"},
	            {0x0809 , "sav" },   
	            {0x080A , "utw"},
	            {0x080B , "4pc"},
	            {0x080C , "ssf"},
	            {0x080F , "bik" },   
	            {0x0810 , "ndb"},
	            {0x0811 , "ptm" },  
	            {0x0812 , "ptt"},
	            {0x0813 , "ncm"},
	            {0x0814 , "mfx"},
	            {0x0815 , "mat"},
	            {0x0816 , "mdb" },       
	            {0x0817 , "say"},
	            {0x0818 , "ttf" },       
	            {0x0819 , "ttc"},
	            {0x081A , "cut" },       
	            {0x081B , "ka"   },      
	            {0x081C , "jpg"},
	            {0x081D , "ico" },       
	            {0x081E , "ogg"},
	            {0x081F , "spt"},
	            {0x0820 , "spw"},
	            {0x0821 , "wfx" },       
	            {0x0822 , "ugm"  },      
	            {0x0823 , "qdb"   },     
	            {0x0824 , "qst"    },    
	            {0x0825 , "npc"},
	            {0x0826 , "spn"},
	            {0x0827 , "utx" },       
	            {0x0828 , "mmd"},
	            {0x0829 , "smm"},
	            {0x082A , "uta" },      
	            {0x082B , "mde"},
	            {0x082C , "mdv"},
	            {0x082D , "mda"},
	            {0x082E , "mba"},
	            {0x082F , "oct"},
	            {0x0830 , "bfx"},
	            {0x0831 , "pdb"},
	            {0x0832 , "TheWitcherSave"},
	            {0x0833 , "pvs"},
	            {0x0834 , "cfx"},
	            {0x0835 , "luc"},
	            {0x0837 , "prb"},
	            {0x0838 , "cam"},
	            {0x0839 , "vds"},
	            {0x083A , "bin"},
	            {0x083B , "wob"},
	            {0x083C , "api"},
	            {0x083D , "properties"},
	            {0x083E , "png"},
	            {0x270B , "big"},
	            {0x270D , "erf"},
	            {0x270E , "bif"},
	            {0x270F , "key" }       
        };
    }
}
