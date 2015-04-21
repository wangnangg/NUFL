// ***********************************************************************
// Copyright (c) 2013 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using NUnit.Engine;
using System.Text;
using System.Xml;

namespace NUFL.Framework.NUnitTestFilter
{
    /// <summary>
    /// IdFilter selects tests based on their id
    /// </summary>
    [Serializable]
    public class IdFilter:TestFilter
    {
        /// <summary>
        /// Construct an empty IdFilter
        /// </summary>
        public IdFilter() : base("<filter><id/></filter>") { }

        /// <summary>
        /// Construct a IdFilter for multiple ids
        /// </summary>
        /// <param name="ids">The ids the filter will recognize.</param>
        public IdFilter(IEnumerable<int> ids)
            : base("<filter><id></id></filter>")
        {
            
            StringBuilder builder = new StringBuilder();
           
            foreach(var id in ids)
            {
                builder.Append(id);
                builder.Append(",");
            }
            builder.Remove(builder.Length - 1, 1);

            this.IdNode.InnerText = builder.ToString();
            this.Text = this.Xml.OuterXml;
        }
        
        public XmlNode IdNode
        {
            get { return this.Xml.FirstChild; }
        }
        public void Add(int id)
        {
            
            this.IdNode.InnerText = this.Xml.InnerText + "," + id.ToString();
            this.IdNode.InnerText = this.Xml.InnerText.Trim(',');
            this.Text = this.Xml.OuterXml;
        }
    }
}
