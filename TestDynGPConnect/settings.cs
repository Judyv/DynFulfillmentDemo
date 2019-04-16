using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// The XmlRootAttribute allows you to set an alternate name 
// for the XML element and its namespace. By 
// default, the XmlSerializer uses the class name. The attribute 
// also allows you to set the XML namespace for the element. Lastly,
// the attribute sets the IsNullable property, which specifies whether 
// the xsi:null attribute appears if the class instance is set to 
// a null reference.
[XmlRootAttribute("DynRateToolSettings", 
IsNullable = false)]
public class DynRateToolSettings
{
    public string DynamicServer = "";
    public string DynamicPort = "";
    public string DynamicCompany = "";
    public string InputFolder = "";
    public string InputFile = "";
    public bool DiagnosticLog = false;
    public bool Backorder = false;
    
    
    public DynRateToolSettings()
    {
        DynamicPort = "48620";
        InputFolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
    }
}


public class SettingsManager
{
    public DynRateToolSettings Settings;
    public string filename;
    public bool SettingsValidated;
    private Logger logger;
    
    public void StoreSettings()
    {
        // Creates an instance of the XmlSerializer class;
        // specifies the type of object to serialize.
        XmlSerializer serializer = new XmlSerializer(typeof(DynRateToolSettings));
        TextWriter writer = new StreamWriter(filename);
        // Serializes the purchase order, and closes the TextWriter.
        serializer.Serialize(writer, Settings);
        writer.Close();
    }

    public void LoadSettings()
    {
        // if file does not exist... just create new settings.
        if (File.Exists(filename) == false)
        {
            Settings = new DynRateToolSettings();
        }
        else
        {

            // Creates an instance of the XmlSerializer class;
            // specifies the type of object to be deserialized.
            XmlSerializer serializer = new XmlSerializer(typeof(DynRateToolSettings));
            // If the XML document has been altered with unknown 
            // nodes or attributes, handles them with the 
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new
            XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new
            XmlAttributeEventHandler(serializer_UnknownAttribute);

            // A FileStream is needed to read the XML document.
            FileStream fs = new FileStream(filename, FileMode.Open);
            // Declares an object variable of the type to be deserialized.
            //DynRateToolSettings po;
            // Uses the Deserialize method to restore the object's state 
            // with data from the XML document. */
            Settings = (DynRateToolSettings)serializer.Deserialize(fs);
            if (Settings.InputFolder.Trim() == "")
                Settings.InputFolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
        }
    }

    protected void serializer_UnknownNode
    (object sender, XmlNodeEventArgs e)
    {
        Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
    }

    protected void serializer_UnknownAttribute
    (object sender, XmlAttributeEventArgs e)
    {
        System.Xml.XmlAttribute attr = e.Attr;
        Console.WriteLine("Unknown attribute " +
        attr.Name + "='" + attr.Value + "'");
    }

    public SettingsManager(string settingfilename, Logger alogger)
    {
        filename = settingfilename;
        SettingsValidated = false;
        logger = alogger;
       
        LoadSettings();
    }


    public bool SettingsAreValid(DynConnection dynConnection, bool LogResults)
    {
        bool tmpValid = true;
        // for service, would be good to show ALL issues in log - not just first issue
        if (Settings.DynamicServer.Trim().Length == 0)
        {
            tmpValid = false;
        }
        if (Settings.DynamicPort.Trim().Length == 0)
        {
            tmpValid = false;
        }
        if (Settings.DynamicCompany.Trim().Length == 0)
        {
            tmpValid = false;
        }
        if (tmpValid)
        {
          // validate dynamics settings
          if (!dynConnection.IsConnected)
          {
              try
              {
                  dynConnection.Connect(Settings.DynamicServer, Settings.DynamicPort);
              }
              catch (System.Exception ex)
              {
                  tmpValid = false;
                  if (LogResults)
                      logger.Log("Error connecting to Dynamics : " + ex.Message);
              }
          }
          if (dynConnection.IsConnected)
          {
              int j = dynConnection.IndexOfCompanyName(Settings.DynamicCompany);
              if (j < 0)
              {
                  tmpValid = false;
              }
          }
          else
              tmpValid = false;
         
        }


        return tmpValid;
    }
}