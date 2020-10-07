Installation of Sofort Gateway Provider for NB_Store
----------------------------------------------------

Step1 - Install Gateway Provider Install package as a normal module in DNN.

Step2 - Update the "gatewayproviders.xml" in the BO>Utilities>Settings>Payment>Gateways.

	Add xml Reference Node of:
		
    <gateway ref="Sofort">
      <name>Sofort</name>
      <assembly>NEvoweb.DNN.Modules.NB_Store.GatewaySofort</assembly>
      <class>NEvoWeb.Modules.NB_Store.Gateway.GatewaySofort</class>
    </gateway>
		
Example:-

<?xml version="1.0" encoding="utf-8" ?>
<root>
  <gateways>  
    <gateway ref="Sofort">
      <name>Sofort</name>
      <assembly>NEvoweb.DNN.Modules.NB_Store.GatewaySofort</assembly>
      <class>NEvoWeb.Modules.NB_Store.Gateway.GatewaySofort</class>
    </gateway>
    <gateway ref="Paypal">
      <name>Paypal</name>
      <assembly>NEvoweb.DNN.Modules.NB_Store.GatewayPayPal</assembly>
      <class>NEvoWeb.Modules.NB_Store.Gateway.GatewayPayPal</class>
    </gateway>
  </gateways>
</root>


Step 3 - Create a payment gateway setting call "sofort.gateway" and enter the settings needed.  
			For a full explaination of the Sofort settings refer to the Sofort documentation.
	
Example:-

<root>
    <ButtonImageURL>/Portals/0/sofortGateway.gif</ButtonImageURL>
    <paymentURL>https://www.directebanking.com/payment/start</paymentURL>
    <user_id>23243</user_id>
    <project_id>343453</project_id>
    <notification_password>xxxxxxxxxxxxxxx</notification_password>
    <project_password>xxxxxxxxxxxxxx</project_password>
    <currency_id>EUR</currency_id>
    <reason_1>Test Reason</reason_1>
</root>

NOTE: Although the "notification_password" has been included, this is not used by the gateway interface.
NOTE: You can find the passwords on the sofort website > your project > Extended Settings > Passwords and hash algorithm

Step 4 - Setup Sofort Setting.
		 
--------------------------------------------------------------------		 
Log into your Sofort project and under "Project Base settings".

Interface Section: 

Success link:
http://www.MyWebsite.com/tabid/63/stg/5/Default.aspx?status=OK&ordID=-USER_VARIABLE_1-

Automatic redirection: True

Abort link:
http://www.MyWebsite.com/tabid/63/stg/5/Default.aspx?status=FAIL&ordID=-USER_VARIABLE_1-


Under Extended Settings>Passwords and hash algorithm

Make the Hash algorithm: sha1

--------------------------------------------------------------------		 

The tabid specified in the return links on sofort must be your store checkout tab.

		 
Step 5 - Select the Payment Provider in the module settings of the Checkout module.

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






