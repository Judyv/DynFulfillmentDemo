using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;

public enum DistTypes : int 
{
    None = 0,
    Serial = 1,
    Lot = 2
}
        
public class DocumentList
{
    public enum IDType { DNS, UPN, SPN, Config}
    public bool IncludeNonInventoryItems { get; set; }
    public bool IncludeKitComponents { get; set; }
    public bool IncludeMiscCharges { get; set; }
    public bool IncludeServices { get; set; }
    public bool IncludeFlatFee { get; set; }
    public bool DoBackOrder { get; set; }
    public string Identity { get; set; }
    public IDType IdentityType { get; set; }
    public string CompanyName { get; set; }
    public Document[] Documents { get; set; }

    private static bool GetSettingValue(string pName, XElement pSourceDoc)
    {
        var xPath = string.Format(
            "/Settings/NameValue[@Name='{0}']",
            pName);

        var nameValue = pSourceDoc.XPathSelectElement(xPath);
        if (nameValue != null)
        {
            var attribute = nameValue.Attribute("Value");
            if (attribute != null)
               return (attribute.Value == "True");
            else 
               return false;
        }
        return false;
    }

    private static bool GetAddMode(XElement pSourceDoc)
    {
        var nameValue = pSourceDoc.XPathSelectElement("FreightInfo/Shipment/NameValue[@Name='Status']");
        if (nameValue != null)
        {
            var attribute = nameValue.Attribute("Value");
            if (attribute != null)
                return (attribute.Value == "Add");
            else
                return false;
        }
        return false;
    }

