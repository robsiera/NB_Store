Installation of PayPal Payment Provider for NB_Store   v1.2
----------------------------------------------------

Step1 - Install Gateway Provider Install package as a normal module in DNN.

Step2 - Update the "gatewayproviders.xml" found in the "/DesktopModules/NB_Store" directory.

	Add xml Reference Node of:
		
    <gateway ref="Paypal">
      <name>Paypal</name>
      <assembly>NEvoweb.DNN.Modules.NB_Store.GatewayPayPal</assembly>
      <class>NEvoWeb.Modules.NB_Store.Gateway.GatewayPayPal</class>
    </gateway>
		
Example:-

<?xml version="1.0" encoding="utf-8" ?>
<root>
  <gateways>  
  <gateway ref="SIPS">
    <name>SIPS</name>
    <assembly>NEvoweb.DNN.Modules.NB_Store.GatewaySIPS</assembly>
    <class>NEvoWeb.Modules.NB_Store.Gateway.GatewaySIPS</class>
  </gateway>
    <gateway ref="Paypal">
      <name>Paypal</name>
      <assembly>NEvoweb.DNN.Modules.NB_Store.GatewayPayPal</assembly>
      <class>NEvoWeb.Modules.NB_Store.Gateway.GatewayPayPal</class>
    </gateway>
  </gateways>
</root>


Step 3 - Create a payment gateway setting call "PayPal.gateway" and enter the settings needed.  
	
Example:-

<root>
        <paymentURL>https://www.sandbox.paypal.com/cgi-bin/webscr</paymentURL>
        <verifyURL>https://www.sandbox.paypal.com/cgi-bin/webscr</verifyURL>
        <PayPalID>paypal@MyWebsite.com</PayPalID>
        <CartName>TEST</CartName>
        <ButtonImageURL>/Desktopmodules/NB_Store_GatewayPayPal/paypal60x38.gif</ButtonImageURL>
        <Currency>EUR</Currency>
        <ReturnURL>http://www.MyWebsite.com/Panier/tabid/56/stg/5/ordid/[ORDERID]/Default.aspx?PayPalExit=RETURN</ReturnURL>
        <ReturnCancelURL>http://www.MyWebsite.com/Panier/tabid/56/stg/5/ordid/[ORDERID]/Default.aspx?PayPalExit=CANCEL</ReturnCancelURL>
        <ReturnNotifyURL>http://www.MyWebsite.com/Panier/tabid/56/stg/4/Default.aspx</ReturnNotifyURL>
        <MerchantLanguage>fr</MerchantLanguage>
        <paypalurl>
          <cmd>_xclick</cmd>
          <bn>NBStore</bn>
          <quantity>1</quantity>
          <undefined_quantity>0</undefined_quantity>
          <no_note>1</no_note>
          <no_shipping>1</no_shipping>
        </paypalurl>
</root>

or for more control you can specify extra url parameters, using the normal NB_Store tokens

<root>
        <paymentURL>https://www.sandbox.paypal.com/cgi-bin/webscr</paymentURL>
        <verifyURL>https://www.sandbox.paypal.com/cgi-bin/webscr</verifyURL>
        <PayPalID>paypal@MyWebsite.com</PayPalID>
        <CartName>TEST</CartName>
        <ButtonImageURL>/Desktopmodules/NB_Store_GatewayPayPal/paypal60x38.gif</ButtonImageURL>
        <Currency>EUR</Currency>
        <ReturnURL>http://www.MyWebsite.com/Panier/tabid/56/stg/5/ordid/[ORDERID]/Default.aspx?PayPalExit=RETURN</ReturnURL>
        <ReturnCancelURL>http://www.MyWebsite.com/Panier/tabid/56/stg/5/ordid/[ORDERID]/Default.aspx?PayPalExit=CANCEL</ReturnCancelURL>
        <ReturnNotifyURL>http://www.MyWebsite.com/Panier/tabid/56/stg/4/Default.aspx</ReturnNotifyURL>
        <MerchantLanguage>fr</MerchantLanguage>
        <paypalurl>
          <cmd>_xclick</cmd>
          <bn>NBStore</bn>
          <quantity>1</quantity>
          <undefined_quantity>0</undefined_quantity>
          <no_note>1</no_note>
          <no_shipping>1</no_shipping>
          <first_name>[OrderUser:FirstName]</first_name>
        </paypalurl>
</root>

Tokens that are automatically populated and CANNOT be specified by using the paypalurl node:

business
item_name
item_number
custom
amount
shipping
tax
currency_code
return
cancel_return
notify_url


	 
Step 4 - Select the Payment Provider in the Checkout module settings.

Step 5 - In the DNN menu "Host>Host Settings", click on "Restart Application".  This will clear the cache and ensure the new provider is used.


NOTE: In your paypal account, 

1 - Setup IPN to be the same return url as "ReturnNotifyURL" in your settings.

2 - Ensure paypal shipping settings have been cleared.






