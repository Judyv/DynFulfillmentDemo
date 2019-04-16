using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestDynGPConnect
{
    public partial class MainForm : Form
    {
        public SettingsManager settingsManager;
        public DynConnection dynConnection;
        public Logger logger;
        public static BindingList<string> ScreenMsgs = new BindingList<string> { };
        private string datapath = "";
        private bool initializing = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string UpdateResponse = "";

            try
            {

                logger.Log("Update Fulfilled Qty - START");
                // Need to add AllocateBy attribute: <SourceDocument DocKey1="ORDST2291" AllocateBy="1"
                //string text = System.IO.File.ReadAllText(@"C:\work\dynamics-gp-connect\TestDynGPConnect\writeback_tofulfill_comps1lot.xml");
                string fname = settingsManager.Settings.InputFolder.TrimEnd('\\') + @"\" + settingsManager.Settings.InputFile;
                string text = System.IO.File.ReadAllText(@fname);
                var DocList = new DocumentList();
                DocList.LoadFromXML(text, logger);
                logger.Log("Update Fulfilled Qty - Successfully Loaded WriteBackXML");
                DocList.LogMe(logger);
                ShowMe(DocList);

                // exit if no documents are of type that need write-back!!!
                if (DocList.AnyNeedFulfillment())
                {
                    var GPIF = new GPInterface(logger,dynConnection.wsDynamicsGP);
                    GPIF.UpdateDocuments(DocList, cbCompany.Text );
                    UpdateResponse = "Successfully updated Fulfilled Qty!";
                    for (int i = 0; i < DocList.Documents.Length; i++)
                    {
                        if (DocList.Documents[i].RequiredUpdate)
                        {
                            UpdateResponse = UpdateResponse + "\t" + DocList.Documents[i].CombinedKey;
                        }
                    }
                }
                else
                {
                    AddScreenMsg("Document type(s) do not require fulfillment.");
                    logger.Log("Document type(s) do not require fulfillment.");
                    UpdateResponse = "Successfully updated Fulfilled Qty!";
                }             

            }
            catch (System.Exception ex)
            {
                UpdateResponse = ex.Message;
            }

            logger.Log(UpdateResponse);
            AddScreenMsg(UpdateResponse);

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        public void AddScreenMsg(string msg)
        {
            ScreenMsgs.Add(msg);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                settingsManager.StoreSettings();
                dynConnection.Disconnect();                
            }
            catch (Exception ex)
            {
                logger.Log("Error disconnecting : " + ex.Message);
                System.Windows.Forms.MessageBox.Show(ex.Message);

            }
        }

        private void InitDynCompanies()
        {

            cbCompany.Items.Clear();
            cbCompany.DisplayMember = "Name";
            for (int i = 0; i < dynConnection.companyList.Length; i++)
            {
                cbCompany.Items.Add(dynConnection.companyList[i]);
                if (settingsManager.Settings.DynamicCompany == dynConnection.companyList[i].Name)
                {
                    cbCompany.SelectedIndex = i;
                }
            }

        }

        private void InitInputFiles()
        {

            cbInputFile.Items.Clear();
            cbInputFile.SelectedIndex = -1;
            cbInputFile.Text = "";
            if (settingsManager.Settings.InputFolder.Trim() != "")
            try
            {
                string[] files = System.IO.Directory.GetFiles(@settingsManager.Settings.InputFolder, "*.XML");

                //cbInputFile.Items.AddRange(files);
                for (int i = 0; i < files.Length; i++)
                {
                    cbInputFile.Items.Add(System.IO.Path.GetFileName(files[i]));
                }
                if (cbInputFile.Items.Count > 0)
                    cbInputFile.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }


        private void DisplaySettings()
        {
            initializing = true;
            try
            {
                edDynServer.Text = settingsManager.Settings.DynamicServer;
                edDynPort.Text = settingsManager.Settings.DynamicPort;
                if (dynConnection.IsConnected)
                {
                    btDynConnect.Text = "Disconnect";

                }
                edInputFolder.Text = settingsManager.Settings.InputFolder;
                cbInputFile.SelectedIndex = cbInputFile.Items.IndexOf(settingsManager.Settings.InputFile);
                if ((cbInputFile.Items.Count > 0) && (cbInputFile.SelectedIndex < 0))
                {
                    cbInputFile.SelectedIndex = 0;
                }
            }
            finally
            {
                initializing = false;
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            try
            {
                datapath = Environment.ExpandEnvironmentVariables("%LocalAppData%") +
                    "\\V-Technologies\\StarShip\\Test\\";
                System.IO.Directory.CreateDirectory(datapath);
                logger = new Logger(datapath);
                logger.Log("Load settings from " + datapath + "DynTestAPI.XML");
                settingsManager = new SettingsManager(datapath + "DynTestAPI.XML", logger);
                dynConnection = new DynConnection();
                lbMessages.DataSource = ScreenMsgs;
                Refresh();
                //btCancel.Enabled = false;
                // Set cursor as hourglass
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    try
                    {
                        if (settingsManager.SettingsAreValid(dynConnection, true))
                        {
                            logger.Log("Settings are valid");
                            btStart.Enabled = true;
                        }
                        else
                        {
                            logger.Log("Settings are not valid");
                            btStart.Enabled = false;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        logger.Log("Error in SettingsAreValid : " + ex.Message);
                        System.Windows.Forms.MessageBox.Show("Error checking settings : " + ex.Message);
                        btStart.Enabled = false;
                    }
                    if (dynConnection.IsConnected)
                    {
                        initializing = true;
                        try
                        {
                            InitDynCompanies();
                        }
                        finally
                        {
                            initializing = false;
                        }
                        if (cbCompany.SelectedIndex >= 0)
                        {
                            settingsManager.Settings.DynamicCompany = cbCompany.Text;
                            dynConnection.CurrentCompany = cbCompany.Text;
                        }
                    }

                    InitInputFiles();

                    DisplaySettings();
                }
                finally
                {
                    // Set cursor as default arrow
                    Cursor.Current = Cursors.Default;
                }
            }
            catch (System.Exception ex)
            {
                logger.Log("Error on startup : " + ex.Message);
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        private void CheckValid()
        {
            if (dynConnection.IsConnected)
            {
                btStart.Enabled = settingsManager.SettingsAreValid(dynConnection, false);
            }
            else
                btStart.Enabled = false;
        }

        private void btDynConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    if (dynConnection.IsConnected == false)
                    {
                        dynConnection.Connect(settingsManager.Settings.DynamicServer, settingsManager.Settings.DynamicPort);
                        if (dynConnection.IsConnected)
                        {
                            InitDynCompanies();

                            if ((cbCompany.Items.Count > 0) && (cbCompany.SelectedIndex < 0))
                                cbCompany.SelectedIndex = 0;

                            if (dynConnection.CurrentCompany != cbCompany.Text)
                            {
                                settingsManager.Settings.DynamicCompany = cbCompany.Text;
                                dynConnection.CurrentCompany = cbCompany.Text;
                                DisplaySettings();
                            }

                            btDynConnect.Text = "Disconnect";

                            CheckValid();

                        }
                    }
                    else
                    {
                        dynConnection.Disconnect();
                        btDynConnect.Text = "Connect";
                    }
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
            catch (System.Exception ex)
            {
                logger.Log("Error connecting to Dynamics : " + ex.Message);
                System.Windows.Forms.MessageBox.Show(ex.Message);

            }

        }

        private void edDynServer_TextChanged(object sender, EventArgs e)
        {
            if (!initializing)
            {
                settingsManager.Settings.DynamicServer = edDynServer.Text;
                CheckValid();
            }
        }

        private void edDynPort_TextChanged(object sender, EventArgs e)
        {
            if (!initializing)
            {
                settingsManager.Settings.DynamicPort = edDynPort.Text;
                CheckValid();
            }
        }

        private void cbCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dynConnection.CurrentCompany != cbCompany.Text)
            {
                settingsManager.Settings.DynamicCompany = cbCompany.Text;
                dynConnection.CurrentCompany = cbCompany.Text;
                DisplaySettings();
                CheckValid();
            }

        }

        private void cbInputFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            settingsManager.Settings.InputFile = cbInputFile.Text;
        }


        private void ShowMe(DocumentList DocList)
        {
            try
            {
                for (int i = 0; i < DocList.Documents.Length; i++)
                {
                    AddScreenMsg("  Document: " + DocList.Documents[i].SopNumber + "," + DocList.Documents[i].TypeStr);
                    AddScreenMsg(string.Format("   AddMode: {0}", DocList.Documents[i].IsAddMode));

                    for (int j = 0; j < DocList.Documents[i].Items.Length; j++)
                    {
                        AddScreenMsg("    Line Item: " + DocList.Documents[i].Items[j].ItemNumber + " ItemSeq = " +
                         string.Format("{0:D}", DocList.Documents[i].Items[j].ItemSequence) +
                         " CompSeq = " + string.Format("{0:D}", DocList.Documents[i].Items[j].ComponentSequence) +
                         " DeltaShip = " + string.Format("{0:0.00}", DocList.Documents[i].Items[j].DeltaShip) +
                         " DistType = " + string.Format("{0:D}", DocList.Documents[i].Items[j].DistributionType));

                        for (int k = 0; k < DocList.Documents[i].Items[j].Distributions.Length; k++)
                        {
                            AddScreenMsg("      Distribution:  LOT/Serial = " +
                             string.Format("{0:D}", DocList.Documents[i].Items[j].Distributions[k].LotSerial) +
                             " Qty = " + string.Format("{0:0.00}", DocList.Documents[i].Items[j].Distributions[k].Quantity));

                        }

                    }
                }

                AddScreenMsg("Settings: ");
                AddScreenMsg(string.Format("   IncludeNonInventoryItems = {0}", DocList.IncludeNonInventoryItems));
                AddScreenMsg(string.Format("   IncludeKitComponents = {0}", DocList.IncludeKitComponents));
                AddScreenMsg(string.Format("   IncludeMiscCharges = {0}", DocList.IncludeMiscCharges));
                AddScreenMsg(string.Format("   IncludeServices = {0}", DocList.IncludeServices));
                AddScreenMsg(string.Format("   IncludeFlatFee = {0}", DocList.IncludeFlatFee));
                AddScreenMsg(string.Format("   BackOrder = {0}", DocList.DoBackOrder));
            }
            catch (System.Exception ex)
            {
                logger.Log("Error logging document list : " + ex.Message);
            }
       }

        private void edInputFolder_Leave(object sender, EventArgs e)
        {
            if (edInputFolder.Text != settingsManager.Settings.InputFolder)
            {
                settingsManager.Settings.InputFolder = edInputFolder.Text;
                InitInputFiles();
            }
        }

 
    }


}