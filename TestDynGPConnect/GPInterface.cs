using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Common;
using Microsoft.Dynamics.GP;
using System.ServiceModel;
//using System.Windows.Forms;


    class FulfillmentLine
    {
        public enum LineType { Inventory, NonInventory, Miscellaneous, ServiceCharge, FlatFee };
        public LineType linetype { get; set; }
        public bool IsKitComponent { get; set; }
        public decimal QtyFulfilled { get; set; }
        public decimal QtyOrdered { get; set; }
        public decimal QtyAllocated { get; set; }
        public bool IsAddMode { get; set; }
        public ShippableItem ShipItem { get; set; }
        public FulfillmentLine KitItem { get; set; }
        public decimal DeltaQty { get; set; }   // amount we will add (may be negative) to sales doc line fulfilled qty
        public string linetypestr 
        { 
            get
            {
                switch (linetype)
                {
                case LineType.NonInventory:
                    return("NonInventory");
                case LineType.Miscellaneous:
                    return("Miscellaneous");
                case LineType.ServiceCharge:
                    return("ServiceCharge");
                case LineType.FlatFee:
                    return("FlatFee");
                    default:
                    return ("Inventory");
                }
            }
        }
        public LineType GetLineType(SalesLine SLine, DynamicsGPClient wsDynamicsGP, Context context)
        {
            ItemKey itemKey;
            Item item;
            itemKey = new ItemKey();
            itemKey.Id = SLine.ItemKey.Id;
             
            if (SLine.IsNonInventory.HasValue && (bool)(SLine.IsNonInventory))
            {
                return(LineType.NonInventory);
            }
            else
            try
            {
                item = wsDynamicsGP.GetItemByKey(itemKey, context);
                switch (item.Type)
                {
                    case Microsoft.Dynamics.GP.ItemType.MiscellaneousCharges:
                        return(LineType.Miscellaneous);                        
                    case Microsoft.Dynamics.GP.ItemType.Service:
                        return(LineType.ServiceCharge);                        
                    case Microsoft.Dynamics.GP.ItemType.FlatFee:
                        return(LineType.FlatFee);                            
                    default:
                        return(LineType.Inventory);                  
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("Error gettiing inventory information for " + itemKey.Id + " : " + ex.Message);
            }
        }

       
        public LineType GetLineType(SalesComponent SLine, DynamicsGPClient wsDynamicsGP, Context context)
        {

            ItemKey itemKey;
            Item item;
            itemKey = new ItemKey();
            itemKey.Id = SLine.ItemKey.Id;

            if (SLine.IsNonInventory.HasValue && (bool)(SLine.IsNonInventory))
            {
                return (LineType.NonInventory);
            }
            else
                try
                {
                    
                    item = wsDynamicsGP.GetItemByKey(itemKey, context);
                    switch (item.Type)
                    {
                        case Microsoft.Dynamics.GP.ItemType.MiscellaneousCharges:
                            return (LineType.Miscellaneous);
                        case Microsoft.Dynamics.GP.ItemType.Service:
                            return (LineType.ServiceCharge);
                        case Microsoft.Dynamics.GP.ItemType.FlatFee:
                            return (LineType.FlatFee);
                        default:
                            return (LineType.Inventory);    // Our group Inventory includes "Inventory" and "Kit" items in GP
                    }
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception("Error gettiing inventory information for "+itemKey.Id+" : "+ ex.Message);
                }
                
        }
        // constructor 
        public FulfillmentLine(SalesLine SLine, Document document, DynamicsGPClient wsDynamicsGP, Context context)
        {
            linetype = GetLineType(SLine, wsDynamicsGP, context);
            IsKitComponent = false;
            KitItem = null;
            IsAddMode = document.IsAddMode;
            ShipItem = Array.Find(document.Items, f => (f.ItemSequence == SLine.Key.LineSequenceNumber));   
            
            QtyOrdered = SLine.Quantity.Value;
            if (SLine is SalesOrderLine)
            {
                QtyFulfilled = (SLine as SalesOrderLine).QuantityFulfilled.Value;
                QtyAllocated = (SLine as SalesOrderLine).QuantityAllocated.Value;
            }
            else if (SLine is SalesFulfillmentOrderLine)
            {
                QtyFulfilled = (SLine as SalesFulfillmentOrderLine).QuantityFulfilled.Value;
                QtyAllocated = (SLine as SalesFulfillmentOrderLine).QuantityAllocated.Value;
            }
            else if (SLine is SalesInvoiceLine)
            {
                QtyFulfilled = (SLine as SalesInvoiceLine).QuantityFulfilled.Value;
                QtyAllocated = (SLine as SalesInvoiceLine).QuantityAllocated.Value;
            }
        }

        // constructor for components
        public FulfillmentLine(SalesComponent SLine, int ComponentSequence, FulfillmentLine kititem, Document document, DynamicsGPClient wsDynamicsGP, Context context)
        {
            linetype = GetLineType(SLine, wsDynamicsGP, context);
            IsKitComponent = true;
            IsAddMode = document.IsAddMode;
            KitItem = kititem;
            ShipItem = Array.Find(document.Items, f => ((f.ItemSequence == KitItem.ShipItem.ItemSequence) &
                     (f.ComponentSequence == ComponentSequence)));
            
            QtyOrdered = SLine.Quantity.Value;
            if (SLine is SalesOrderComponent)
            {
                QtyFulfilled = (SLine as SalesOrderComponent).QuantityFulfilled.Value;
                QtyAllocated = (SLine as SalesOrderComponent).QuantityAllocated.Value;
            }
            else if (SLine is SalesFulfillmentOrderComponent)
            {
                QtyFulfilled = (SLine as SalesFulfillmentOrderComponent).QuantityFulfilled.Value;
                QtyAllocated = (SLine as SalesFulfillmentOrderComponent).QuantityAllocated.Value;
            }
            else if (SLine is SalesInvoiceComponent)
            {
                QtyFulfilled = (SLine as SalesInvoiceComponent).QuantityFulfilled.Value;
                QtyAllocated = (SLine as SalesInvoiceComponent).QuantityAllocated.Value;
            }
        }

        // Does user have settings in StarShip to exclude this type of line?
        public bool IsExcludedFromShipment(DocumentList DocList)
        {
            if (IsKitComponent && !DocList.IncludeKitComponents)
                return true;
            else
            {
                switch (this.linetype)
                {
                    case LineType.NonInventory:
                        return !DocList.IncludeNonInventoryItems;
                    case LineType.Miscellaneous:
                        return !DocList.IncludeMiscCharges;
                    case LineType.ServiceCharge:
                        return !DocList.IncludeServices;
                    case LineType.FlatFee:
                        return !DocList.IncludeFlatFee;
                    default:
                        return false;
                }
            }
        }

        public void CalculateDelta(DocumentList DocList)
        {
            decimal max = 0;

            // inventory item  - only fulfill up to allocated qty
            if (this.linetype == LineType.Inventory)
                max = this.QtyAllocated;
            // miscellaneous, flat fee, etc. do not have allocated qty, fullfill up to qty ordered
            else
                max = this.QtyOrdered;
            // sItem will be null if not found in WriteBack item list. 
            if (ShipItem != null)
            {
                // don't go above max
                if ((ShipItem.DeltaShip > 0) & ((this.QtyFulfilled + ShipItem.DeltaShip) > max))
                    DeltaQty = this.QtyFulfilled - max;
                // don't go below 0
                else if ((ShipItem.DeltaShip < 0) & ((this.QtyFulfilled + ShipItem.DeltaShip) < 0))
                    DeltaQty = -this.QtyFulfilled;
                else
                    DeltaQty = ShipItem.DeltaShip;
            }
            // not found in write-back list -- if this item is excluded from write-back list by settings, then
            // we want to set fullfed qty to max if adding, or 0 if deleting order.  
            // For kit components, we determine quantity by pro-rating based on kit/component ratio
            else if (IsExcludedFromShipment(DocList))
            {
                // Apply ratio of Component Order Qty/Kit Ordered Qty to the final DELTA we calculated for the kit
                if (IsKitComponent && (KitItem != null))  // KitItem is the master kit, should never be null
                {
                    if (KitItem == null)
                        throw new System.Exception("No Kit Item provided to component quantity calculation.");   
                    decimal QtyPerKit = this.QtyOrdered / KitItem.QtyOrdered;
                    DeltaQty = QtyPerKit * KitItem.DeltaQty;
                    if ((this.QtyFulfilled + DeltaQty) > max)
                        DeltaQty = this.QtyFulfilled - max;
                    else if ((this.QtyFulfilled + DeltaQty) < 0)
                        DeltaQty = -this.QtyFulfilled;
                }
                else if (IsAddMode)
                    DeltaQty = this.QtyFulfilled - max;
                else
                    DeltaQty = -this.QtyFulfilled;
            }
            // otherwise, don't change fulfilled qty - this item wasn't shipped.
            else
                DeltaQty = 0;
        }

        // Lots for Sales Lines
        public void DoLotDistribution(SalesLine SLine, Logger gplog)
        {

            var lots = new List<SalesLineLot>();
            if (SLine is SalesOrderLine)
            {
                lots = (SLine as SalesOrderLine).Lots.ToList<SalesLineLot>();                
            }
            else if (SLine is SalesFulfillmentOrderLine)
            {
                lots = (SLine as SalesFulfillmentOrderLine).Lots.ToList<SalesLineLot>();  
            }
            else if (SLine is SalesInvoiceLine)
            {
                lots = (SLine as SalesInvoiceLine).Lots.ToList<SalesLineLot>();  
            }
            int seq = 0;
            if (lots.Count > 0)
            {
                seq = lots[lots.Count - 1].Key.SequenceNumber;
                gplog.Log(string.Format("  Existing Lots : {0}, Seq = {1} ", lots.Count.ToString(), seq.ToString()));
            }
            if (DeltaQty > 0)
            {
                for (int i = 0; i < ShipItem.Distributions.Length; i++)
                {
                    SalesLineLot lot = new SalesLineLot();
                    lot.LotNumber = ShipItem.Distributions[i].LotSerial;
                    lot.Quantity = new Quantity { Value = ShipItem.Distributions[i].Quantity };
                    gplog.Log(string.Format("  Lot : {0}, Qty = {1} ",
                                  ShipItem.Distributions[i].LotSerial,
                                  ShipItem.Distributions[i].Quantity.ToString()));
                    lot.Bin = null;
                    lot.Key = new SalesLineLotKey();
                    lot.Key.QuantityType = QuantityType.OnHand;
                    lot.Key.SequenceNumber = seq++;
                    //gplog.Log(string.Format("  lot.key.SequenceNumber : {0} ", seq.ToString()));                                  
                    lot.Key.SalesLineKey = new SalesLineKey();
                    lot.Key.SalesLineKey.LineSequenceNumber = SLine.Key.LineSequenceNumber;
                    lot.Key.SalesLineKey.SalesDocumentKey = new SalesDocumentKey();
                    lot.Key.SalesLineKey.SalesDocumentKey.Id = SLine.Key.SalesDocumentKey.Id;
                    lots.Add(lot);
                }
                if (SLine is SalesOrderLine)
                {
                    (SLine as SalesOrderLine).Lots = lots.ToArray();                                        
                }
                else if (SLine is SalesFulfillmentOrderLine)
                {
                    (SLine as SalesFulfillmentOrderLine).Lots = lots.ToArray();
                }
                else if (SLine is SalesInvoiceLine)
                {
                    (SLine as SalesInvoiceLine).Lots = lots.ToArray();
                }                
            }
            else
            {
                gplog.Log("  Remove lots by setting DeleteOnUpdate property");

                if (SLine is SalesOrderLine)
                {
                    for (int i = 0; i < (SLine as SalesOrderLine).Lots.Length; i++)
                       (SLine as SalesOrderLine).Lots[i].DeleteOnUpdate = true;
                }
                else if (SLine is SalesFulfillmentOrderLine)
                {
                    for (int i = 0; i < (SLine as SalesFulfillmentOrderLine).Lots.Length; i++)
                        (SLine as SalesFulfillmentOrderLine).Lots[i].DeleteOnUpdate = true;
                }
                else if (SLine is SalesInvoiceLine)
                {
                    for (int i = 0; i < (SLine as SalesInvoiceLine).Lots.Length; i++)
                        (SLine as SalesInvoiceLine).Lots[i].DeleteOnUpdate = true;
                }
            }
            
        }

        // Lots for Component Lines
        public void DoLotDistribution(SalesComponent SLine, Logger gplog)
        {
            var lots = new List<SalesComponentLot>();

            if (SLine is SalesOrderComponent)
            {
                lots = (SLine as SalesOrderComponent).Lots.ToList<SalesComponentLot>();
            }
            else if (SLine is SalesFulfillmentOrderComponent)
            {
                lots = (SLine as SalesFulfillmentOrderComponent).Lots.ToList<SalesComponentLot>();
            }
            else if (SLine is SalesInvoiceComponent)
            {
                lots = (SLine as SalesInvoiceComponent).Lots.ToList<SalesComponentLot>();
            }                

            int seq = 0;
            if (lots.Count > 0)
            {
                seq = lots[lots.Count - 1].Key.SequenceNumber;
                gplog.Log(string.Format("  Existing Lots : {0}, Seq = {1} ",lots.Count.ToString(),seq.ToString()));                                  
            }
            if (DeltaQty > 0)
            {
                for (int i = 0; i < ShipItem.Distributions.Length; i++)
                {
                    SalesComponentLot lot = new SalesComponentLot();
                    lot.LotNumber = ShipItem.Distributions[i].LotSerial;
                    lot.Quantity = new Quantity { Value = ShipItem.Distributions[i].Quantity };
                    gplog.Log(string.Format("  Lot : {0}, Qty = {1} ",
                                  ShipItem.Distributions[i].LotSerial,
                                  ShipItem.Distributions[i].Quantity.ToString()));
                    lot.Bin = null;
                    lot.Key = new SalesComponentLotKey();
                    lot.Key.QuantityType = QuantityType.OnHand;
                    lot.Key.SequenceNumber = seq++;
                    //gplog.Log(string.Format("  lot.key.SequenceNumber : {0} ", seq.ToString()));                                  
                    lot.Key.SalesComponentKey = new SalesComponentKey();
                    lot.Key.SalesComponentKey.ComponentSequenceNumber = SLine.Key.ComponentSequenceNumber;
                    lot.Key.SalesComponentKey.SalesLineKey = new SalesLineKey();
                    lot.Key.SalesComponentKey.SalesLineKey.LineSequenceNumber = SLine.Key.SalesLineKey.LineSequenceNumber;
                    lot.Key.SalesComponentKey.SalesLineKey.SalesDocumentKey = new SalesDocumentKey();
                    lot.Key.SalesComponentKey.SalesLineKey.SalesDocumentKey.Id = SLine.Key.SalesLineKey.SalesDocumentKey.Id;                   
                    lots.Add(lot);
                }
                if (SLine is SalesOrderComponent)
                {
                    (SLine as SalesOrderComponent).Lots = lots.ToArray();
                }
                else if (SLine is SalesFulfillmentOrderComponent)
                {
                    (SLine as SalesFulfillmentOrderComponent).Lots = lots.ToArray();
                }
                else if (SLine is SalesInvoiceComponent)
                {
                    (SLine as SalesInvoiceComponent).Lots = lots.ToArray();
                }
            }
            else
            {
                gplog.Log("  Remove lots by setting DeleteOnUpdate property");

                if (SLine is SalesOrderComponent)
                {
                    for (int i = 0; i < (SLine as SalesOrderComponent).Lots.Length; i++)
                        (SLine as SalesOrderComponent).Lots[i].DeleteOnUpdate = true;
                }
                else if (SLine is SalesFulfillmentOrderComponent)
                {
                    for (int i = 0; i < (SLine as SalesFulfillmentOrderComponent).Lots.Length; i++)
                        (SLine as SalesFulfillmentOrderComponent).Lots[i].DeleteOnUpdate = true;
                }
                else if (SLine is SalesInvoiceComponent)
                {
                    for (int i = 0; i < (SLine as SalesInvoiceComponent).Lots.Length; i++)
                        (SLine as SalesInvoiceComponent).Lots[i].DeleteOnUpdate = true;
                }
            }

        }

        // Serials for Sales Lines
        public void DoSerialDistribution(SalesLine SLine, Logger gplog)
        {
            var serials = new List<SalesLineSerial>();

            if (SLine is SalesOrderLine)
            {
                serials = (SLine as SalesOrderLine).Serials.ToList<SalesLineSerial>();
            }
            else if (SLine is SalesFulfillmentOrderLine)
            {
                serials = (SLine as SalesFulfillmentOrderLine).Serials.ToList<SalesLineSerial>();
            }
            else if (SLine is SalesInvoiceLine)
            {
                serials = (SLine as SalesInvoiceLine).Serials.ToList<SalesLineSerial>();
            }
            int seq = 0;
            if (serials.Count > 0)
            {
                seq = serials[serials.Count - 1].Key.SequenceNumber;
                gplog.Log(string.Format("  Existing Serials : {0}, Seq = {1} ", serials.Count.ToString(), seq.ToString()));
            }
            if (DeltaQty > 0)
            {
                for (int i = 0; i < ShipItem.Distributions.Length; i++)
                {
                    SalesLineSerial serial = new SalesLineSerial();
                    serial.SerialNumber = ShipItem.Distributions[i].LotSerial;
                    gplog.Log("  Serial : "+ShipItem.Distributions[i].LotSerial);                                
                    serial.Bin = null;
                    serial.Key = new SalesLineSerialKey();
                    serial.Key.QuantityType = QuantityType.OnHand;
                    serial.Key.SequenceNumber = seq++;
                    serial.Key.SalesLineKey = new SalesLineKey();
                    serial.Key.SalesLineKey.LineSequenceNumber = SLine.Key.LineSequenceNumber;
                    serial.Key.SalesLineKey.SalesDocumentKey = new SalesDocumentKey();
                    serial.Key.SalesLineKey.SalesDocumentKey.Id = SLine.Key.SalesDocumentKey.Id;
                    serials.Add(serial);
                }
                if (SLine is SalesOrderLine)
                {
                    (SLine as SalesOrderLine).Serials = serials.ToArray();
                }
                else if (SLine is SalesFulfillmentOrderLine)
                {
                    (SLine as SalesFulfillmentOrderLine).Serials = serials.ToArray();
                }
                else if (SLine is SalesInvoiceLine)
                {
                    (SLine as SalesInvoiceLine).Serials = serials.ToArray();
                }
            }
            else
            {
                gplog.Log("  Remove serials by setting DeleteOnUpdate property");

                if (SLine is SalesOrderLine)
                {
                    for (int i = 0; i < (SLine as SalesOrderLine).Serials.Length; i++)
                        (SLine as SalesOrderLine).Serials[i].DeleteOnUpdate = true;
                }
                else if (SLine is SalesFulfillmentOrderLine)
                {
                    for (int i = 0; i < (SLine as SalesFulfillmentOrderLine).Serials.Length; i++)
                        (SLine as SalesFulfillmentOrderLine).Serials[i].DeleteOnUpdate = true;
                }
                else if (SLine is SalesInvoiceLine)
                {
                    for (int i = 0; i < (SLine as SalesInvoiceLine).Serials.Length; i++)
                        (SLine as SalesInvoiceLine).Serials[i].DeleteOnUpdate = true;
                }
            }

        }

        // Serials for Component Lines
        public void DoSerialDistribution(SalesComponent SLine, Logger gplog)
        {
            var serials = new List<SalesComponentSerial>();

            if (SLine is SalesOrderComponent)
            {
                serials = (SLine as SalesOrderComponent).Serials.ToList<SalesComponentSerial>();
            }
            else if (SLine is SalesFulfillmentOrderComponent)
            {
                serials = (SLine as SalesFulfillmentOrderComponent).Serials.ToList<SalesComponentSerial>();
            }
            else if (SLine is SalesInvoiceComponent)
            {
                serials = (SLine as SalesInvoiceComponent).Serials.ToList<SalesComponentSerial>();
            }
            int seq = 0;
            if (serials.Count > 0)
            {
                seq = serials[serials.Count - 1].Key.SequenceNumber;
                gplog.Log(string.Format("  Existing Serials : {0}, Seq = {1} ", serials.Count.ToString(), seq.ToString()));
            }
            if (DeltaQty > 0)
            {
                for (int i = 0; i < ShipItem.Distributions.Length; i++)
                {
                    SalesComponentSerial serial = new SalesComponentSerial();
                    serial.SerialNumber = ShipItem.Distributions[i].LotSerial;
                    gplog.Log("  Serial : " + ShipItem.Distributions[i].LotSerial);
                    serial.Bin = null;
                    serial.Key = new SalesComponentSerialKey();
                    serial.Key.QuantityType = QuantityType.OnHand;
                    serial.Key.SequenceNumber = seq++;
                    serial.Key.SalesComponentKey = new SalesComponentKey();                    
                    serial.Key.SalesComponentKey.ComponentSequenceNumber = SLine.Key.ComponentSequenceNumber;
                    serial.Key.SalesComponentKey.SalesLineKey = new SalesLineKey();
                    serial.Key.SalesComponentKey.SalesLineKey.LineSequenceNumber = SLine.Key.SalesLineKey.LineSequenceNumber;
                    serial.Key.SalesComponentKey.SalesLineKey.SalesDocumentKey = new SalesDocumentKey();
                    serial.Key.SalesComponentKey.SalesLineKey.SalesDocumentKey.Id = SLine.Key.SalesLineKey.SalesDocumentKey.Id;
                    serials.Add(serial);
                }
                if (SLine is SalesOrderComponent)
                {
                    (SLine as SalesOrderComponent).Serials = serials.ToArray();
                }
                else if (SLine is SalesFulfillmentOrderComponent)
                {
                    (SLine as SalesFulfillmentOrderComponent).Serials = serials.ToArray();
                }
                else if (SLine is SalesInvoiceComponent)
                {
                    (SLine as SalesInvoiceComponent).Serials = serials.ToArray();
                }
            }
            else
            {
                gplog.Log("  Remove serials by setting DeleteOnUpdate property");

                if (SLine is SalesOrderComponent)
                {
                    for (int i = 0; i < (SLine as SalesOrderComponent).Serials.Length; i++)
                        (SLine as SalesOrderComponent).Serials[i].DeleteOnUpdate = true;
                }
                else if (SLine is SalesFulfillmentOrderComponent)
                {
                    for (int i = 0; i < (SLine as SalesFulfillmentOrderComponent).Serials.Length; i++)
                        (SLine as SalesFulfillmentOrderComponent).Serials[i].DeleteOnUpdate = true;
                }
                else if (SLine is SalesInvoiceComponent)
                {
                    for (int i = 0; i < (SLine as SalesInvoiceComponent).Serials.Length; i++)
                        (SLine as SalesInvoiceComponent).Serials[i].DeleteOnUpdate = true;
                }
            }

        }

        public void DoDistribution(SalesLine SLine, Logger gplog)
        {
            gplog.Log("Serial/LOT Distribution - for item " + ShipItem.ItemNumber);
            if (ShipItem.DistributionType == DistTypes.Lot)
                DoLotDistribution(SLine, gplog);
            else
                DoSerialDistribution(SLine, gplog);
        }

        public void DoDistribution(SalesComponent SLine, Logger gplog)
        {
            gplog.Log("Serial/LOT Distribution - for component " + ShipItem.ItemNumber);
            if (ShipItem.DistributionType == DistTypes.Lot)
                DoLotDistribution(SLine, gplog);
            else
                DoSerialDistribution(SLine, gplog);
        }
    }

    class GPInterface
    {
        private Logger gplog { get; set; }

        public static Company[] CompanyList;
        private static DynamicsGPClient wsDynamicsGP;

        public GPInterface(Logger aLogger, DynamicsGPClient aDynGP)
        {
            gplog = aLogger;
            wsDynamicsGP = aDynGP;
        }

       

        public void UpdateDocuments(DocumentList DocList, String CompanyName)
        {
            CompanyKey companyKey;
            Company company;
            Microsoft.Dynamics.Common.Context context;
            Microsoft.Dynamics.GP.SalesDocumentKey SalesKey;
            FulfillmentLine FLine;
            FulfillmentLine CLine;
            string pmtinfo = "";      
            
            gplog.Log("UpdateDocuments : START");

            try
            {                
                try
                {
                    if (CompanyList == null)
                    {
                        gplog.Log("Before load company list");
                        LoadCompanyList(wsDynamicsGP);
                        gplog.Log("After load company list");
                    }
                    // Create a context with which to call the web service
                    context = new Context();
                    // Specify which company to use (lesson company)
                    companyKey = new CompanyKey();
                    company = Array.Find(CompanyList, f => f.Name == CompanyName);
                    if (company == null)
                        throw new System.Exception("Company not found : "+CompanyName);   
                    companyKey.Id = company.Key.Id;
                    // Set up the context
                    context.OrganizationKey = (OrganizationKey)companyKey;


                    for (int i = 0; i < DocList.Documents.Length; i++)
                    {
                        SalesKey = new Microsoft.Dynamics.GP.SalesDocumentKey();

                        SalesKey.Id = DocList.Documents[i].SopNumber;

                        // Set up a policy object
                        Policy salesOrderPolicy = wsDynamicsGP.GetPolicyByOperation(GetUpdateOperation(DocList.Documents[i].DocType), context);

                        string s = DocList.Documents[i].AllocateBy.ToString();
                        gplog.Log(string.Format("Document : {0},{1}, AllocateBy = {2} ",
                                  DocList.Documents[i].SopNumber,DocList.Documents[i].TypeStr,
                                  DocList.Documents[i].AllocateBy.ToString()));

                        DocList.Documents[i].RequiredUpdate = false;
                            
                        if (DocList.Documents[i].AllocateBy != Document.AllocateMethods.Lines)
                        {
                            continue;   // nothing to do
                        }                        

                        switch (DocList.Documents[i].DocType)
                        {
                            case Document.SopType.SalesOrder:
                            {
                                
                                var salesdoc = wsDynamicsGP.GetSalesOrderByKey(SalesKey, context);
                                
                                //gplog.Log("Got salesdoc");
                                //UpdateLines(salesdoc);
                           
                                for (int j = 0; j < salesdoc.Lines.Length; j++)  
                                {                                    
                                    FLine = new FulfillmentLine(salesdoc.Lines[j], DocList.Documents[i], wsDynamicsGP, context);
                                    FLine.CalculateDelta(DocList);
                                    gplog.Log(string.Format("  Item : {0:D} {1} {2} Fulfillment delta = {3:G} ",salesdoc.Lines[j].Key.LineSequenceNumber,salesdoc.Lines[j].ItemKey.Id,FLine.linetypestr,FLine.DeltaQty));

                                    if (Math.Abs(FLine.DeltaQty) != 0)
                                    {
                                        salesdoc.Lines[j].QuantityFulfilled.Value = salesdoc.Lines[j].QuantityFulfilled.Value + FLine.DeltaQty;
                                        if (FLine.ShipItem.DistributionType != DistTypes.None)
                                        {                                            
                                            FLine.DoDistribution(salesdoc.Lines[j], gplog);
                                        }
                                        DocList.Documents[i].RequiredUpdate = true;
                                        if (DocList.DoBackOrder)
                                        {
                                            if (FLine.DeltaQty > 0)
                                            {
                                                salesdoc.Lines[j].QuantityToInvoice.Value = salesdoc.Lines[j].QuantityFulfilled.Value;
                                                salesdoc.Lines[j].QuantityToBackorder.Value = salesdoc.Lines[j].Quantity.Value - salesdoc.Lines[j].QuantityFulfilled.Value - salesdoc.Lines[j].QuantityCanceled.Value;                                               
                                            }
                                            else  //delete
                                            {
                                                salesdoc.Lines[j].QuantityToInvoice.Value = salesdoc.Lines[j].Quantity.Value - salesdoc.Lines[j].QuantityFulfilled.Value - salesdoc.Lines[j].QuantityCanceled.Value;
                                                salesdoc.Lines[j].QuantityToBackorder.Value = 0;
                                            }
                                            
                                            gplog.Log(string.Format("       QTY Backordered = {0:G} ", salesdoc.Lines[j].QuantityToBackorder.Value));
                                        }
                                    }
                                    else if (DocList.DoBackOrder)
                                    {
                                        if ((DocList.Documents[i].IsAddMode) && (salesdoc.Lines[j].QuantityToBackorder.Value != (salesdoc.Lines[j].Quantity.Value - salesdoc.Lines[j].QuantityFulfilled.Value - salesdoc.Lines[j].QuantityCanceled.Value)))
                                        {
                                            salesdoc.Lines[j].QuantityToInvoice.Value = salesdoc.Lines[j].QuantityFulfilled.Value;
                                            salesdoc.Lines[j].QuantityToBackorder.Value = salesdoc.Lines[j].Quantity.Value - salesdoc.Lines[j].QuantityFulfilled.Value - salesdoc.Lines[j].QuantityCanceled.Value;                                            
                                            DocList.Documents[i].RequiredUpdate = true;
                                            gplog.Log(string.Format("       QTY Backordered = {0:G} ", salesdoc.Lines[j].QuantityToBackorder.Value));
                                        }
                                        else if ((!DocList.Documents[i].IsAddMode) && ((salesdoc.Lines[j].QuantityToBackorder.Value > 0))) //would have backordered everything on add, now zero out
                                        {
                                            salesdoc.Lines[j].QuantityToBackorder.Value = 0;
                                            salesdoc.Lines[j].QuantityToInvoice.Value = salesdoc.Lines[j].Quantity.Value - salesdoc.Lines[j].QuantityFulfilled.Value - salesdoc.Lines[j].QuantityCanceled.Value;
                                            DocList.Documents[i].RequiredUpdate = true;
                                            gplog.Log(string.Format("       QTY Backordered = {0:G} ", salesdoc.Lines[j].QuantityToBackorder.Value));
                                        }                                        
                                    }
                                    // can't skip if FLine.DeltaQty = 0 because if bringing over kit components, user
                                    // sees kit and components and could possibly not ship kit, but still ship components                                    
                                    if (salesdoc.Lines[j].Components.Length > 0)
                                    {
                                        for (int k = 0; k < salesdoc.Lines[j].Components.Length; k++)
                                        {
                                            CLine = new FulfillmentLine(salesdoc.Lines[j].Components[k], salesdoc.Lines[j].Components[k].Key.ComponentSequenceNumber, FLine, DocList.Documents[i], wsDynamicsGP, context);
                                            CLine.CalculateDelta(DocList);
                                            gplog.Log(string.Format("    > Components : {0:D} {1} {2} Fulfillment delta = {3:G} ", salesdoc.Lines[j].Components[k].Key.ComponentSequenceNumber, salesdoc.Lines[j].Components[k].ItemKey.Id, CLine.linetypestr, CLine.DeltaQty));
                                            if (Math.Abs(CLine.DeltaQty) != 0)
                                            {
                                                salesdoc.Lines[j].Components[k].QuantityFulfilled.Value = salesdoc.Lines[j].Components[k].QuantityFulfilled.Value + CLine.DeltaQty;
                                                if (CLine.ShipItem.DistributionType != DistTypes.None)
                                                {
                                                    CLine.DoDistribution(salesdoc.Lines[j].Components[k], gplog);
                                                }
                                                DocList.Documents[i].RequiredUpdate = true;
                                                if (DocList.DoBackOrder)
                                                {                                                    
                                                    if (FLine.DeltaQty > 0)
                                                    {
                                                        salesdoc.Lines[j].Components[k].QuantityToInvoice.Value = salesdoc.Lines[j].Components[k].QuantityFulfilled.Value;
                                                        salesdoc.Lines[j].Components[k].QuantityToBackorder.Value = salesdoc.Lines[j].Components[k].Quantity.Value - salesdoc.Lines[j].Components[k].QuantityFulfilled.Value - salesdoc.Lines[j].Components[k].QuantityCanceled.Value;
                                                    }
                                                    else
                                                    {
                                                        salesdoc.Lines[j].Components[k].QuantityToInvoice.Value = salesdoc.Lines[j].Components[k].Quantity.Value - salesdoc.Lines[j].Components[k].QuantityFulfilled.Value - salesdoc.Lines[j].Components[k].QuantityCanceled.Value;
                                                        salesdoc.Lines[j].Components[k].QuantityToBackorder.Value = 0;
                                                    }
                                                    gplog.Log(string.Format("       QTY Backordered = {0:G} ", salesdoc.Lines[j].Components[k].QuantityToBackorder.Value));
                                                }
                                            }
                                            else if (DocList.DoBackOrder)
                                            {
                                                if ((DocList.Documents[i].IsAddMode) &&  (salesdoc.Lines[j].Components[k].QuantityToBackorder.Value != (salesdoc.Lines[j].Components[k].Quantity.Value - salesdoc.Lines[j].Components[k].QuantityFulfilled.Value - salesdoc.Lines[j].Components[k].QuantityCanceled.Value)))
                                                {
                                                    salesdoc.Lines[j].Components[k].QuantityToInvoice.Value = salesdoc.Lines[j].Components[k].QuantityFulfilled.Value;
                                                    salesdoc.Lines[j].Components[k].QuantityToBackorder.Value = salesdoc.Lines[j].Components[k].Quantity.Value - salesdoc.Lines[j].Components[k].QuantityFulfilled.Value - salesdoc.Lines[j].Components[k].QuantityCanceled.Value;
                                                    gplog.Log(string.Format("       QTY Backordered = {0:G} ", salesdoc.Lines[j].Components[k].QuantityToBackorder.Value));
                                                    DocList.Documents[i].RequiredUpdate = true;
                                                }
                                                else if ((!DocList.Documents[i].IsAddMode) &&  (salesdoc.Lines[j].Components[k].QuantityToBackorder.Value > 0))
                                                {
                                                    salesdoc.Lines[j].Components[k].QuantityToInvoice.Value = salesdoc.Lines[j].Components[k].Quantity.Value - salesdoc.Lines[j].Components[k].QuantityFulfilled.Value - salesdoc.Lines[j].Components[k].QuantityCanceled.Value;
                                                    salesdoc.Lines[j].Components[k].QuantityToBackorder.Value = 0;
                                                    gplog.Log(string.Format("       QTY Backordered = {0:G} ", salesdoc.Lines[j].Components[k].QuantityToBackorder.Value));
                                                    DocList.Documents[i].RequiredUpdate = true;
                                                }
                                            }
                                        }
                                    }
                                }

                                // defect 17797
                                if (salesdoc.ProcessHolds != null)
                                    salesdoc.ProcessHolds = null;
                                // if we set payments to null, will not delete and re-create
                                // but leave existing entries.  Customer wants this fix (17797).                                
                                Decimal TotPmts = 0;
                                if (salesdoc.Payments != null)
                                {
                                    for (int p = 0; p < salesdoc.Payments.Length; p++)
                                    {
                                        TotPmts = TotPmts + salesdoc.Payments[p].PaymentAmount.Value;
                                    }
                                    pmtinfo = "Total Payments = " + TotPmts.ToString();
                                    salesdoc.Payments = null;
                                }
                                if (salesdoc.PaymentAmount != null)
                                {
                                    pmtinfo = pmtinfo + " PaymentAmount = " + salesdoc.PaymentAmount.Value.ToString();
                                    salesdoc.PaymentAmount = null;
                                }
                                if (salesdoc.DepositAmount != null)
                                {
                                    pmtinfo = pmtinfo + " DepositAmount = " + salesdoc.DepositAmount.Value.ToString();
                                    salesdoc.DepositAmount = null;
                                } 
                                // Update the sales order object
                                if (DocList.Documents[i].RequiredUpdate)
                                {
                                    wsDynamicsGP.UpdateSalesOrder(salesdoc, context, salesOrderPolicy);
                                }

                                break;
                            }
                            case Document.SopType.Invoice:
                            {
                                throw new System.Exception("Sales Transactions of type Invoice not supported");
                                
                            }
                            case Document.SopType.FulfillmentOrder:
                            {
                                throw new System.Exception("Sales Transactions of type Fulfillment Order not supported");
                                
                                
                            }
                                        
                        }                        
                    }
                }
                finally
                {
                                }
                gplog.Log("Payment Info : " + pmtinfo);

                gplog.Log("UpdateDocuments : END");
            }
            catch (System.Exception ex)
            {
                gplog.Log("Payment Info : " + pmtinfo);

                throw new System.Exception("Error Updating Fulfilled Quantities : " + ex.Message);
            }
        }

        protected string GetUpdateOperation(Document.SopType DocType)
        {
            switch (DocType)
            {
                case Document.SopType.Quote:
                    return "";
                case Document.SopType.SalesOrder:
                    return "UpdateSalesOrder";
                case Document.SopType.Invoice:
                    return "UpdateSalesInvoice";                   
                case Document.SopType.SalesReturn:
                    return "";                    
                case Document.SopType.BackOrder:
                    return "";                   case Document.SopType.FulfillmentOrder:
                    return "UpdateSalesFulfillmentOrder";                    
                default:
                    return "";                   
            }
        }

        /* public void Disconnect()
        {
            if (wsDynamicsGP == null)
            {
                gplog.Log("Disconnecting : proxy was not instantiated");   
            }
            else
            try
            {
                // Close the service
                if (wsDynamicsGP.State != CommunicationState.Faulted)
                {
                    wsDynamicsGP.Close();
                    gplog.Log("Disconnecting : closed successfully");   
                }
                else
                    gplog.Log("Disconnecting : was faulted");                   

            }
            catch (System.Exception ex)
            {
                gplog.Log("Error Disconnecting : "+ex.Message);   
            }

        } */
         
        protected void LoadCompanyList(DynamicsGPClient wsDynamicsGP)
        {
            Context context;
            BetweenRestrictionOfNullableOfint companyRestriction;
            CompanyCriteria companyCriteria;
            
            try
            {
                
                    // Create a context object
                    context = new Context();

                    // Set up the context object
                    // To retrieve from the system database set the organization key to null
                    context.OrganizationKey = null;

                    // Create a restriction object to query by company ID
                    // Query for all possible company ID values
                    companyRestriction = new BetweenRestrictionOfNullableOfint();
                    companyRestriction.From = -32768;
                    companyRestriction.To = 32767;

                    // Create a company criteria object and add the restriction object 
                    companyCriteria = new CompanyCriteria();
                    companyCriteria.Id = companyRestriction;

                    // Retrieve the list of companies
                    CompanyList = wsDynamicsGP.GetCompanyList(companyCriteria, context);

                    
               
            }

            catch (System.Exception ex)
            {
                throw new System.Exception("Error loading company list : "+ex.Message);   
            }

        }
    }

