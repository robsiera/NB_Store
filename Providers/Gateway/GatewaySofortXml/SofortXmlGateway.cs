/* --- Copyright (c) notice NevoWeb ---
  Copyright (c) 2013 SARL NevoWeb.  www.nevoweb.com. All rights are reserved.
 Author: D.C.Lee
 ------------------------------------------------------------------------
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 DEALINGS IN THE SOFTWARE.
 ------------------------------------------------------------------------
 This copyright notice may NOT be removed, obscured or modified without written consent from the author.
 --- End copyright notice --- 
*/

using System;
using System.Collections;
using System.Web;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Xml;
using NEvoWeb.Modules.NB_Store;


namespace Nevoweb.NBrightStore.Gateway
{

    public class SofortXml : GatewayInterface
    {

        public override string GetButtonHtml(int PortalID, int OrderID, string Lang)
        {
            string strHtml = "";
            string gatewayParams = SharedFunctions.GetStoreSetting(PortalID, "sofortxml.gateway", "None");

            if (!string.IsNullOrEmpty(gatewayParams))
            {
                var xmlDoc = new XmlDataDocument();
                xmlDoc.LoadXml(gatewayParams);

                var buttonimageurl = "";
                //Move the xml format settings into the a hashtable so we can use it easily.
                var selectSingleNode = xmlDoc.SelectSingleNode("multipay/nbs/buttonimageurl");
                if (selectSingleNode != null)
                {
                    buttonimageurl = selectSingleNode.InnerText;
                }

                //Build Html to display the payment button.
                strHtml = "<input type='image' name='sofortxmlgateway' border='0' src='" + buttonimageurl + "' />";

            }

            return strHtml;
        }


        // ------------------------------------------------------------------------
        // Step Three: Auto Response from banking gateway.  ()
        // If the banking gateway supports Automatic response (A url is send to the store just after the users make the payment,
        // but before they click to return to the store) then this function deals with what needs to be done in NB_Store.
        // NOTE:
        // - Not all gateways support this, in which case the payment processing for NB_Store needs to be done in the "GetCompletedHtml" function.
        // ------------------------------------------------------------------------
        public override void AutoResponse(int PortalID, System.Web.HttpRequest Request)
        {
            // NOTE:  THIS METHOD HAS NOW BEEN REPLACED BY THE USE OF THE Nevoweb.NBrightStore.Gateway.ExampleGateway.NotifyProvider.
 
            // It will still work.  However if the payment provider requires a specific response, then in DNN6 the whole web page is also returned.
            // Using the Notify web handler avoids this.

        }

        public override string GetCompletedHtml(int PortalID, int UserID, System.Web.HttpRequest Request)
        {
            SettingsController objSCtrl = new SettingsController();
            OrderController objOCtrl = new OrderController();
            ProductController objPCtrl = new ProductController();
            NB_Store_OrdersInfo objOInfo = default(NB_Store_OrdersInfo);
            NB_Store_SettingsTextInfo objSInfo = default(NB_Store_SettingsTextInfo);
            int ordID = -1;
            string rtnMsg = "";

            //Get the order id back from the url.
            var strID = Request.QueryString["ordID"];

            //set rtnMsg to security warning message.
            objSInfo = objSCtrl.GetSettingsText(PortalID, "paymentSECURITY.text", SharedFunctions.GetCurrentCulture());
            rtnMsg = objSInfo.SettingText;
            if (int.TryParse(strID, out ordID)) 
            {
                objOInfo = objOCtrl.GetOrder(ordID);
                if ((objOInfo != null))
                {
                    //Here we make sure that the user  trying to view the order is the same as the user that placed the order.
                    //If this is not the case, then we want to display the security warning.

                    if (objOInfo.UserID == UserID)
                    {
                        //The user is OK, so continue to pocess the order.
                        //  In most circustances the order will already have been processed by the autoresponse function.
                        //  However it's always good practise to check this and process the order if it has not been done already.
                        //  If the payment provider does not support AutoResponse, then you must do the order processing here.

                        switch (Request.QueryString["SofortExit"].ToUpper())
                        {
                            case "CANCEL":
                                // check if order has already been processed
                                if (!objOInfo.OrderIsPlaced)
                                {
                                    //check if order has already been cancelled
                                    if (objOInfo.OrderStatusID != 30)
                                    {
                                        //remove qty in trans
                                        objPCtrl.RemoveModelQtyTrans(objOInfo.OrderID);
                                        //set order status 
                                        objOInfo.OrderStatusID = 30;
                                        objOCtrl.UpdateObjOrder(objOInfo);
                                    }
                                    //set Cancel order message
                                    objSInfo = objSCtrl.GetSettingsText(PortalID, "paymentFAIL.text", SharedFunctions.GetCurrentCulture());
                                    rtnMsg = objSInfo.SettingText;

                                }
                                break;
                            case "RETURN":
                                // check if order has already been processed
                                if (!objOInfo.OrderIsPlaced)
                                {
                                    //remove qty in trans
                                    objPCtrl.UpdateStockLevel(objOInfo.OrderID);
                                    //set order status to Payed
                                    objOInfo.OrderStatusID = 40;
                                    objOInfo.OrderNumber = objOInfo.PortalID.ToString("00") + "-" + UserID.ToString("0000#") + "-" + objOInfo.OrderID.ToString("0000#") + "-" + objOInfo.OrderDate.ToString("yyyyMMdd");
                                    objOInfo.OrderIsPlaced = true;
                                    objOCtrl.UpdateObjOrder(objOInfo);
                                    SharedFunctions.SendEmailToClient(objOInfo.PortalID, SharedFunctions.GetClientEmail(objOInfo.PortalID, objOInfo), objOInfo.OrderNumber, objOInfo, "paymentOK.email", SharedFunctions.GetCurrentCulture());
                                    SharedFunctions.SendEmailToManager(objOInfo.PortalID, objOInfo.OrderNumber, objOInfo, "paymentOK.email");
                                }
                                //set completed order message
                                objSInfo = objSCtrl.GetSettingsText(PortalID, "paymentOK.text", SharedFunctions.GetCurrentCulture());
                                rtnMsg = objSInfo.SettingText;

                                break;
                            default:
                                break;
                            // break
                        }

                        //Do Token Replacement
                        var objTR = new TokenStoreReplace(objOInfo, SharedFunctions.GetMerchantCulture(PortalID));
                        rtnMsg = objTR.DoTokenReplace(rtnMsg);
                    }
                }
            }
            return rtnMsg;
        }