    public void LoadFromXML(string WriteBackXMLString, Logger logger)
    {

        try
        {
            XElement WriteXML = XElement.Parse(WriteBackXMLString);

            Identity= "";
            IdentityType = IDType.SPN;

            if (WriteXML.Attribute("GPIdentity") != null)
                Identity = WriteXML.Attribute("GPIdentity").Value;
            if (WriteXML.Attribute("GPIdentityType") != null)
            {
                if (WriteXML.Attribute("GPIdentityType").Value == "CFG")
                    IdentityType = IDType.Config;
                else if (WriteXML.Attribute("GPIdentityType").Value == "DNS")
                    IdentityType = IDType.DNS;
                else if (WriteXML.Attribute("GPIdentityType").Value == "UPN")
                    IdentityType = IDType.UPN;
            }
            if (WriteXML.Attribute("DoBackOrder") != null)
                DoBackOrder = (WriteXML.Attribute("DoBackOrder").Value.ToUpper() == "TRUE");
            
            
            IEnumerable<XElement> list = WriteXML.XPathSelectElements("/SourceDocument");
            var docs = new List<Document>();

            foreach (XElement el in list)
            {
                if (string.IsNullOrEmpty(this.CompanyName))
                    this.CompanyName = el.Attribute("SourceCompanyName").Value;

                var aDocument = new Document();
                aDocument.CombinedKey = el.Attribute("_CombinedKey").Value;
                if ((el.Attribute("AllocateBy") == null) |
                    (el.Attribute("AllocateBy").Value == "0"))
                    aDocument.AllocateBy = Document.AllocateMethods.Document;
                else
                    aDocument.AllocateBy = Document.AllocateMethods.Lines;

                aDocument.IsAddMode = true;
                XElement mode = el.XPathSelectElement("FreightInfo/Shipment/NameValue[@Name='Status']");
                if (mode != null)
                {
                    aDocument.IsAddMode = (mode.Attribute("Value").Value.ToUpper() == "ADD");
                    //logger.Log("mode = " + mode.Attribute("Value").Value + " aDocument.IsAddMode = " + aDocument.IsAddMode.ToString());
                }
                
                var items = new List<ShippableItem>();

                IEnumerable<XElement> ilist = el.XPathSelectElements("OrderItems/OrderItem");
                foreach (XElement oi in ilist)
                {
                    var anItem = new ShippableItem();

                    XElement nv = oi.XPathSelectElement("NameValue[@Name='Extra Key1']");
                    if (nv != null)
                        try
                        {
                            anItem.ItemSequence = Convert.ToInt32(nv.Attribute("Value").Value);
                        }
                        catch (System.Exception ex)
                        {
                            throw new System.Exception("Error converting Extra Key1 value " + nv.Attribute("Value").Value + ex.Message);
                        }
                    nv = oi.XPathSelectElement("NameValue[@Name='Extra Key2']");
                    if (nv != null)
                        try
                        {
                            anItem.ComponentSequence = Convert.ToInt32(nv.Attribute("Value").Value);
                        }
                        catch (System.Exception ex)
                        {
                            throw new System.Exception("Error converting Extra Key2 value " + nv.Attribute("Value").Value + ex.Message);
                        }
                    nv = oi.XPathSelectElement("NameValue[@Name='ItemNumber']");
                    if (nv != null)
                    {
                        anItem.ItemNumber = nv.Attribute("Value").Value;
                    }
                    nv = oi.XPathSelectElement("NameValue[@Name='DeltaShipQty']");
                    if (nv != null)
                        try
                        {
                            anItem.DeltaShip = Convert.ToDecimal(nv.Attribute("Value").Value);
                        }
                        catch (System.Exception ex)
                        {
                            throw new System.Exception("Error converting DeltaShipQty value " + nv.Attribute("Value").Value + ex.Message);
                        }
                    anItem.DistributionType = DistTypes.None;
                    nv = oi.XPathSelectElement("Distributions");
                    if (nv != null)
                        try
                        {
                            if (nv.Attribute("Type").Value == "1")
                                anItem.DistributionType = DistTypes.Serial;
                            else
                                anItem.DistributionType = DistTypes.Lot;                            
                        }
                        catch (System.Exception ex)
                        {
                            throw new System.Exception("Error converting Distribution Type value " + nv.Attribute("Value").Value + ex.Message);
                        }

                     var distributions = new List<Distribution>();
                     IEnumerable<XElement> dlist = oi.XPathSelectElements("Distributions/Distribution");
                     foreach (XElement di in dlist)
                     {
                         var aDist = new Distribution();
                         aDist.LotSerial = di.Attribute("ID").Value;
                         aDist.Quantity = Convert.ToDecimal(di.Attribute("Qty").Value);
                         distributions.Add(aDist);
                     }
                     anItem.Distributions = distributions.ToArray();
                     items.Add(anItem);

                }
                aDocument.Items = items.ToArray();
                docs.Add(aDocument);
            }
            this.Documents = docs.ToArray();

            // Get Settings
            try
            {
                IncludeNonInventoryItems = GetSettingValue("IncludeNonInventory", WriteXML);
                IncludeKitComponents = GetSettingValue("IncludeKitComponents", WriteXML);
                IncludeMiscCharges = GetSettingValue("IncludeMiscCharges", WriteXML);
                IncludeServices = GetSettingValue("IncludeServices", WriteXML);
                IncludeFlatFee = GetSettingValue("IncludeFlatFee", WriteXML);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("Error reading settings : " + ex.Message);   
            }

        }
        catch (System.Exception ex)
        {
            throw new System.Exception("Error parsing writebackxml : " + ex.Message);   
        }

    }

