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

namespace NUFL.Framework.TestRunner
{
    [Serializable]
    public class NameFilter:TestFilter
    {

        public NameFilter() : base("<filter><tests></tests></filter>") { }


        public NameFilter(IEnumerable<string> names)
            : base("<filter><tests></tests></filter>")
        {
            string text = this.Text;
            XmlDocument document = new XmlDocument();
            document.LoadXml(text);
            var tests_node = document.SelectSingleNode("//tests");
            foreach(var name in names)
            {
                XmlElement node = document.CreateElement("test");
                node.InnerText = name;
                tests_node.AppendChild(node);
            }

            this.Text = document.OuterXml;
   
        }
       
    }
}
