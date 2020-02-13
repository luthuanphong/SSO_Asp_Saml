using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace WebApplication2.Saml
{
    public class SamlRequestMessage
    {
        public string id;
        private string issue_instant;
        private AppSettings appSettings;

        public enum AuthRequestFormat
        {
            Base64 = 1
        }

        public SamlRequestMessage(AppSettings appSettings)
        {
            this.appSettings = appSettings;
            id = "_" + System.Guid.NewGuid().ToString();
            issue_instant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public string GetRequest(AuthRequestFormat format)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                using (XmlWriter xw = XmlWriter.Create(sw, xws))
                {
                    xw.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("ID", id);
                    xw.WriteAttributeString("Version", "2.0");
                    xw.WriteAttributeString("IssueInstant", issue_instant);
                    //xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
                    //xw.WriteAttributeString("AssertionConsumerServiceURL", appSettings.assertionConsumerServiceUrl);

                    xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString(appSettings.issuer);
                    xw.WriteEndElement();

                    //xw.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
                    //xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:unspecified");
                    //xw.WriteAttributeString("AllowCreate", "true");
                    //xw.WriteEndElement();

                    //xw.WriteStartElement("samlp", "RequestedAuthnContext", "urn:oasis:names:tc:SAML:2.0:protocol");
                    //xw.WriteAttributeString("Comparison", "exact");

                    //xw.WriteStartElement("saml", "AuthnContextClassRef", "urn:oasis:names:tc:SAML:2.0:assertion");
                    //xw.WriteString("urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport");
                    //xw.WriteEndElement();

                    //xw.WriteEndElement(); // RequestedAuthnContext

                    xw.WriteEndElement();
                }

                if (format == AuthRequestFormat.Base64)
                {
                    var bytes = Encoding.UTF8.GetBytes(sw.ToString());
                    using(var output = new MemoryStream())
                    {
                        using (var zip = new DeflateStream(output, CompressionMode.Compress))
                        {
                            zip.Write(bytes, 0, bytes.Length);
                        }

                        var base64 = Convert.ToBase64String(output.ToArray());
                        return HttpUtility.UrlEncode(base64);
                    }
                }

                return null;
            }
        }
    }
}