    public void LogMe(Logger logger)
    {
        try
        {
            for (int i = 0; i < this.Documents.Length; i++)
            {
                logger.Log("  Document: " + this.Documents[i].SopNumber +"," +this.Documents[i].TypeStr);
                logger.Log(string.Format("   AddMode: {0}",this.Documents[i].IsAddMode));

                for (int j = 0; j < this.Documents[i].Items.Length; j++)
                {
                    logger.Log("    Line Item: " + this.Documents[i].Items[j].ItemNumber+ " ItemSeq = "+ 
                         string.Format("{0:D}",this.Documents[i].Items[j].ItemSequence) +
                         " CompSeq = "+string.Format("{0:D}",this.Documents[i].Items[j].ComponentSequence)+
                         " DeltaShip = "+string.Format("{0:0.00}",this.Documents[i].Items[j].DeltaShip)+
                         " DistType = "+string.Format("{0:D}",this.Documents[i].Items[j].DistributionType));

                    for (int k = 0; k < this.Documents[i].Items[j].Distributions.Length; k++)
                    {
                        logger.Log("      Distribution:  LOT/Serial = " +
                             string.Format("{0:D}", this.Documents[i].Items[j].Distributions[k].LotSerial) +
                             " Qty = " + string.Format("{0:0.00}", this.Documents[i].Items[j].Distributions[k].Quantity));

                    }
                    
                }
            }

            logger.Log("Settings: ");
            logger.Log(string.Format("   IncludeNonInventoryItems = {0}",this.IncludeNonInventoryItems));
            logger.Log(string.Format("   IncludeKitComponents = {0}",this.IncludeKitComponents));
            logger.Log(string.Format("   IncludeMiscCharges = {0}",this.IncludeMiscCharges));
            logger.Log(string.Format("   IncludeServices = {0}",this.IncludeServices));
            logger.Log(string.Format("   IncludeFlatFee = {0}",this.IncludeFlatFee));
            logger.Log(string.Format("   BackOrder = {0}", this.DoBackOrder));
        }
        catch (System.Exception ex)
        {
            logger.Log("Error logging document list : " + ex.Message);
        }
    }

    public bool AnyNeedFulfillment()
    {
        bool Any = false;
        try
        {
            for (int i = 0; i < this.Documents.Length; i++)
            {
                if ((this.Documents[i].AllocateBy == Document.AllocateMethods.Lines) &

                    ((this.Documents[i].DocType == Document.SopType.SalesOrder) |
                     (this.Documents[i].DocType == Document.SopType.Invoice) |
                     (this.Documents[i].DocType == Document.SopType.FulfillmentOrder)))
                {
                    Any = true;
                }               
            } 
            return (Any);
        }
        
        catch (System.Exception ex)
        {
            throw new System.Exception("Error checking document list : " + ex.Message);
        }
    }

}

public class Document
{
    public enum SopType { Quote, SalesOrder, Invoice, SalesReturn, BackOrder, FulfillmentOrder };
    public enum AllocateMethods { Document, Lines };
    
    public string TypeStr { get; set; }
    public bool IsAddMode { get; set; }
    public SopType DocType
    {
        get
        {
            if (TypeStr == "Quote")
                return SopType.Quote;
            else if (TypeStr == "Order")
                return SopType.SalesOrder;
            else if (TypeStr == "Invoice")
                return SopType.Invoice;
            else if (TypeStr == "Return")
                return SopType.SalesReturn;
            else if (TypeStr == "Back Order")
                return SopType.BackOrder;
            else if (TypeStr == "Fulfillment Order")
                return SopType.FulfillmentOrder;
            else
                return SopType.SalesOrder;
        }
    }
    public string SopNumber { get; set; }
    string _combinedkey = "";
    public string CombinedKey
    {
        get { return this._combinedkey; }
        set
        {
            this._combinedkey = value;
            int i = value.IndexOf("|@|");
            SopNumber = value.Substring(0, i);
            string s = value.Substring(i + 3, value.Length - (i + 3));
            i = s.IndexOf("|");
            if (i > 0)
                s = s.Substring(0, i);
            TypeStr = s;
        }

    }
    public AllocateMethods AllocateBy { get; set; }
    public bool RequiredUpdate { get; set; }
    public ShippableItem[] Items { get; set; }

}

public class ShippableItem
{
    public string ItemNumber { get; set; }

    public int ItemSequence { get; set; }
    public int ComponentSequence { get; set; }
    public DistTypes DistributionType { get; set; }

    public decimal DeltaShip { get; set; }
    public Distribution[] Distributions { get; set; }

}

public class Distribution
{
    public string LotSerial { get; set; }

    public decimal Quantity { get; set; }

}
