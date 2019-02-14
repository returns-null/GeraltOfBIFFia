﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WitcherBIFTool
{
    public partial class BIFTool : Form
    {
        String pathContext = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        
        public BIFTool()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openKeyFileDialog.InitialDirectory = pathContext;
            openKeyFileDialog.Title = "Choose a Witcher 1 .KEY File...";
            openKeyFileDialog.ShowDialog();
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            
            TreeNode selNode = BIFTreeView.SelectedNode;

            if (selNode != null && selNode.Parent == null)
            {
                saveResourceDialog.Title = "Choose an export directory - loose files will be exported here.";
                saveResourceDialog.FileName = selNode.Text;
                DialogResult dr = saveResourceDialog.ShowDialog();

                if (dr == System.Windows.Forms.DialogResult.OK)
                {

                    foreach(TreeNode child in selNode.Nodes)
                    {
                        BIFSaveOps.ExtractResourceFromTVNode(selNode.Text, child, pathContext, null);
                    }

                }
            }
            else if (selNode != null && selNode.Parent != null)
            {

                BIFSaveOps.ExtractResourceFromTVNode(selNode.Parent.Text,selNode, pathContext, saveResourceDialog);

            }
        }

        private void openKeyFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            BIFTreeView.Nodes.Clear();

            try
            {

                pathContext = Path.GetDirectoryName(openKeyFileDialog.FileName) + "\\";
                String modName = Path.GetFileName(openKeyFileDialog.FileName);
                BIF_KEY tannen = new BIF_KEY(pathContext + modName);

                foreach (BIF_FILETABLE_ENTRY file in tannen.FILETABLE)
                {
                    TreeNode BIFLevelNode = new TreeNode(file.BIFName);

                    foreach (BIF_KEYTABLE_ENTRY key in file.ownedResources)
                    {
                        TreeNode ResourceLevelNode = new TreeNode(BIF_Utility.makeNewResName(key.ResourceName, key.ResourceType));
                        ResourceLevelNode.Tag = key;
                        BIFLevelNode.Nodes.Add(ResourceLevelNode);
                    }
                    BIFTreeView.Nodes.Add(BIFLevelNode);
                }
            }
            catch (FileNotFoundException ee)
            {
                MessageBox.Show("There was a problem reading the BIF index (KEY) file: \r\n"
                    + ee.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    class BIFSaveOps
    {
        public static void ExtractResourceFromTVNode(String BIFNameIn, TreeNode selNode, String pathContext, SaveFileDialog saveResourceDialog)
        {
            String BIFname = BIFNameIn;
            BIF_KEYTABLE_ENTRY nodeData = ((BIF_KEYTABLE_ENTRY)selNode.Tag);
            String RESname = nodeData.ResourceName;

            try
            {
                DialogResult dr = DialogResult.Abort;

                MemoryStream ms = BIF_DATA.getFileDataFromBIF(pathContext + BIFname, nodeData.ResourceID);

                if( saveResourceDialog != null )
                {
                    saveResourceDialog.FileName = BIF_Utility.makeNewResName(nodeData.ResourceName, nodeData.ResourceType);
                    dr = saveResourceDialog.ShowDialog();
                }

                if (saveResourceDialog == null || dr == System.Windows.Forms.DialogResult.OK)
                {
                    StreamWriter sw = new StreamWriter(new FileStream(BIF_Utility.makeNewResName(nodeData.ResourceName, nodeData.ResourceType), FileMode.Create));
                    ms.Position = 0;
                    BIF_Utility.StreamCopyTo(ms, sw.BaseStream);
                    sw.Close();
                }
            }
            catch (FileNotFoundException fnf_ee)
            {
                MessageBox.Show("There was a problem retreiving the file from the BIF archive: \r\n"
                    + fnf_ee.Message + "\r\nHint: Voice localization files for The Witcher DLC (e.g. M1_3_00.bif ) are stored in the voices subdirectory.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ee)
            {
                MessageBox.Show("There was a problem retreiving the file from the BIF archive: \r\n"
                    + ee.Message + "",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}