        public override bool GetBankClick(int PortalID, System.Web.HttpRequest Request)
        {
            //test if button has been clicked
            if ((Request.Form["sofortxmlgateway.x"] != null))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void SetBankRemotePost(int PortalID, int OrderID, string Lang, System.Web.HttpRequest Request)
        {
            // RemotePost class from NB_Store, this generates the re-direct html required.

            // clear the bank gateway redirect, so if there is an error we donmt; redirect to the wrong gateway.
            NB_Store_CartInfo objCInfo = CurrentCart.GetCurrentCart(PortalID);
            objCInfo.BankHtmlRedirect = "";
            CurrentCart.Save(objCInfo);

            //test if Pay button has been clicked
            if (GetBankClick(PortalID, Request))
            {
                //Get the gateway settings from the store setting Database
                string gatewayParams = SharedFunctions.GetStoreSetting(PortalID, "sofortxml.gateway", "None");

                if (!string.IsNullOrEmpty(gatewayParams))
                {
                    // get the gateway into a xml doc.
                    var xmlDoc = new XmlDataDocument();
                    xmlDoc.LoadXml(gatewayParams);

                   //-------------------------------------------------
                    // remove these extra values from the XML, they are added to the xml for ease of implementation and setup, but not needed by sofort
                    var xmlNod = xmlDoc.SelectSingleNode("multipay/nbs");
                    if (xmlNod != null) xmlNod.ParentNode.RemoveChild(xmlNod); 
                    
                    //Get the order we want to pay for.
                    OrderController objOCtrl = new OrderController();
                    NB_Store_OrdersInfo oInfo = objOCtrl.GetOrder(OrderID);
                    if ((oInfo != null))
                    {
                        var strXml = xmlDoc.OuterXml;
                        strXml = strXml.Replace("[ORDERID]", oInfo.OrderID.ToString(""));

                        var sendAmt = oInfo.Total.ToString("0.00");
                        // on some server runnign different langauge, a comma might be output as the deimcal seperator. 
                        sendAmt = sendAmt.Replace(",", ".");

                        strXml = strXml.Replace("[TOTALAMT]", SharedFunctions.HTTPPOSTEncode(sendAmt));
                        if (Lang.Length >= 2) strXml = strXml.Replace("[LANG]", Lang.Substring(0, 2));


                        //update stock transaction in progress
                        ProductController objPCtrl = new ProductController();
                        objPCtrl.UpdateModelQtyTrans(oInfo.OrderID);

                        //set order status to redirect to bank
                        oInfo.OrderStatusID = 20;
                        objOCtrl.UpdateObjOrder(oInfo);

                        // cal lthe sofort webservice using a webrequest and capture the xml return. log what we pass.
                        SharedFunctions.UpdateLog("Sofort Post XML = " + strXml);
                        var xmldata = Utils.PostSofortXmlData(PortalID, strXml);

                        var redirectUrl = "";
                        if (xmldata != "")
                        {
                            xmlDoc = new XmlDataDocument();
                            xmlDoc.LoadXml(xmldata);
                            var selectSingleNode = xmlDoc.SelectSingleNode("new_transaction/payment_url");
                            if (selectSingleNode != null) redirectUrl = selectSingleNode.InnerText;
                        }

                        //Now assign the values we need to the RemotePost class, ready to build the re-direct html.
                        var rPost = new RemotePost();
                        if (redirectUrl == "")
                        {
                            // if we have a empty redirectUrl, it because we have a error!!..so log it.
                            SharedFunctions.UpdateLog("Sofort Redirect HTML = " + xmldata);

                            //Save this data on the cart, so we can see what a error.
                            objCInfo.BankHtmlRedirect = rPost.GetPostHtml(xmldata);
                            CurrentCart.Save(objCInfo);
                        }
                        else
                        {
                            rPost.Url = redirectUrl;

                            //-------------------------------------------------
                            //We are now going to save the re-direct html into the cart data on the database.
                            //  This is because the actual re-direct is done by the checkout module, when it loads.
                            //Get the cart data from the database.
                            objCInfo = CurrentCart.GetCurrentCart(PortalID);
                            //get the gateway processing image we're going to display while connecting to the payment provider.
                            var gatewayImg = SharedFunctions.GetStoreSetting(PortalID, "gateway.loadimage", "None");
                            //Build and assign the re-direct html to the cart.
                            objCInfo.BankHtmlRedirect = rPost.GetPostHtml(gatewayImg);
                            //Save this data on the cart.
                            CurrentCart.Save(objCInfo);
                            //-------------------------------------------------
                        }


                    }
                }
            }
        }



    }


}