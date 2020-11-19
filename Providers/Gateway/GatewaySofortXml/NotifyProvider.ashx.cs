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
using System.Web;
using NEvoWeb.Modules.NB_Store;

namespace Nevoweb.NBrightStore.Gateway
{

    public abstract class SofortXmlNotify : System.Web.IHttpHandler
	{

		#region "Event Handlers"
		public bool IsReusable {
			get { return true; }
		}


		public void ProcessRequest(System.Web.HttpContext context)
		{
            try
            {
                // not doing anything on this provider, but there are options in the Sofort gateway to use this. So I've left it here incase it's implemented later.
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                //something went wrong, lets log the error...
                SharedFunctions.SendEmailToAdministrator(0, "AutoReturn Notify ERROR", ex.ToString());
            }


		}
		#endregion

		#region "Private Methods"




		#endregion

	}

}
