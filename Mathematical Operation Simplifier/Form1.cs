using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Mathematical_Operation_Simplifier
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        #region Variables
        string directoryName = "";
        string filePath = "";
        static ModuleDefMD module = null;
        public Thread thr;
        #endregion

        #region Github
        private void Label2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Rhotav");
        }
        #endregion

        #region Save
        static void SaveAssembly()
        {
            var writerOptions = new NativeModuleWriterOptions(module, true);
            writerOptions.Logger = DummyLogger.NoThrowInstance;
            writerOptions.MetadataOptions.Flags = (MetadataFlags.PreserveTypeRefRids | MetadataFlags.PreserveTypeDefRids | MetadataFlags.PreserveFieldRids | MetadataFlags.PreserveMethodRids | MetadataFlags.PreserveParamRids | MetadataFlags.PreserveMemberRefRids | MetadataFlags.PreserveStandAloneSigRids | MetadataFlags.PreserveEventRids | MetadataFlags.PreservePropertyRids | MetadataFlags.PreserveTypeSpecRids | MetadataFlags.PreserveMethodSpecRids | MetadataFlags.PreserveStringsOffsets | MetadataFlags.PreserveUSOffsets | MetadataFlags.PreserveBlobOffsets | MetadataFlags.PreserveAll | MetadataFlags.AlwaysCreateGuidHeap | MetadataFlags.PreserveExtraSignatureData | MetadataFlags.KeepOldMaxStack);
            module.NativeWrite(Path.GetDirectoryName(module.Location) + @"\" + Path.GetFileNameWithoutExtension(module.Location) + "_simplfied.exe", writerOptions);
        }

        #endregion

        #region Select
        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Executable Files|*.exe|DLL Files |*.dll";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    module = ModuleDefMD.Load(open.FileName);
                    filePath = open.FileName;
                    label4.Text = "Loaded !";
                    label4.ForeColor = Color.Lime;
                    listBox1.Items.Clear();
                    label3.Visible = false;
                    listBox1.Items.Add("File is loaded !");
                    listBox1.Items.Add("EntryPoint MDToken : 0x" + module.EntryPoint.MDToken.ToString());
                }
            }
            catch (Exception ex)
            {
                filePath = "";
                module = null;
                MessageBox.Show(ex.Message, "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label4.Text = "Not Loaded !";
                label4.ForeColor = Color.Lime;
                label3.Visible = true;

            }
        }
        #endregion

        private void Button2_Click(object sender, EventArgs e)
        {
            if (filePath != string.Empty && module != null)
            {
                thr = new Thread(new ThreadStart(CodeBlock));
                thr.Start();
            }
        }
        public void CodeBlock()
        {
            try
            {
                listBox1.Items.Add("Mathematical operations simplifying..");
                foreach (TypeDef type in module.Types)
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (!method.HasBody) continue;

                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_I4 &&
                                method.Body.Instructions[i + 1].OpCode == OpCodes.Ldc_I4 &&
                                method.Body.Instructions[i + 3].OpCode == OpCodes.Ldc_I4)
                            {
                                int one = method.Body.Instructions[i].GetLdcI4Value();
                                int two = method.Body.Instructions[i + 1].GetLdcI4Value();
                                int three = method.Body.Instructions[i + 3].GetLdcI4Value();
                                string opOne = islemDondur(method.Body.Instructions[i + 2].OpCode.ToString());
                                string opTwo = islemDondur(method.Body.Instructions[i + 4].OpCode.ToString());

                                int value = (int)(Eval(one, opOne, two, opTwo, three));

                                method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                                method.Body.Instructions[i].Operand = value;
                                method.Body.Instructions.RemoveAt(i + 1);
                                method.Body.Instructions.RemoveAt(i + 1);
                                method.Body.Instructions.RemoveAt(i + 1);
                                method.Body.Instructions.RemoveAt(i + 1);
                            }
                        }
                    }
                }
                SaveAssembly();
                listBox1.Items.Add("Compeleted and Saved !");
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
            }
        }


        #region Math
        public static string islemDondur(string deger)
        {
            switch (deger)
            {
                case "div":
                    return "/";
                case "sub":
                    return "-";
                case "add":
                    return "+";
            }
            return "";
        }
        public static double Eval(int bir, string islemBir, int iki, string islemİki, int uc)
        {
            string islem = bir.ToString() + islemBir + iki.ToString() + islemİki + uc.ToString();

            double sonuc = Convert.ToDouble(new DataTable().Compute(islem, null));

            return sonuc;
        }
        #endregion

        #region DragDrop
        private void ListBox1_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                Array array = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (array != null)
                {
                    string text = array.GetValue(0).ToString();
                    int num = text.LastIndexOf(".");
                    if (num != -1)
                    {
                        string text2 = text.Substring(num);
                        text2 = text2.ToLower();
                        if (text2 == ".exe" || text2 == ".dll")
                        {
                            Activate();
                            int num2 = text.LastIndexOf("\\");
                            if (num2 != -1)
                            {
                                directoryName = text.Remove(num2, text.Length - num2);
                            }
                            if (directoryName.Length == 2)
                            {
                                directoryName += "\\";
                            }
                            module = ModuleDefMD.Load(text);
                            filePath = text;
                            label4.Text = "Loaded !";
                            label4.ForeColor = Color.Lime;
                            label3.Visible = false;
                            listBox1.Items.Clear();
                            listBox1.Items.Add("File is loaded !");
                            listBox1.Items.Add("EntryPoint MDToken : 0x" + module.EntryPoint.MDToken.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                filePath = "";
                module = null;
                MessageBox.Show(ex.Message, "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label4.Text = "Not Loaded !";
                label4.ForeColor = Color.Red;

            }
        }

        private void ListBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        #endregion
    }
}
