﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SasHarness
{
    /// <summary>
    /// A simple class to "wrap" the features of the SAS Workspace
    /// for use within the main calling program
    /// </summary>
    public class SasServer
    {
        /// <summary>
        /// Name of the server ("SASApp")
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Node name of the server
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Port (number), such as 8591
        /// </summary>
        public string Port { get; set; }
        /// <summary>
        /// User ID that can connect to a Workspace
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Password to connect to the Workspace 
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Whether to use Local (COM) connection instead of IOM Bridge
        /// </summary>
        public bool UseLocal { get; set; }

        /// <summary>
        /// Property for the SAS Workspace connection.
        /// Will connect if needed.
        /// </summary>
        public SAS.Workspace Workspace
        {
            get
            {
                if (_workspace == null)
                    Connect();
                
                if (_workspace!=null)
                    return _workspace;
                else
                    throw new Exception("Could not connect to SAS Workspace");
            }
        }

        /// <summary>
        /// Is a Workspace connected?
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _workspace != null;     
            }
        }

        /// <summary>
        /// Close the Workspace if connected
        /// </summary>
        public void Close()
        {
            if (IsConnected) _workspace.Close();
            _workspace = null;
        }

        #region Save and restore settings for convenience
        public string ToXml()
        {
            // note: we're not saving the password
            XElement settings = new XElement("SasServer");
            settings.SetAttributeValue("name", Name);
            settings.SetAttributeValue("host", Host);
            settings.SetAttributeValue("port", Port);
            settings.SetAttributeValue("userid", UserId);
            settings.SetAttributeValue("useLocal", XmlConvert.ToString(UseLocal));

            return settings.ToString();
        }

        public static SasServer FromXml(string xml)
        {
            SasServer s = new SasServer();
            XElement settings = XElement.Parse(xml);
            if (settings.Attribute("name") != null)
                s.Name = settings.Attribute("name").Value;
            if (settings.Attribute("host") != null)
                s.Host = settings.Attribute("host").Value;
            if (settings.Attribute("port") != null)
                s.Port = settings.Attribute("port").Value;
            if (settings.Attribute("userid") != null)
                s.UserId = settings.Attribute("userid").Value;
            if (settings.Attribute("useLocal") != null)
                s.UseLocal = XmlConvert.ToBoolean(settings.Attribute("useLocal").Value);

            return s;
        }
        #endregion

        /// <summary>
        /// Connect to a SAS Workspace
        /// </summary>
        public void Connect()
        {
            if (_workspace != null)
                try
                {
                    _workspace.Close();
                }
                catch { }
                finally
                { 
                    _workspace = null; 
                }

            if (!UseLocal)
            {
                // Connect using the IOM Bridge (TCP) for remote server
                SASObjectManager.IObjectFactory2 obObjectFactory = 
                    new SASObjectManager.ObjectFactoryMulti2();
                SASObjectManager.ServerDef obServer = 
                    new SASObjectManager.ServerDef();
                obServer.MachineDNSName = Host;
                obServer.Protocol = SASObjectManager.Protocols.ProtocolBridge;
                obServer.Port = Convert.ToInt32(Port);
                obServer.ClassIdentifier = "440196d4-90f0-11d0-9f41-00a024bb830c";
                _workspace = (SAS.Workspace)obObjectFactory.CreateObjectByServer(
                    Name, true, 
                    obServer, 
                    UserId, 
                    Password);

            }
            else
            {
                // Connect using COM protocol, locally installed SAS only
                SASObjectManager.IObjectFactory2 obObjectFactory = new SASObjectManager.ObjectFactoryMulti2();
                SASObjectManager.ServerDef obServer = new SASObjectManager.ServerDef();
                obServer.MachineDNSName = Host;
                obServer.Protocol = SASObjectManager.Protocols.ProtocolCom;
                obServer.Port = 0;
                _workspace = (SAS.Workspace)obObjectFactory.CreateObjectByServer(Name, true, obServer, null, null);
            }
        }

        private SAS.Workspace _workspace = null;


    }
}
