Installation of SofortXml Gateway Provider for NB_Store
----------------------------------------------------

Step1 - Install Gateway Provider Install package as a normal module in DNN.

Step2 - Update the "gatewayproviders.xml" in the BO>Utilities>Settings>Payment>Gateways.

	Add xml Reference Node of:
		
    <gateway ref="Sofort">
      <name>Sofort</name>
      <assembly>NBrightStore.GatewaySofortXml</assembly>
      <class>Nevoweb.NBrightStore.Gateway.SofortXml</class>
    </gateway>
		
Example:-

<?xml version="1.0" encoding="utf-8" ?>
<root>
  <gateways>  
    <gateway ref="Sofort">
      <name>Sofort</name>
      <assembly>NBrightStore.GatewaySofortXml</assembly>
      <class>Nevoweb.NBrightStore.Gateway.SofortXml</class>
    </gateway>
    <gateway ref="Paypal">
      <name>Paypal</name>
      <assembly>NEvoweb.DNN.Modules.NB_Store.GatewayPayPal</assembly>
      <class>NEvoWeb.Modules.NB_Store.Gateway.GatewayPayPal</class>
    </gateway>
  </gateways>
</root>


Step 3 - Create a payment gateway setting call "sofortxml.gateway" and enter the settings needed.  
			For a full explaination of the Sofort settings refer to the Sofort documentation.
	
Example:-

<?xml version="1.0" encoding="UTF-8"?>
<multipay>
      <nbs>
        <customernumber>00000</customernumber>
        <apikey>XXXXXXXXXXXXXXXXXXXXXXXXXXXX</apikey>
        <buttonimageurl>/DesktopModules/NB_Store_GatewaySofortXml/SOFORT-Banking_product.png</buttonimageurl>
        <soforturi>https://api.sofort.com/api/xml</soforturi>
      </nbs>
      <project_id>000000</project_id>
      <language_code>[LANG]</language_code>
      <interface_version></interface_version>
      <amount>[TOTALAMT]</amount>
      <currency_code>EUR</currency_code>
      <reasons>
            <reason></reason>
      </reasons>
      <user_variables>
            <orderid>[ORDERID]</orderid>
      </user_variables>
      <success_url>http://test1.nevoweboffice.com/tabid/XX/stg/5/ordID/[ORDERID]/SofortExit/RETURN/Checkout.aspx</success_url>
      <abort_url>http://test1.nevoweboffice.com/tabid/XX/stg/5/ordID/[ORDERID]/SofortExit/CANCEL/Checkout.aspx</abort_url>
      <notification_urls>
        <notification_url><![CDATA[]]></notification_url>
      </notification_urls>
      <su>
            <customer_protection>1</customer_protection>
      </su>
</multipay>

NOTE: The tabid specified in the return links must be your store checkout tab.

--------------------------------------------------------------------		 
		 
Step 5 - Select the Payment Provider in the Checkout module settings.

Step 6 - In the DNN menu "Host>Host Settings", click on "Restart Application".  This will clear the cache and ensure the new provider is used.


NOTE: A setting of "sofort.html" can be created in the BO>Utilities>templates.

Example:
--------------------------------------------------------------------------
<input type="image" src="/Portals/0/sofortGateway.gif" name="sofortGateway" />
<div id="dropbox" style="display:none;">
<input name="txtIBAN" type="text" value="" maxlength="15" id="txtIBAN" style="width:100px;" />
On the next screen you will be asked for the first 3 digits of you bank account number. <br/>
You can also give digits 3, 4 and 5 of your IBAN number.
</div>
--------------------------------------------------------------------------






